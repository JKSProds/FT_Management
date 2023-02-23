using System;
using MySql.Simple;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using QRCoder;
using iTextSharp.text.pdf;
using OfficeOpenXml.Style;
using System.Net;
using Newtonsoft.Json;
using PdfSharpCore.Drawing;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Custom;

namespace FT_Management.Models
{
    public class FT_ManagementContext
    {

        public string ConnectionString { get; set; }
        private string FT_Logo_Print { get; set; }

        public FT_ManagementContext(string connectionString, string FT_Logo)
        {
            this.ConnectionString = connectionString;
            this.FT_Logo_Print = FT_Logo;

            try
            {
                Database db = ConnectionString;
            }
            catch
            {
                Console.WriteLine("Não foi possivel conectar á BD MySQL!");
            }
            if (this.ObterListaFeriados(DateTime.Now.Year.ToString()).Count() == 0) ObterFeriadosAPI(DateTime.Now.Year.ToString());
        }

        //UTILIZADOR
        #region UTILIZADORES
        public List<Utilizador> ObterListaUtilizadores(bool Enable, bool Viatura)
        {
            List<Utilizador> LstUtilizadores = new List<Utilizador>();
            string sqlQuery = "SELECT * FROM sys_utilizadores inner join dat_acessos_utilizador on sys_utilizadores.IdUtilizador = dat_acessos_utilizador.IdUtilizador " + (Enable ? "WHERE enable=1" : "") + " order by NomeCompleto;";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstUtilizadores.Add(new Utilizador()
                    {
                        Id = result["IdUtilizador"],
                        NomeUtilizador = result["NomeUtilizador"],
                        Password = result["Password"],
                        NomeCompleto = result["NomeCompleto"],
                        TipoUtilizador = result["TipoUtilizador"],
                        EmailUtilizador = result["EmailUtilizador"],
                        Telemovel = result["TelemovelUtilizador"],
                        IdPHC = result["IdPHC"],
                        Admin = result["admin"],
                        Enable = result["enable"],
                        Acessos = result["acessos"],
                        Dev = result["dev"],
                        TipoMapa = result["TipoMapa"],
                        CorCalendario = result["CorCalendario"],
                        Pin = result["PinUtilizador"],
                        Iniciais = result["IniciaisUtilizador"],
                        DataNascimento = result["DataNascimento"],
                        IdArmazem = result["IdArmazem"],
                        TipoTecnico = result["TipoTecnico"],
                        Zona = result["Zona"],
                        ChatToken = result["ChatToken"],
                        SecondFactorAuthStamp = result["SecondFactorAuthStamp"],
                        NotificacaoAutomatica = result["NotificacaoAutomatica"],
                        UltimoAcesso = result["DataUltimoAcesso"],
                        AcessoAtivo = result["TipoUltimoAcesso"] == 1,
#if !DEBUG
                        ImgUtilizador = string.IsNullOrEmpty(result["ImgUtilizador"]) ? "/img/user.png" : result["ImgUtilizador"],
#endif
                    });
                    if (Viatura) LstUtilizadores.Last().Viatura = ObterViatura(LstUtilizadores.Last());
                }
            }
            return LstUtilizadores;
        }
        public List<Utilizador> ObterListaTecnicos(bool Enable, bool Viatura)
        {
            return ObterListaUtilizadores(Enable, Viatura).Where(u => u.TipoUtilizador == 1).ToList();
        }
        public List<Utilizador> ObterListaComerciais(bool Enable)
        {
            return ObterListaUtilizadores(Enable, false).Where(u => u.TipoUtilizador == 2).ToList();
        }
        public Utilizador ObterUtilizador(int Id)
        {
            Utilizador utilizador = new Utilizador();
            string sqlQuery = "SELECT * FROM sys_utilizadores left join sys_api_keys on sys_utilizadores.IdUtilizador=sys_api_keys.IdUtilizador where sys_utilizadores.IdUtilizador=" + Id + ";";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    utilizador = new Utilizador()
                    {
                        Id = result["IdUtilizador"],
                        NomeUtilizador = result["NomeUtilizador"],
                        Password = result["Password"],
                        NomeCompleto = result["NomeCompleto"],
                        TipoUtilizador = int.Parse(result["TipoUtilizador"]),
                        EmailUtilizador = result["EmailUtilizador"],
                        Admin = result["admin"] == 1,
                        Enable = result["enable"] == 1,
                        Acessos = result["acessos"],
                        Dev = result["dev"],
                        Telemovel = result["TelemovelUtilizador"],
                        CorCalendario = result["CorCalendario"],
                        IdPHC = result["IdPHC"],
                        TipoMapa = result["TipoMapa"],
                        Pin = result["PinUtilizador"],
                        Iniciais = result["IniciaisUtilizador"],
                        DataNascimento = result["DataNascimento"],
                        IdArmazem = result["IdArmazem"],
                        TipoTecnico = result["TipoTecnico"],
                        Zona = result["Zona"],
                        ChatToken = result["ChatToken"],
                        NotificacaoAutomatica = result["NotificacaoAutomatica"],
                        SecondFactorAuthStamp = result["SecondFactorAuthStamp"],
#if !DEBUG 
                        ImgUtilizador = string.IsNullOrEmpty(result["ImgUtilizador"]) ? "/img/user.png" : result["ImgUtilizador"],
#endif
                    };
                    if (!string.IsNullOrEmpty(result["ID"]))
                    {
                        utilizador.ApiKey = new ApiKey()
                        {
                            Id = result["ID"],
                            Descricao = result["Descricao"],
                            Utilizador = new Utilizador() { Id = result["IdUtilizador"] },
                            Key = result["ApiKey"]
                        };
                    }
                    utilizador.Viatura = ObterViatura(utilizador);

                }
            }
            return utilizador;
        }

        public List<Marcacao> ObterUtilizadorMarcacao(List<Marcacao> LstMarcacao)
        {
            List<Utilizador> LstUtilizadores = ObterListaUtilizadores(false, false);

            foreach (var item in LstMarcacao)
            {
                if (item.IdTecnico == 0) { item.Tecnico = new Utilizador(); }
                else
                {
                    item.Tecnico = LstUtilizadores.Where(u => u.IdPHC == item.IdTecnico).FirstOrDefault() ?? new Utilizador();

                    try
                    {
                        foreach (var tech in item.LstTecnicosSelect)
                        {
                            item.LstTecnicos.Add(LstUtilizadores.Where(u => u.IdPHC == tech).FirstOrDefault() ?? new Utilizador());
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Não foi possivel obter a lista de técnicos do PHC!\r\n(Exception: " + ex.Message + ")");
                    }
                }
            }

            return LstMarcacao;
        }
        public int ObterIdUtilizadorApiKey(string ApiKey)
        {
            string sqlQuery = "SELECT IdUtilizador FROM sys_api_keys where ApiKey='" + ApiKey + "';";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    return int.Parse(result["IdUtilizador"]);
                }
            }
            return 0;
        }

        public List<Zona> ObterZonas()
        {
            string sqlQuery = "SELECT * FROM sys_zonas;";
            List<Zona> LstZonas = new List<Zona>();

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstZonas.Add(new Zona() { Id = result["IdZona"], Valor = result["Zona"] });
                }
            }
            return LstZonas;
        }

        public List<TipoTecnico> ObterTipoTecnicos()
        {
            string sqlQuery = "SELECT * FROM sys_tipo_tecnico;";
            List<TipoTecnico> LstTipoTecnico = new List<TipoTecnico>();

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstTipoTecnico.Add(new TipoTecnico() { Id = result["IdTipoTecnico"], Valor = result["TipoTecnico"] });
                }
            }
            return LstTipoTecnico;
        }

        public void NovoUtilizador(Utilizador utilizador)
        {
            string sql = "INSERT INTO sys_utilizadores (IdUtilizador, NomeUtilizador, Password, PinUtilizador, NomeCompleto, TipoUtilizador, EmailUtilizador, admin, enable, acessos, dev, IdPHC, IdArmazem, IniciaisUtilizador, CorCalendario, TipoMapa, DataNascimento, TelemovelUtilizador, ImgUtilizador, TipoTecnico, Zona, ChatToken, NotificacaoAutomatica, SecondFactorAuthStamp) VALUES ";

            sql += ("('" + utilizador.Id + "', '" + utilizador.NomeUtilizador + "', '" + utilizador.Password + "', '" + utilizador.Pin + "', '" + utilizador.NomeCompleto + "', '" + utilizador.TipoUtilizador + "', '" + utilizador.EmailUtilizador + "', '" + (utilizador.Admin ? "1" : "0") + "', '" + (utilizador.Enable ? "1" : "0") + "', '" + (utilizador.Acessos ? "1" : "0") + "', '" + (utilizador.Dev ? "1" : "0") + "', '" + utilizador.IdPHC + "', '" + utilizador.IdArmazem + "', '" + utilizador.Iniciais + "', '" + utilizador.CorCalendario + "', " + utilizador.TipoMapa + ", '" + utilizador.DataNascimento.ToString("yyyy-MM-dd") + "', '" + utilizador.ObterTelemovelFormatado(true) + "', '" + utilizador.ImgUtilizador + "', '" + utilizador.TipoTecnico + "', '" + utilizador.Zona + "', '" + utilizador.ChatToken + "', '" + utilizador.NotificacaoAutomatica + "', '" + utilizador.SecondFactorAuthStamp + "') \r\n");

            sql += " ON DUPLICATE KEY UPDATE Password = VALUES(Password), PinUtilizador = VALUES(PinUtilizador), NomeCompleto = VALUES(NomeCompleto), TipoUtilizador = VALUES(TipoUtilizador), EmailUtilizador = VALUES(EmailUtilizador), admin = VALUES(admin), enable = VALUES(enable), acessos = VALUES(acessos), dev = VALUES(dev), IdPHC = VALUES(IdPHC), IdArmazem = VALUES(IdArmazem), IniciaisUtilizador = VALUES(IniciaisUtilizador), CorCalendario = VALUES(CorCalendario), TipoMapa = VALUES(TipoMapa), DataNascimento = VALUES(DataNascimento), TelemovelUtilizador = VALUES(TelemovelUtilizador), ImgUtilizador = VALUES(ImgUtilizador), TipoTecnico = VALUES(TipoTecnico), Zona = VALUES(Zona), ChatToken = VALUES(ChatToken), NotificacaoAutomatica = VALUES(NotificacaoAutomatica), SecondFactorAuthStamp = VALUES(SecondFactorAuthStamp);";

            using (Database db = ConnectionString)
            {
                db.Execute(sql);
            }
        }

        public void ApagarUtilizador(Utilizador u)
        {
            string sql = "DELETE FROM sys_utilizadores WHERE IdUtilizador=" + u.Id + ";\r\n";

            Database db = ConnectionString;

            db.Execute(sql);
            db.Connection.Close();
        }
        public string NovaApiKey(Utilizador utilizador)
        {
            string RandomApiKey = GetRandomString(40);
            string sql = "INSERT INTO sys_api_keys (ID, Descricao, IdUtilizador, ApiKey) VALUES ";

            sql += ("(0, '" + utilizador.NomeUtilizador + "', '" + utilizador.Id + "', '" + RandomApiKey + "') \r\n");

            sql += " ON DUPLICATE KEY UPDATE Descricao = VALUES(Descricao), ApiKey = VALUES(ApiKey);";

            using (Database db = ConnectionString)
            {
                db.Execute(sql);
            }
            return RandomApiKey;
        }

        public Viatura ObterViatura(Utilizador utilizador)
        {
            Viatura res = new Viatura();

            string sqlQuery = "SELECT *, (Select  fim_localizacao from dat_viaturas_viagens where matricula_viatura=dat_viaturas.matricula_viatura order by timestamp DESC limit 1) as localizacao2 FROM dat_viaturas where IdUtilizador='" + utilizador.Id + "' order by UltimaAtualizacao DESC LIMIT 1;";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    res = new Viatura()
                    {
                        Matricula = result["matricula_viatura"],
                        LocalizacaoMorada = result["localizacao"],
                        Latitude = result["latitude"],
                        Longitude = result["longitude"],
                        KmsAtuais = result["ultimoKms"],
                        Velocidade = result["Velocidade"],
                        Combustivel = result["Combustivel"],
                        Utilizador = utilizador,
                        Ignicao = result["ignicao"] == 1,
                        Buzzer = result["Buzzer"] == 1,
                        UltimoUpdate = DateTime.Parse(result["UltimaAtualizacao"])
                    };
                    if (DateTime.Parse(result["timestamp"]) < DateTime.Now.AddMinutes(-60))
                    {
                        res.Ignicao = result["localizacao2"] == "";
                        res.LocalizacaoMorada = result["localizacao2"] == "" ? "Não foi possivel obter a localização desta viatura!" : result["localizacao2"];
                    }
                }
            }


            return res == null ? new Viatura() : res;
        }

        public Viatura ObterViatura(string Matricula)
        {
            Viatura res = new Viatura();

            string sqlQuery = "SELECT *, (Select  fim_localizacao from dat_viaturas_viagens where matricula_viatura=dat_viaturas.matricula_viatura order by timestamp DESC limit 1) as localizacao2 FROM dat_viaturas where matricula_viatura='" + Matricula + "' order by UltimaAtualizacao DESC LIMIT 1;";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    res = new Viatura()
                    {
                        Matricula = result["matricula_viatura"],
                        LocalizacaoMorada = result["localizacao"],
                        Latitude = result["latitude"],
                        Longitude = result["longitude"],
                        KmsAtuais = result["ultimoKms"],
                        Velocidade = result["Velocidade"],
                        Combustivel = result["Combustivel"],
                        Utilizador = ObterUtilizador(int.Parse(result["IdUtilizador"])),
                        Ignicao = result["ignicao"] == 1,
                        Buzzer = result["Buzzer"] == 1,
                        UltimoUpdate = DateTime.Parse(result["UltimaAtualizacao"])
                    };
                    if (DateTime.Parse(result["timestamp"]) < DateTime.Now.AddMinutes(-60))
                    {
                        res.Ignicao = result["localizacao2"] == "";
                        res.LocalizacaoMorada = result["localizacao2"] == "" ? "Não foi possivel obter a localização desta viatura!" : result["localizacao2"];
                    }
                }
            }


            return res == null ? new Viatura() : res;
        }

        public List<Viatura> ObterViaturas()
        {
            List<Viatura> res = new List<Viatura>();
            List<Utilizador> LstUtilizadores = ObterListaUtilizadores(true, false).ToList();
            string sqlQuery = "SELECT * FROM dat_viaturas;";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    res.Add(new Viatura()
                    {
                        Matricula = result["matricula_viatura"],
                        LocalizacaoMorada = result["localizacao"],
                        Latitude = result["latitude"],
                        Longitude = result["longitude"],
                        KmsAtuais = result["ultimoKms"],
                        Velocidade = result["Velocidade"],
                        Combustivel = result["Combustivel"],
                        Utilizador = LstUtilizadores.Where(u => u.Id == int.Parse(result["IdUtilizador"])).FirstOrDefault(),
                        Ignicao = result["ignicao"] == 1,
                        Buzzer = result["Buzzer"] == 1,
                        UltimoUpdate = DateTime.Parse(result["UltimaAtualizacao"])
                    });
                    if (res.Last().Utilizador == null) res.Last().Utilizador = new Utilizador();
                }
            }


            return res.OrderBy(v => v.Utilizador.NomeCompleto).ToList();
        }

        public void AtualizarBuzzer(Viatura v)
        {
            string sql = "UPDATE dat_viaturas set Buzzer=" + v.Buzzer + " WHERE matricula_viatura='" + v.Matricula + "';";

            using (Database db = ConnectionString)
            {
                db.Execute(sql);
            }
        }

        public List<Viagem> ObterViagens(string Matricula, string DataViagens)
        {
            List<Viagem> res = new List<Viagem>();
            string sqlQuery = "SELECT * FROM dat_viaturas_viagens where matricula_viatura='" + Matricula + "' and inicio_viagem>='" + DateTime.Parse(DataViagens).ToString("yyyy-MM-dd") + " 00:00:00' and fim_viagem<='" + DateTime.Parse(DataViagens).ToString("yyyy-MM-dd") + " 23:59:59';";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    res.Add(new Viagem()
                    {
                        Matricula = result["matricula_viatura"],
                        Inicio_Viagem = result["inicio_viagem"],
                        Fim_Viagem = result["fim_viagem"],
                        Inicio_Local = result["inicio_localizacao"],
                        Fim_Local = result["fim_localizacao"],
                        Inicio_Kms = result["inicio_kms"],
                        Fim_Kms = result["fim_kms"],
                        Distancia_Viagem = result["distancia_viagem"],
                        Tempo_Viagem = result["tempo_viagem"]
                    });
                }
            }

            return res.OrderBy(v => v.Inicio_Viagem).ToList();
        }

        public List<string> ObterPermissoes()
        {
            List<string> res = new List<string>();
            string sqlQuery = "SELECT * FROM sys_permissoes;";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    res.Add(result["Nome"] + "_View");
                    res.Add(result["Nome"] + "_Edit");
                    res.Add(result["Nome"] + "_Admin");
                }
            }

            return res;
        }

        #endregion

        //PARAMETROS
        #region PARAMETROS
        public string ObterParam(string NomeParam)
        {
            string res = "";
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT Valor FROM sys_params where Nome = '" + NomeParam + "';");
                result.Read();

                res = result["Valor"];
            }

            return res;
        }
        public static string ObterParam(string NomeParam, string ConnectionString)
        {
            string res = "";
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT Valor FROM sys_params where Nome = '" + NomeParam + "';");
                result.Read();

                res = result["Valor"];
            }

            return res;
        }
        public List<String> ObterEmailsCC(int TipoEmail)
        {
            List<String> LstEmails = new List<String>();
            using (Database db = ConnectionString)
            {
                string sql = "SELECT Email FROM sys_emails where Tipo=" + TipoEmail + ";";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstEmails.Add(result[0]);
                }
            }

            return LstEmails;

        }
        public DateTime ObterUltimaModificacaoPHC(string tabela)
        {
            DateTime res = new DateTime();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT ultimamodificacao FROM sys_tabelas where nometabela = '" + tabela + "'; ");
                while (result.Read())
                {
                    res = result[0];
                }
            }


            return res;
        }
        public void AtualizarUltimaModificacao(string tabela, string data)
        {
            string sql = "UPDATE sys_tabelas set ultimamodificacao='" + data + "' where nometabela='" + tabela + "'";

            Database db = ConnectionString;

            db.Execute(sql);
            db.Connection.Close();
        }
        #endregion

        //LOGS
        #region LOGS
        public List<Log> ObterListaLogs(int IdUtilizador)
        {
            List<Log> LstLogs = new List<Log>();
            Utilizador u = ObterUtilizador(IdUtilizador);
            string sqlQuery = "SELECT * FROM dat_logs where id_user=" + u.Id + " order by data_log;";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstLogs.Add(new Log()
                    {
                        Id = result["id_log"],
                        Utilizador = u,
                        Descricao = result["msg_log"],
                        Data = result["data_log"],
                        Tipo = result["tipo_log"]
                    });
                }
            }
            return LstLogs;
        }
        public void AdicionarLog(int id_user, string msg, int tipo)
        {
            string sql = "INSERT INTO dat_logs (id_user, msg_log, tipo_log) VALUES ('" + id_user + "', '" + msg + "', " + tipo + ");";

            Database db = ConnectionString;

            db.Execute(sql);
            db.Connection.Close();
        }
        #endregion

        //VISITAS
        #region Visitas
        public List<Visita> ObterListaVisitas(int IdComercial, DateTime DataInicial, DateTime DataFinal)
        {
            PHCContext phccontext = new PHCContext(ConfigurationManager.AppSetting["ConnectionStrings:PHCConnection"], ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"]);

            List<Visita> LstVisitas = new List<Visita>();
            String DataI = DataInicial.ToString("yyyy-MM-dd");
            String DataF = DataFinal.ToString("yyyy-MM-dd");
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_visitas where dat_visitas.idcomercial=" + IdComercial + " AND DataVisita>='" + DataI + "'  AND DataVisita<='" + DataF + "';");
                while (result.Read())
                {
                    LstVisitas.Add(new Visita()
                    {
                        IdVisita = result["IdVisita"],
                        DataVisita = DateTime.Parse(result["DataVisita"]),
                        Cliente = result["IdContacto"] > 0 ? ObterClienteContacto(result["IdContacto"]) : phccontext.ObterClienteSimples(result["IdCliente"], result["IdLoja"]),
                        ResumoVisita = result["ResumoVisita"],
                        EstadoVisita = result["EstadoVisita"],
                        ObsVisita = result["ObsVisita"],
                        VisitaStamp = result["VisitaStamp"],
                        IdComercial = result["idcomercial"]

                    });
                }
            }

            return LstVisitas;

        }
        public List<Visita> ObterListaVisitas(DateTime DataInicial, DateTime DataFinal)
        {
            PHCContext phccontext = new PHCContext(ConfigurationManager.AppSetting["ConnectionStrings:PHCConnection"], ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"]);

            List<Visita> LstVisitas = new List<Visita>();
            String DataI = DataInicial.ToString("yyyy-MM-dd");
            String DataF = DataFinal.ToString("yyyy-MM-dd");
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_visitas where DataVisita>='" + DataI + "'  AND DataVisita<='" + DataF + "' order by DataVisita, IdComercial;;");
                while (result.Read())
                {
                    LstVisitas.Add(new Visita()
                    {
                        IdVisita = result["IdVisita"],
                        DataVisita = DateTime.Parse(result["DataVisita"]),
                        Cliente = result["IdContacto"] > 0 ? ObterClienteContacto(result["IdContacto"]) : phccontext.ObterClienteSimples(result["IdCliente"], result["IdLoja"]),
                        ResumoVisita = result["ResumoVisita"],
                        EstadoVisita = result["EstadoVisita"],
                        ObsVisita = result["ObsVisita"],
                        VisitaStamp = result["VisitaStamp"],
                        IdComercial = result["idcomercial"]

                    });
                }
            }

            return LstVisitas;

        }
        public List<Visita> ObterListaVisitasCliente(int IdCliente, int IdLoja)
        {
            List<Visita> LstVisitas = new List<Visita>();
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_visitas where IdCliente='" + IdCliente + "'  AND IdLoja='" + IdLoja + "' order by DataVisita, IdComercial;;");
                while (result.Read())
                {
                    LstVisitas.Add(new Visita()
                    {
                        IdVisita = result["IdVisita"],
                        DataVisita = DateTime.Parse(result["DataVisita"]),
                        ResumoVisita = result["ResumoVisita"],
                        EstadoVisita = result["EstadoVisita"],
                        ObsVisita = result["ObsVisita"],
                        VisitaStamp = result["VisitaStamp"],
                        IdComercial = result["idcomercial"],
                        Propostas = ObterListaPropostasVisita(result["IdVisita"])

                    });
                }
            }

            return LstVisitas;

        }
        public Visita ObterVisita(int IdVisita)
        {
            PHCContext phccontext = new PHCContext(ConfigurationManager.AppSetting["ConnectionStrings:PHCConnection"], ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"]);

            Visita visita = new Visita();
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_visitas where IdVisita = " + IdVisita + ";");
                result.Read();

                visita = new Visita()
                {
                    IdVisita = result["IdVisita"],
                    DataVisita = DateTime.Parse(result["DataVisita"]),
                    Cliente = result["IdContacto"] > 0 ? ObterClienteContacto(result["IdContacto"]) : phccontext.ObterClienteSimples(result["IdCliente"], result["IdLoja"]),
                    ResumoVisita = result["ResumoVisita"],
                    EstadoVisita = result["EstadoVisita"],
                    ObsVisita = result["ObsVisita"],
                    VisitaStamp = result["VisitaStamp"],
                    IdComercial = result["idcomercial"],
                    Contacto = new Contacto() { IdContacto = result["IdContacto"] },
                    UrlAnexos = result["UrlAnexos"]
                };
            }

            visita.Propostas = ObterListaPropostasVisita(IdVisita);
            return visita;
        }

        public Proposta ObterProposta(int IdProposta)
        {
            List<Proposta> LstPropostas = new List<Proposta>();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_propostas where dat_propostas.IdProposta=" + IdProposta + ";");
                while (result.Read())
                {
                    LstPropostas.Add(new Proposta()
                    {
                        IdProposta = result["IdProposta"],
                        Comercial = new Utilizador() { Id = result["IdComercial"] },
                        //Visita = ObterVisita(result["IdVisita"]),
                        IdVisita = result["IdVisita"],
                        DataProposta = result["DataProposta"],
                        EstadoProposta = result["EstadoProposta"],
                        ValorProposta = result["ValorProposta"],
                        UrlProposta = result["UrlAnexo"],
                        ObsProposta = result["ObsProposta"]
                    });
                }
            }
            return LstPropostas.First();

        }
        public List<Proposta> ObterListaPropostasVisita(int IdVisita)
        {
            List<Proposta> LstPropostas = new List<Proposta>();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_propostas where dat_propostas.idvisita=" + IdVisita + ";");
                while (result.Read())
                {
                    LstPropostas.Add(new Proposta()
                    {
                        IdProposta = result["IdProposta"],
                        Comercial = ObterUtilizador(result["IdComercial"]),
                        //Visita = ObterVisita(result["IdVisita"]),
                        DataProposta = result["DataProposta"],
                        EstadoProposta = result["EstadoProposta"],
                        ValorProposta = result["ValorProposta"],
                        UrlProposta = result["UrlAnexo"],
                        ObsProposta = result["ObsProposta"]
                    });
                }
            }
            return LstPropostas;
        }
        public List<CalendarioEvent> ConverterVisitasEventos(List<Visita> Visitas)
        {
            List<CalendarioEvent> LstEventos = new List<CalendarioEvent>();

            DateTime dataMarcacao = DateTime.Parse(DateTime.Now.ToShortDateString() + " 00:00:00");
            dataMarcacao.AddMinutes(5);
            Utilizador comercial = new Utilizador();
            foreach (var item in Visitas)
            {
                if (LstEventos.Count > 0 && LstEventos.Last().IdTecnico != item.IdComercial) { dataMarcacao = dataMarcacao.AddMinutes(5); comercial = ObterUtilizador(item.IdComercial); }
                if (dataMarcacao.ToShortDateString() != item.DataVisita.ToShortDateString()) dataMarcacao = DateTime.Parse(item.DataVisita.ToShortDateString() + " 00:00:00");
                if (LstEventos.Count == 0) comercial = ObterUtilizador(item.IdComercial);

                LstEventos.Add(new CalendarioEvent
                {
                    id = item.IdVisita.ToString(),
                    calendarId = "1",
                    title = (item.EstadoVisita == "Finalizado" ? "✔ " : item.EstadoVisita != "Criado" && item.EstadoVisita != "Agendado" ? "⌛ " : item.DataVisita < DateTime.Now ? "❌ " : "") + comercial.Iniciais + " - " + item.Cliente.NomeCliente,
                    location = item.Cliente.MoradaCliente,
                    start = dataMarcacao,
                    end = dataMarcacao.AddMinutes(25),
                    IdTecnico = item.IdComercial,
                    raw = "Visita/?idVisita=" + item.IdVisita + "&IdComercial=" + (item.IdComercial),
                    category = "time",
                    dueDateClass = "",
                    bgColor = (comercial.CorCalendario == string.Empty ? "#3371FF" : comercial.CorCalendario),
                    body = item.ResumoVisita
                });
                dataMarcacao = dataMarcacao.AddMinutes(20);
            }

            return LstEventos;
        }
        public void CriarVisitas(List<Visita> LstVisita)
        {
            int max = 1000;
            int j = 0;
            for (int i = 0; j < LstVisita.Count; i++)
            {
                if ((j + max) > LstVisita.Count) max = (LstVisita.Count - j);

                string sql = "INSERT INTO dat_visitas (IdVisita,DataVisita,IdCliente,IdLoja,ResumoVisita,EstadoVisita,ObsVisita,IdComercial, IdContacto, UrlAnexos) VALUES ";

                foreach (var visita in LstVisita.GetRange(j, max))
                {
                    if (visita.ObsVisita is null) visita.ObsVisita = String.Empty;
                    sql += ("('" + visita.IdVisita + "', '" + visita.DataVisita.ToString("yy-MM-dd") + "', '" + visita.Cliente.IdCliente + "', '" + visita.Cliente.IdLoja + "', '" + visita.ResumoVisita.Replace("'", "''").Replace("\\", "").ToString() + "', '" + visita.EstadoVisita + "', '" + visita.ObsVisita.Replace("'", "''").Replace("\\", "").ToString() + "', '" + visita.IdComercial + "', '" + visita.Contacto.IdContacto + "', '" + visita.UrlAnexos + "'), \r\n");
                    i++;
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE DataVisita=VALUES(DataVisita), IdCliente = VALUES(IdCliente), ResumoVisita = VALUES(ResumoVisita), EstadoVisita = VALUES(EstadoVisita), ObsVisita = VALUES(ObsVisita), IdComercial = VALUES(IdComercial), IdContacto = VALUES(IdContacto), UrlAnexos = VALUES(UrlAnexos);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();

                j += max;
            }
        }
        public void CriarPropostas(List<Proposta> LstPropostas)
        {
            int max = 1000;
            int j = 0;
            for (int i = 0; j < LstPropostas.Count; i++)
            {
                if ((j + max) > LstPropostas.Count) max = (LstPropostas.Count - j);

                string sql = "INSERT INTO dat_propostas (IdProposta,DataProposta,IdComercial,IdVisita,EstadoProposta,ValorProposta,UrlAnexo, ObsProposta) VALUES ";

                foreach (var proposta in LstPropostas.GetRange(j, max))
                {
                    sql += ("('" + proposta.IdProposta + "', '" + proposta.DataProposta.ToString("yy-MM-dd") + "', '" + proposta.Comercial.Id + "', '" + proposta.Visita.IdVisita + "', '" + proposta.EstadoProposta.Replace("'", "''").Replace("\\", "").ToString() + "', '" + proposta.ValorProposta + "', '" + proposta.UrlProposta + "', '" + proposta.ObsProposta + "'), \r\n");
                    i++;
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE DataProposta=VALUES(DataProposta), IdComercial = VALUES(IdComercial), EstadoProposta = VALUES(EstadoProposta), ValorProposta = VALUES(ValorProposta), UrlAnexo = VALUES(UrlAnexo), ObsProposta = VALUES(ObsProposta);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();

                j += max;
            }
        }
        public void ApagarVisita(int id)
        {
            Database db = ConnectionString;
            String sql = "delete from dat_visitas Where IdVisita='" + id + "';";
            db.Execute(sql);
            db.Connection.Close();
        }
        public void ApagarProposta(int id)
        {
            Database db = ConnectionString;
            String sql = "delete from dat_propostas Where IdProposta='" + id + "';";
            db.Execute(sql);
            db.Connection.Close();
        }
        #endregion

        //FERIAS
        #region Ferias
        public bool VerificarFeriasUtilizador(int IdUtilizador, DateTime Data)
        {
            bool res = false;

            using (Database db = ConnectionString)
            {
                using var resultQuery = db.QueryValue("select count(*) from dat_ferias where DataInicio<='" + Data.ToString("yyyy-MM-dd") + "' AND DataFim >= '" + Data.ToString("yyyy-MM-dd") + "' AND IdUtilizador=" + IdUtilizador + ";");
                res = (resultQuery > 0);
            }

            return res;
        }
        public int ObterDiasDireitoUtilizador(int IdUtilizador, string Ano)
        {
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_ferias_utilizador where IdUtilizador='" + IdUtilizador + "' AND Ano='" + Ano + "';");
                while (result.Read())
                {

                    return int.Parse(result["DiasDireito"]);
                }
            }
            return 0;
        }
        public FeriasUtilizador ObterListaFeriasUtilizador(int IdUtilizador)
        {
            FeriasUtilizador feriasUtilizador = new FeriasUtilizador();

            using (Database db = ConnectionString)
            {
                string lastYear = (int.Parse(this.ObterAnoAtivo()) - 1).ToString();
                using var resultQuery = db.QueryValue("SELECT Count(*) from dat_ferias_utilizador where IdUtilizador='" + IdUtilizador + "' AND Ano='" + this.ObterAnoAtivo() + "';");
                if (resultQuery == 0) CriarFeriasUtilizador(IdUtilizador, this.ObterAnoAtivo(), 23 + (ObterDiasDireitoUtilizador(IdUtilizador, lastYear) - int.Parse(ObterFeriasDias(IdUtilizador, lastYear))));
            }

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_ferias_utilizador where IdUtilizador='" + IdUtilizador + "' AND Ano='" + this.ObterAnoAtivo() + "';");
                while (result.Read())
                {

                    feriasUtilizador.utilizador = ObterUtilizador(int.Parse(result["IdUtilizador"]));
                    feriasUtilizador.DiasMarcados = int.Parse(ObterFeriasMarcadas(IdUtilizador, this.ObterAnoAtivo()));
                    feriasUtilizador.DiasTotais = int.Parse(ObterFeriasDias(IdUtilizador, this.ObterAnoAtivo()));
                    feriasUtilizador.DiasDisponiveis = int.Parse(result["DiasDireito"]);
                    feriasUtilizador.Ferias = ObterListaFerias(IdUtilizador);
                }
            }

            return feriasUtilizador;

        }
        public List<Ferias> ObterListaFerias(int IdUtilizador)
        {
            List<Ferias> LstFerias = new List<Ferias>();
            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM dat_ferias where IdUtilizador='" + IdUtilizador + "' AND Ano='" + this.ObterAnoAtivo() + "' order by DataInicio;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstFerias.Add(new Ferias()
                    {
                        Id = result["Id"],
                        IdUtilizador = result["IdUtilizador"],
                        DataInicio = result["DataInicio"],
                        DataFim = result["DataFim"],
                        Validado = result["Validado"],
                        Obs = result["Obs"],
                        Aniversario = result["Aniversario"] == "True",
                        ValidadoPor = result["ValidadoPor"],
                        ValidadoPorNome = result["ValidadoPor"] == 0 ? "" : ObterUtilizador(result["ValidadoPor"]).NomeCompleto,
                    });
                }
            }

            return LstFerias;

        }

        public List<Ferias> ObterListaFerias(DateTime dataInicio, DateTime dataFim, int IdUtilizador)
        {
            List<Ferias> LstFerias = new List<Ferias>();
            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM dat_ferias where IdUtilizador='" + IdUtilizador + "' AND (DataInicio between '" + dataInicio.ToString("yyyy-MM-dd") + "' AND '" + dataFim.ToString("yyyy-MM-dd") + "' or '" + dataInicio.ToString("yyyy-MM-dd") + "' between DataInicio and DataFim)  order by DataInicio;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstFerias.Add(new Ferias()
                    {
                        Id = result["Id"],
                        IdUtilizador = result["IdUtilizador"],
                        DataInicio = result["DataInicio"],
                        DataFim = result["DataFim"],
                        Validado = result["Validado"],
                        Obs = result["Obs"],
                        Aniversario = result["Aniversario"] == "True",
                        ValidadoPor = result["ValidadoPor"],
                        ValidadoPorNome = result["ValidadoPor"] == 0 ? "" : ObterUtilizador(result["ValidadoPor"]).NomeCompleto,
                    });
                }
            }

            return LstFerias;

        }
        public List<Ferias> ObterListaFerias()
        {
            List<Ferias> LstFerias = new List<Ferias>();
            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM dat_ferias where Ano='" + this.ObterAnoAtivo() + "' order by DataInicio;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstFerias.Add(new Ferias()
                    {
                        Id = result["Id"],
                        IdUtilizador = result["IdUtilizador"],
                        DataInicio = result["DataInicio"],
                        DataFim = result["DataFim"],
                        Validado = result["Validado"],
                        Obs = result["Obs"],
                        Aniversario = result["Aniversario"] == "True",
                        ValidadoPor = result["ValidadoPor"],
                        ValidadoPorNome = result["ValidadoPor"] == 0 ? "" : ObterUtilizador(result["ValidadoPor"]).NomeCompleto,
                    });
                }
            }

            return LstFerias;

        }
        public List<Ferias> ObterListaFerias(DateTime dataInicio, DateTime dataFim)
        {
            List<Ferias> LstFerias = new List<Ferias>();
            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM dat_ferias where DataInicio between '" + dataInicio.ToString("yyyy-MM-dd") + "' AND '" + dataFim.ToString("yyyy-MM-dd") + "' or '" + dataInicio.ToString("yyyy-MM-dd") + "' between DataInicio and DataFim  order by DataInicio;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstFerias.Add(new Ferias()
                    {
                        Id = result["Id"],
                        IdUtilizador = result["IdUtilizador"],
                        DataInicio = result["DataInicio"],
                        DataFim = result["DataFim"],
                        Validado = result["Validado"],
                        Obs = result["Obs"],
                        Aniversario = result["Aniversario"] == "True",
                        ValidadoPor = result["ValidadoPor"],
                        ValidadoPorNome = result["ValidadoPor"] == 0 ? "" : ObterUtilizador(result["ValidadoPor"]).NomeCompleto,
                    });
                }
            }

            return LstFerias;

        }
        public List<Ferias> ObterListaFeriasValidadas()
        {
            List<Ferias> LstFerias = new List<Ferias>();
            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM dat_ferias where Ano='" + this.ObterAnoAtivo() + "' AND Validado=1 order by DataInicio;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstFerias.Add(new Ferias()
                    {
                        Id = result["Id"],
                        IdUtilizador = result["IdUtilizador"],
                        DataInicio = result["DataInicio"],
                        DataFim = result["DataFim"],
                        Validado = result["Validado"],
                        Obs = result["Obs"],
                        Aniversario = result["Aniversario"] == "True",
                        ValidadoPor = result["ValidadoPor"],
                        ValidadoPorNome = result["ValidadoPor"] == 0 ? "" : ObterUtilizador(result["ValidadoPor"]).NomeCompleto,
                    });
                }
            }

            return LstFerias;

        }
        public Ferias ObterFerias(int Id)
        {
            List<Ferias> LstFerias = new List<Ferias>();
            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM dat_ferias where Id='" + Id + "';";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstFerias.Add(new Ferias()
                    {
                        Id = result["Id"],
                        IdUtilizador = result["IdUtilizador"],
                        DataInicio = result["DataInicio"],
                        DataFim = result["DataFim"],
                        Validado = result["Validado"],
                        Obs = result["Obs"],
                        Aniversario = result["Aniversario"] == "True",
                        ValidadoPor = result["ValidadoPor"],
                        ValidadoPorNome = result["ValidadoPor"] == 0 ? "" : ObterUtilizador(result["ValidadoPor"]).NomeCompleto,
                    });
                }
            }

            return LstFerias.Count() > 0 ? LstFerias.FirstOrDefault() : new Ferias();

        }
        public List<Feriado> ObterListaFeriados(string Ano)
        {
            List<Feriado> LstFeriados = new List<Feriado>();
            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM dat_feriados where DataFeriado>='" + Ano + "-01-01' AND DataFeriado<='" + Ano + "-12-31' order by DataFeriado;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstFeriados.Add(new Feriado()
                    {
                        Id = result["Id"],
                        DescFeriado = result["DescFeriado"],
                        DataFeriado = result["DataFeriado"]
                    });
                }
            }

            return LstFeriados;

        }
        public void ObterFeriadosAPI(string ano)
        {
            List<Feriado> LstFeriados = new List<Feriado>();

            try
            {
                using (WebClient wc = new WebClient())
                {
                    var json = wc.DownloadString("https://date.nager.at/api/v3/PublicHolidays/" + ano + "/PT");

                    dynamic dynJson = JsonConvert.DeserializeObject(json);
                    foreach (var item in dynJson)
                    {
                        if (item.global == "True")
                        {
                            Feriado feriado = new Feriado()
                            {
                                DataFeriado = item.date,
                                DescFeriado = item.localName
                            };
                            LstFeriados.Add(feriado);
                        }
                    }
                }
            }
            catch
            {
            }

            CriarFeriados(LstFeriados);
        }
        public List<Ferias> ObterListaFeriasValidar()
        {
            List<Ferias> LstFerias = new List<Ferias>();
            List<Utilizador> LstUtilizadores = this.ObterListaUtilizadores(false, false);
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_ferias where mail_validacao=0 and Validado=0 AND Ano='" + this.ObterAnoAtivo() + "' order by DataInicio;");
                while (result.Read())
                {
                    LstFerias.Add(new Ferias()
                    {
                        Id = result["Id"],
                        IdUtilizador = result["IdUtilizador"],
                        Utilizador = LstUtilizadores.Where(u => u.Id == result["IdUtilizador"]).FirstOrDefault(),
                        DataInicio = result["DataInicio"],
                        DataFim = result["DataFim"],
                        Validado = result["Validado"],
                        Obs = result["Obs"],
                        ValidadoPor = result["ValidadoPor"],
                        ValidadoPorNome = result["ValidadoPor"] == 0 ? "" : ObterUtilizador(result["ValidadoPor"]).NomeCompleto,
                    });
                }
            }

            return LstFerias;

        }
        public void ValidarEmailEnviado()
        {
            string sql = "UPDATE dat_ferias set mail_validacao='1'";

            Database db = ConnectionString;

            db.Execute(sql);
            db.Connection.Close();
        }
        public string ObterFeriasMarcadas(int IdUtilizador, string Ano)
        {

            using (Database db = ConnectionString)
            {

                using var result = db.QueryValue("SELECT COALESCE(SUM(DATEDIFF(DataFim, DataInicio) + 1),0) FROM dat_ferias where Validado=1 AND IdUtilizador ='" + IdUtilizador + "' AND Ano='" + Ano + "';");
                return result;
            }

        }
        public string ObterFeriasDias(int IdUtilizador, string Ano)
        {

            using (Database db = ConnectionString)
            {

                using var result = db.QueryValue("SELECT COALESCE(SUM(DATEDIFF(DataFim, DataInicio) + 1),0) FROM dat_ferias where IdUtilizador ='" + IdUtilizador + "' AND Ano='" + Ano + "';");
                return result;
            }

        }
        public string ObterAnoAtivo()
        {

            using (Database db = ConnectionString)
            {

                using var result = db.QueryValue("SELECT IFNULL( (SELECT Ano from dat_ferias_ano where Active=1 LIMIT 1), '" + DateTime.Now.Year + "') as Ano;");
                return result;
            }

        }
        public List<CalendarioEvent> ConverterFeriasEventos(List<Ferias> Ferias, List<Feriado> Feriados)
        {
            List<CalendarioEvent> LstEventos = new List<CalendarioEvent>();

            foreach (var item in Ferias)
            {
                Utilizador ut = ObterUtilizador(item.IdUtilizador);
                LstEventos.Add(new CalendarioEvent
                {
                    id = item.Id.ToString(),
                    calendarId = "1",
                    body = item.Obs,
                    title = item.Emoji + ut.NomeCompleto,
                    start = item.DataInicio,
                    end = item.DataInicio != item.DataFim ? item.DataFim.AddDays(1) : item.DataInicio.AddDays(1),
                    isAllDay = true,
                    url = "Detalhes/" + item.IdUtilizador,
                    category = "time",
                    dueDateClass = "",
                    editable = false,
                    color = (ut.CorCalendario == string.Empty ? "#3371FF" : ut.CorCalendario),
                });
            }

            foreach (var item in Feriados)
            {

                LstEventos.Add(new CalendarioEvent
                {
                    id = item.Id.ToString(),
                    title = item.Emoji + item.DescFeriado,
                    start = item.DataFeriado,
                    end = item.DataFeriado,
                    category = "time",
                    dueDateClass = "",
                    isAllDay = true,
                    calendarId = "1",
                    bgColor = "#FF5733"
                });
            }

            return LstEventos;
        }
        public byte[] GerarMapaFerias(string Ano)
        {
            using ExcelPackage package = new ExcelPackage(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "FT_Ferias.xlsx"));
            //ExcelWorksheet workSheet = package.Workbook.Worksheets["Table1"];
            ExcelWorksheet workSheet = package.Workbook.Worksheets.First();
            //int totalRows = workSheet.Dimension.Rows;
            List<Utilizador> LstUtilizadores = ObterListaUtilizadores(true, false);
            List<Ferias> LstFerias = ObterListaFerias();
            List<Feriado> LstFeriados = ObterListaFeriados(Ano);
            int count = 0;

            int y = 1;
            int x = 1;

            workSheet.Cells[y, x].Value = Ano;
            workSheet.Cells[y, x].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            workSheet.Cells[y, x].Style.Font.Bold = true;

            //workSheet.Cells[y, 33].Value = "Total";
            //workSheet.Cells[y, 33].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            //workSheet.Cells[y, 33].Style.Font.Bold = true;

            for (int i = 0; i < 12; i++)
            {
                y += 1;
                workSheet.Cells[y, x].Value = new DateTime(int.Parse(Ano), i + 1, 1).ToString("MMMM");
                workSheet.Cells[y, x].Style.Border.BorderAround(ExcelBorderStyle.Medium);
                workSheet.Cells[y, x].Style.Font.Bold = true;

                for (int j = 1; j <= DateTime.DaysInMonth(int.Parse(Ano), i + 1); j++)
                {
                    workSheet.Cells[y, j + 1].Value = j;
                    workSheet.Cells[y, j + 1].Style.Border.BorderAround(ExcelBorderStyle.Medium);
                }

                y += 1;
                foreach (var item in LstUtilizadores)
                {
                    if (LstFerias.Where(f => f.IdUtilizador == item.Id && (f.DataInicio.Month == i + 1 || f.DataFim.Month == i + 1)).Count() > 0)
                    {
                        workSheet.Cells[y, x].Value = item.NomeCompleto;
                        workSheet.Cells[y, x].Style.Border.BorderAround(ExcelBorderStyle.Medium);

                        for (int j = 1; j <= DateTime.DaysInMonth(int.Parse(Ano), i + 1); j++)
                        {
                            DateTime DataAtual = new DateTime(int.Parse(Ano), i + 1, j);
                            List<Ferias> LstFeriasUtilizador = LstFerias.Where(f => f.DataInicio <= DataAtual && f.DataFim >= DataAtual && f.IdUtilizador == item.Id).ToList();
                            if (LstFeriasUtilizador.Count() > 0)
                            {
                                workSheet.Cells[y, j + 1].Value = "X";
                                workSheet.Cells[y, j + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                workSheet.Cells[y, j + 1].Style.Fill.BackgroundColor.SetColor(LstFeriasUtilizador.First().Validado ? Color.LightGreen : Color.LightBlue);
                                count += 1;
                            }

                            if (DataAtual.DayOfWeek == DayOfWeek.Saturday || DataAtual.DayOfWeek == DayOfWeek.Sunday)
                            {
                                workSheet.Cells[y, j + 1].Value = DataAtual.ToString("ddd").Substring(0, 1).ToUpper();
                                workSheet.Cells[y, j + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                workSheet.Cells[y, j + 1].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                            }

                            if (LstFeriados.Where(f => f.DataFeriado == DataAtual).Count() > 0)
                            {
                                workSheet.Cells[y, j + 1].Value = "F";
                                workSheet.Cells[y, j + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                workSheet.Cells[y, j + 1].Style.Fill.BackgroundColor.SetColor(Color.Gray);

                            }
                            workSheet.Cells[y, j + 1].Style.Border.BorderAround(ExcelBorderStyle.Medium);
                        }

                        //workSheet.Cells[y, 33].Value = count;
                        //workSheet.Cells[y, 33].Style.Border.BorderAround(ExcelBorderStyle.Medium);
                        //count = 0;

                        y += 1;
                    }
                }

            }

            y += 1;
            workSheet.Cells[y, x].Value = "Nome";
            workSheet.Cells[y, x].Style.Border.BorderAround(ExcelBorderStyle.Medium);

            x += 1;
            workSheet.Cells[y, x].Value = "DM";
            workSheet.Cells[y, x].Style.Border.BorderAround(ExcelBorderStyle.Medium);

            x += 1;
            workSheet.Cells[y, x].Value = "DD";
            workSheet.Cells[y, x].Style.Border.BorderAround(ExcelBorderStyle.Medium);

            x += 1;
            workSheet.Cells[y, x].Value = "DT";
            workSheet.Cells[y, x].Style.Border.BorderAround(ExcelBorderStyle.Medium);


            x = 1;

            foreach (var item in LstUtilizadores)
            {
                FeriasUtilizador feriasUtilizador = ObterListaFeriasUtilizador(item.Id);

                y += 1;
                workSheet.Cells[y, x].Value = item.NomeCompleto;
                workSheet.Cells[y, x].Style.Border.BorderAround(ExcelBorderStyle.Medium);

                x += 1;
                workSheet.Cells[y, x].Value = feriasUtilizador.DiasMarcados;
                workSheet.Cells[y, x].Style.Border.BorderAround(ExcelBorderStyle.Medium);

                x += 1;
                workSheet.Cells[y, x].Value = feriasUtilizador.DiasDisponiveis - feriasUtilizador.DiasMarcados;
                workSheet.Cells[y, x].Style.Border.BorderAround(ExcelBorderStyle.Medium);
                x += 1;
                workSheet.Cells[y, x].Value = feriasUtilizador.DiasDisponiveis;
                workSheet.Cells[y, x].Style.Border.BorderAround(ExcelBorderStyle.Medium);

                x = 1;
            }
            return package.GetAsByteArray();
        }
        public void CriarFerias(List<Ferias> LstFerias)
        {
            int max = 1000;
            int j = 0;
            string Ano = this.ObterAnoAtivo();
            for (int i = 0; j < LstFerias.Count; i++)
            {
                if ((j + max) > LstFerias.Count) max = (LstFerias.Count - j);

                string sql = "INSERT INTO dat_ferias (Id,IdUtilizador,DataInicio,DataFim,Validado,Obs, Aniversario, ValidadoPor, Ano) VALUES ";

                foreach (var ferias in LstFerias.GetRange(j, max))
                {
                    sql += ("('" + ferias.Id + "', '" + ferias.IdUtilizador + "', '" + ferias.DataInicio.ToString("yy-MM-dd") + "', '" + ferias.DataFim.ToString("yy-MM-dd") + "', '" + (ferias.Validado ? "1" : "0") + "', '" + ferias.Obs + "', '" + ferias.Aniversario + "', " + ferias.ValidadoPor + ", " + Ano + "), \r\n");
                    i++;
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE IdUtilizador=VALUES(IdUtilizador), DataInicio = VALUES(DataInicio), DataFim = VALUES(DataFim), Validado = VALUES(Validado), Obs = VALUES(Obs), ValidadoPor= VALUES(ValidadoPor);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();

                j += max;
                //Console.WriteLine("A ler Marcacao: " + j + " de " + LstMarcacao.Count());
            }
        }
        public void CriarFeriados(List<Feriado> LstFeriados)
        {
            int max = 1000;
            int j = 0;
            for (int i = 0; j < LstFeriados.Count; i++)
            {
                if ((j + max) > LstFeriados.Count) max = (LstFeriados.Count - j);

                string sql = "INSERT INTO dat_feriados (DataFeriado,DescFeriado) VALUES ";

                foreach (var feriado in LstFeriados.GetRange(j, max))
                {
                    sql += ("('" + feriado.DataFeriado.ToString("yy-MM-dd") + "', '" + feriado.DescFeriado + "'), \r\n");
                    i++;
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE DataFeriado = VALUES(DataFeriado), DescFeriado = VALUES(DescFeriado);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();

                j += max;
            }
        }
        public void CriarFeriasUtilizador(int IdUtilizador, string Ano, int DiasDireito)
        {
            Utilizador u = this.ObterUtilizador(IdUtilizador);

            string sqlDelete = "Delete from dat_ferias_utilizador where IdUtilizador = '" + IdUtilizador + "' AND Ano='" + Ano + "';";

            string sqlInsert = "INSERT INTO dat_ferias_utilizador (IdUtilizador,Ano,DiasDireito) VALUES ";
            sqlInsert += ("('" + IdUtilizador + "', '" + Ano + "', '" + DiasDireito + "');");

            DateTime dataAniversario = DateTime.Parse(u.DataNascimento.ToString(Ano + "/MM/dd"));

            while (dataAniversario.DayOfWeek == DayOfWeek.Saturday || dataAniversario.DayOfWeek == DayOfWeek.Sunday)
            {
                dataAniversario = dataAniversario.AddDays(1);
            }

            if (!VerificarFeriasUtilizador(IdUtilizador, dataAniversario)) CriarFerias(new List<Ferias>() { new Ferias() { IdUtilizador = IdUtilizador, DataInicio = dataAniversario, DataFim = dataAniversario, Obs = "Dia de Aniversário", Aniversario = true, Validado = true, ValidadoPorNome = "FT", ValidadoPor = 0 } });

            Database db = ConnectionString;

            db.Execute(sqlDelete);
            db.Execute(sqlInsert);
            db.Connection.Close();
        }
        public void ApagarFerias(int Id)
        {
            string sql = "DELETE FROM dat_ferias where Id=" + Id + ";";

            using Database db = ConnectionString;
            db.Execute(sql);
        }

        #endregion

        //ACESSOS
        #region Acessos
        public List<Acesso> ObterListaAcessos(DateTime Data)
        {
            List<Acesso> LstAcessos = new List<Acesso>();
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_acessos where DataHoraAcesso>'" + Data.ToString("yyyy-MM-dd") + " 00:00:00' AND DataHoraAcesso<'" + Data.ToString("yyyy-MM-dd") + " 23:59:59' order by DataHoraAcesso;");
                while (result.Read())
                {
                    LstAcessos.Add(new Acesso()
                    {
                        Id = result["Id"],
                        Utilizador = ObterUtilizador(result["IdUtilizador"]),
                        Data = result["DataHoraAcesso"],
                        Temperatura = result["Temperatura"],
                        Tipo = result["Tipo"],
                        App = result["App"] == 1
                    });
                }
            }

            return LstAcessos;

        }
        public List<Acesso> ObterListaAcessosMes(DateTime Data)
        {
            List<Acesso> LstAcessos = new List<Acesso>();
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_acessos where DataHoraAcesso>'" + Data.Year + "-" + Data.Month + "-01 00:00:00' AND DataHoraAcesso<'" + Data.Year + "-" + Data.Month + "-31 23:59:59' order by DataHoraAcesso;");
                while (result.Read())
                {
                    LstAcessos.Add(new Acesso()
                    {
                        Id = result["Id"],
                        Utilizador = ObterUtilizador(result["IdUtilizador"]),
                        Data = result["DataHoraAcesso"],
                        Temperatura = result["Temperatura"],
                        Tipo = result["Tipo"],
                        App = result["App"] == 1
                    });
                }
            }

            return LstAcessos;

        }
        public byte[] GerarMapaPresencas(DateTime Data)
        {
            using ExcelPackage package = new ExcelPackage(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "FT_Presencas.xlsx"));
            //ExcelWorksheet workSheet = package.Workbook.Worksheets["Table1"];
            ExcelWorksheet workSheet = package.Workbook.Worksheets.First();
            int totalRows = workSheet.Dimension.Rows;
            List<Utilizador> LstUtilizadores = ObterListaUtilizadores(true, false).Where(u => u.Acessos).ToList();
            List<Acesso> LstAcessos = ObterListaAcessosMes(Data);

            int y = 5;
            int x = 1;

            workSheet.Cells[4, 1].Value = Data.ToString("MMMM yyyy");


            foreach (var utilizador in LstUtilizadores)
            {
                workSheet.Cells[y, x].Value = utilizador.NomeCompleto;
                y += 4;
            }

            if (LstAcessos.Count > 0)
            {
                for (int i = 1; i < LstAcessos.Last().Data.Day + 1; i++)
                {
                    y = 5;

                    foreach (Utilizador utilizador in LstUtilizadores)
                    {
                        int j = y;
                        List<Acesso> Lst = LstAcessos.Where(u => u.Data.Day == i).Where(u => u.Utilizador.Id == utilizador.Id).ToList();
                        DateTime dataAtual = DateTime.Parse(i + "-" + Data.ToString("MM-yyyy"));
                        if (VerificarFeriasUtilizador(utilizador.Id, dataAtual))
                        {
                            workSheet.Cells[j, i + 1].Value = "FÉRIAS";
                            workSheet.Cells[j, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            workSheet.Cells[j, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                        }
                        else
                        if (!(dataAtual.DayOfWeek == DayOfWeek.Saturday || dataAtual.DayOfWeek == DayOfWeek.Sunday))
                        {
                            if (Lst.Count() == 0)
                            {
                                workSheet.Cells[j, i + 1].Value = utilizador.TipoUtilizador == 1 ? "E: 9:00 Externo" : utilizador.TipoUtilizador == 2 ? "E: 9:00 Comercial" : "E: 09:00";
                                workSheet.Cells[j, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                workSheet.Cells[j, i + 1].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                                workSheet.Cells[j + 1, i + 1].Value = utilizador.TipoUtilizador == 1 ? "S: 18:30 Externo" : utilizador.TipoUtilizador == 2 ? "S: 18:30 Comercial" : "S: 18:30 ";
                                workSheet.Cells[j + 1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                workSheet.Cells[j + 1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                            }
                            else
                            {
                                foreach (var acesso in Lst)
                                {
                                    workSheet.Cells[j, i + 1].Value = acesso.TipoAcesso.Substring(0, 1) + ": " + acesso.Data.ToShortTimeString();
                                    j++;
                                }
                            }
                        }
                        y += 4;
                    }
                }
            }

            return package.GetAsByteArray();
        }
        public DateTime ObterDataUltimoAcesso(int IdPHC)
        {

            DateTime res = new DateTime();

            using (Database db = ConnectionString)
            {
                using var resultQuery = db.QueryValue("select DataUltimoAcesso from dat_acessos_utilizador where IdUtilizador=(SELECT IdUtilizador FROM sys_utilizadores WHERE IdPHC = " + IdPHC + " LIMIT 1);");
                res = resultQuery.HasData() ? DateTime.Parse(resultQuery) : new DateTime();
            }

            return res;

        }
        public Acesso ObterUltimoAcesso(int Id)
        {
            Acesso a = new Acesso();

            using Database db = ConnectionString;
            using var result = db.Query("select * from dat_acessos_utilizador where IdUtilizador=" + Id + ";");
            while (result.Read())
            {
                a = new Acesso()
                {

                    IdUtilizador = result["IdUtilizador"],
                    Data = result["DataUltimoAcesso"],
                    Tipo = result["TipoUltimoAcesso"],
                    App = result["App"] == "1"
                };
            }
            return a;
        }

        public void CriarAcesso(List<Acesso> LstAcessos)
        {
            if (LstAcessos.Count > 0)
            {
                string sql1 = "INSERT INTO dat_acessos (IdUtilizador,DataHoraAcesso,Tipo, Temperatura, App) VALUES ";
                string sql2 = "INSERT INTO dat_acessos_utilizador (IdUtilizador, DataUltimoAcesso, TipoUltimoAcesso, App) VALUES";

                foreach (Acesso acesso in LstAcessos.OrderBy(a => a.Data))
                {
                    sql1 += "((SELECT IdUtilizador FROM sys_utilizadores WHERE IdPHC = " + acesso.IdUtilizador + "), '" + acesso.Data.ToString("yyyy-MM-dd HH:mm:ss") + "', " + acesso.Tipo + ", '" + acesso.Temperatura + "', 1),\r\n";

                    if (acesso.Data > ObterDataUltimoAcesso(acesso.IdUtilizador)) sql2 += "((SELECT IdUtilizador FROM sys_utilizadores WHERE IdPHC = " + acesso.IdUtilizador + "), '" + acesso.Data.ToString("yyyy-MM-dd HH:mm:ss") + "', " + acesso.Tipo + ", 1),\r\n";
                }

                sql1 = sql1.Remove(sql1.Count() - 3);
                sql1 += ";";
                sql2 = sql2.Remove(sql2.Count() - 3);
                sql2 += " ON DUPLICATE KEY UPDATE DataUltimoAcesso = VALUES(DataUltimoAcesso), TipoUltimoAcesso = VALUES(TipoUltimoAcesso);";

                try
                {
                    Database db = ConnectionString;

                    db.Execute(sql1);
                    db.Execute(sql2);
                    db.Connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        public void CriarAcessoInterno(List<Acesso> LstAcessos)
        {
            if (LstAcessos.Count > 0)
            {
                string sql1 = "INSERT INTO dat_acessos (IdUtilizador,DataHoraAcesso,Tipo, Temperatura, App) VALUES ";
                string sql2 = "INSERT INTO dat_acessos_utilizador (IdUtilizador, DataUltimoAcesso, TipoUltimoAcesso, App, timestamp) VALUES";

                foreach (Acesso acesso in LstAcessos.OrderBy(a => a.Data))
                {
                    sql1 += "(" + acesso.IdUtilizador + ", '" + acesso.Data.ToString("yyyy-MM-dd HH:mm:ss") + "', " + acesso.Tipo + ", '" + acesso.Temperatura + "', 0),\r\n";

                    if (acesso.Data > ObterDataUltimoAcesso(acesso.IdUtilizador)) sql2 += "(" + acesso.IdUtilizador + ", '" + acesso.Data.ToString("yyyy-MM-dd HH:mm:ss") + "', " + acesso.Tipo + ", 0, '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'),\r\n";
                }

                sql1 = sql1.Remove(sql1.Count() - 3);
                sql1 += ";";
                sql2 = sql2.Remove(sql2.Count() - 3);
                sql2 += " ON DUPLICATE KEY UPDATE DataUltimoAcesso = VALUES(DataUltimoAcesso), TipoUltimoAcesso = VALUES(TipoUltimoAcesso);";

                try
                {
                    Database db = ConnectionString;

                    db.Execute(sql1);
                    db.Execute(sql2);
                    db.Connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        public void ApagarAcesso(int Id)
        {
            string sql = "DELETE FROM dat_acessos where Id=" + Id + ";";

            using Database db = ConnectionString;
            db.Execute(sql);
        }

        #endregion

        //Notificacoes
        #region Notificacoes
        public List<Notificacao> ObterNotificacoesPendentes()
        {
            List<Notificacao> LstNotificacoes = new List<Notificacao>();
            using Database db = ConnectionString;
            using var result = db.Query("SELECT * FROM dat_notificacoes where Pendente=0;");
            while (result.Read())
            {
                LstNotificacoes.Add(new Notificacao()
                {
                    ID = result["Id"],
                    Mensagem = result["Mensagem"],
                    Destino = result["Destino"],
                    Utilizador = ObterUtilizador(result["IdUtilizador"]),
                    Tipo = result["Tipo"],
                    Pendente = result["Pendente"] == 1
                });
            }

            return LstNotificacoes;
        }
        public void CriarNotificacao(Notificacao notificacao)
        {

            string sql = "INSERT INTO dat_notificacoes (Mensagem,Destino,IdUtilizador, Tipo, Pendente) VALUES ('" + notificacao.Mensagem + "', '" + notificacao.Destino + "', '" + notificacao.Utilizador.Id + "', '" + notificacao.Tipo + "', '" + notificacao.Pendente + "', )";

            try
            {
                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();
            }
            catch
            {
            }
        }
        #endregion

        //CONTACTOS
        #region Contactos
        public Contacto ObterContacto(int id)
        {
            PHCContext phccontext = new PHCContext(ConfigurationManager.AppSetting["ConnectionStrings:PHCConnection"], ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"]);

            Contacto contacto = new Contacto();
            using Database db = ConnectionString;
            using var result = db.Query("SELECT * FROM dat_contactos where Id=" + id + ";");
            result.Read();
            if (result.Reader.HasRows)
            {
                contacto = new Contacto()
                {
                    IdContacto = result["Id"],
                    NomeContacto = result["Nome"],
                    MoradaContacto = result["Morada"],
                    PessoaContacto = result["PessoaContacto"],
                    EmailContacto = result["Email"],
                    TelefoneContacto = result["Telefone"],
                    NIFContacto = result["NIF"],
                    Obs = result["Obs"],
                    TipoContacto = result["TipoContacto"],
                    URL = result["URL"],
                    DataContacto = result["DataContacto"],
                    AreaNegocio = result["AreaNegocio"],
                    CargoPessoaContacto = result["CargoPessoaContacto"],
                    ValidadoPorAdmin = result["ValidadoPorAdmin"],
                    Cliente = phccontext.ObterCliente(int.Parse(result["IdCliente"]), int.Parse(result["IdLoja"])),
                    IdCliente = result["IdCliente"],
                    IdLoja = result["IdLoja"],
                    IdComercial = result["IdComercial"],
                    Comercial = ObterUtilizador(result["IdComercial"]),
                    IdUtilizador = result["IdUtilizador"],
                    Utilizador = ObterUtilizador(result["IdUtilizador"]),
                    Historico = ObterHistoricoContactos(id),
                    ContactosAdicionais = ObterContactosAdicionais(id)
                };
            }

            return contacto;
        }
        public Cliente ObterClienteContacto(int id)
        {

            Cliente cliente = new Cliente();
            using Database db = ConnectionString;
            using var result = db.Query("SELECT * FROM dat_contactos where Id=" + id + ";");
            result.Read();
            if (result.Reader.HasRows)
            {
                cliente = new Cliente()
                {
                    NomeCliente = result["Nome"],
                    MoradaCliente = result["Morada"],
                    PessoaContatoCliente = result["PessoaContacto"],
                    EmailCliente = result["Email"],
                    TelefoneCliente = result["Telefone"],
                    NumeroContribuinteCliente = result["NIF"],
                };
            }

            return cliente;
        }
        public List<String> ObterListaAreasNegocio()
        {

            List<String> LstAreasNegocio = new List<String>();
            string sqlQuery = "SELECT * FROM sys_areas_negocio;";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstAreasNegocio.Add(result["Nome"]);
                }
            }
            return LstAreasNegocio;
        }
        public List<Contacto> ObterListaContactos()
        {

            List<Contacto> LstContacto = new List<Contacto>();
            string sqlQuery = "SELECT * FROM dat_contactos;";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstContacto.Add(new Contacto()
                    {
                        IdContacto = result["Id"],
                        NomeContacto = result["Nome"],
                        MoradaContacto = result["Morada"],
                        PessoaContacto = result["PessoaContacto"],
                        EmailContacto = result["Email"],
                        TelefoneContacto = result["Telefone"],
                        NIFContacto = result["NIF"],
                        Obs = result["Obs"],
                        TipoContacto = result["TipoContacto"],
                        URL = result["URL"],
                        DataContacto = result["DataContacto"],
                        AreaNegocio = result["AreaNegocio"],
                        IdCliente = result["IdCliente"],
                        IdLoja = result["IdLoja"],
                        IdComercial = result["IdComercial"],
                        Comercial = ObterUtilizador(result["IdComercial"]),
                        IdUtilizador = result["IdUtilizador"],
                        Utilizador = ObterUtilizador(result["IdUtilizador"])
                    });
                }
            }
            return LstContacto;
        }
        public List<HistoricoContacto> ObterHistoricoContactos(int IdContacto)
        {

            List<HistoricoContacto> LstHistorico = new List<HistoricoContacto>();
            string sqlQuery = "SELECT * FROM dat_contactos_historico WHERE IdContacto=" + IdContacto + ";";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstHistorico.Add(new HistoricoContacto()
                    {
                        Id = result["Id"],
                        IdContacto = result["IdContacto"],
                        IdComercial = ObterUtilizador(int.Parse(result["IdComercial"])),
                        Data = result["Data"],
                        Obs = result["Obs"]
                    });
                    if (LstHistorico.Last().IdComercial.Id == 0) LstHistorico.Last().IdComercial.NomeCompleto = "Não Definido";
                }
            }
            return LstHistorico;
        }
        public List<ContactosAdicionais> ObterContactosAdicionais(int IdContacto)
        {

            List<ContactosAdicionais> LstContactosAdicionais = new List<ContactosAdicionais>();
            string sqlQuery = "SELECT * FROM dat_contactos_adicionais WHERE IdContacto=" + IdContacto + ";";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstContactosAdicionais.Add(new ContactosAdicionais()
                    {
                        Id = result["IdContactoAdicional"],
                        IdContacto = result["IdContacto"],
                        PessoaContacto = result["PessoaContacto"],
                        CargoPessoaContacto = result["Cargo"],
                        TelefoneContacto = result["Telemovel"],
                        EmailContacto = result["Email"]
                    });

                }
            }
            return LstContactosAdicionais;
        }
        public ContactosAdicionais ObterContactoAdicional(int Id)
        {

            List<ContactosAdicionais> LstContactosAdicionais = new List<ContactosAdicionais>();
            string sqlQuery = "SELECT * FROM dat_contactos_adicionais WHERE IdContactoAdicional=" + Id + ";";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstContactosAdicionais.Add(new ContactosAdicionais()
                    {
                        Id = result["IdContactoAdicional"],
                        IdContacto = result["IdContacto"],
                        PessoaContacto = result["PessoaContacto"],
                        CargoPessoaContacto = result["Cargo"],
                        TelefoneContacto = result["Telemovel"],
                        EmailContacto = result["Email"]
                    });

                }
            }
            return LstContactosAdicionais.First();
        }
        public List<Contacto> ObterListaContactos(string Filtro)
        {
            DateTime dt = new DateTime();
            DateTime.TryParse(Filtro, out dt);

            List<Utilizador> LstUtilizadores = ObterListaUtilizadores(false, false);

            List<Contacto> LstContacto = new List<Contacto>();
            string sqlQuery = "SELECT * FROM dat_contactos where Nome like '%" + Filtro + "%' or Morada like '%" + Filtro + "%' or PessoaContacto like '%" + Filtro + "%' or Email like '%" + Filtro + "%' or TipoContacto like '%" + Filtro + "%' or DataContacto like '%" + dt.ToString("yyyy-MM-dd") + "%';";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstContacto.Add(new Contacto()
                    {
                        IdContacto = result["Id"],
                        NomeContacto = result["Nome"],
                        MoradaContacto = result["Morada"],
                        PessoaContacto = result["PessoaContacto"],
                        //CargoPessoaContacto = result["CargoPessoaContacto"],
                        EmailContacto = result["Email"],
                        TelefoneContacto = result["Telefone"],
                        //NIFContacto = result["NIF"],
                        //Obs = result["Obs"],
                        //TipoContacto = result["TipoContacto"],
                        URL = result["URL"],
                        //DataContacto = result["DataContacto"],
                        AreaNegocio = result["AreaNegocio"],
                        ValidadoPorAdmin = result["ValidadoPorAdmin"],
                        //IdCliente = result["IdCliente"],
                        //IdLoja = result["IdLoja"],
                        //IdComercial = result["IdComercial"],
                        Comercial = LstUtilizadores.Where(u => u.Id == result["IdComercial"]).First(),
                        //IdUtilizador = result["IdUtilizador"],
                        Utilizador = LstUtilizadores.Where(u => u.Id == result["IdUtilizador"]).First()
                    });
                }
            }
            return LstContacto;
        }
        public bool ExisteNIFDuplicadoContacto(string NIF)
        {
            if (String.IsNullOrEmpty(NIF)) return false;
            int res = 0;
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT Count(*) as Count FROM dat_contactos where NIF = '" + NIF + "';");
                result.Read();

                res = result["Count"];
            }

            return res > 0;
        }
        public void CriarHistoricoContacto(HistoricoContacto historicoContacto)
        {
            string sql = "INSERT INTO dat_contactos_historico (Id, IdContacto, IdComercial, Data, Obs) VALUES (" + historicoContacto.Id + ", " + historicoContacto.IdContacto + ",  " + historicoContacto.IdComercial.Id + ", '" + historicoContacto.Data.ToString("yyyy-MM-dd") + "', '" + historicoContacto.Obs + "');";

            using Database db = ConnectionString;
            db.Execute(sql);
        }

        public void CriarContactoAdicional(ContactosAdicionais c)
        {
            string sql = "INSERT INTO dat_contactos_adicionais (IdContacto, PessoaContacto, Cargo, Telemovel, Email, CriadoPor) VALUES (" + c.IdContacto + ", '" + c.PessoaContacto + "', '" + c.CargoPessoaContacto + "', '" + c.TelefoneContacto + "', '" + c.EmailContacto + "', '" + c.CriadoPor + "');";

            using Database db = ConnectionString;
            db.Execute(sql);
        }
        public void CriarContactos(List<Contacto> LstContacto)
        {
            if (LstContacto.Count() > 0)
            {

                string sql = "INSERT INTO dat_contactos (Id, Nome, Morada, PessoaContacto, CargoPessoaContacto, Email, Telefone, NIF, Obs, URL, DataContacto, TipoContacto, AreaNegocio, ValidadoPorAdmin, IdCliente, IdLoja, IdUtilizador, IdComercial) VALUES ";

                foreach (var contacto in LstContacto)
                {
                    sql += ("('" + contacto.IdContacto + "', '" + contacto.NomeContacto.Replace("'", "''") + "', '" + contacto.MoradaContacto.Replace("'", "''") + "', '" + contacto.PessoaContacto.Replace("'", "''") + "', '" + contacto.CargoPessoaContacto.Replace("'", "''") + "', '" + contacto.EmailContacto.Replace("'", "''") + "', '" + contacto.TelefoneContacto.Replace("'", "''") + "', '" + contacto.NIFContacto.Replace("'", "''") + "', '" + contacto.Obs.Replace("'", "''") + "', '" + contacto.URL + "', '" + contacto.DataContacto.ToString("yy-MM-dd") + "', '" + contacto.TipoContacto + "', '" + contacto.AreaNegocio + "', '" + (contacto.ValidadoPorAdmin ? 1 : 0) + "', '" + contacto.IdCliente + "', '" + contacto.IdLoja + "', '" + contacto.IdUtilizador + "', '" + contacto.IdComercial + "'), \r\n");
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE Nome = VALUES(Nome), Morada = VALUES(Morada), IdCliente = VALUES(IdCliente), IdLoja = VALUES(IdLoja), PessoaContacto = VALUES(PessoaContacto), CargoPessoaContacto = VALUES(CargoPessoaContacto), Email = VALUES(Email), Telefone = VALUES(Telefone), NIF = VALUES(NIF), Obs = VALUES(Obs), URL = VALUES(URL), TipoContacto = VALUES(TipoContacto), ValidadoPorAdmin = VALUES(ValidadoPorAdmin), AreaNegocio = VALUES(AreaNegocio), IdComercial = VALUES(IdComercial);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();
            }
        }
        public void ApagarContacto(int Id)
        {
            string sql = "DELETE FROM dat_contactos where Id=" + Id + ";";

            using Database db = ConnectionString;
            db.Execute(sql);

            ApagarHistoricoContacto(Id);
            ApagarContactosAdicionais(Id);

        }
        public void ApagarHistoricoContacto(int Id)
        {
            string sql = "DELETE FROM dat_contactos_historico where IdContacto=" + Id + ";";

            using Database db = ConnectionString;
            db.Execute(sql);
        }
        public void ApagarContactosAdicionais(int IdContacto)
        {
            string sql = "DELETE FROM dat_contactos_adicionais where IdContacto=" + IdContacto + ";";

            using Database db = ConnectionString;
            db.Execute(sql);
        }
        public void ApagarContactoAdicional(int Id)
        {
            string sql = "DELETE FROM dat_contactos_adicionais where IdContactoAdicional=" + Id + ";";

            using Database db = ConnectionString;
            db.Execute(sql);
        }
        #endregion

        //OUTROS
        static string GetRandomString(int length)
        {
            string s = "";
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

            using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            {
                while (s.Length != length)
                {
                    byte[] oneByte = new byte[1];
                    provider.GetBytes(oneByte);
                    char character = (char)oneByte[0];
                    if (valid.Contains(character))
                    {
                        s += character;
                    }
                }
            }
            return s;
        }

        public List<CalendarioEvent> ConverterMarcacoesEventos(List<Marcacao> Marcacoes)
        {
            List<CalendarioEvent> LstEventos = new List<CalendarioEvent>();

            DateTime dataMarcacao = DateTime.Parse(DateTime.Now.ToShortDateString() + " 00:00:00");
            dataMarcacao.AddMinutes(5);
            foreach (var item in Marcacoes.OrderBy(m => m.IdTecnico).OrderBy(m => m.DataMarcacao))
            {
                try
                {
                    if (item.Tecnico != null)
                    {
                        if (LstEventos.Count > 0 && LstEventos.Last().IdTecnico != item.Tecnico.Id) dataMarcacao = dataMarcacao.AddMinutes(10);
                        if (dataMarcacao.ToShortDateString() != item.DataMarcacao.ToShortDateString()) dataMarcacao = DateTime.Parse(item.DataMarcacao.ToShortDateString() + " 00:00:00");

                        LstEventos.Add(new CalendarioEvent
                        {
                            id = item.IdMarcacao.ToString() + "_" + dataMarcacao.ToString("yyyyMMdd"),
                            IdMarcacao = item.IdMarcacao,
                            calendarId = "1",
                            title = item.EmojiEstado + item.Cliente.NomeCliente,
                            start = dataMarcacao,
                            end = dataMarcacao.AddMinutes(25),
                            IdTecnico = item.Tecnico.Id,
                            category = "time",
                            editable = true,
                            dueDateClass = dataMarcacao.ToShortDateString(),
                            color = (item.Tecnico.CorCalendario == string.Empty ? "#3371FF" : item.Tecnico.CorCalendario),
                            url = "Pedido/" + item.IdMarcacao
                        });
                        dataMarcacao = dataMarcacao.AddMinutes(30);
                    }
                }
                catch
                {

                }

            }
            return LstEventos;
        }
        public Bitmap DesenharEtiqueta80x50QR(Produto produto)
        {

            int x = 0;
            int y = 0;
            int width = 1024;
            int height = 641;

            Bitmap bm = new Bitmap(width, height);

            Font fontHeader = new Font("Rubik", 70, FontStyle.Bold);
            Font fontBody = new Font("Tahoma", 40, FontStyle.Regular);
            Font fontFooter = new Font("Rubik", 22, FontStyle.Regular);
            Font fontBold = new Font("Rubik", 28, FontStyle.Bold);

            StringFormat format = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };



            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.White);

                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                x = 50;
                if (File.Exists(FT_Logo_Print)) { Image img = System.Drawing.Image.FromFile(FT_Logo_Print, true); gr.DrawImage(img, x, y, 280, 165); }

                y += 30;
                gr.DrawString("Food-Tech", fontHeader, Brushes.Black, x + 400, y);

                x = 10;
                y += 165;

                gr.DrawString(produto.Designacao_Produto, fontBody, Brushes.Black, new Rectangle(x, y, width - (x * 2), 200), format);

                y += 250;
                gr.DrawString(produto.Ref_Produto, fontHeader, new SolidBrush(Color.Black), new RectangleF(x, y, width - (x * 2) - 200, 80), format);

                y += 95;

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(produto.Ref_Produto, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                gr.DrawImage(qrCodeImage, width - 220, height - 220, 200, 200);

                x += 40;
                gr.DrawString("geral@food-tech.pt", fontFooter, Brushes.Black, new Rectangle(x, y, width - (x * 2) - 200, 35), format);

                if (produto.Pos_Stock.Trim().Length > 0)
                {
                    gr.DrawString(produto.Pos_Stock, fontBold, Brushes.Black, new Rectangle(width - 185, height - 235, 150, 35), format);
                    gr.DrawRectangle(new Pen(Color.Black, 5), new Rectangle(width - 190, height - 240, 140, 40));
                }
            }

            return bm;
        }
        public Bitmap DesenharEtiqueta40x25QR(Produto produto)
        {

            int x = 0;
            int y = 0;
            int width = 1024;
            int height = 641;

            Bitmap bm = new Bitmap(width, height);

            Font fontHeader = new Font("Rubik", 30, FontStyle.Bold);
            Font fontBody = new Font("Tahoma", 40, FontStyle.Regular);
            Font fontFooter = new Font("Rubik", 16, FontStyle.Regular);
            Font fontBold = new Font("Rubik", 28, FontStyle.Bold);

            StringFormat format = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.White);

                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                for (int i = 0; i < 4; i++)
                {
                    y += 50;
                    for (int j = 0; j < 4; j++)
                    {
                        //x +=20;
                        //if (File.Exists(FT_Logo_Print)) { Image img = System.Drawing.Image.FromFile(FT_Logo_Print, true); gr.DrawImage(img, x, y, 84, 50); }

                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(produto.Ref_Produto, QRCodeGenerator.ECCLevel.Q);
                        QRCode qrCode = new QRCode(qrCodeData);
                        Bitmap qrCodeImage = qrCode.GetGraphic(20);

                        gr.DrawImage(qrCodeImage, x, y, 100, 100);

                        x += 100;
                        gr.DrawString(produto.Ref_Produto, fontHeader, new SolidBrush(Color.Black), new RectangleF(x, y, width / 2 - 100, 50), format);

                        gr.DrawString("geral@food-tech.pt", fontFooter, Brushes.Black, new Rectangle(x, y + 60, width / 2 - 100, 35), format);


                        x = width / 2;

                    }
                    x = 0;
                    y += 100;
                }
            }

            return bm;
        }
        public Bitmap DesenharEtiqueta80x25QR(Produto produto)
        {

            int x = 10;
            int y = 0;
            int width = 1024;
            int height = 320;

            Bitmap bm = new Bitmap(width, 641);

            Font fontHeader = new Font("Rubik", 40, FontStyle.Bold);
            Font fontBody = new Font("Tahoma", 34, FontStyle.Bold);
            Font fontFooter = new Font("Rubik", 16, FontStyle.Regular);

            StringFormat format = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };

            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.White);

                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (File.Exists(FT_Logo_Print)) { Image img = System.Drawing.Image.FromFile(FT_Logo_Print, true); gr.DrawImage(img, x + 10, height - 130, 200, 120); }


                y += 10;

                gr.DrawString(produto.Designacao_Produto, fontBody, Brushes.Black, new Rectangle(x, y, width - (x * 2), 200), format);

                y += 200;
                gr.DrawString(produto.Ref_Produto, fontHeader, new SolidBrush(Color.Black), new RectangleF(x + 220, y, width - (x * 2) - 420, 80), format);

                y += 70;

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(produto.Ref_Produto, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                gr.DrawImage(qrCodeImage, width - 210, height - 150, 150, 150);

                if (produto.Pos_Stock.Length > 0)
                {
                    gr.DrawString(produto.Pos_Stock, fontFooter, Brushes.Black, new Rectangle(width - 190, height - 160, 110, 20), format);
                    gr.DrawRectangle(new Pen(Color.Black, 5), new Rectangle(width - 190, height - 165, 110, 30));

                }

                gr.DrawString("geral@food-tech.pt", fontFooter, Brushes.Black, new Rectangle(x, y, width - (x * 2), 30), format);

            }

            return bm;
        }
        public Bitmap DesenharEtiquetaMarcacao(Marcacao marcacao)
        {

            int x = 0;
            int y = 0;
            int width = 1024;
            int height = 641;

            Bitmap bm = new Bitmap(width, height);

            Font fontHeader = new Font("Rubik", 70, FontStyle.Bold);
            Font fontBody = new Font("Tahoma", 40, FontStyle.Regular);
            Font fontFooter = new Font("Rubik", 22, FontStyle.Regular);

            StringFormat format = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };

            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.White);

                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (File.Exists(FT_Logo_Print)) { Image img = System.Drawing.Image.FromFile(FT_Logo_Print, true); gr.DrawImage(img, x, y, 400, 235); }

                y += 65;
                gr.DrawString("Food-Tech", fontHeader, Brushes.Black, x + 400, y);

                x = 10;
                y += 165;

                gr.DrawString(marcacao.Cliente.NomeCliente + "\r\n(" + marcacao.EstadoMarcacaoDesc + ")", fontBody, Brushes.Black, new Rectangle(x, y, width - (x * 2), 200), format);

                y += 250;
                gr.DrawString(marcacao.IdMarcacao.ToString(), fontHeader, new SolidBrush(Color.Black), new RectangleF(x, y, width - (x * 2) - 200, 80), format);

                y += 95;

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(marcacao.GetUrl.ToString(), QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                gr.DrawImage(qrCodeImage, width - 220, height - 220, 200, 200);

                gr.DrawString("geral@food-tech.pt", fontFooter, Brushes.Black, new Rectangle(x, y, width - (x * 2) - 200, 30), format);

            }

            return bm;
        }
        public Bitmap DesenharEtiquetaPicking(Picking p)
        {

            int x = 0;
            int y = 0;
            int width = 1024;
            int height = 641;

            Bitmap bm = new Bitmap(width, height);

            Font fontHeader = new Font("Rubik", 70, FontStyle.Bold);
            Font fontBody = new Font("Tahoma", 30, FontStyle.Bold);
            Font fontFooter = new Font("Rubik", 22, FontStyle.Regular);

            StringFormat format = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };

            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.White);

                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (File.Exists(FT_Logo_Print)) { Image img = System.Drawing.Image.FromFile(FT_Logo_Print, true); gr.DrawImage(img, x, y, 400, 235); }

                y += 65;
                gr.DrawString("Food-Tech", fontHeader, Brushes.Black, x + 400, y);

                x = 10;
                y += 165;

                gr.DrawString("Cliente: " + p.Encomenda.NomeCliente + "\r\n\r\nEncomenda: " + p.Encomenda.Id + "\r\nData: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), fontBody, Brushes.Black, new Rectangle(x, y, width - (x * 2), 200), format);

                y += 350;

                gr.DrawString("geral@food-tech.pt", fontFooter, Brushes.Black, new Rectangle(x, y, width - (x * 2) - 200, 30), format);

            }

            return bm;
        }
        public Bitmap DesenharFolhaObraSimples(FolhaObra fo)
        {

            int x = 0;
            int y = 0;
            int width = 1024;
            int height = (840 + fo.PecasServico.Where(p => !p.Ref_Produto.Contains("SRV")).Count() * 140) * 2;

            Bitmap bm = new Bitmap(width, height);

            Font fontHeader = new Font("Rubik", 70, FontStyle.Bold);
            Font fontBody = new Font("Rubik", 22, FontStyle.Bold);
            Font fontFooter = new Font("Rubik", 22, FontStyle.Regular);

            string HeaderPagina = "Original";

            StringFormat format = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };
            StringFormat formatRight = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Far
            };
            StringFormat formatLeft = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Near
            };

            Pen penB = new Pen(Color.Black, 5);
            Pen pen = new Pen(Color.Transparent, 5);

            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.White);
                Rectangle rect = new Rectangle();

                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                for (int i = 0; i <= 1; i++)
                {
                    if (File.Exists(FT_Logo_Print)) { Image img = System.Drawing.Image.FromFile(FT_Logo_Print, true); gr.DrawImage(img, x, y, 400, 235); }

                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(fo.GetUrl, QRCodeGenerator.ECCLevel.Q);
                    QRCode qrCode = new QRCode(qrCodeData);
                    Bitmap qrCodeImage = qrCode.GetGraphic(20);

                    gr.DrawImage(qrCodeImage, width - 200, y + 20, 200, 200);

                    rect = new Rectangle(x + 410, y + 20, width - 620, 40);
                    gr.DrawString(HeaderPagina, fontBody, Brushes.Black, rect, formatRight);
                    gr.DrawRectangle(pen, rect);

                    y += 40;
                    rect = new Rectangle(x + 410, y + 20, width - 620, 40);
                    gr.DrawString("Nº PAT: " + fo.IdFolhaObra, fontFooter, Brushes.Black, rect, formatRight);
                    gr.DrawRectangle(pen, rect);

                    y += 40;
                    rect = new Rectangle(x + 410, y + 20, width - 620, 40);
                    gr.DrawString("Nº Assis. Técnica: " + fo.IdAT, fontFooter, Brushes.Black, rect, formatRight);
                    gr.DrawRectangle(pen, rect);

                    y += 40;
                    rect = new Rectangle(x + 410, y + 20, width - 620, 40);
                    gr.DrawString("Nº Marcação: " + fo.IdMarcacao, fontFooter, Brushes.Black, rect, formatRight);
                    gr.DrawRectangle(pen, rect);

                    y += 40;
                    rect = new Rectangle(x + 410, y + 20, width - 620, 40);
                    gr.DrawString("Data: " + fo.DataServico.ToShortDateString(), fontFooter, Brushes.Black, rect, formatRight);
                    gr.DrawRectangle(pen, rect);

                    y += 60;
                    rect = new Rectangle(x + 10, y + 20, 400, 240);
                    gr.DrawRectangle(penB, rect);

                    rect = new Rectangle(x + 410, y + 20, width - 420, 240);
                    gr.DrawRectangle(penB, rect);

                    rect = new Rectangle(x + 10, y + 20, 400, 40);
                    gr.DrawString("Subic, Lda", fontFooter, Brushes.Black, rect, format);
                    gr.DrawRectangle(pen, rect);

                    rect = new Rectangle(x + 410, y + 20, width - 420, 40);
                    gr.DrawString("Dados do Cliente:", fontFooter, Brushes.Black, rect, formatLeft);
                    gr.DrawString("Estab: " + fo.ClienteServico.IdCliente + " Loja: " + fo.ClienteServico.IdLoja, fontFooter, Brushes.Black, rect, formatRight);
                    gr.DrawRectangle(pen, rect);

                    y += 40;
                    rect = new Rectangle(x + 10, y + 20, 400, 40);
                    gr.DrawString("NIF: 515 609 013", fontFooter, Brushes.Black, rect, format);
                    gr.DrawRectangle(pen, rect);

                    rect = new Rectangle(x + 410, y + 20, width - 420, 80);
                    gr.DrawString("Nome: " + fo.ClienteServico.NomeCliente, fontFooter, Brushes.Black, rect, formatLeft);
                    gr.DrawRectangle(pen, rect);

                    y += 40;

                    rect = new Rectangle(x + 10, y + 20, 400, 160);
                    gr.DrawString("Morada: Rua Engenheiro Sabino Marques, 144, 4470-605 Maia", fontFooter, Brushes.Black, rect, format);
                    gr.DrawRectangle(pen, rect);

                    y += 40;
                    rect = new Rectangle(x + 410, y + 20, width - 420, 80);
                    gr.DrawString("Morada: " + fo.ClienteServico.MoradaCliente, fontFooter, Brushes.Black, rect, formatLeft);
                    gr.DrawRectangle(pen, rect);

                    y += 80;
                    rect = new Rectangle(x + 410, y + 20, width - 420, 40);
                    gr.DrawString("NIF: " + fo.ClienteServico.NumeroContribuinteCliente, fontFooter, Brushes.Black, rect, formatLeft);
                    gr.DrawRectangle(pen, rect);

                    y += 60;

                    rect = new Rectangle(x + 10, y + 20, width - 20, 80);
                    gr.DrawRectangle(penB, rect);

                    rect = new Rectangle(x + 10, y + 20, width - 20, 40);
                    gr.DrawString("Equipamento", fontFooter, Brushes.Black, rect, format);
                    gr.DrawRectangle(pen, rect);

                    y += 40;
                    rect = new Rectangle(x + 10, y + 20, width - 20, 40);
                    gr.DrawString("Marca: " + fo.EquipamentoServico.MarcaEquipamento, fontFooter, Brushes.Black, rect, formatLeft);
                    gr.DrawString("Modelo: " + fo.EquipamentoServico.ModeloEquipamento, fontFooter, Brushes.Black, rect, format);
                    gr.DrawString("N/S: " + fo.EquipamentoServico.NumeroSerieEquipamento, fontFooter, Brushes.Black, rect, formatRight);
                    gr.DrawRectangle(pen, rect);

                    y += 60;
                    rect = new Rectangle(x + 10, y + 20, width - 20, 60);
                    if (fo.PecasServico.Where(p => !p.Ref_Produto.Contains("SRV")).Count() > 0)
                    {
                        gr.DrawString("Peças retiradas da: " + fo.GuiaTransporteAtual, fontBody, Brushes.Black, rect, format);
                    }
                    else
                    {
                        gr.DrawString("Não foram retiradas nenhumas peças nesta assistência!", fontBody, Brushes.Black, rect, format);
                    }

                    y += 20;
                    gr.DrawRectangle(penB, rect);

                    foreach (var peca in fo.PecasServico.Where(p => !p.Ref_Produto.Contains("SRV")))
                    {
                        y += 60;
                        rect = new Rectangle(x + 10, y + 20, width - 20, 120);
                        gr.DrawRectangle(penB, rect);

                        rect = new Rectangle(x + 10, y + 20, width - 20, 40);
                        gr.DrawString("Referência: " + peca.Ref_Produto, fontFooter, Brushes.Black, rect, formatLeft);
                        gr.DrawRectangle(pen, rect);
                        y += 40;
                        rect = new Rectangle(x + 10, y + 20, width - 20, 40);
                        gr.DrawString("Designação: " + peca.Designacao_Produto, fontFooter, Brushes.Black, rect, formatLeft);
                        gr.DrawRectangle(pen, rect);
                        y += 40;
                        rect = new Rectangle(x + 10, y + 20, width - 20, 40);
                        gr.DrawString("Qtd: " + peca.Stock_Fisico + " " + peca.TipoUn, fontFooter, Brushes.Black, rect, formatLeft);
                        gr.DrawRectangle(pen, rect);
                    }

                    y += 60;
                    rect = new Rectangle(x + 10, y + 20, width - 20, 40);
                    gr.DrawString("Cliente: " + fo.ConferidoPor, fontFooter, Brushes.Black, rect, formatLeft);
                    gr.DrawString("Técnico: " + fo.IntervencaosServico.First().NomeTecnico, fontFooter, Brushes.Black, rect, formatRight);
                    gr.DrawRectangle(penB, rect);

                    y += 150;
                    penB.DashStyle = DashStyle.Dash;
                    gr.DrawLine(penB, new Point(0, y), new Point(width, y));
                    penB.DashStyle = DashStyle.Solid;

                    y += 80;
                    HeaderPagina = "Duplicado";
                }

            }
            return bm;
        }
        public Bitmap DesenharEtiquetaFolhaObra(FolhaObra fo)
        {

            int x = 0;
            int y = 0;
            int width = 1024;
            int height = 641;

            Bitmap bm = new Bitmap(width, height);

            Font fontHeader = new Font("Rubik", 70, FontStyle.Bold);
            Font fontBody = new Font("Tahoma", 22, FontStyle.Regular);
            Font fontFooter = new Font("Rubik", 22, FontStyle.Regular);

            StringFormat format = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };

            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.White);

                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (File.Exists(FT_Logo_Print)) { Image img = System.Drawing.Image.FromFile(FT_Logo_Print, true); gr.DrawImage(img, x, y, 400, 235); }

                y += 65;
                gr.DrawString("Food-Tech", fontHeader, Brushes.Black, x + 400, y);

                x = 10;
                y += 165;

                gr.DrawString(fo.ClienteServico.NomeCliente + " (N/S: " + fo.EquipamentoServico.NumeroSerieEquipamento + ")\r\n " + fo.RelatorioServico, fontBody, Brushes.Black, new Rectangle(x, y, width - (x * 2), 200), format);

                y += 250;
                gr.DrawString("FO Nº " + fo.IdFolhaObra.ToString(), fontHeader, new SolidBrush(Color.Black), new RectangleF(x, y, width - (x * 2) - 200, 80), format);

                y += 95;

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(fo.GetUrl.ToString(), QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                gr.DrawImage(qrCodeImage, width - 220, height - 220, 200, 200);

                gr.DrawString("geral@food-tech.pt", fontFooter, Brushes.Black, new Rectangle(x, y, width - (x * 2) - 200, 30), format);

            }

            return bm;
        }
        public MemoryStream PreencherFormularioFolhaObra(FolhaObra folhaobra)
        {
            string pdfTemplate = AppDomain.CurrentDomain.BaseDirectory + "FT_FolhaObra.pdf";
            var outputPdfStream = new MemoryStream();
            PdfReader pdfReader = new PdfReader(pdfTemplate);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, outputPdfStream) { FormFlattening = true, FreeTextFlattening = true };
            AcroFields pdfFormFields = pdfStamper.AcroFields;

            pdfFormFields.SetField("At", folhaobra.IdAT.ToString());
            pdfFormFields.SetField("Pat", folhaobra.IdFolhaObra.ToString());
            pdfFormFields.SetField("Marcação", folhaobra.IdMarcacao.ToString());
            pdfFormFields.SetField("Data", folhaobra.DataServico.ToShortDateString());
            pdfFormFields.SetField("Marca", folhaobra.EquipamentoServico.MarcaEquipamento);
            pdfFormFields.SetField("Modelo", folhaobra.EquipamentoServico.ModeloEquipamento);
            pdfFormFields.SetField("Serie", folhaobra.EquipamentoServico.NumeroSerieEquipamento);
            pdfFormFields.SetField("Nome", folhaobra.ClienteServico.NomeCliente);
            pdfFormFields.SetField("Morada", folhaobra.ClienteServico.MoradaCliente);
            pdfFormFields.SetField("Contribuinte", folhaobra.ClienteServico.NumeroContribuinteCliente);
            pdfFormFields.SetField("ID", folhaobra.ClienteServico.IdCliente.ToString() + " / " + folhaobra.ClienteServico.IdLoja.ToString());
            pdfFormFields.SetField("Relatório", folhaobra.RelatorioServico);
            pdfFormFields.SetField("Referencia", folhaobra.ReferenciaServico);
            pdfFormFields.SetField("Tecnico", folhaobra.IntervencaosServico.Last().NomeTecnico);
            pdfFormFields.SetField("Cliente", folhaobra.ConferidoPor);
            pdfFormFields.SetField("Guia", folhaobra.GuiaTransporteAtual);

            //Mao de Obra
            int i = 1;
            foreach (Intervencao intervencao in folhaobra.IntervencaosServico)
            {
                pdfFormFields.SetField("Data_" + i, intervencao.DataServiço.ToString("dd/MM/yy"));
                pdfFormFields.SetField("Tec_" + i, intervencao.NomeTecnico);
                pdfFormFields.SetField("Inicio_" + i, intervencao.HoraInicio.ToString("HH:mm"));
                pdfFormFields.SetField("Fim_" + i, intervencao.HoraFim.ToString("HH:mm"));

                i++;
                if (i == 14) break;
            }

            //Peças
            int p = 1;
            foreach (Produto pecas in folhaobra.PecasServico)
            {
                pdfFormFields.SetField("Ref_" + p, pecas.Ref_Produto);
                if (pecas.Designacao_Produto.Length > 50)
                {
                    pdfFormFields.SetField("Desig_" + p, pecas.Designacao_Produto.Substring(0, 50));
                }
                else
                {
                    pdfFormFields.SetField("Desig_" + p, pecas.Designacao_Produto);
                }
                pdfFormFields.SetField("Qtd_" + p, pecas.Stock_Fisico.ToString() + " " + pecas.TipoUn.ToString());

                p++;
                if (p == 20) break;
            }

            folhaobra.RubricaCliente = "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";
            if (!string.IsNullOrEmpty(folhaobra.RubricaCliente))
            {
                PushbuttonField ad = pdfFormFields.GetNewPushbuttonFromField("Signature");
                ad.Layout = PushbuttonField.LAYOUT_ICON_ONLY;
                ad.BorderColor = iTextSharp.text.BaseColor.White;
                ad.ProportionalIcon = true;
                ad.Image = iTextSharp.text.Image.GetInstance(Convert.FromBase64String(folhaobra.RubricaCliente));
                pdfFormFields.ReplacePushbuttonField("Signature", ad.Field);
            }

            pdfStamper.FormFlattening = true;
            pdfStamper.SetFullCompression();
            pdfStamper.Close();

            return outputPdfStream;

        }
        public MemoryStream PreencherFormularioFolhaObra2(FolhaObra folhaobra)
        {
            string pdfTemplate = AppDomain.CurrentDomain.BaseDirectory + "FT_FolhaObra_OLD.pdf";
            var outputPdfStream = new MemoryStream();
            PdfReader pdfReader = new PdfReader(pdfTemplate);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, outputPdfStream) { FormFlattening = true, FreeTextFlattening = true };
            AcroFields pdfFormFields = pdfStamper.AcroFields;

            //if (folhaobra.RubricaCliente != null)
            //{
            //    var fldPosition = pdfFormFields.GetFieldPositions("assinatura");
            //    Rectangle rectangle = new Rectangle((int)fldPosition[1], (int)fldPosition[2] - 7, (int)fldPosition[3], 30);
            //    folhaobra.RubricaCliente = folhaobra.RubricaCliente.Replace("data:image/png;base64,", "").Trim();

            //    if ((folhaobra.RubricaCliente.Length % 4 == 0) && Regex.IsMatch(folhaobra.RubricaCliente, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
            //    {
            //        byte[] imageBytes = Convert.FromBase64String(folhaobra.RubricaCliente);

            //        iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageBytes);

            //        image.ScaleToFit(rectangle.Width, rectangle.Height);
            //        image.SetAbsolutePosition(rectangle.Left + 2, rectangle.Top - 2);

            //        pdfStamper.GetOverContent((int)fldPosition[0]).AddImage(image);

            //    }
            //}

            pdfFormFields.SetField("IdFolhaObra", folhaobra.IdAT);
            pdfFormFields.SetField("Marcação", folhaobra.IdMarcacao.ToString());
            pdfFormFields.SetField("Registado", folhaobra.IntervencaosServico.Last().NomeTecnico);
            if (folhaobra.AssistenciaRemota)
            {
                pdfFormFields.SetFieldProperty("Remoto", "textsize", 26f, null);
                pdfFormFields.SetFieldProperty("Remoto", "textcolor", iTextSharp.text.BaseColor.Red, null);
                pdfFormFields.SetField("Remoto", "REMOTO");
            }
            else if ((folhaobra.DataServico.DayOfWeek == DayOfWeek.Sunday) || (folhaobra.DataServico.DayOfWeek == DayOfWeek.Saturday))
            {
                pdfFormFields.SetFieldProperty("Remoto", "textsize", 20f, null);
                pdfFormFields.SetFieldProperty("Remoto", "textcolor", iTextSharp.text.BaseColor.Red, null);
                pdfFormFields.SetField("Remoto", "FIM-DE-SEMANA");
            }

            if (folhaobra.PecasServico.Count() > 0 && folhaobra.GuiaTransporteAtual != "GT" + folhaobra.DataServico.Year + "BO91/" && folhaobra.GuiaTransporteAtual != string.Empty) pdfFormFields.SetField("GT", "Peça(s) retirada(s) da " + folhaobra.GuiaTransporteAtual);

            //Equipamento
            pdfFormFields.SetField("Designação", folhaobra.EquipamentoServico.DesignacaoEquipamento);
            pdfFormFields.SetField("Marca", folhaobra.EquipamentoServico.MarcaEquipamento);
            pdfFormFields.SetField("Mod", folhaobra.EquipamentoServico.ModeloEquipamento);
            pdfFormFields.SetField("NºSerie", folhaobra.EquipamentoServico.NumeroSerieEquipamento);
            pdfFormFields.SetField(string.IsNullOrEmpty(folhaobra.EstadoEquipamento) ? "" : folhaobra.EstadoEquipamento, "On");
            pdfFormFields.SetField("RGPD", "Yes");

            //Cliente
            pdfFormFields.SetField("Referencia", folhaobra.ReferenciaServico);
            pdfFormFields.SetField("Cliente", folhaobra.ClienteServico.NomeCliente);
            pdfFormFields.SetField("Contribuinte", folhaobra.ClienteServico.NumeroContribuinteCliente);

            //Assistencia
            pdfFormFields.SetField("trabalho efectuado", folhaobra.RelatorioServico);
            pdfFormFields.SetField("SituacoesPendentes", folhaobra.SituacoesPendentes);
            if (folhaobra.IntervencaosServico.Count > 0)
            {
                //Mao de Obra
                int i = 1;
                foreach (Intervencao intervencao in folhaobra.IntervencaosServico)
                {
                    pdfFormFields.SetField("DataRow" + i, intervencao.DataServiço.ToString("dd/MM"));
                    pdfFormFields.SetField("TécnicoRow" + i, intervencao.NomeTecnico);
                    pdfFormFields.SetField("InícioRow" + i, intervencao.HoraInicio.ToString("HH:mm"));
                    pdfFormFields.SetField("FimRow" + i, intervencao.HoraFim.ToString("HH:mm"));

                    var ts = new TimeSpan(intervencao.HoraFim.Hour - intervencao.HoraInicio.Hour, intervencao.HoraFim.Minute - intervencao.HoraInicio.Minute, 00);
                    ts = TimeSpan.FromMinutes(15 * Math.Ceiling(ts.TotalMinutes / 15));

                    pdfFormFields.SetField("HorasRow" + i, ts.Hours.ToString() + "." + (ts.Minutes * 100) / 60);
                    i++;
                    if (i == 9) break;
                }

                pdfFormFields.SetField("DataConclusao", folhaobra.IntervencaosServico.Last().DataServiço.ToString("dd/MM/yyyy"));
                pdfFormFields.SetField("o tecnico", folhaobra.IntervencaosServico.Last().NomeTecnico);
            }

            pdfFormFields.SetFieldProperty("Text5", "textsize", 9f, null);
            pdfFormFields.SetField("Text5", folhaobra.DataServico.ToString("dd/MM/yyyy"));

            if (folhaobra.PecasServico.Count > 0)
            {

                //Peças
                int p = 1;
                foreach (Produto pecas in folhaobra.PecasServico)
                {
                    pdfFormFields.SetFieldProperty("ReferênciaRow" + p, "textsize", 6f, null);
                    pdfFormFields.SetField("ReferênciaRow" + p, pecas.Ref_Produto);
                    pdfFormFields.SetFieldProperty("DesignaçãoRow" + p, "textsize", 6f, null);
                    if (pecas.Designacao_Produto.Length > 50)
                    {
                        pdfFormFields.SetField("DesignaçãoRow" + p, pecas.Designacao_Produto.Substring(0, 50));
                    }
                    else
                    {
                        pdfFormFields.SetField("DesignaçãoRow" + p, pecas.Designacao_Produto);
                    }
                    pdfFormFields.SetFieldProperty("QuantRow" + p, "textsize", 6f, null);
                    pdfFormFields.SetField("QuantRow" + p, pecas.Stock_Fisico.ToString() + " " + pecas.TipoUn.ToString()); ;
                    p++;
                    if (p == 10) break;
                }
            }
            if (folhaobra.ConferidoPor == string.Empty) folhaobra.ConferidoPor = folhaobra.ClienteServico.PessoaContatoCliente;
            pdfFormFields.SetField("o cliente", folhaobra.ConferidoPor);

            pdfStamper.FormFlattening = true;
            pdfStamper.SetFullCompression();
            pdfStamper.Close();

            return outputPdfStream;

        }
        public MemoryStream BitMapToMemoryStream(string filePath, int w, int h)
        {
            var ms = new MemoryStream();

            PdfSharpCore.Pdf.PdfDocument doc = new PdfSharpCore.Pdf.PdfDocument();
            PdfSharpCore.Pdf.PdfPage page = new PdfSharpCore.Pdf.PdfPage
            {
                Width = w,
                Height = h
            };

            XImage img = XImage.FromFile(filePath);
            img.Interpolate = false;

            doc.Pages.Add(page);

            XGraphics xgr = XGraphics.FromPdfPage(doc.Pages[0]);
            XRect box = new XRect(0, 0, w, h);
            xgr.DrawImage(img, box);

            doc.Save(ms, false);

            System.IO.File.Delete(filePath);

            return ms;
        }
        public Cliente ObterCliente(Cliente c)
        {
            string sqlQuery = "SELECT * FROM dat_clientes where Cliente_Stamp='" + c.ClienteStamp + "';";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    c.Senha = result[1];
                    c.Latitude = result[2];
                    c.Longitude = result[3];
                }
            }
            return c;

        }
        public List<Cliente> ObterClientes()
        {
            string sqlQuery = "SELECT * FROM dat_clientes;";
            List<Cliente> LstClientes = new List<Cliente>();

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstClientes.Add(new Cliente { ClienteStamp = result[0], Senha = result[1], Latitude = result[2], Longitude = result[3] });
                }
            }
            return LstClientes;

        }
        public string CriarSenhaCliente(string Cliente_Stamp)
        {
            string res = "";
            res = GetRandomString(12);
            string sql = "INSERT INTO dat_clientes (Cliente_Stamp, Senha) VALUES ('" + Cliente_Stamp + "', '" + res + "')  ON DUPLICATE KEY UPDATE Senha = VALUES(Senha);";

            Database db = ConnectionString;

            db.Execute(sql);
            db.Connection.Close();

            return res;
        }
        public void GuardarLocalizacaoCliente(Cliente c)
        {
            string sql = "INSERT INTO dat_clientes (Cliente_Stamp, Latitude, Longitude) VALUES ('" + c.ClienteStamp + "', '" + c.Latitude + "', '" + c.Longitude + "')  ON DUPLICATE KEY UPDATE Latitude = VALUES(Latitude), Longitude = VALUES(Longitude);";

            Database db = ConnectionString;

            db.Execute(sql);
            db.Connection.Close();
        }

        public List<Atividade> ObterAtividade(Marcacao m)
        {
            string sqlQuery = "SELECT * FROM dat_marcacoes_atividade where IdMarcacao=" + m.IdMarcacao + ";";
            List<Atividade> LstAtividades = new List<Atividade>();

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstAtividades.Add(new Atividade
                    {
                        Id = result["Id"],
                        Tipo = result["TipoAtividade"],
                        Nome = result["NomeAtividade"],
                        CriadoPor = result["CriadoPor"],
                        Data = result["DataAtividade"]
                    });
                }
            }
            return LstAtividades;

        }

        public void CriarCodigo(Codigo c)
        {

            string sql = "INSERT INTO dat_codigos (CodigoValidacao, EstadoCodigo, Observacoes, Utilizador, ValidadeCodigo) VALUES ('" + c.Stamp + "', '" + c.Estado + "', '" + c.Obs + "', '" + c.utilizador.Id + "', '" + c.ValidadeCodigo.ToString("yyyy-MM-dd HH:mm:ss") + "') ;";

            Database db = ConnectionString;

            db.Execute(sql);
            db.Connection.Close();
        }
        public void AtualizarCodigo(string stamp, int estado)
        {

            string sql = "UPDATE dat_codigos set EstadoCodigo=" + estado + " WHERE CodigoValidacao='" + stamp + "';";

            Database db = ConnectionString;

            db.Execute(sql);
            db.Connection.Close();
        }

        public int ValidarCodigo(string stamp)
        {
            int res = 0;
            string sqlQuery = "SELECT EstadoCodigo FROM dat_codigos where CodigoValidacao='" + stamp + "' and ValidadeCodigo > '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "';";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    res = int.Parse(result[0]);
                }
            }
            return res;
        }

        public Codigo ObterCodigo(string stamp)
        {
            Codigo c = new Codigo();
            string sqlQuery = "SELECT * FROM dat_codigos where CodigoValidacao='" + stamp + "' and ValidadeCodigo > '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' and EstadoCodigo=0;";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    c = new Codigo()
                    {
                        Stamp = result["CodigoValidacao"],
                        Estado = result["EstadoCodigo"],
                        Obs = result["Observacoes"],
                        ValidadeCodigo = result["ValidadeCodigo"],
                        utilizador = this.ObterUtilizador(int.Parse(result["Utilizador"])),
                    };
                }
            }
            return c;
        }

        //Produtos (INATIVO)
        //public void CriarProduto(List<Produto> LstProdutos)
        //{
        //    int max = 1000;
        //    int j = 0;
        //    for (int i = 0; j < LstProdutos.Count; i++)
        //    {
        //        if ((j + max) > LstProdutos.Count) max = (LstProdutos.Count - j);

        //        string sql = "INSERT INTO dat_produtos (Stamp, Designacao, Quantidade, NumeroSerie, Armazem) VALUES ";

        //        foreach (var produto in LstProdutos.GetRange(j, max))
        //        {
        //            foreach (var equipamento in produto.Equipamentos)
        //            {
        //                sql += ("('" + produto.StampProduto + "', '" + produto.Designacao_Produto + "', '" + produto.Stock_Fisico + "', '" + equipamento.NumeroSerieEquipamento + "', '" + produto.Armazem_ID + "'), \r\n");
        //            }
        //            i++;
        //        }
        //        sql = sql.Remove(sql.Count() - 4);

        //        sql += " ON DUPLICATE KEY UPDATE Quantidade=VALUES(Quantidade), Armazem = VALUES(Armazem);";

        //        Database db = ConnectionString;

        //        db.Execute(sql);
        //        db.Connection.Close();

        //        j += max;
        //        //Console.WriteLine("A ler Marcacao: " + j + " de " + LstMarcacao.Count());
        //    }
        //}
        //public Produto ObterProduto(Produto p)
        //{
        //    string sqlQuery = "SELECT * FROM dat_produtos where Stamp='" + p.StampProduto + "';";

        //    using Database db = ConnectionString;
        //    using (var result = db.Query(sqlQuery))
        //    {
        //        while (result.Read())
        //        {
        //            p.Stock_Fisico += Double.Parse(result["Quantidade"]);
        //            p.Equipamentos.Add(new Equipamento() { NumeroSerieEquipamento = result["NumeroSerie"] });
        //        }
        //    }
        //    return p;
        //}
        //public List<Produto> ObterProdutos()
        //{
        //    string sqlQuery = "SELECT * FROM dat_produtos;";
        //    List<Produto> LstProdutos = new List<Produto>();

        //    using Database db = ConnectionString;
        //    using (var result = db.Query(sqlQuery))
        //    {
        //        while (result.Read())
        //        {
        //            int i = LstProdutos.IndexOf(LstProdutos.Where(p => p.StampProduto == result["Stamp"]).DefaultIfEmpty().First());
        //            if (i >= 0)
        //            {
        //                LstProdutos[i].Stock_Fisico += Double.Parse(result["Quantidade"]);
        //                LstProdutos[i].Equipamentos.Add(new Equipamento() { NumeroSerieEquipamento = result["NumeroSerie"] });
        //            }
        //            else
        //            {
        //                Produto p = new Produto();
        //                p.StampProduto = result["Stamp"];
        //                p.Stock_Fisico += Double.Parse(result["Quantidade"]);
        //                p.Equipamentos = new List<Equipamento>() { new Equipamento() { NumeroSerieEquipamento = result["NumeroSerie"] } };
        //                LstProdutos.Add(p);
        //            }
        //        }
        //    }
        //    return LstProdutos;
        //}
    }
}