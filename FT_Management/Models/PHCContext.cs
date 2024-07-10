using System.Xml.XPath;
using iTextSharp.text;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MySqlX.XDevAPI.Common;

namespace FT_Management.Models
{
    public class PHCContext
    {
        private string ConnectionString { get; set; }
        private readonly int TIMEOUT = 240;
        private FT_ManagementContext FT_ManagementContext { get; set; }

        public PHCContext(string connectionString, string mySqlConnectionString)
        {
            this.ConnectionString = connectionString;
            SqlConnection cnn;
            FT_ManagementContext = new FT_ManagementContext(mySqlConnectionString);

            try
            {
                cnn = new SqlConnection(connectionString);
                cnn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel conectar á BD PHC!\r\n(Exception: " + ex.Message + ")");
            }
        }
        public List<string> ExecutarQuery(string SQL_Query)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };
            int i = 0;

            try
            {
                Console.WriteLine("Query: " + SQL_Query);

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand(SQL_Query, conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        if (result.HasRows)
                        {
                            for (int j = 0; j < result.FieldCount; j++)
                            {
                                if (res.Count() <= i)
                                {
                                    res.Add(result[j].ToString());
                                }
                                else
                                {
                                    res[i] = result[j].ToString();
                                }
                                i++;
                            }
                        }
                    }
                    if (!result.HasRows && result.RecordsAffected > 0) res[0] = "1";
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel executar query.\r\n(Exception: " + ex.Message + ")");
            }

            Console.WriteLine("Resultado: " + string.Join(" | ", res.Select(x => x)));
            return res;
        }
#region Acessos
        public bool ValidarAcesso(RegistroAcessos r, Utilizador u, int Horas) {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };
            string SQL_Query = "";

            try
            {
                if (r.TipoHorasExtra > 0 && Horas > 0 && r.TipoHorasExtra != 6) {
                    SQL_Query = "EXEC WEB_Insere_HS ";

                    SQL_Query += "@NO = '" + r.Utilizador.IdFuncionario + "', ";
                    SQL_Query += "@DATA = '" + r.Data.ToString("yyyy-MM-dd") + "', ";
                    SQL_Query += "@NTIPO = '" + 1 + "', ";
                    SQL_Query += "@HSHECOD = '" + "1" + "', ";
                    SQL_Query += "@TYFALTA = '" + "0" + "', ";
                    SQL_Query += "@HORASEXTRA = '" + "1" + "', ";
                    SQL_Query += "@TEMPOFALTA = '" + "0" + "', ";
                    SQL_Query += "@NOME_UTILIZADOR = '" + u.NomeCompleto + "'; ";

                    res = ExecutarQuery(SQL_Query);

                    Horas -=1;
                }
                if (Horas > 0) {
                    SQL_Query = "EXEC WEB_Insere_HS ";

                    SQL_Query += "@NO = '" + r.Utilizador.IdFuncionario + "', ";
                    SQL_Query += "@DATA = '" + r.Data.ToString("yyyy-MM-dd") + "', ";
                    SQL_Query += "@NTIPO = '" + (r.TipoFalta != 0 ? 2 : (r.TipoHorasExtra != 0 ? 1 : 3)) + "', ";
                    SQL_Query += "@HSHECOD = '" + r.TipoHorasExtra + "', ";
                    SQL_Query += "@TYFALTA = '" + r.TipoFalta + "', ";
                    SQL_Query += "@HORASEXTRA = '" + (r.TipoHorasExtra != 0 ? Horas.ToString() : "0") + "', ";
                    SQL_Query += "@TEMPOFALTA = '" + (r.TipoFalta != 0 ? Horas.ToString() : "0") + "', ";
                    SQL_Query += "@NOME_UTILIZADOR = '" + u.NomeCompleto + "'; ";

                    res = ExecutarQuery(SQL_Query);
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel enviar o acesso para o PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res[0] != "-1";

        }
        

         public List<KeyValuePair<int, string>> ObterTipoAcessos()
        {

            return new List<KeyValuePair<int, string>>() { new KeyValuePair<int, string>(1, "Horas Extraordinárias"), new KeyValuePair<int, string>(2, "Faltas")};

        }

        public List<KeyValuePair<int, string>> ObterTipoHorasExtras()
        {
            List<KeyValuePair<int, string>> res = new List<KeyValuePair<int, string>>();

            try
                {
                    SqlConnection conn = new SqlConnection(ConnectionString);
                    conn.Open();
                    SqlCommand command = new SqlCommand("select hshestamp, codigo, descricao, factor, cm, cmdesc, razao from hshe(nolock) where codigo = 3 or codigo = 6 or codigo = 7;", conn)
                    {
                        CommandTimeout = TIMEOUT
                    };

                    using (SqlDataReader result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            res.Add(new KeyValuePair<int, string>(int.Parse(result["codigo"].ToString()), result["descricao"].ToString()));
                        }
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Não foi possivel ler os tipos de horas extra!\r\n(Exception: " + ex.Message + ")");
                }

                return res;
        }

        public List<KeyValuePair<int, string>> ObterTipoFaltas()
        {
            List<KeyValuePair<int, string>> res = new List<KeyValuePair<int, string>>();

            try
                {
                    SqlConnection conn = new SqlConnection(ConnectionString);
                    conn.Open();
                    SqlCommand command = new SqlCommand("select tystamp, codigo, descricao, cm, cmdesc, desconta, just, refe from ty(nolock) where cm <> 0;", conn)
                    {
                        CommandTimeout = TIMEOUT
                    };

                    using (SqlDataReader result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            res.Add(new KeyValuePair<int, string>(int.Parse(result["codigo"].ToString()), result["descricao"].ToString()));
                        }
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Não foi possivel ler os tipos de faltas!\r\n(Exception: " + ex.Message + ")");
                }
           return res;
        }


        #endregion
    }
}

      