﻿using System.Globalization;
using System.Text.RegularExpressions;
using System.util;
using Custom;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace FT_Management.Models
{
    public class FT_ManagementContext
    {

        public string ConnectionString { get; set; }
        public FT_ManagementContext(string connectionString)
        {
            this.ConnectionString = connectionString;

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
            string sqlQuery = "SELECT sys_utilizadores.*, IFNULL(dat_acessos_utilizador.DataUltimoAcesso, '') as DataUltimoAcesso, IFNULL(dat_acessos_utilizador.TipoUltimoAcesso, '') as TipoUltimoAcesso FROM sys_utilizadores left join dat_acessos_utilizador on sys_utilizadores.IdUtilizador = dat_acessos_utilizador.IdUtilizador " + (Enable ? "WHERE enable=1" : "") + " order by NomeCompleto;";

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
                        IsencaoHorario = result["isencao"],
                        Dev = result["dev"],
                        Dashboard = result["dashboard"],
                        TipoMapa = result["TipoMapa"],
                        CorCalendario = result["CorCalendario"],
                        Pin = result["PinUtilizador"],
                        Iniciais = result["IniciaisUtilizador"],
                        DataNascimento = result["DataNascimento"],
                        IdArmazem = result["IdArmazem"],
                        IdFuncionario = result["IdFunc"],
                        TipoTecnico = result["TipoTecnico"],
                        Zona = result["Zona"],
                        ChatToken = result["ChatToken"],
                        FaceRec = result["FaceRec"],
                        SecondFactorAuthStamp = result["SecondFactorAuthStamp"],
                        NotificacaoAutomatica = result["NotificacaoAutomatica"],
                        UltimoAcesso = string.IsNullOrEmpty(result["DataUltimoAcesso"]) ? new DateTime() : result["DataUltimoAcesso"],
                        AcessoAtivo = string.IsNullOrEmpty(result["DataUltimoAcesso"]) ? false : result["TipoUltimoAcesso"] == 1,
                        StampMoradaCargaDescarga = result["StampMoradaCargaDescarga"],
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
            return ObterListaUtilizadores(Enable, Viatura).Where(u => u.IdPHC > 0 && u.IdArmazem > 0).ToList();
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
                        IsencaoHorario = result["isencao"],
                        Dev = result["dev"],
                        Dashboard = result["dashboard"],
                        Telemovel = result["TelemovelUtilizador"],
                        CorCalendario = result["CorCalendario"],
                        IdPHC = result["IdPHC"],
                        IdFuncionario = result["IdFunc"],
                        TipoMapa = result["TipoMapa"],
                        Pin = result["PinUtilizador"],
                        Iniciais = result["IniciaisUtilizador"],
                        DataNascimento = result["DataNascimento"],
                        IdArmazem = result["IdArmazem"],
                        TipoTecnico = result["TipoTecnico"],
                        Zona = result["Zona"],
                        ChatToken = result["ChatToken"],
                        FaceRec = result["FaceRec"],
                        NotificacaoAutomatica = result["NotificacaoAutomatica"],
                        SecondFactorAuthStamp = result["SecondFactorAuthStamp"],
                        StampMoradaCargaDescarga = result["StampMoradaCargaDescarga"],
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

        public Utilizador ObterUtilizadorSimples(int Id)
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
                        IsencaoHorario = result["isencao"],
                        Dev = result["dev"],
                        Dashboard = result["dashboard"],
                        Telemovel = result["TelemovelUtilizador"],
                        CorCalendario = result["CorCalendario"],
                        IdPHC = result["IdPHC"],
                        IdFuncionario = result["IdFunc"],
                        TipoMapa = result["TipoMapa"],
                        Pin = result["PinUtilizador"],
                        Iniciais = result["IniciaisUtilizador"],
                        DataNascimento = result["DataNascimento"],
                        IdArmazem = result["IdArmazem"],
                        TipoTecnico = result["TipoTecnico"],
                        Zona = result["Zona"],
                        ChatToken = result["ChatToken"],
                        FaceRec = result["FaceRec"],
                        NotificacaoAutomatica = result["NotificacaoAutomatica"],
                        SecondFactorAuthStamp = result["SecondFactorAuthStamp"],
                        StampMoradaCargaDescarga = result["StampMoradaCargaDescarga"]
                    };
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

        public List<Zona> ObterZonas(bool Piquete)
        {
            string sqlQuery = "SELECT * FROM sys_zonas "+ (Piquete ? "WHERE IdZona<3" : "") +" ;";
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
            string sql = "INSERT INTO sys_utilizadores (IdUtilizador, NomeUtilizador, Password, FaceRec, PinUtilizador, NomeCompleto, TipoUtilizador, EmailUtilizador, admin, enable, acessos, isencao, dev, dashboard, IdPHC, IdArmazem, IdFunc, IniciaisUtilizador, CorCalendario, TipoMapa, DataNascimento, TelemovelUtilizador, ImgUtilizador, TipoTecnico, Zona, ChatToken, NotificacaoAutomatica, SecondFactorAuthStamp) VALUES ";

            sql += ("('" + utilizador.Id + "', '" + utilizador.NomeUtilizador + "', '" + utilizador.Password + "',  '" + utilizador.FaceRec + "', '" + utilizador.Pin + "', '" + utilizador.NomeCompleto + "', '" + utilizador.TipoUtilizador + "', '" + utilizador.EmailUtilizador + "', '" + (utilizador.Admin ? "1" : "0") + "', '" + (utilizador.Enable ? "1" : "0") + "', '" + (utilizador.Acessos ? "1" : "0") + "', '" + (utilizador.IsencaoHorario ? "1" : "0") + "', '" + (utilizador.Dev ? "1" : "0") + "','" + (utilizador.Dashboard ? "1" : "0") + "', '" + utilizador.IdPHC + "', '" + utilizador.IdArmazem + "', '" + utilizador.IdFuncionario + "', '" + utilizador.Iniciais + "', '" + utilizador.CorCalendario + "', " + utilizador.TipoMapa + ", '" + utilizador.DataNascimento.ToString("yyyy-MM-dd") + "', '" + utilizador.ObterTelemovelFormatado(true) + "', '" + utilizador.ImgUtilizador + "', '" + utilizador.TipoTecnico + "', '" + utilizador.Zona + "', '" + utilizador.ChatToken + "', '" + utilizador.NotificacaoAutomatica + "', '" + utilizador.SecondFactorAuthStamp + "') ");

            sql += " ON DUPLICATE KEY UPDATE Password = VALUES(Password), FaceRec = VALUES(FaceRec), PinUtilizador = VALUES(PinUtilizador), NomeCompleto = VALUES(NomeCompleto), TipoUtilizador = VALUES(TipoUtilizador), EmailUtilizador = VALUES(EmailUtilizador), admin = VALUES(admin), enable = VALUES(enable), acessos = VALUES(acessos), isencao = VALUES(isencao), dev = VALUES(dev), dashboard = VALUES(dashboard), IdPHC = VALUES(IdPHC), IdArmazem = VALUES(IdArmazem), IdFunc = VALUES(IdFunc), IniciaisUtilizador = VALUES(IniciaisUtilizador), CorCalendario = VALUES(CorCalendario), TipoMapa = VALUES(TipoMapa), DataNascimento = VALUES(DataNascimento), TelemovelUtilizador = VALUES(TelemovelUtilizador), ImgUtilizador = VALUES(ImgUtilizador), TipoTecnico = VALUES(TipoTecnico), Zona = VALUES(Zona), ChatToken = VALUES(ChatToken), NotificacaoAutomatica = VALUES(NotificacaoAutomatica), SecondFactorAuthStamp = VALUES(SecondFactorAuthStamp);";

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
            string sqlQuery = "SELECT * FROM dat_viaturas_viagens where matricula_viatura='" + Matricula + "' and inicio_viagem>='" + DateTime.Parse(DataViagens).ToString("yyyy-MM-dd") + " 00:00:00' and fim_viagem<='" + DateTime.Parse(DataViagens).ToString("yyyy-MM-dd") + " 23:59:59' and fim_kms > 0;";

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
        public async void ObterFeriadosAPI(string ano)
        {
            List<Feriado> LstFeriados = new List<Feriado>();

            try
            {
                using (HttpClient wc = new HttpClient())
                {
                    var json = await wc.GetStringAsync("https://date.nager.at/api/v3/PublicHolidays/" + ano + "/PT");

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


        public List<CalendarioEvent> ConverterFeriadosEventos(List<Feriado> Feriados)
        {
            List<CalendarioEvent> LstEventos = new List<CalendarioEvent>();

           
            foreach (var item in Feriados)
            {
                try
                {
                        LstEventos.Add(new CalendarioEvent
                        {
                            id = item.Id.ToString(),
                            calendarId = "1",
                            title = item.Emoji + item.DescFeriado,
                            start = item.DataFeriado,
                            end = item.DataFeriado,
                            isAllDay = true,
                            category = "time",
                            editable = false
                        });
                }
                catch
                {

                }

            }
            return LstEventos;
        }

        public List<CalendarioEvent> ConverterPiquetesEventos(List<Piquete> piquetes)
        {
            List<CalendarioEvent> LstEventos = new List<CalendarioEvent>();

           
            foreach (var item in piquetes)
            {
                try
                {
                        LstEventos.Add(new CalendarioEvent
                        {
                            id = item.Stamp,
                            calendarId = "1",
                            title = "🏖️😓🛠 PIQUETE",
                            start = item.Data.AddDays(5),
                            end = item.Data.AddDays(6),
                            isAllDay = true,
                            category = "time",
                            editable = false,
                            color = "#f99f1e"
                        });
                }
                catch
                {

                }

            }
            return LstEventos;
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
                    url = "Utilizador/" + item.IdUtilizador,
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
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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
                                workSheet.Cells[y, j + 1].Style.Fill.BackgroundColor.SetColor(LstFeriasUtilizador.First().Validado ? System.Drawing.Color.LightGreen : System.Drawing.Color.LightBlue);
                                count += 1;
                            }

                            if (DataAtual.DayOfWeek == DayOfWeek.Saturday || DataAtual.DayOfWeek == DayOfWeek.Sunday)
                            {
                                workSheet.Cells[y, j + 1].Value = DataAtual.ToString("ddd").Substring(0, 1).ToUpper();
                                workSheet.Cells[y, j + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                workSheet.Cells[y, j + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
                            }

                            if (LstFeriados.Where(f => f.DataFeriado == DataAtual).Count() > 0)
                            {
                                workSheet.Cells[y, j + 1].Value = "F";
                                workSheet.Cells[y, j + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                workSheet.Cells[y, j + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);

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
                        App = result["App"] == 1,
                        Validado = result["Validado"] == 1
                    });
                }
            }

            return LstAcessos;

        }

        public Acesso ObterAcesso(int id)
        {
            List<Acesso> LstAcessos = new List<Acesso>();
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_acessos where Id="+id+";");
                while (result.Read())
                {
                    LstAcessos.Add(new Acesso()
                    {
                        Id = result["Id"],
                        Utilizador = ObterUtilizador(result["IdUtilizador"]),
                        Data = result["DataHoraAcesso"],
                        Temperatura = result["Temperatura"],
                        Tipo = result["Tipo"],
                        App = result["App"] == 1,
                        Validado = result["Validado"] == 1
                    });
                }
            }

            return LstAcessos.DefaultIfEmpty(new Acesso()).First();

        }

        public List<CalendarioEvent> ConverterAcessosEventos(List<RegistroAcessos> Acessos)
        {
            List<CalendarioEvent> LstEventos = new List<CalendarioEvent>();

            foreach (var item in Acessos.OrderBy(a => a.Utilizador.Id).OrderBy(a => a.Data))
            {
                try
                {
                    if (item.Utilizador.Id != 0)
                    {
                        LstEventos.Add(new CalendarioEvent
                        {
                            id = item.E1.ToString(),
                            calendarId = "1",
                            title = item.E1.Data.ToShortTimeString(),
                            start = item.E1.Data,
                            end = item.S1.Data == DateTime.MinValue ? item.E1.Data : item.S1.Data,
                            category = "time",
                            url = "Acessos/" + item.Utilizador.Id
                        });
                    }
                }
                catch
                {

                }

            }
            return LstEventos;
        }

        public List<RegistroAcessos> ObterListaRegistroAcessos(DateTime start, DateTime end)
        {
            List<RegistroAcessos> LstRegistroAcessos = new List<RegistroAcessos>();
            List<Acesso> LstAcessos = ObterListaAcessos(start, end);
            List<Utilizador> LstUtilizadores = ObterListaUtilizadores(true, false).Where(u=>u.Acessos).ToList();


            foreach (Utilizador u in LstUtilizadores) {
                u.BancoHoras = ObterHorasBanco(u);
                List<Acesso> TMPAcessos = LstAcessos.Where(a => a.Utilizador.Id == u.Id).ToList();
                LstRegistroAcessos.Add(new RegistroAcessos() {
                    Utilizador =u,
                    E1 = TMPAcessos.Count() >= 1? TMPAcessos[0] : new Acesso(),
                    S1 = TMPAcessos.Count() >= 2? TMPAcessos[1] : new Acesso(),
                    E2 = TMPAcessos.Count() >= 3? TMPAcessos[2] : new Acesso(),
                    S2 = TMPAcessos.Count() >= 4? TMPAcessos[3] : new Acesso(),
                    Data = TMPAcessos.Count() >=1 ? TMPAcessos.First().Data : DateTime.MinValue
                });
                LstRegistroAcessos.Last().Utilizador.Ferias = VerificarFeriasUtilizador(u.Id, LstRegistroAcessos.Last().Data);
            }

            return LstRegistroAcessos;

        }

        public RegistroAcessos ObterListaRegistroAcessos(List<Acesso> LstAcessos)
        {
            List<RegistroAcessos> LstRegistroAcessos = new List<RegistroAcessos>();
            List<Utilizador> LstUtilizadores = ObterListaUtilizadores(true, false).Where(u=>u.Acessos).ToList();


            foreach (Utilizador u in LstUtilizadores) {
                u.BancoHoras = ObterHorasBanco(u);
                List<Acesso> TMPAcessos = LstAcessos.Where(a => a.Utilizador.Id == u.Id).ToList();
                if (TMPAcessos.Count() > 0) {
                LstRegistroAcessos.Add(new RegistroAcessos() {
                    Utilizador =u,
                    E1 = TMPAcessos.Count() >= 1? TMPAcessos[0] : new Acesso(),
                    S1 = TMPAcessos.Count() >= 2? TMPAcessos[1] : new Acesso(),
                    E2 = TMPAcessos.Count() >= 3? TMPAcessos[2] : new Acesso(),
                    S2 = TMPAcessos.Count() >= 4? TMPAcessos[3] : new Acesso(),
                    Data = TMPAcessos.Count() >=1 ? TMPAcessos.First().Data : DateTime.MinValue
                });
                }
            }

            return LstRegistroAcessos.DefaultIfEmpty(new RegistroAcessos()).First();

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
                        App = result["App"] == 1,
                        Validado = result["Validado"] == 1
                    });
                }
            }

            return LstAcessos;

        }


        public List<Acesso> ObterListaAcessos(DateTime dInicio, DateTime dFim)
        {
            List<Acesso> LstAcessos = new List<Acesso>();
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_acessos where DataHoraAcesso>'" + dInicio.ToString("yyyy-MM-dd 00:00:00") +"' AND DataHoraAcesso<'" + dFim.ToString("yyyy-MM-dd 23:59:59") + "' order by DataHoraAcesso;");
                while (result.Read())
                {
                    LstAcessos.Add(new Acesso()
                    {
                        Id = result["Id"],
                        Utilizador = ObterUtilizador(result["IdUtilizador"]),
                        Data = result["DataHoraAcesso"],
                        Temperatura = result["Temperatura"],
                        Tipo = result["Tipo"],
                        App = result["App"] == 1,
                        Validado = result["Validado"] == 1
                    });
                }
            }

            return LstAcessos;

        }

        public byte[] GerarMapaPresencas(DateTime dInicio, DateTime dFim)
        {
            using ExcelPackage package = new ExcelPackage(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "FT_Presencas.xlsx"));
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelWorksheet workSheet = package.Workbook.Worksheets.First();
            int totalRows = workSheet.Dimension.Rows;
            
            DateTime dataAtual = dInicio;

             List<Acesso> LstAcessos = ObterListaAcessos(dInicio, dFim);
            List<Feriado> LstFeriados = ObterListaFeriados(dInicio.Year.ToString()).ToList();
            List<Utilizador> LstUtilizadores = LstAcessos.Select(access => access.Utilizador) // Select the user from each access object
            .GroupBy(user => user.Id).Select(group => group.First()).OrderBy(user => user.NomeCompleto).Where(a => a.Acessos).ToList();

            int y = 5;
            int x = 1;

            workSheet.Cells[4, 1].Value = dInicio.ToString("dd-MM") + " - " + dFim.ToString("dd-MM");

            //if (Data.Month < 05 && Data.Year < 2023) LstUtilizadores.Remove(LstUtilizadores.Where(u => u.Id==29).First());
            //if (Data.Month < 05 && Data.Year < 2023) LstUtilizadores.Remove(LstUtilizadores.Where(u => u.Id==30).First());
            //if (Data.Month < 12 && Data.Year < 2023) LstUtilizadores.Where(u => u.Id==4).First().NomeCompleto = "Ricardo Almeida";
            //if (Data.Month < 6 && Data.Year < 2024) LstUtilizadores.Remove(LstUtilizadores.Where(u => u.Id==35).First());
            //if (Data.Month > 4 && Data.Year > 2023) LstUtilizadores.Remove(LstUtilizadores.Where(u => u.Id==23).First());
                
            foreach (var utilizador in LstUtilizadores)
            {
                workSheet.Cells[y, x].Value = utilizador.NomeCompleto;
                y += 4;
            }

        if (LstAcessos.Count > 0)
            {
                while (dataAtual.Date <= dFim.Date)
                { 
                    y = 5;
                    workSheet.Cells[y-1, x + 1].Value = dataAtual.Date.Day;
                    
                    foreach (Utilizador utilizador in LstUtilizadores)
                    {   
                        
                        int j = y;
                        List<Acesso> Lst = LstAcessos.Where(u => u.Data.ToShortDateString() == dataAtual.ToShortDateString()).Where(u => u.Utilizador.Id == utilizador.Id).ToList();

                        if (VerificarFeriasUtilizador(utilizador.Id, dataAtual))
                        {
                            workSheet.Cells[j, x + 1].Value = "FÉRIAS";
                            workSheet.Cells[j, x + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            workSheet.Cells[j, x + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                        }
                        else
                        if (LstFeriados.Where(f => f.DataFeriado == dataAtual).Count() > 0) {
                            workSheet.Cells[j, x + 1].Value = LstFeriados.Where(f => f.DataFeriado == dataAtual).First().DescFeriado;
                            workSheet.Cells[j, x + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            workSheet.Cells[j, x + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Orange);
                                
                        } else if (!(dataAtual.DayOfWeek == DayOfWeek.Saturday || dataAtual.DayOfWeek == DayOfWeek.Sunday))
                        {
                            if (Lst.Count() == 0)
                            {
                                workSheet.Cells[j, x + 1].Value = utilizador.TipoUtilizador == 1 ? "E: 9:00 Externo" : utilizador.TipoUtilizador == 2 ? "E: 9:00 Comercial" : "E: 09:00";
                                workSheet.Cells[j, x + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                workSheet.Cells[j, x + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                                workSheet.Cells[j + 1, x + 1].Value = utilizador.TipoUtilizador == 1 ? "S: 18:30 Externo" : utilizador.TipoUtilizador == 2 ? "S: 18:30 Comercial" : "S: 18:30 ";
                                workSheet.Cells[j + 1, x + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                workSheet.Cells[j + 1, x + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                            }else if (Lst.Count() == 1 && Lst.First().TipoAcesso.Substring(0, 1) == "S") {
                                workSheet.Cells[j, x + 1].Value = utilizador.TipoUtilizador == 1 ? "E: 9:00 Externo" : utilizador.TipoUtilizador == 2 ? "E: 9:00 Comercial" : "E: 09:00";
                                workSheet.Cells[j, x + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                workSheet.Cells[j, x + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                            }else if (Lst.Count() == 1 && Lst.First().TipoAcesso.Substring(0, 1) != "S") {
                                Acesso a = Lst.First();
                                workSheet.Cells[j, x + 1].Value = a.TipoAcesso.Substring(0, 1) + ": " + a.Data.ToShortTimeString();
                                workSheet.Cells[j + 1, x + 1].Value = utilizador.TipoUtilizador == 1 ? "S: 18:30 Externo" : utilizador.TipoUtilizador == 2 ? "S: 18:30 Comercial" : "S: 18:30 ";
                                workSheet.Cells[j + 1, x + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                workSheet.Cells[j + 1, x + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                            }else if (Lst.Count > 4 || Lst.Count() == 3) {
                                    workSheet.Cells[j, x + 1].Value = Lst.First().TipoAcesso.Substring(0, 1) + ": " + Lst.First().Data.ToShortTimeString();
                                    j++;
                                    workSheet.Cells[j, x + 1].Value = Lst.Last().TipoAcesso.Substring(0, 1) + ": " + Lst.First().Data.ToShortTimeString();
                                    j++;
                            }else
                            {
                                foreach (var acesso in Lst)
                                {
                                    workSheet.Cells[j, x + 1].Value = acesso.TipoAcesso.Substring(0, 1) + ": " + acesso.Data.ToShortTimeString();
                                    j++;
                                }
                            }
                        }
                    y += 4;
                    }
                    x+=1;
                    dataAtual = dataAtual.AddDays(1);
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        public int CriarAcessoInterno(List<Acesso> LstAcessos)
        {
            int a = 0;
            bool UltimoAcesso = false;
            if (LstAcessos.Count > 0)
            {
                string sql1 = "INSERT INTO dat_acessos (IdUtilizador,DataHoraAcesso,Tipo, Temperatura, App) VALUES ";
                string sql2 = "INSERT INTO dat_acessos_utilizador (IdUtilizador, DataUltimoAcesso, TipoUltimoAcesso, App, timestamp) VALUES";

                foreach (Acesso acesso in LstAcessos.OrderBy(a => a.Data))
                {
                    sql1 += "(" + acesso.IdUtilizador + ", '" + acesso.Data.ToString("yyyy-MM-dd HH:mm:ss") + "', " + acesso.Tipo + ", '" + acesso.Temperatura + "', 0),\r\n";

                    if (acesso.Data > ObterDataUltimoAcesso(acesso.IdUtilizador)) 
                     {
                        sql2 += "(" + acesso.IdUtilizador + ", '" + acesso.Data.ToString("yyyy-MM-dd HH:mm:ss") + "', " + acesso.Tipo + ", 0, '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'),\r\n";
                        UltimoAcesso = true;
                     }
                }

                sql1 = sql1.Remove(sql1.Count() - 3);
                sql1 += ";";
                sql2 = sql2.Remove(sql2.Count() - 3);
                sql2 += " ON DUPLICATE KEY UPDATE DataUltimoAcesso = VALUES(DataUltimoAcesso), TipoUltimoAcesso = VALUES(TipoUltimoAcesso);";

                try
                {
                    Database db = ConnectionString;

                    if (UltimoAcesso) db.Execute(sql2);
                    db.Execute(sql1);

                    using var result = db.Query("select LAST_INSERT_ID();");
                    while (result.Read())
                    {
                        a = int.Parse(result[0]);
                    }

                    db.Connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return a;
        }

        public void AtualizarAcessoInterno(Acesso acesso)
        {
                string sql1 = "Update dat_acessos set DataHoraAcesso='"+acesso.Data.ToString("yyyy-MM-dd HH:mm:ss")+"', Validado='"+(acesso.Validado ? 1 : 0)+"' WHERE Id="+acesso.Id+";";
                string sql2 = "INSERT INTO dat_acessos_utilizador (IdUtilizador, DataUltimoAcesso, TipoUltimoAcesso, App, timestamp) VALUES (" + acesso.IdUtilizador + ", '" + acesso.Data.ToString("yyyy-MM-dd HH:mm:ss") + "', " + acesso.Tipo + ", 0, '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "') ON DUPLICATE KEY UPDATE DataUltimoAcesso = VALUES(DataUltimoAcesso), TipoUltimoAcesso = VALUES(TipoUltimoAcesso);";
                try
                {
                    Database db = ConnectionString;

                    db.Execute(sql1);
                    if (acesso.Data > ObterDataUltimoAcesso(acesso.IdUtilizador)) db.Execute(sql2);
                    db.Connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
        }
        public void ApagarAcesso(int Id)
        {
            string sql = "DELETE FROM dat_acessos where Id=" + Id + ";";

            using Database db = ConnectionString;
            db.Execute(sql);
        }

        public bool CriarRegistoBancoHoras(RegistroAcessos r, Utilizador u, int NHoras)
        {
                bool res = false;
                string obs = "Criado um registo de banco de horas pelo utilizador " + u.NomeCompleto + " do utilizador " + r.Utilizador.NomeCompleto + " de " + (r.TipoHorasExtra > 0 ? " Horas Extraordinarias " : " Falta ") + ". " + NHoras + " Hora(s).";
                
                string sql = "INSERT INTO dat_banco_horas (Stamp,IdUtilizador,Tipo,Horas,IdAcessos,Observacoes,DataAcessos) VALUES ";

                sql += "('"+ DateTime.Now.Ticks.ToString() +"', '"+ r.Utilizador.Id +"', '"+ (r.TipoHorasExtra > 0 ? 1 : 2) +"', '"+ NHoras.ToString() +"', '" + r.E1.Id + ", " + r.S1.Id + ", " + r.E2.Id + ", " + r.S2.Id +"', '"+ obs +"', '"+ r.Data.ToString("yyyy-MM-dd") +"');";

                Database db = ConnectionString;

                res = db.Execute(sql) == 1;
                db.Connection.Close();

                return res;
        }

        public int ObterHorasBanco(Utilizador u) {
            int res = 0;

            using Database db = ConnectionString;
            using var result = db.Query("SELECT SUM(CASE WHEN Tipo = 1 THEN Horas WHEN Tipo = 2 THEN -Horas ELSE 0 END) AS total_horas FROM dat_banco_horas WHERE IdUtilizador="+ u.Id +";");
            while (result.Read())
            {
                int.TryParse(result[0], out res);
            }
            return res;
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
                    UtilizadorDestino = ObterUtilizadorSimples(result["UtilizadorDestino"]),
                    UtilizadorOrigem = ObterUtilizadorSimples(result["UtilizadorOrigem"]),
                    Tipo = result["Tipo"],
                    Pendente = result["Pendente"] == 1
                });
            }

            return LstNotificacoes;
        }

        public List<Notificacao> ObterNotificacoesPendentes(int id)
        {
            List<Notificacao> LstNotificacoes = new List<Notificacao>();
            using Database db = ConnectionString;
            using var result = db.Query("SELECT * FROM dat_notificacoes where Pendente=0 and timestamp > '"+DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd")+"' and UtilizadorDestino=" + id+";");
            while (result.Read())
            {
                LstNotificacoes.Add(new Notificacao()
                {
                    ID = int.Parse(result["Id"]),
                    Mensagem = result["Mensagem"],
                    UtilizadorDestino = ObterUtilizadorSimples(int.Parse(result["UtilizadorDestino"])),
                    UtilizadorOrigem = ObterUtilizadorSimples(int.Parse(result["UtilizadorOrigem"])),
                    Tipo = result["Tipo"],
                    Pendente = result["Pendente"] == "1"
                });
            }

            return LstNotificacoes;
        }

        public bool CriarNotificacao(Notificacao notificacao)
        {

            string sql = "INSERT INTO dat_notificacoes (Mensagem,UtilizadorDestino,UtilizadorOrigem, Tipo, Pendente) VALUES ('" + notificacao.Mensagem + "', '" + notificacao.UtilizadorDestino.Id + "', '" + notificacao.UtilizadorOrigem.Id + "', '" + notificacao.Tipo + "', '" + (notificacao.Pendente ? "1" : "0") + "');";

            try
            {
                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ApagarNotificacao(int id)
        {

            string sql = "UPDATE dat_notificacoes SET Pendente=1 where Id="+id+";";

            try
            {
                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();
                return true;
            }
            catch
            {
                return false;
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
                    Contactos = [new() { Contacto = result["Telefone"].ToString()}],
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

        #region Piquete

            public List<Piquete> ObterPiquetes(DateTime dInicio, DateTime dFim) {
            System.Globalization.CultureInfo cultura = System.Globalization.CultureInfo.CurrentCulture;
            System.Globalization.Calendar calendario = cultura.Calendar;

            List<Piquete> LstP = new List<Piquete>();
            List<Piquete> LstP2 = new List<Piquete>();
            List<TipoTecnico> LstT = ObterTipoTecnicos();
            List<Ferias> LstF = ObterListaFeriasValidadas();
            List<Zona> LstZ = ObterZonas(true);

            string sqlQuery = "SELECT * FROM dat_piquete WHERE STR_TO_DATE(CONCAT(SUBSTRING(stamp, 1, 4), '-01-01') + INTERVAL (SUBSTRING(stamp, 6, 2) - 1) WEEK, '%Y-%m-%d') BETWEEN '" + dInicio.ToString("yyyy-MM-dd") + "' AND '" + dFim.ToString("yyyy-MM-dd") + "';";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstP2.Add(new Piquete(){
                        Stamp = result["Stamp"],
                        IdUtilizador = result["IdUtilizador"],
                        Utilizador = ObterUtilizador(result["IdUtilizador"])
                    });
                }
            }

            DateTime dataAtual = dInicio;
            while (dataAtual <= dFim)
            {
                int numeroSemana = calendario.GetWeekOfYear(dataAtual, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                foreach (var t in LstT) {
                    foreach (var z in LstZ) {
                        string Stamp = dataAtual.Year.ToString() + "," + numeroSemana + "," + z.Id + "," + t.Id;
                        if (LstP2.Where(x => x.Stamp == Stamp).Count() > 0) {
                            LstP.Add(LstP2.Where(x => x.Stamp == Stamp).First());
                        }else{
                        LstP.Add(new Piquete() {
                            Stamp = dataAtual.Year.ToString() + "," + numeroSemana + "," + z.Id + "," + t.Id,
                            Utilizador = new Utilizador(),
                            });
                        }
                       LstP.Last().Valido = !LstF
    .Any(x => x.IdUtilizador == LstP.Last().IdUtilizador &&
               (calendario.GetWeekOfYear(x.DataInicio, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday) == numeroSemana ||
                calendario.GetWeekOfYear(x.DataFim, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday) == numeroSemana) && x.DataFim.DayOfWeek==DayOfWeek.Friday);
         }
                }
                // Incrementa a data atual em 7 dias (uma semana)
                dataAtual = dataAtual.AddDays(7);
            }

            
            return LstP;
        }

        public List<Piquete> ObterPiquetes(DateTime dInicio, DateTime dFim, Utilizador u) {
            System.Globalization.CultureInfo cultura = System.Globalization.CultureInfo.CurrentCulture;
            System.Globalization.Calendar calendario = cultura.Calendar;

            List<Piquete> LstP = new List<Piquete>();
            List<Piquete> LstP2 = new List<Piquete>();
            List<TipoTecnico> LstT = ObterTipoTecnicos();
            List<Ferias> LstF = ObterListaFeriasValidadas();
            List<Zona> LstZ = ObterZonas(true);

            string sqlQuery = "SELECT * FROM dat_piquete WHERE STR_TO_DATE(CONCAT(SUBSTRING(stamp, 1, 4), '-01-01') + INTERVAL (SUBSTRING(stamp, 6, 2) - 1) WEEK, '%Y-%m-%d') BETWEEN '" + dInicio.ToString("yyyy-MM-dd") + "' AND '" + dFim.ToString("yyyy-MM-dd") + "' and IdUtilizador="+u.Id+";";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstP2.Add(new Piquete(){
                        Stamp = result["Stamp"],
                        IdUtilizador = result["IdUtilizador"],
                        Utilizador = ObterUtilizador(result["IdUtilizador"])
                    });
                }
            }

            DateTime dataAtual = dInicio;
            while (dataAtual <= dFim)
            {
                int numeroSemana = calendario.GetWeekOfYear(dataAtual, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                foreach (var t in LstT) {
                    foreach (var z in LstZ) {
                        string Stamp = dataAtual.Year.ToString() + "," + numeroSemana + "," + z.Id + "," + t.Id;
                        if (LstP2.Where(x => x.Stamp == Stamp).Count() > 0) {
                            LstP.Add(LstP2.Where(x => x.Stamp == Stamp).First());
                        }else{
                        LstP.Add(new Piquete() {
                            Stamp = dataAtual.Year.ToString() + "," + numeroSemana + "," + z.Id + "," + t.Id,
                            Utilizador = new Utilizador(),
                            });
                        }
                       LstP.Last().Valido = !LstF
    .Any(x => x.IdUtilizador == LstP.Last().IdUtilizador &&
               (calendario.GetWeekOfYear(x.DataInicio, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday) == numeroSemana ||
                calendario.GetWeekOfYear(x.DataFim, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday) == numeroSemana) && x.DataFim.DayOfWeek==DayOfWeek.Friday);
                    }
                }
                // Incrementa a data atual em 7 dias (uma semana)
                dataAtual = dataAtual.AddDays(7);
            }

            
            return LstP.Where(p => p.IdUtilizador != 0).ToList();
        }

        public void GerarPiquetes(DateTime dInicio, DateTime dFim) {
            System.Globalization.CultureInfo cultura = System.Globalization.CultureInfo.CurrentCulture;
            System.Globalization.Calendar calendario = cultura.Calendar;

            List<Piquete> LstP = ObterPiquetes(dInicio, dFim);
            List<TipoTecnico> LstT = ObterTipoTecnicos();
            List<Zona> LstZ = ObterZonas(true);

            foreach (Piquete p in LstP) {
                if (p.IdUtilizador == 0) p.Utilizador = ObterUtilizadorMaisInativo(p.Data, LstT.Where(x => x.Id == p.Tipo).First(), LstZ.Where(x => x.Id == p.Zona).First());
                p.IdUtilizador = p.Utilizador.Id;
                CriarPiquete(p);
            }
        }


        public Utilizador ObterUtilizadorMaisInativo(DateTime Data, TipoTecnico Tipo, Zona Zona)
         {
            string sqlQuery = "SELECT u.IdUtilizador, u.Zona, u.TipoTecnico FROM sys_utilizadores u LEFT JOIN (SELECT IdUtilizador, MAX(CONCAT(SUBSTRING_INDEX(SUBSTRING_INDEX(Stamp, ',', 1), ',', -1), ',', SUBSTRING_INDEX(SUBSTRING_INDEX(Stamp, ',', 2), ',', -1))) AS UltimoPiquete FROM dat_piquete GROUP BY IdUtilizador) p ON u.IdUtilizador = p.IdUtilizador WHERE (p.IdUtilizador IS NULL OR p.UltimoPiquete < CONCAT(YEAR('"+Data.ToString("yyyy-MM-dd")+"'), ',', WEEK('"+Data.ToString("yyyy-MM-dd")+"'))) and u.Zona="+Zona.Id+" and enable=1 and u.TipoTecnico="+Tipo.Id+" and u.TipoUtilizador=1 Order BY UltimoPiquete ASC LIMIT 1;";
            Utilizador u = new Utilizador();

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    u = ObterUtilizador(result[0]);
                }
            }
            return u;
         }

        public Piquete ObterPiquete(string Stamp) {
            Piquete p = new Piquete();

            string sqlQuery = "SELECT * FROM dat_piquete WHERE Stamp='"+Stamp+"';";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    p = new Piquete(){
                        Stamp = result["Stamp"],
                        IdUtilizador = result["IdUtilizador"],
                        Utilizador = ObterUtilizador(result["IdUtilizador"])
                    };
                }
            }
            return p;
        }

        public void CriarPiquete(Piquete p)
        {
            string sql = "INSERT INTO dat_piquete (Stamp, IdUtilizador) VALUES ('"+ p.Stamp +"', '"+ p.IdUtilizador +"') ON DUPLICATE KEY UPDATE IdUtilizador = VALUES(IdUtilizador);";
            using Database db = ConnectionString;
            db.Execute(sql);
        }


        public bool VerificarPiquete(DateTime d, Utilizador u) {
            int numeroSemana = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(d, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            bool res = false;

            string sqlQuery = "SELECT COUNT(*) FROM dat_piquete WHERE Stamp LIKE '"+ d.Year +","+ numeroSemana +"%' and IdUtilizador="+u.Id+";";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    res = int.Parse(result[0]) > 0;
                }
            }
            return res;
        }


        public void ApagarPiquete(Piquete p)
        {
            string sql = "DELETE FROM dat_piquete WHERE Stamp='"+p.Stamp+"';";

            using Database db = ConnectionString;
            db.Execute(sql);
        }

        #endregion

        //OUTROS
        static string GetRandomString(int length)
        {
            string s = "";
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

            using (var rng = RandomNumberGenerator.Create())
            {
                while (s.Length != length)
                {
                    byte[] oneByte = new byte[1];
                    rng.GetBytes(oneByte);
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

        //NOT WORKING
  /*      public MemoryStream DesenharFolhaObraSimples(FolhaObra fo)
        {
            var stream = new System.IO.MemoryStream();
            int x = 0;
            int y = 0;
            int width = 1024;
            int height = (840 + fo.PecasServico.Where(p => !p.Ref_Produto.Contains("SRV")).Count() * 140) * 2;

            // Cria uma nova imagem com o tamanho especificado
            Font fontHeader = new Font(SystemFonts.Collection.Get("Rubik"), 70, FontStyle.Bold);
            Font fontBody = new Font(SystemFonts.Collection.Get("Rubik"), 22, FontStyle.Bold);
            Font fontFooter = new Font(SystemFonts.Collection.Get("Rubik"), 22);

            string HeaderPagina = "Original";

            var penB = Pens.Solid(Color.Black, 5);
            var penTransparent = Pens.Solid(Color.Transparent, 5);

            var textOptions = new TextOptions(fontHeader)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var textOptionsRight = new TextOptions(fontBody)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            var textOptionsLeft = new TextOptions(fontBody)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            using (var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height))
            {
               image.Mutate(ctx =>
            {
        ctx.Clear(Color.White);

        for (int i = 0; i <= 1; i++)
        {
            

            ctx.DrawText(HeaderPagina, fontBody, Color.Black, new PointF(x + 410, y + 20));
            y += 40;
            ctx.DrawText("Nº PAT: " + fo.IdFolhaObra, fontFooter, Color.Black, new PointF(x + 410, y + 20));
            y += 40;
            ctx.DrawText("Nº Assis. Técnica: " + fo.IdAT, fontFooter, Color.Black, new PointF(x + 410, y + 20));
            y += 40;
            ctx.DrawText("Nº Marcação: " + fo.IdMarcacao, fontFooter, Color.Black, new PointF(x + 410, y + 20));
            y += 40;
            ctx.DrawText("Data: " + fo.DataServico.ToShortDateString(), fontFooter, Color.Black, new PointF(x + 410, y + 20));
            y += 60;

            var clientRect = new RectangleF(x + 10, y + 20, 400, 240);
            ctx.Draw(penB, clientRect);

            var clientDataRect = new RectangleF(x + 410, y + 20, width - 420, 240);
            ctx.Draw(penB, clientDataRect);

            var rect = new RectangleF(x + 10, y + 20, 400, 40);
            ctx.DrawText("Subic, Lda", fontFooter, Color.Black, rect);
            ctx.Draw(penTransparent, rect);

            rect = new RectangleF(x + 410, y + 20, width - 420, 40);
            ctx.DrawText("Dados do Cliente:", fontFooter, Color.Black, rect);
            ctx.DrawText("Estab: " + fo.ClienteServico.IdCliente + " Loja: " + fo.ClienteServico.IdLoja, fontFooter, Color.Black, rect);
            y += 40;

            rect = new RectangleF(x + 10, y + 20, 400, 40);
            ctx.DrawText("NIF: 515 609 013", fontFooter, Color.Black, rect, textOptionsCenter);
            y += 40;

            rect = new RectangleF(x + 410, y + 20, width - 420, 80);
            ctx.DrawText("Nome: " + fo.ClienteServico.NomeCliente, fontFooter, Color.Black, rect, textOptionsLeft);
            y += 40;

            rect = new RectangleF(x + 10, y + 20, 400, 160);
            ctx.DrawText("Morada: Rua Engenheiro Sabino Marques, 144, 4470-605 Maia", fontFooter, Color.Black, rect, textOptionsCenter);
            y += 40;

            rect = new RectangleF(x + 410, y + 20, width - 420, 80);
            ctx.DrawText("Morada: " + fo.ClienteServico.MoradaCliente, fontFooter, Color.Black, rect, textOptionsLeft);
            y += 80;

            rect = new RectangleF(x + 410, y + 20, width - 420, 40);
            ctx.DrawText("NIF: " + fo.ClienteServico.NumeroContribuinteCliente, fontFooter, Color.Black, rect, textOptionsLeft);
            y += 60;

            rect = new RectangleF(x + 10, y + 20, width - 20, 80);
            ctx.Draw(penB, rect);

            rect = new RectangleF(x + 10, y + 20, width - 20, 40);
            ctx.DrawText("Equipamento", fontFooter, Color.Black, rect, textOptionsCenter);
            y += 40;

            rect = new RectangleF(x + 10, y + 20, width - 20, 40);
            ctx.DrawText("Marca: " + fo.EquipamentoServico.MarcaEquipamento, fontFooter, Color.Black, rect, textOptionsLeft);
            ctx.DrawText("Modelo: " + fo.EquipamentoServico.ModeloEquipamento, fontFooter, Color.Black, rect, textOptionsCenter);
            ctx.DrawText("N/S: " + fo.EquipamentoServico.NumeroSerieEquipamento, fontFooter, Color.Black, rect, textOptionsRight);
            y += 60;

            rect = new RectangleF(x + 10, y + 20, width - 20, 60);
            if (fo.PecasServico.Where(p => !p.Ref_Produto.Contains("SRV")).Count() > 0)
            {
                ctx.DrawText("Peças retiradas da: " + fo.GuiaTransporteAtual, fontBody, Color.Black, rect, textOptionsCenter);
            }
            else
            {
                ctx.DrawText("Não foram retiradas nenhumas peças nesta assistência!", fontBody, Color.Black, rect, textOptionsCenter);
            }
            y += 20;

            foreach (var peca in fo.PecasServico.Where(p => !p.Ref_Produto.Contains("SRV")))
            {
                y += 60;
                rect = new RectangleF(x + 10, y + 20, width - 20, 120);
                ctx.Draw(penB, rect);

                rect = new RectangleF(x + 10, y + 20, width - 20, 40);
                ctx.DrawText("Referência: " + peca.Ref_Produto, fontFooter, Color.Black, rect, textOptionsLeft);
                y += 40;
                rect = new RectangleF(x + 10, y + 20, width - 20, 40);
                ctx.DrawText("Designação: " + peca.Designacao_Produto, fontFooter, Color.Black, rect, textOptionsLeft);
                y += 40;
                rect = new RectangleF(x + 10, y + 20, width - 20, 40);
                ctx.DrawText("Qtd: " + peca.Stock_Fisico + " " + peca.TipoUn, fontFooter, Color.Black, rect, textOptionsLeft);
            }

            y += 60;
            rect = new RectangleF(x + 10, y + 20, width - 20, 40);
            ctx.DrawText("Cliente: " + fo.ConferidoPor, fontFooter, Color.Black, rect, textOptionsLeft);
            ctx.DrawText("Técnico: " + fo.IntervencaosServico.First().NomeTecnico, fontFooter, Color.Black, rect, textOptionsRight);

            y += 150;
            var penDashed = Pens.Dash(Color.Black, 5, new float[] { 5, 5 });
            ctx.DrawLine(penDashed, new PointF(0, y), new PointF(width, y));
            y += 80;
            HeaderPagina = "Duplicado";
        }
    });

    return image
}*/

public MemoryStream DesenharTicketFO(FolhaObra fo)
        {
            var stream = new System.IO.MemoryStream();

            int x = 10;
            int y = 0;
            int width = 1024;
            int height = (840 + fo.PecasServico.Where(p => !p.Ref_Produto.Contains("SRV")).Count() * 140) * 2;

            Font fontHeader = new Font(SystemFonts.Collection.Get("Rubik"), 70, FontStyle.Bold);
            Font fontBody = new Font(SystemFonts.Collection.Get("Rubik"), 22, FontStyle.Bold);
            Font fontFooter = new Font(SystemFonts.Collection.Get("Rubik"), 22);

            string Header = "Original";

            using (var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height))
            {
                image.Mutate(imageContext =>
                {
                    imageContext.BackgroundColor(Color.White);

                    var img = Image.Load(Directory.GetCurrentDirectory() + "/wwwroot/img/ft_logo.png");
                    img.Mutate(x => x.Resize(400, 235));
                    imageContext.DrawImage(img, new Point(x, y), 1);

                    y+=10;
                    imageContext.DrawText(new TextOptions(fontBody)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width - 100, y),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = 100
                    }, Header, Color.Black);

                    y+=30;

                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(fo.GetUrl, QRCodeGenerator.ECCLevel.Q);
                    BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);

                    var qr = Image.Load(qrCode.GetGraphic(20));
                    qr.Mutate(x => x.Resize(180, 180));
                    imageContext.DrawImage(qr, new Point(width - 180, y), 1);

                    imageContext.DrawText(new TextOptions(fontBody)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(x + 400 + 30, y),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width - 100 - 400  - x
                    }, "SUBIC, LDA - FOOD-TECH", Color.Black);
                    y+=25;
                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(x + 400 + 30, y),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width - 100 - 400 - x
                    }, "R. Eng. Sabino Marques, 144", Color.Black);
                    y+=25;                    
                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(x + 400 + 30, y),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width - 100 - 400 - x
                    }, "4470-605 Maia", Color.Black);
                    y+=25;
                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(x + 400 + 30, y),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width - 100 - 400 - x
                    }, "Tel: 229 479 670", Color.Black);
                    y+=25;
                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(x + 400 + 30, y),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width - 100 - 400 - x
                    }, "NIF : 515 609 013", Color.Black);
                    y+=40;
                    imageContext.DrawLines(new Pen(Color.Black, 2), new Point(x + 400 + 30, y), new Point(width - 180, y));
                    y+=10;
                    imageContext.DrawText(new TextOptions(fontBody)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(x + 400 + 30, y),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width - 100 - 400 - x
                    }, fo.IdAT + " | " +fo.IdFolhaObra + " | " +fo.IdMarcacao, Color.Black);
                    y+=40;

                    // Define the vertices (corners) of the rectangle
                    PointF[] rectangleVertices =
                    {
                        new PointF(x, y),   // Top-left corner
                        new PointF(width/2-10, y),   // Top-right corner
                        new PointF(width/2-10, y+100),   // Bottom-right corner
                        new PointF(x, y+100)    // Bottom-left corner
                    };

                    // Draw the rectangle onto the image
                    imageContext.DrawPolygon(new Pen(Color.Black, 2), rectangleVertices);

                    // Define the vertices (corners) of the rectangle
                    rectangleVertices = new PointF[]
                    {
                        new PointF(width/2+10, y),   // Top-left corner
                        new PointF(width-25, y),   // Top-right corner
                        new PointF(width-25, y+100),   // Bottom-right corner
                        new PointF(width/2+10, y+100)    // Bottom-left corner
                    };

                    // Draw the rectangle onto the image
                    imageContext.DrawPolygon(new Pen(Color.Black, 2), rectangleVertices);
                    imageContext.DrawText(new TextOptions(fontBody)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(x+10, y),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width/2
                    }, fo.ClienteServico.NomeCliente + " (" + fo.ClienteServico.IdCliente + " / " + fo.ClienteServico.IdLoja + ")", Color.Black);
                    y+=25;
                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(x+10, y),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width/2
                    }, fo.ClienteServico.MoradaCliente, Color.Black);
                    y+=50;
                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(x+10, y),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width/2
                    }, "NIF: " + fo.ClienteServico.NumeroContribuinteCliente, Color.Black);
                    y+=25;
                    y-=100;
                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width/2+25, y),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width/2-30
                    }, fo.EquipamentoServico.DescricaoEquipamento, Color.Black);
                    y+=25;
                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width/2+25, y),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width/2-30
                    }, "Marca: " + fo.EquipamentoServico.MarcaEquipamento, Color.Black);
                    y+=25;
                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width/2+25, y),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width/2-30
                    }, "Modelo: " + fo.EquipamentoServico.ModeloEquipamento, Color.Black);
                    y+=25;
                    imageContext.DrawText(new TextOptions(fontBody)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width/2+25, y),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width/2-30
                    }, "N/S: " + fo.EquipamentoServico.NumeroSerieEquipamento, Color.Black);
                    y+=25;
                });

                // render onto an Image
                image.SaveAsBmp(stream);
                stream.Position = 0;
            }

            return stream;
        }

        public MemoryStream DesenharEtiquetaMarcacao(Marcacao marcacao)
        {
            var stream = new System.IO.MemoryStream();

            int x = 10;
            int y = 0;
            int width = 1024;
            int height = 641;

            Font fontHeader = new Font(SystemFonts.Collection.Get("Rubik"), 110, FontStyle.Bold);
            Font fontBody = new Font(SystemFonts.Collection.Get("Rubik"), 60);
            Font fontFooter = new Font(SystemFonts.Collection.Get("Rubik"), 40);

            using (var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height))
            {
                image.Mutate(imageContext =>
                {
                    imageContext.BackgroundColor(Color.White);

                    var img = Image.Load(Directory.GetCurrentDirectory() + "/wwwroot/img/logo_website.png");
                    img.Mutate(x => x.Resize(700, 158));
                    imageContext.DrawImage(img, new Point(x, y), 1);

                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(marcacao.GetUrl.ToString(), QRCodeGenerator.ECCLevel.Q);
                    BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);

                    var qr = Image.Load(qrCode.GetGraphic(20));
                    qr.Mutate(x => x.Resize(180, 180));
                    imageContext.DrawImage(qr, new Point(width - 180, 0), 1);

                    y += 170;

                    imageContext.DrawText(new TextOptions(fontBody)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width / 2, y),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width
                    }, marcacao.Cliente.NomeCliente + "\r\n(" + marcacao.EstadoMarcacaoDesc + ")", Color.Black);

                    y += 290;
                    imageContext.DrawText(new TextOptions(fontHeader)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width / 2, y),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }, marcacao.IdMarcacao.ToString(), Color.Black);

                    var r = new RectangularPolygon(width / 2 / 2, y, width / 2, 120);
                    imageContext.Draw(Color.FromRgb(54, 100, 157), 6, ApplyRoundCorners(r, 50));

                    y += 130;
                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width / 2, y),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }, "geral@food-tech.pt", Color.Black);
                });

                // render onto an Image
                image.SaveAsBmp(stream);
                stream.Position = 0;
            }

            return stream;
        }

        public MemoryStream DesenharEtiquetaFolhaObra(FolhaObra fo)
        {
            var stream = new System.IO.MemoryStream();

            int x = 10;
            int y = 0;
            int width = 1024;
            int height = 641;

            Font fontHeader = new Font(SystemFonts.Collection.Get("Rubik"), 110, FontStyle.Bold);
            Font fontBody = new Font(SystemFonts.Collection.Get("Rubik"), 60);
            Font fontFooter = new Font(SystemFonts.Collection.Get("Rubik"), 40);

            using (var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height))
            {
                image.Mutate(imageContext =>
                {
                    imageContext.BackgroundColor(Color.White);

                    var img = Image.Load(Directory.GetCurrentDirectory() + "/wwwroot/img/logo_website.png");
                    img.Mutate(x => x.Resize(700, 158));
                    imageContext.DrawImage(img, new Point(x, y), 1);

                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(fo.GetUrl.ToString(), QRCodeGenerator.ECCLevel.Q);
                    BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);

                    var qr = Image.Load(qrCode.GetGraphic(20));
                    qr.Mutate(x => x.Resize(180, 180));
                    imageContext.DrawImage(qr, new Point(width - 180, 0), 1);

                    y += 170;

                    imageContext.DrawText(new TextOptions(fontBody)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width / 2, y),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width
                    }, fo.ClienteServico.NomeCliente + "\r\n " + fo.EquipamentoServico.MarcaEquipamento.Trim() + " " + fo.EquipamentoServico.ModeloEquipamento.Trim() + " (" + fo.EquipamentoServico.NumeroSerieEquipamento + ")", Color.Black);

                    y += 290;
                    imageContext.DrawText(new TextOptions(fontHeader)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width / 2, y),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }, fo.IdFolhaObra.ToString(), Color.Black);

                    var r = new RectangularPolygon(width / 2 / 2, y, width / 2, 120);
                    imageContext.Draw(Color.FromRgb(54, 100, 157), 6, ApplyRoundCorners(r, 50));

                    y += 130;
                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width / 2, y),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }, "geral@food-tech.pt", Color.Black);
                });

                // render onto an Image
                image.SaveAsBmp(stream);
                stream.Position = 0;
            }

            return stream;
        }

        public MemoryStream DesenharEtiquetaPicking(Picking pi)
        {
            var stream = new System.IO.MemoryStream();

            int x = 10;
            int y = 50;
            int width = 1024;
            int height = 641;

            Font fontHeader = new Font(SystemFonts.Collection.Get("Rubik"), 110, FontStyle.Bold);
            Font fontBody = new Font(SystemFonts.Collection.Get("Rubik"), 50);
            Font fontFooter = new Font(SystemFonts.Collection.Get("Rubik"), 40);

            using (var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height))
            {
                image.Mutate(imageContext =>
                {
                    imageContext.BackgroundColor(Color.White);

                    var img = Image.Load(Directory.GetCurrentDirectory() + "/wwwroot/img/logo_website.png");
                    img.Mutate(x => x.Resize(700, 158));
                    imageContext.DrawImage(img, new Point(x, y), 1);

                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(pi.IdPicking.ToString(), QRCodeGenerator.ECCLevel.Q);
                    BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);

                    var qr = Image.Load(qrCode.GetGraphic(20));
                    qr.Mutate(x => x.Resize(180, 180));
                    imageContext.DrawImage(qr, new Point(width - 180, 0), 1);

                    y += 170;

                    imageContext.DrawText(new TextOptions(fontBody)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width / 2, y),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width
                    }, "Cliente: " + pi.Encomenda.NomeCliente.Trim() + "\r\nData: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), Color.Black);

                    y += 240;
                    imageContext.DrawText(new TextOptions(fontHeader)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width / 2, y),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }, pi.Encomenda.Id.ToString(), Color.Black);

                    var r = new RectangularPolygon(10, y, width - 20, 120);
                    imageContext.Draw(Color.FromRgb(54, 100, 157), 6, ApplyRoundCorners(r, 50));

                    y += 130;
                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width / 2, y),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }, "pecas@food-tech.pt", Color.Black);
                });

                // render onto an Image
                image.SaveAsBmp(stream);
                stream.Position = 0;
            }

            return stream;
        }

        public MemoryStream DesenharDossier(Dossier d)
        {
            var stream = new System.IO.MemoryStream();
            if (string.IsNullOrEmpty(d.AtCode)) d.AtCode = string.Empty;

            int x = 50;
            int y = 50;
            int width = 2480; // A4 width at 300 DPI
            int height = 3508; // A4 height at 300 DPI

            Font fontHeader = new Font(SystemFonts.Collection.Get("Rubik"), 110, FontStyle.Bold);
            Font fontCabecalho = new Font(SystemFonts.Collection.Get("Rubik"), 40, FontStyle.Bold);
            Font fontBody = new Font(SystemFonts.Collection.Get("Rubik"), 40);
            Font fontFooter = new Font(SystemFonts.Collection.Get("Rubik"), 30);

        using (var image = new Image<Rgba32>(width, height))
        {
            image.Mutate(imageContext =>
            {
                imageContext.BackgroundColor(Color.White);

                var img = Image.Load(Directory.GetCurrentDirectory() + "/wwwroot/img/logo_website.png"); // Adicione o caminho correto para o arquivo de imagem
                img.Mutate(x => x.Resize(700, 158));
                imageContext.DrawImage(img, new Point(x, x + 75), 1);

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(d.Iniciais == "TV" ? d.GetUrlViagem : d.GetUrl, QRCodeGenerator.ECCLevel.Q);
                BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);

                var qr = Image.Load(qrCode.GetGraphic(20));
                qr.Mutate(x => x.Resize(300, 300));
                imageContext.DrawImage(qr, new Point(width - 400, y), 1);

                // Informações da empresa
                imageContext.DrawText(new TextOptions(fontHeader)
                {
                    TextAlignment = TextAlignment.Center,
                    Origin = new System.Numerics.Vector2(width/2 + 200, y),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    WordBreaking = WordBreaking.Normal,
                    WrappingLength = width - 700 - 400
                }, d.NomeDossier.ToUpper(), Color.Red);

                y+=300;

                // Desenhar linha separadora
                imageContext.DrawLines(Color.Black, 5, new PointF[] { new PointF(x, y), new PointF(width - x, y) });

                y += 50;

                // Informações da empresa
                imageContext.DrawText(new TextOptions(fontCabecalho)
                {
                    TextAlignment = TextAlignment.Start,
                    Origin = new System.Numerics.Vector2(x, y),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    WordBreaking = WordBreaking.Normal,
                    WrappingLength = width
                }, $"{"SUBIC Lda"}", Color.Black);
                
                y +=50;

                imageContext.DrawText(new TextOptions(fontBody)
                {
                    TextAlignment = TextAlignment.Start,
                    Origin = new System.Numerics.Vector2(x, y),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    WordBreaking = WordBreaking.Normal,
                    WrappingLength = width
                }, $"{"R. Eng. Sabino Marques, 144 4475-605 Maia"}\n{"+351 229 479 670"}\n{"pecas@food-tech.pt"}", Color.Black);

                y+=150;
                 // Informações do técnico
                imageContext.DrawText(new TextOptions(fontBody)
                {
                    TextAlignment = TextAlignment.Start,
                    Origin = new System.Numerics.Vector2(x, y),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    WordBreaking = WordBreaking.Normal,
                    WrappingLength = width
                }, $"Utilizador: {d.Tecnico.NomeCompleto}", Color.Black);
                
                y-=200;
                x = width-(width/2);

                // Informações da fatura
                imageContext.DrawText(new TextOptions(fontBody)
                {
                    TextAlignment = TextAlignment.Start,
                    Origin = new System.Numerics.Vector2(x, y),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    WordBreaking = WordBreaking.Normal,
                    WrappingLength = width
                }, $"Dossier Nº: {d.Iniciais}/{d.IdDossier}\nData: {d.DataCriacao:dd/MM/yyyy}\nCódigo AT: {d.AtCode}", Color.Black);
                
                y += 160;
                // Informações do cliente
                imageContext.DrawText(new TextOptions(fontBody)
                {
                    TextAlignment = TextAlignment.Start,
                    Origin = new System.Numerics.Vector2(x, y),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    WordBreaking = WordBreaking.Normal,
                    WrappingLength = width-(width/2)
                }, $"{d.Cliente.NomeCliente}({d.Cliente.IdCliente}/{d.Cliente.IdLoja})", Color.Black);

                y += 150;
                x = 50;

                // Desenhar linha separadora
                imageContext.DrawLines(Color.Black, 5, new PointF[] { new PointF(x, y), new PointF(width - x, y) });

                y+= 50;

                // Desenhar tabela
                int tableX = x;
                int tableY = y;
                int tableWidth = width - 2 * x;
                int rowHeight = 100;
                int headerHeight = 80;

                // Largura das colunas
                int leftColWidth = tableWidth / 6;
                int middleColWidth = tableWidth - 800;
                int rightColWidth = tableWidth - leftColWidth - middleColWidth;

                // Desenhar cabeçalho da tabela
                string[] tableHeaders = { "Referência", "Designação", "Quantidade" };
                int[] colWidths = { leftColWidth, middleColWidth, rightColWidth };
                for (int i = 0; i < tableHeaders.Length; i++)
                {
                    imageContext.DrawText(new TextOptions(fontCabecalho)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(tableX + (i == 0 ? leftColWidth / 2 : (i == 1 ? leftColWidth + middleColWidth / 2 : leftColWidth + middleColWidth + rightColWidth / 2)), tableY + headerHeight / 2),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = colWidths[i]
                    }, tableHeaders[i], Color.Black);
                }

                tableY += headerHeight;


                foreach (var l in d.Linhas)
                {
                    int i = 0;

                    imageContext.DrawText(new TextOptions(fontBody)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(tableX + (i == 0 ? leftColWidth / 2 : (i == 1 ? leftColWidth + middleColWidth / 2 : leftColWidth + middleColWidth + rightColWidth / 2)), tableY + rowHeight / 2),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = colWidths[i]
                    }, l.Referencia, Color.Black);
                    i++;
                    imageContext.DrawText(new TextOptions(fontBody)
                        {
                            TextAlignment = TextAlignment.Center,
                            Origin = new System.Numerics.Vector2(tableX + (i == 0 ? leftColWidth / 2 : (i == 1 ? leftColWidth + middleColWidth / 2 : leftColWidth + middleColWidth + rightColWidth / 2)), tableY + rowHeight / 2),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            WordBreaking = WordBreaking.Normal,
                            WrappingLength = colWidths[i]
                        }, l.Designacao, Color.Black);
                    i++;
                    imageContext.DrawText(new TextOptions(fontBody)
                        {
                            TextAlignment = TextAlignment.Center,
                            Origin = new System.Numerics.Vector2(tableX + (i == 0 ? leftColWidth / 2 : (i == 1 ? leftColWidth + middleColWidth / 2 : leftColWidth + middleColWidth + rightColWidth / 2)), tableY + rowHeight / 2),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            WordBreaking = WordBreaking.Normal,
                            WrappingLength = colWidths[i]
                        }, l.Quantidade == 0 ? "" : l.Quantidade.ToString(), Color.Black);

                    tableY += rowHeight;
                }

                // Desenhar bordas da tabela
                // Bordas horizontais
                imageContext.DrawLines(Color.Black, 2, new PointF[] {
                    new PointF(tableX, y),
                    new PointF(tableX + tableWidth, y)
                });

                imageContext.DrawLines(Color.Black, 2, new PointF[] {
                    new PointF(tableX, y + rowHeight),
                    new PointF(tableX + tableWidth, y + rowHeight)
                });

                imageContext.DrawLines(Color.Black, 2, new PointF[] {
                    new PointF(tableX, tableY),
                    new PointF(tableX + tableWidth, tableY)
                });

                // Bordas verticais
                int xOffset = 0;
                foreach (var width in colWidths)
                {
                    imageContext.DrawLines(Color.Black, 2, new PointF[] {
                        new PointF(tableX + xOffset, y),
                        new PointF(tableX + xOffset, tableY)
                    });
                    xOffset += width;
                }

                imageContext.DrawLines(Color.Black, 2, new PointF[] {
                        new PointF(tableX + xOffset, y),
                        new PointF(tableX + xOffset, tableY)
                    });

                y = tableY + 20; // Ajuste y após a tabela

                y = height - 200;
                imageContext.DrawText(new TextOptions(fontFooter)
                {
                    TextAlignment = TextAlignment.Center,
                    Origin = new System.Numerics.Vector2(width / 2, y),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    WordBreaking = WordBreaking.Normal,
                    WrappingLength = width
                }, "JKSProds - Software", Color.Black);
            });

                // render onto an Image
                image.SaveAsBmp(stream);
                stream.Position = 0;
            }

            return stream;
        }

        public MemoryStream DesenharEtiquetaPecaGarantia(Dossier d, Produto p)
        {
            var stream = new System.IO.MemoryStream();

            int x = 10;
            int y = 0;
            int width = 1024;
            int height = 641;

            Font fontHeader = new Font(SystemFonts.Collection.Get("Rubik"), 80, FontStyle.Bold);
            Font fontBody = new Font(SystemFonts.Collection.Get("Rubik"), 50);
            Font fontFooter = new Font(SystemFonts.Collection.Get("Rubik"), 40);

            using (var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height))
            {
                image.Mutate(imageContext =>
                {
                    imageContext.BackgroundColor(Color.White);

                    var img = Image.Load(Directory.GetCurrentDirectory() + "/wwwroot/img/logo_website.png");
                    img.Mutate(x => x.Resize(700, 158));
                    imageContext.DrawImage(img, new Point(x, y + 20), 1);

                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(d.Marcacao.GetUrl, QRCodeGenerator.ECCLevel.Q);
                    BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);

                    var qr = Image.Load(qrCode.GetGraphic(20));
                    qr.Mutate(x => x.Resize(180, 180));
                    imageContext.DrawImage(qr, new Point(width - 200, 0), 1);

                    y += 170;

                    imageContext.DrawText(new TextOptions(fontBody)
                    {
                        Origin = new System.Numerics.Vector2(width / 2, y),
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width,
                    }, p.MotivoGarantia + "\r\n" + p.Ref_Produto + "\r\n" + p.Designacao_Produto + "\r\n", Color.Black);

                    y += 310;
                    imageContext.DrawText(new TextOptions(fontHeader)
                    {
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Origin = new System.Numerics.Vector2(width / 2, y)
                    },"N/S: " + d.FolhaObra.EquipamentoServico.NumeroSerieEquipamento, Color.Black);

                    var r = new RectangularPolygon(10, y, width - 20, 90);
                    imageContext.Draw(Color.FromRgb(54, 100, 157), 6, ApplyRoundCorners(r, 50));

                    y += 90;
                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width / 2, y),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }, "RMA Fornecedor Nº " + d.IdDossier.ToString() + " | Marcação Nº " + d.Marcacao.IdMarcacao, Color.Black);

                });

                // render onto an Image
                image.SaveAsBmp(stream);
                stream.Position = 0;
            }

            return stream;
        }


        public MemoryStream DesenharEtiquetaProduto(Produto p)
        {
            var stream = new System.IO.MemoryStream();

            int x = 10;
            int y = 0;
            int width = 1024;
            int height = 641;

            Font fontHeader = new Font(SystemFonts.Collection.Get("Rubik"), 115, FontStyle.Bold);
            Font fontBody = new Font(SystemFonts.Collection.Get("Rubik"), 60);
            Font fontFooter = new Font(SystemFonts.Collection.Get("Rubik"), 40);

            using (var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height))
            {
                image.Mutate(imageContext =>
                {
                    imageContext.BackgroundColor(Color.White);

                    var img = Image.Load(Directory.GetCurrentDirectory() + "/wwwroot/img/logo_website.png");
                    img.Mutate(x => x.Resize(700, 158));
                    imageContext.DrawImage(img, new Point(x, y + 20), 1);

                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(p.Ref_Produto, QRCodeGenerator.ECCLevel.Q);
                    BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);

                    var qr = Image.Load(qrCode.GetGraphic(20));
                    qr.Mutate(x => x.Resize(200, 200));
                    imageContext.DrawImage(qr, new Point(width - 200, 0), 1);

                    y += 170;

                    imageContext.DrawText(new TextOptions(fontBody)
                    {
                        Origin = new System.Numerics.Vector2(width / 2, y),
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width,
                    }, p.Designacao_Produto.Trim(), Color.Black);

                    y += 240;
                    imageContext.DrawText(new TextOptions(fontHeader)
                    {
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Origin = new System.Numerics.Vector2(width / 2, y)
                    }, p.Ref_Produto, Color.Black);

                    var r = new RectangularPolygon(10, y, width - 20, 130);
                    imageContext.Draw(Color.FromRgb(54, 100, 157), 6, ApplyRoundCorners(r, 50));

                    y += 140;
                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width / 2, y),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }, "pecas@food-tech.pt", Color.Black);

                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        Origin = new System.Numerics.Vector2(width - 100, y),
                        HorizontalAlignment = HorizontalAlignment.Right
                    }, p.Pos_Stock, Color.Black);

                });

                // render onto an Image
                image.SaveAsBmp(stream);
                stream.Position = 0;
            }

            return stream;
        }

        public MemoryStream DesenharEtiquetaProdutoPequena(Produto p)
        {
            var stream = new System.IO.MemoryStream();

            int x = 10;
            int y = 0;
            int width = 1024;
            int height = 641;

            Font fontHeader = new Font(SystemFonts.Collection.Get("Rubik"), 70, FontStyle.Bold);
            Font fontBody = new Font(SystemFonts.Collection.Get("Rubik"), 50);
            Font fontFooter = new Font(SystemFonts.Collection.Get("Rubik"), 20);

            using (var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height))
            {
                image.Mutate(imageContext =>
                {
                    imageContext.BackgroundColor(Color.White);

                    imageContext.DrawText(new TextOptions(fontBody)
                    {
                        Origin = new System.Numerics.Vector2(width / 2, y),
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width,
                    }, p.Designacao_Produto.Trim(), Color.Black);

                    y += 150;

                    var img = Image.Load(Directory.GetCurrentDirectory() + "/wwwroot/img/ft_logo.png");
                    img.Mutate(x => x.Resize(173, 100));
                    imageContext.DrawImage(img, new Point(x, y), 1);

                    imageContext.DrawText(new TextOptions(fontHeader)
                    {
                        Origin = new System.Numerics.Vector2(width / 2, y),
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        WordBreaking = WordBreaking.Normal,
                        WrappingLength = width,
                    }, p.Ref_Produto.Trim(), Color.Black);


                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(p.Ref_Produto, QRCodeGenerator.ECCLevel.Q);
                    BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);

                    var qr = Image.Load(qrCode.GetGraphic(20));
                    qr.Mutate(x => x.Resize(150, 150));
                    imageContext.DrawImage(qr, new Point(width - 150, y - 50), 1);

                    y += 90;

                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        TextAlignment = TextAlignment.Center,
                        Origin = new System.Numerics.Vector2(width / 2, y),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }, "pecas@food-tech.pt", Color.Black);

                    imageContext.DrawText(new TextOptions(fontFooter)
                    {
                        Origin = new System.Numerics.Vector2(width - 300, y),
                        HorizontalAlignment = HorizontalAlignment.Right
                    }, p.Pos_Stock, Color.Black);

                });

                // render onto an Image
                image.SaveAsBmp(stream);
                stream.Position = 0;
            }

            return stream;
        }

        public MemoryStream DesenharEtiquetaMultipla(Produto p)
        {
            var stream = new System.IO.MemoryStream();

            int x = 100;
            int y = 0;
            int width = 1024;
            int height = 641;

            Font fontHeader = new Font(SystemFonts.Collection.Get("Rubik"), 50, FontStyle.Bold);
            Font fontBody = new Font(SystemFonts.Collection.Get("Rubik"), 80);
            Font fontFooter = new Font(SystemFonts.Collection.Get("Rubik"), 30);

            using (var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height))
            {
                image.Mutate(imageContext =>
                {
                    imageContext.BackgroundColor(Color.White);
                    for (int i = 0; i < 4; i++)
                    {
                        y += 50;
                        for (int j = 0; j < 4; j++)
                        {
                            QRCodeGenerator qrGenerator = new QRCodeGenerator();
                            QRCodeData qrCodeData = qrGenerator.CreateQrCode(p.Ref_Produto, QRCodeGenerator.ECCLevel.Q);
                            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);

                            var qr = Image.Load(qrCode.GetGraphic(20));
                            qr.Mutate(x => x.Resize(100, 100));
                            imageContext.DrawImage(qr, new Point(x + 300, y), 1);

                            x += 100;
                            imageContext.DrawText(new TextOptions(fontHeader)
                            {
                                Origin = new System.Numerics.Vector2(x, y),
                                WrappingLength = width / 2,
                                HorizontalAlignment = HorizontalAlignment.Center
                            }, p.Ref_Produto, Color.Black);

                            imageContext.DrawText(new TextOptions(fontFooter)
                            {
                                TextAlignment = TextAlignment.Center,
                                Origin = new System.Numerics.Vector2(x, y + 60),
                                WrappingLength = width / 2 - 100,
                                HorizontalAlignment = HorizontalAlignment.Center
                            }, "pecas@food-tech.pt", Color.Black);

                            x = width / 2 + 100;

                        }
                        x = 100;
                        y += 100;
                    }
                });

                // render onto an Image
                image.SaveAsBmp(stream);
                stream.Position = 0;
            }

            return stream;
        }

        private static IPath ApplyRoundCorners(RectangularPolygon rectangularPolygon, float radius)
        {
            var squareSize = new SizeF(radius, radius);
            var ellipseSize = new SizeF(radius * 2, radius * 2);
            var offsets = new[]
            {
        (0, 0),
        (1, 0),
        (0, 1),
        (1, 1),
    };
            var holes = offsets.Select(
                offset =>
                {
                    var squarePos = new PointF(
                        offset.Item1 == 0 ? rectangularPolygon.Left : rectangularPolygon.Right - radius,
                        offset.Item2 == 0 ? rectangularPolygon.Top : rectangularPolygon.Bottom - radius
                    );
                    var circlePos = new PointF(
                        offset.Item1 == 0 ? rectangularPolygon.Left + radius : rectangularPolygon.Right - radius,
                        offset.Item2 == 0 ? rectangularPolygon.Top + radius : rectangularPolygon.Bottom - radius
                    );
                    return new RectangularPolygon(squarePos, squareSize)
                        .Clip(new EllipsePolygon(circlePos, ellipseSize));
                }
            );
            return rectangularPolygon.Clip(holes);
        }
        public MemoryStream PreencherFormularioFolhaObra(FolhaObra folhaobra)
        {
            string pdfTemplate = AppDomain.CurrentDomain.BaseDirectory + (folhaobra.ClienteServico.IdCliente != 355 ? "FT_FolhaObra.pdf" : "FT_FolhaObra_Asgo.pdf" );
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
            pdfFormFields.SetField("Tecnico", folhaobra.Utilizador.NomeCompleto);
            pdfFormFields.SetField("Cliente", folhaobra.ConferidoPor);
            pdfFormFields.SetField("Guia", folhaobra.GuiaTransporteAtual);
            pdfFormFields.SetField("Obs", (folhaobra.AssistenciaRemota ? "REMOTO " : "") + (folhaobra.Piquete ? "PIQUETE " : "") + (folhaobra.Instalação ? "INSTALAÇÃO " : "") + (folhaobra.Contrato ? "CONTRATO " : ""));

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

        public MemoryStream PreencherFormularioCertificado(FolhaObra fo)
        {
            string pdfTemplate = AppDomain.CurrentDomain.BaseDirectory + "FT_Certificado.pdf";
            var outputPdfStream = new MemoryStream();
            PdfReader pdfReader = new PdfReader(pdfTemplate);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, outputPdfStream) { FormFlattening = true, FreeTextFlattening = true };
            AcroFields pdfFormFields = pdfStamper.AcroFields;

            foreach (var l in fo.CheckList.Split(";"))
            {
                if (!string.IsNullOrEmpty(l)) pdfFormFields.SetField("txtChecklist", pdfFormFields.GetField("txtChecklist") + (l.Split("|").Last() == "1" ? "OK" : "NOK") + " - " + l.Split("|").First() + "\r\n");
            }

            pdfFormFields.SetField("txtEquipamento", fo.EquipamentoServico.MarcaEquipamento + " " + fo.EquipamentoServico.ModeloEquipamento + " (" + fo.EquipamentoServico.NumeroSerieEquipamento.ToString() + ")");
            pdfFormFields.SetField("txtData", fo.DataServico.ToShortDateString());
            pdfFormFields.SetField("txtTecnico", fo.Utilizador.NomeCompleto);
            pdfFormFields.SetField("txtRelatorio", fo.RelatorioServico);

            pdfStamper.FormFlattening = true;
            pdfStamper.SetFullCompression();
            pdfStamper.Close();

            return outputPdfStream;

        }


        public MemoryStream MemoryStreamToPDF(MemoryStream stream, int w, int h)
        {
            var ms = new MemoryStream();

            PdfSharpCore.Pdf.PdfDocument doc = new PdfSharpCore.Pdf.PdfDocument();
            PdfSharpCore.Pdf.PdfPage page = new PdfSharpCore.Pdf.PdfPage
            {
                Width = w,
                Height = h
            };

            XImage img = XImage.FromStream(() => stream);
            img.Interpolate = false;

            doc.Pages.Add(page);

            XGraphics xgr = XGraphics.FromPdfPage(doc.Pages[0]);
            XRect box = new XRect(0, 0, w, h);
            xgr.DrawImage(img, box);

            doc.Save(ms, false);

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

            string sql = "INSERT INTO dat_codigos (CodigoValidacao, EstadoCodigo, ObsInternas, Observacoes, Utilizador, ValidadeCodigo) VALUES ('" + c.Stamp + "', '" + c.Estado + "', '" + c.ObsInternas + "','" + c.Obs + "', '" + c.utilizador.Id + "', '" + c.ValidadeCodigo.ToString("yyyy-MM-dd HH:mm:ss") + "') ;";

            Database db = ConnectionString;

            db.Execute(sql);
            db.Connection.Close();
        }
        public void AtualizarCodigo(string stamp, int estado, int utilizador)
        {

            string sql = "UPDATE dat_codigos set EstadoCodigo=" + estado + ", ValidadoPor=" + utilizador + " WHERE CodigoValidacao='" + stamp + "';";

            Database db = ConnectionString;

            db.Execute(sql);
            db.Connection.Close();
        }

        public Codigo ObterCodigo(string stamp)
        {
            Codigo c = new Codigo();
            string sqlQuery = "SELECT * FROM dat_codigos where CodigoValidacao='" + stamp + "';";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    c = new Codigo()
                    {
                        Stamp = result["CodigoValidacao"],
                        Estado = result["EstadoCodigo"],
                        ObsInternas = result["ObsInternas"],
                        Obs = result["Observacoes"],
                        ValidadeCodigo = result["ValidadeCodigo"],
                        utilizador = this.ObterUtilizador(int.Parse(result["Utilizador"])),
                        ValidadoPor = this.ObterUtilizador(int.Parse(result["ValidadoPor"]))
                    };
                }
            }
            return c;
        }


        public List<CategoriaResolucao> ObterCategoriasResolucao()
        {
            List<CategoriaResolucao> c = new List<CategoriaResolucao>();
            string sqlQuery = "SELECT a.Id1, a.Texto, b.Id2, b.Texto, c.Id3, c.Texto, c.Relatorio FROM dat_categoria_resolucao a join dat_categoria_resolucao b on b.Id1=a.Id1 && b.Id2 <> 0 join dat_categoria_resolucao c on c.Id1=a.Id1 && c.Id2=b.Id2 && c.Id3 <> 0 group by a.Id1, b.Id2, c.Id3;";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    c.Add(new CategoriaResolucao()
                    {
                        Id_1 = result[0],
                        Categoria1 = result[1],
                        Id_2 = result[2],
                        Categoria2 = result[3],
                        Id_3 = result[4],
                        Categoria3 = result[5],
                        ExemploRelatorio = result[6]
                    });
                }
            }
            return c;
        }

        public CategoriaResolucao ObterCategoriaResolucao(CategoriaResolucao cat)
        {
            List<CategoriaResolucao> c = new List<CategoriaResolucao>();
            string sqlQuery = "SELECT a.Id1, a.Texto, b.Id2, b.Texto, c.Id3, c.Texto, c.Relatorio FROM dat_categoria_resolucao a join dat_categoria_resolucao b on b.Id1=a.Id1 && b.Id2 <> 0 join dat_categoria_resolucao c on c.Id1=a.Id1 && c.Id2=b.Id2 && c.Id3 <> 0 where a.Id1="+cat.Id_1+" and b.Id2="+cat.Id_2+" and c.Id3="+cat.Id_3+" group by a.Id1, b.Id2, c.Id3;";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    c.Add(new CategoriaResolucao()
                    {
                        Id_1 = result[0],
                        Categoria1 = result[1],
                        Id_2 = result[2],
                        Categoria2 = result[3],
                        Id_3 = result[4],
                        Categoria3 = result[5],
                        ExemploRelatorio = result[6]
                    });
                }
            }
            return c.DefaultIfEmpty(new CategoriaResolucao()).First();
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