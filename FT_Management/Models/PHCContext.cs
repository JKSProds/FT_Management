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
                FT_ManagementContext.CriarVendedores(ObterVendedores(DateTime.Parse("01/01/1900 00:00:00")));
                FT_ManagementContext.CriarClientes(ObterClientes(DateTime.Parse("01/01/1900 00:00:00")));
                FT_ManagementContext.CriarFornecedores(ObterFornecedores(DateTime.Parse("01/01/1900 00:00:00")));
                FT_ManagementContext.CriarEquipamentos(ObterEquipamentos(DateTime.Parse("01/01/1900 00:00:00")));
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
                Console.WriteLine("Não foi possivel ler as referencias do PHC!");
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

                SqlCommand command = new SqlCommand("SELECT no, estab, nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, email, tipo, vendedor, usrdata, usrhora FROM cl where cl.usrdata>'" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "';", conn);

                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstClientes.Add(new Cliente()
                        {
                            IdCliente = int.Parse(result["no"].ToString()),
                            IdLoja = int.Parse(result["estab"].ToString()),
                            NomeCliente = result["nome"].ToString().Trim(),
                            NumeroContribuinteCliente = result["ncont"].ToString().Trim(),
                            TelefoneCliente = result["telefone"].ToString().Trim(),
                            PessoaContatoCliente = result["contacto"].ToString().Trim(),
                            MoradaCliente = result["endereco"].ToString().Trim(),
                            EmailCliente = result["email"].ToString().Trim(),
                            IdVendedor = int.Parse(result["vendedor"].ToString().Trim()),
                            TipoCliente = result["tipo"].ToString().Trim()
                        });
                    }
                }

                conn.Close();

                FT_ManagementContext.AtualizarUltimaModificacao("cl");

                Console.WriteLine("Clientes e Lojas atualizadas com sucesso! (PHC -> MYSQL)");

            }
            catch
            {
                Console.WriteLine("Não foi possivel ler os clientes do PHC!");
            }

            return LstClientes;
        }
        public List<Vendedor> ObterVendedores(DateTime dataUltimaLeitura)
        {

            List<Vendedor> LstVendedor = new List<Vendedor>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("SELECT vendedor, vendnm FROM cl where cl.usrdata>'" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "' GROUP BY vendedor, vendnm order by vendedor;", conn);

                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstVendedor.Add(new Vendedor()
                        {
                            IdVendedor = int.Parse(result["vendedor"].ToString()),
                            NomeVendedor = result["vendnm"].ToString()
                        });
                    }
                }

                conn.Close();

                FT_ManagementContext.AtualizarUltimaModificacao("cl");

                Console.WriteLine("Vendedores atualizados com sucesso! (PHC -> MYSQL)");

            }
            catch
            {
                Console.WriteLine("Não foi possivel ler os vendedores do PHC!");
            }

            return LstVendedor;
        }
        public List<Fornecedor> ObterFornecedores(DateTime dataUltimaLeitura)
        {

            List<Fornecedor> LstFornecedor = new List<Fornecedor>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("SELECT no, nome, CONCAT(morada, ' ', local, ' ', codpost) as MoradaFornecedor, telefone, email, contacto, obs FROM fl where usrdata>'" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "';", conn);

                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstFornecedor.Add(new Fornecedor()
                        {
                            IdFornecedor = int.Parse(result["no"].ToString()),
                            NomeFornecedor = result["nome"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''"),
                            MoradaFornecedor = result["MoradaFornecedor"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''"),
                            ContactoFornecedor = result["telefone"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''"),
                            EmailFornecedor = result["email"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''"),
                            PessoaContactoFornecedor = result["contacto"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''"),
                            Obs = result["obs"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''"),
                            ReferenciaFornecedor = "N/D"
                        });
                    }
                }

                conn.Close();

                FT_ManagementContext.AtualizarUltimaModificacao("fl");

                Console.WriteLine("Fornecedores atualizados com sucesso! (PHC -> MYSQL)");

            }
            catch
            {
                Console.WriteLine("Não foi possivel ler os Fornecedores do PHC!");
            }

            return LstFornecedor;
        }
        public List<Equipamento> ObterEquipamentos(DateTime dataUltimaLeitura)
        {

            List<Equipamento> LstEquipamento = new List<Equipamento>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("SELECT serie, design, marca, maquina, ref, no, estab, flno FROM ma where usrdata>'" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "';", conn);

                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstEquipamento.Add(new Equipamento()
                        {
                            DesignacaoEquipamento = result["design"].ToString().Trim(),
                            MarcaEquipamento = result["marca"].ToString().Trim(),
                            ModeloEquipamento = result["maquina"].ToString().Trim(),
                            NumeroSerieEquipamento = result["serie"].ToString().Trim().Replace('\\', ' '),
                            RefProduto = result["ref"].ToString().Trim(),
                            IdCliente = int.Parse(result["no"].ToString()),
                            IdLoja = int.Parse(result["estab"].ToString()),
                            IdFornecedor = int.Parse(result["flno"].ToString())

                        });
                    }
                }

                conn.Close();

                FT_ManagementContext.AtualizarUltimaModificacao("ma");

                Console.WriteLine("Equipamentos atualizados com sucesso! (PHC -> MYSQL)");

            }
            catch
            {
                Console.WriteLine("Não foi possivel ler os Equipamentos do PHC!");
            }

            return LstEquipamento;
        }
    }
}
