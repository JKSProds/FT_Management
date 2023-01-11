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
                        TipoMapa = result["TipoMapa"],
                        CorCalendario = result["CorCalendario"],
                        Pin = result["PinUtilizador"],
                        Iniciais = result["IniciaisUtilizador"],
                        DataNascimento = result["DataNascimento"],
                        IdArmazem = result["IdArmazem"],
                        TipoTecnico = result["TipoTecnico"],
                        Zona = result["Zona"],
                        ChatToken = result["ChatToken"],
#if !DEBUG 
                        ImgUtilizador = string.IsNullOrEmpty(result["ImgUtilizador"]) ? "/img/user.png" : result["ImgUtilizador"],
#endif                        
                        UltimoAcesso = result["DataUltimoAcesso"],
                        AcessoAtivo = result["TipoUltimoAcesso"] == 1,
                        Viatura = new Viatura() { Matricula = result["Matricula_Viatura"] }
                    });
                    if (!string.IsNullOrEmpty(LstUtilizadores.Last().Viatura.Matricula) && Viatura)
                    {
                        LstUtilizadores.Last().Viatura = ObterViatura(LstUtilizadores.Last());
                    }
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
#if !DEBUG 
                        ImgUtilizador = string.IsNullOrEmpty(result["ImgUtilizador"]) ? "/img/user.png" : result["ImgUtilizador"],
#endif
                        Viatura = new Viatura() { Matricula = result["Matricula_Viatura"] }
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
                    if (!string.IsNullOrEmpty(utilizador.Viatura.Matricula))
                    {
                        utilizador.Viatura = ObterViatura(utilizador);
                    }

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
            string sql = "INSERT INTO sys_utilizadores (IdUtilizador, NomeUtilizador, Password, PinUtilizador, NomeCompleto, TipoUtilizador, EmailUtilizador, admin, enable, IdPHC, IdArmazem, IniciaisUtilizador, CorCalendario, TipoMapa, DataNascimento, TelemovelUtilizador, Matricula_Viatura, ImgUtilizador, TipoTecnico, Zona, ChatToken) VALUES ";

            sql += ("('" + utilizador.Id + "', '" + utilizador.NomeUtilizador + "', '" + utilizador.Password + "', '" + utilizador.Pin + "', '" + utilizador.NomeCompleto + "', '" + utilizador.TipoUtilizador + "', '" + utilizador.EmailUtilizador + "', '" + (utilizador.Admin ? "1" : "0") + "', '" + (utilizador.Enable ? "1" : "0") + "', '" + utilizador.IdPHC + "', '" + utilizador.IdArmazem + "', '" + utilizador.Iniciais + "', '" + utilizador.CorCalendario + "', " + utilizador.TipoMapa + ", '" + utilizador.DataNascimento.ToString("yyyy-MM-dd") + "', '" + utilizador.ObterTelemovelFormatado(true) + "', '" + utilizador.Viatura.Matricula + "', '" + utilizador.ImgUtilizador + "', '" + utilizador.TipoTecnico + "', '" + utilizador.Zona + "', '" + utilizador.ChatToken + "') \r\n");

            sql += " ON DUPLICATE KEY UPDATE Password = VALUES(Password), PinUtilizador = VALUES(PinUtilizador), NomeCompleto = VALUES(NomeCompleto), TipoUtilizador = VALUES(TipoUtilizador), EmailUtilizador = VALUES(EmailUtilizador), admin = VALUES(admin), enable = VALUES(enable), IdPHC = VALUES(IdPHC), IdArmazem = VALUES(IdArmazem), IniciaisUtilizador = VALUES(IniciaisUtilizador), CorCalendario = VALUES(CorCalendario), TipoMapa = VALUES(TipoMapa), DataNascimento = VALUES(DataNascimento), TelemovelUtilizador = VALUES(TelemovelUtilizador), Matricula_Viatura = VALUES(Matricula_Viatura), ImgUtilizador = VALUES(ImgUtilizador), TipoTecnico = VALUES(TipoTecnico), Zona = VALUES(Zona), ChatToken = VALUES(ChatToken);";

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

            string sqlQuery = "SELECT *, (Select  fim_localizacao from dat_viaturas_viagens where matricula_viatura='" + utilizador.Viatura.Matricula + "' order by timestamp DESC limit 1) as localizacao2 FROM dat_viaturas where matricula_viatura='" + utilizador.Viatura.Matricula + "';";

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
                        Utilizador = utilizador,
                        Ignicao = result["ignicao"] == 1
                    };
                    if (DateTime.Parse(result["timestamp"]) < DateTime.Now.AddMinutes(-60))
                    {
                        res.Ignicao = result["localizacao2"] == "";
                        res.LocalizacaoMorada = result["localizacao2"] == "" ? "Não foi possivel obter a localização desta viatura!" : result["localizacao2"];
                    }
                }
            }


            return res;
        }

        public List<Viatura> ObterViaturas()
        {
            List<Viatura> res = new List<Viatura>();
            List<Utilizador> LstUtilizadores = ObterListaUtilizadores(true, false).Where(u => u.Viatura.Matricula != "").ToList();
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
                        Utilizador = LstUtilizadores.Where(u => u.Viatura.Matricula == result["matricula_viatura"]).FirstOrDefault(),
                        Ignicao = result["ignicao"] == 1
                    });
                    if (res.Last().Utilizador == null) res.Last().Utilizador = new Utilizador();
                }
            }


            return res.OrderBy(v => v.Utilizador.NomeCompleto).ToList();
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
                string sql = "SELECT * FROM dat_ferias order by DataInicio;";
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
                    title = ut.NomeCompleto,
                    start = item.DataInicio,
                    end = item.DataInicio != item.DataFim ? item.DataFim.AddDays(1) : item.DataInicio.AddDays(1),
                    isAllDay = true,
                    url = "Detalhes/" + item.IdUtilizador,
                    category = "time",
                    dueDateClass = "",
                    color = (ut.CorCalendario == string.Empty ? "#3371FF" : ut.CorCalendario),
                });
            }

            foreach (var item in Feriados)
            {

                LstEventos.Add(new CalendarioEvent
                {
                    id = item.Id.ToString(),
                    title = item.DescFeriado,
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

                string sql = "INSERT INTO dat_ferias (Id,IdUtilizador,DataInicio,DataFim,Validado,Obs, ValidadoPor, Ano) VALUES ";

                foreach (var ferias in LstFerias.GetRange(j, max))
                {
                    sql += ("('" + ferias.Id + "', '" + ferias.IdUtilizador + "', '" + ferias.DataInicio.ToString("yy-MM-dd") + "', '" + ferias.DataFim.ToString("yy-MM-dd") + "', '" + (ferias.Validado ? "1" : "0") + "', '" + ferias.Obs + "', " + ferias.ValidadoPor + ", " + Ano + "), \r\n");
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

            if (!VerificarFeriasUtilizador(IdUtilizador, dataAniversario)) CriarFerias(new List<Ferias>() { new Ferias() { IdUtilizador = IdUtilizador, DataInicio = dataAniversario, DataFim = dataAniversario, Obs = "Dia de Aniversário", Validado = true, ValidadoPorNome = "FT", ValidadoPor = 0 } });

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
            List<Utilizador> LstUtilizadores = ObterListaUtilizadores(true, false);
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
        public DateTime ObterUltimoAcesso(int IdPHC)
        {

            DateTime res = new DateTime();

            using (Database db = ConnectionString)
            {
                using var resultQuery = db.QueryValue("select DataUltimoAcesso from dat_acessos_utilizador where IdUtilizador=(SELECT IdUtilizador FROM sys_utilizadores WHERE IdPHC = " + IdPHC + " LIMIT 1);");
                res = resultQuery.HasData() ? DateTime.Parse(resultQuery) : new DateTime();
            }

            return res;

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

                    if (acesso.Data > ObterUltimoAcesso(acesso.IdUtilizador)) sql2 += "((SELECT IdUtilizador FROM sys_utilizadores WHERE IdPHC = " + acesso.IdUtilizador + "), '" + acesso.Data.ToString("yyyy-MM-dd HH:mm:ss") + "', " + acesso.Tipo + ", 1),\r\n";
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
                catch (Exception)
                {
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
                            title = (item.EstadoMarcacao == 4 || item.EstadoMarcacao == 9 || item.EstadoMarcacao == 10 ? "✔ " : item.EstadoMarcacao != 1 && item.EstadoMarcacao != 26 ? "⌛ " : item.EstadoMarcacaoDesc == "Criado" && item.Utilizador.NomeCompleto == "MailTrack" ? "🤖 " : item.DataMarcacao < DateTime.Now && item.EstadoMarcacaoDesc != "Criado" ? "❌ " : "") + item.Cliente.NomeCliente,
                            start = dataMarcacao,
                            end = dataMarcacao.AddMinutes(25),
                            IdTecnico = item.Tecnico.Id,
                            category = "time",
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
        public Bitmap DesenharEtiqueta80x50(Produto produto)
        {

            int x = 30;
            int y = 0;
            int width = 300;
            int height = 188;

            Bitmap bm = new Bitmap(width, height);

            Font fontHeader = new Font("Tahoma", 18, FontStyle.Bold);
            Font fontBody = new Font("Tahoma", 8, FontStyle.Regular);

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


                if (File.Exists(FT_Logo_Print)) { Image img = System.Drawing.Image.FromFile(FT_Logo_Print, true); gr.DrawImage(img, x, y, 85, 50); }

                y += 10;
                gr.DrawString("Food-Tech", fontHeader, Brushes.Black, x + 85 + 10, y);

                x = 10;
                y += 40;
                gr.DrawString(produto.Designacao_Produto, fontBody, Brushes.Black, new Rectangle(x, y, width - (x * 2), 35), format);


                y += 40;
                Barcode.Code93 code = new Barcode.Code93
                {
                    DrawText = false
                };

                gr.DrawImage(code.desenharBarcode(produto.Ref_Produto), 10, y, width - (x * 2), 90);
                y += 60;
                gr.DrawString(produto.Ref_Produto, fontHeader, new SolidBrush(Color.Black), new RectangleF(x, y, width - (x * 2), 20), format);

                gr.DrawString("geral@food-tech.pt", fontBody, Brushes.Black, new Rectangle(x, height - 20, width - (x * 2), 20), format);

            }

            return bm;
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
            pdfFormFields.SetField(folhaobra.EstadoEquipamento, "On");
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