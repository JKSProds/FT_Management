using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace FT_Management.Models
{
    public class PHCContext
    {
        private string ConnectionString { get; set; }
        private FT_ManagementContext FT_ManagementContext { get; set; }
        public PHCContext(string connectionString, string mySqlConnectionString)
        {
            this.ConnectionString = connectionString;
            SqlConnection cnn;

            FT_ManagementContext = new FT_ManagementContext(mySqlConnectionString, "");

            try
            {
                cnn = new SqlConnection(connectionString);
                FT_ManagementContext.CriarArtigos(ObterProdutos(DateTime.Parse("01/01/1900 00:00:00")));
                FT_ManagementContext.CriarClientes(ObterClientes(DateTime.Parse("01/01/1900 00:00:00")));
                Console.WriteLine("Connectado á Base de Dados PHC com sucesso!");
            }
            catch
            {
                Console.WriteLine("Não foi possivel conectar á BD PHC!");
            }
        }

        public List<Produto> ObterProdutos(DateTime dataUltimaLeitura)
        {
             
            List<Produto> LstProdutos = new List<Produto>();

            try
            {

            SqlConnection conn = new SqlConnection(ConnectionString);

            conn.Open();

            SqlCommand command = new SqlCommand("SELECT sa.ref, st.design, sa.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec FROM sa inner join st on sa.ref=st.ref where sa.usrdata>'"+ dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") +"';", conn);

            using (SqlDataReader result = command.ExecuteReader())
            {
                while (result.Read())
                {
                    LstProdutos.Add(new Produto()
                    {
                        Ref_Produto = result["ref"].ToString(),
                        Designacao_Produto = result["design"].ToString(),
                        Stock_Fisico = double.Parse(result["stock_fis"].ToString()),
                        Stock_PHC = double.Parse(result["stock"].ToString()),
                        Stock_Rec = double.Parse(result["qttrec"].ToString()),
                        Stock_Res = double.Parse(result["rescli"].ToString()),
                        Armazem_ID = int.Parse(result["armazem"].ToString())
                    });
                }
            }

            conn.Close();

            FT_ManagementContext.AtualizarUltimaModificacao("sa");

            Console.WriteLine("Stock's atualizados com sucesso! (PHC -> MYSQL)");

            }
            catch
            {
                Console.WriteLine("Não foi possivel conectar á BD PHC!");
            }

            return LstProdutos;
        }
        public List<Cliente> ObterClientes(DateTime dataUltimaLeitura)
        {

            List<Cliente> LstClientes = new List<Cliente>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("SELECT no, estab, nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, email, usrdata, usrhora FROM cl where cl.usrdata>'" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "' AND estab=0;", conn);

                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstClientes.Add(new Cliente()
                        {
                            IdCliente = int.Parse(result["no"].ToString()),
                            NomeCliente = result["nome"].ToString().Trim(),
                            NumeroContribuinteCliente = result["ncont"].ToString().Trim(),
                            TelefoneCliente = result["telefone"].ToString().Trim(),
                            PessoaContatoCliente = result["contacto"].ToString().Trim(),
                            MoradaCliente = result["endereco"].ToString().Trim(),
                            EmailCliente = result["email"].ToString().Trim()
                        });
                    }
                }

                conn.Close();

                FT_ManagementContext.AtualizarUltimaModificacao("cl");

                Console.WriteLine("Clientes e Lojas atualizadass com sucesso! (PHC -> MYSQL)");

            }
            catch
            {
                Console.WriteLine("Não foi possivel conectar á BD PHC!");
            }

            return LstClientes;
        }
    }
}
