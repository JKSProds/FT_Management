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
                        Dev = result["dev"],
                        Dashboard = result["dashboard"],
                        TipoMapa = result["TipoMapa"],
                        CorCalendario = result["CorCalendario"],
                        Pin = result["PinUtilizador"],
                        Iniciais = result["IniciaisUtilizador"],
                        DataNascimento = result["DataNascimento"],
                        IdArmazem = result["IdArmazem"],
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
                        Dashboard = result["dashboard"],
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
                        Dev = result["dev"],
                        Dashboard = result["dashboard"],
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
                        FaceRec = result["FaceRec"],
                        NotificacaoAutomatica = result["NotificacaoAutomatica"],
                        SecondFactorAuthStamp = result["SecondFactorAuthStamp"],
                        StampMoradaCargaDescarga = result["StampMoradaCargaDescarga"]
                    };
                }
            }
            return utilizador;
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
            string sql = "INSERT INTO sys_utilizadores (IdUtilizador, NomeUtilizador, Password, FaceRec, PinUtilizador, NomeCompleto, TipoUtilizador, EmailUtilizador, admin, enable, acessos, dev, dashboard, IdPHC, IdArmazem, IniciaisUtilizador, CorCalendario, TipoMapa, DataNascimento, TelemovelUtilizador, ImgUtilizador, TipoTecnico, Zona, ChatToken, NotificacaoAutomatica, SecondFactorAuthStamp) VALUES ";

            sql += ("('" + utilizador.Id + "', '" + utilizador.NomeUtilizador + "', '" + utilizador.Password + "',  '" + utilizador.FaceRec + "', '" + utilizador.Pin + "', '" + utilizador.NomeCompleto + "', '" + utilizador.TipoUtilizador + "', '" + utilizador.EmailUtilizador + "', '" + (utilizador.Admin ? "1" : "0") + "', '" + (utilizador.Enable ? "1" : "0") + "', '" + (utilizador.Acessos ? "1" : "0") + "', '" + (utilizador.Dev ? "1" : "0") + "','" + (utilizador.Dashboard ? "1" : "0") + "', '" + utilizador.IdPHC + "', '" + utilizador.IdArmazem + "', '" + utilizador.Iniciais + "', '" + utilizador.CorCalendario + "', " + utilizador.TipoMapa + ", '" + utilizador.DataNascimento.ToString("yyyy-MM-dd") + "', '" + utilizador.ObterTelemovelFormatado(true) + "', '" + utilizador.ImgUtilizador + "', '" + utilizador.TipoTecnico + "', '" + utilizador.Zona + "', '" + utilizador.ChatToken + "', '" + utilizador.NotificacaoAutomatica + "', '" + utilizador.SecondFactorAuthStamp + "') ");

            sql += " ON DUPLICATE KEY UPDATE Password = VALUES(Password), FaceRec = VALUES(FaceRec), PinUtilizador = VALUES(PinUtilizador), NomeCompleto = VALUES(NomeCompleto), TipoUtilizador = VALUES(TipoUtilizador), EmailUtilizador = VALUES(EmailUtilizador), admin = VALUES(admin), enable = VALUES(enable), acessos = VALUES(acessos), dev = VALUES(dev), dashboard = VALUES(dashboard), IdPHC = VALUES(IdPHC), IdArmazem = VALUES(IdArmazem), IniciaisUtilizador = VALUES(IniciaisUtilizador), CorCalendario = VALUES(CorCalendario), TipoMapa = VALUES(TipoMapa), DataNascimento = VALUES(DataNascimento), TelemovelUtilizador = VALUES(TelemovelUtilizador), ImgUtilizador = VALUES(ImgUtilizador), TipoTecnico = VALUES(TipoTecnico), Zona = VALUES(Zona), ChatToken = VALUES(ChatToken), NotificacaoAutomatica = VALUES(NotificacaoAutomatica), SecondFactorAuthStamp = VALUES(SecondFactorAuthStamp);";

            using (Database db = ConnectionString)
            {
                db.Execute(sql);
            }
        }
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
                        App = result["App"] == 1
                    });
                }
            }

            return LstAcessos;

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
                LstRegistroAcessos.Last().Utilizador.Ferias = false;
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

        public bool CriarRegistoBancoHoras(RegistroAcessos r, Utilizador u, int NHoras)
        {
                bool res = false;
                string obs = "Criado um registo de banco de horas pelo utilizador " + u.NomeCompleto + " do utilizador " + r.Utilizador.NomeCompleto + " de " + (r.TipoHorasExtra > 0 ? " Horas Extraordinarias " : " Falta ") + ". " + NHoras + " Hora(s).";
                
                string sql = "INSERT INTO dat_banco_horas (Stamp,IdUtilizador,Tipo,Horas,IdAcessos,Observacoes,DataAcessos) VALUES ";

                sql += "('"+ DateTime.Now.Ticks.ToString() +"', '"+ r.Utilizador.Id +"', '"+ (r.TipoHorasExtra > 0 ? 1 : 2) +"', '"+ NHoras.ToString() +"', '" + r.E1.Id + ", " + r.S1.Id + ", " + r.E2.Id + ", " + r.S2.Id +"', '"+ obs +"', '"+ r.Data +"');";

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

        public byte[] GerarMapaPresencas(DateTime dInicio, DateTime dFim)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new ExcelPackage(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "Asgo_Presencas.xlsx"));
            ExcelWorksheet workSheet = package.Workbook.Worksheets.First();
            int totalRows = workSheet.Dimension.Rows;

            DateTime dataAtual = dInicio;
            List<Utilizador> LstUtilizadores = ObterListaUtilizadores(true, false).Where(u => u.Acessos).ToList();
            List<Acesso> LstAcessos = ObterListaAcessos(dInicio, dFim);

            int y = 5;
            int x = 1;

            workSheet.Cells[4, 1].Value = dInicio.ToString("dd-MM") + " - " + dFim.ToString("dd-MM");


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

                            if (Lst.Count() == 0)
                            {
                                //workSheet.Cells[j, i + 1].Value = utilizador.TipoUtilizador == 1 ? "E: 9:00 Externo" : utilizador.TipoUtilizador == 2 ? "E: 9:00 Comercial" : "E: 09:00";
                                //workSheet.Cells[j, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                //workSheet.Cells[j, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                                //workSheet.Cells[j + 1, i + 1].Value = utilizador.TipoUtilizador == 1 ? "S: 18:30 Externo" : utilizador.TipoUtilizador == 2 ? "S: 18:30 Comercial" : "S: 18:30 ";
                                //workSheet.Cells[j + 1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                //workSheet.Cells[j + 1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                            }
                            else
                            {
                                foreach (var acesso in Lst)
                                {
                                    workSheet.Cells[j, x + 1].Value = acesso.TipoAcesso.Substring(0, 1) + ": " + acesso.Data.ToShortTimeString();
                                    j++;
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


        public void CriarCodigo(Codigo c)
        {

            string sql = "INSERT INTO dat_codigos (CodigoValidacao, EstadoCodigo, ObsInternas, Observacoes, Utilizador, ValidadeCodigo) VALUES ('" + c.Stamp + "', '" + c.Estado + "', '" + c.ObsInternas + "','" + c.Obs + "', '" + c.utilizador.Id + "', '" + c.ValidadeCodigo.ToString("yyyy-MM-dd HH:mm:ss") + "') ;";

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
        #endregion
    }
}