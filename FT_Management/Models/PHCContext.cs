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
                Console.WriteLine("Connectado á Base de Dados PHC com sucesso!");

                FT_ManagementContext.CriarArtigos(ObterProdutos(DateTime.Parse("01/01/1900 00:00:00")));
                FT_ManagementContext.CriarVendedores(ObterVendedores(DateTime.Parse("01/01/1900 00:00:00")));
                FT_ManagementContext.CriarClientes(ObterClientes(DateTime.Parse("01/01/1900 00:00:00")));
                FT_ManagementContext.CriarFornecedores(ObterFornecedores(DateTime.Parse("01/01/1900 00:00:00")));
                FT_ManagementContext.CriarEquipamentos(ObterEquipamentos(DateTime.Parse("01/01/1900 00:00:00")));
                FT_ManagementContext.CriarFolhasObra(ObterFolhasObra(DateTime.Parse("01/01/1900 00:00:00")));
                FT_ManagementContext.CriarIntervencoes(ObterIntervencoes(DateTime.Parse("01/01/1900 00:00:00")));
                FT_ManagementContext.CriarPecasFolhaObra(ObterPecas(DateTime.Parse("01/01/1900 00:00:00")));
                FT_ManagementContext.CriarMarcacoes(ObterMarcacoes(DateTime.Parse("01/01/1900 00:00:00")));
                FT_ManagementContext.CriarTecnicosMarcacao(ObterTecnicosMarcacao(DateTime.Parse("01/01/1900 00:00:00")));
            }
            catch
            {
                Console.WriteLine("Não foi possivel conectar á BD PHC!");
            }
        }

        public void AtualizarTudo()
        {
            FT_ManagementContext.CriarArtigos(ObterProdutos(FT_ManagementContext.ObterUltimaModificacaoPHC("sa")));
            FT_ManagementContext.CriarVendedores(ObterVendedores(FT_ManagementContext.ObterUltimaModificacaoPHC("cl")));
            FT_ManagementContext.CriarClientes(ObterClientes(FT_ManagementContext.ObterUltimaModificacaoPHC("cl")));
            FT_ManagementContext.CriarFornecedores(ObterFornecedores(FT_ManagementContext.ObterUltimaModificacaoPHC("fl")));
            FT_ManagementContext.CriarEquipamentos(ObterEquipamentos(FT_ManagementContext.ObterUltimaModificacaoPHC("ma")));
            FT_ManagementContext.CriarFolhasObra(ObterFolhasObra(FT_ManagementContext.ObterUltimaModificacaoPHC("pa")));
            FT_ManagementContext.CriarIntervencoes(ObterIntervencoes(FT_ManagementContext.ObterUltimaModificacaoPHC("mh")));
            FT_ManagementContext.CriarPecasFolhaObra(ObterPecas(FT_ManagementContext.ObterUltimaModificacaoPHC("bi")));
        }
        public void AtualizarArtigos()
        {
            FT_ManagementContext.CriarArtigos(ObterProdutos(FT_ManagementContext.ObterUltimaModificacaoPHC("sa")));
        }
        public void AtualizarClientes()
        {
            FT_ManagementContext.CriarVendedores(ObterVendedores(FT_ManagementContext.ObterUltimaModificacaoPHC("cl")));
            FT_ManagementContext.CriarClientes(ObterClientes(FT_ManagementContext.ObterUltimaModificacaoPHC("cl")));
        }
        public void AtualizarFornecedores()
        {
            FT_ManagementContext.CriarFornecedores(ObterFornecedores(FT_ManagementContext.ObterUltimaModificacaoPHC("fl")));
        }
        public void AtualizarEquipamentos()
        {
            FT_ManagementContext.CriarEquipamentos(ObterEquipamentos(FT_ManagementContext.ObterUltimaModificacaoPHC("ma")));
        }
        public void AtualizarFolhasObra()
        {
            AtualizarClientes();
            AtualizarEquipamentos();
            FT_ManagementContext.CriarFolhasObra(ObterFolhasObra(FT_ManagementContext.ObterUltimaModificacaoPHC("pa")));
            FT_ManagementContext.CriarIntervencoes(ObterIntervencoes(FT_ManagementContext.ObterUltimaModificacaoPHC("mh")));
            FT_ManagementContext.CriarPecasFolhaObra(ObterPecas(FT_ManagementContext.ObterUltimaModificacaoPHC("bi")));
        }
        public void AtualizarMarcacoes()
        { 
            FT_ManagementContext.CriarMarcacoes(ObterMarcacoes(FT_ManagementContext.ObterUltimaModificacaoPHC("u_marcacao")));
            FT_ManagementContext.CriarTecnicosMarcacao(ObterTecnicosMarcacao(FT_ManagementContext.ObterUltimaModificacaoPHC("u_mtecnicos")));
        }

        public List<Produto> ObterProdutos(DateTime dataUltimaLeitura)
        {
             
            List<Produto> LstProdutos = new List<Produto>();

            try
            {

            SqlConnection conn = new SqlConnection(ConnectionString);

            conn.Open();

            SqlCommand command = new SqlCommand("SELECT sa.ref, st.design, sa.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec FROM sa inner join st on sa.ref=st.ref where sa.usrdata>='"+ dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") +"';", conn);

            using (SqlDataReader result = command.ExecuteReader())
            {
                while (result.Read())
                {
                    LstProdutos.Add(new Produto()
                    {
                        Ref_Produto = result["ref"].ToString(),
                        Designacao_Produto = result["design"].ToString(),
                        //Stock_Fisico = double.Parse(result["stock_fis"].ToString()),
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

                SqlCommand command = new SqlCommand("SELECT no, estab, cl.nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, u_clresp.emailfo, tipo, vendedor, cl.usrdata, cl.usrhora FROM cl full outer join u_clresp on cl.clstamp=u_clresp.clstamp where cl.usrdata>='" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "';", conn);

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

                SqlCommand command = new SqlCommand("SELECT vendedor, vendnm FROM cl where cl.usrdata>='" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "' GROUP BY vendedor, vendnm order by vendedor;", conn);

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

                SqlCommand command = new SqlCommand("SELECT no, nome, CONCAT(morada, ' ', local, ' ', codpost) as MoradaFornecedor, telefone, email, contacto, obs FROM fl where usrdata>='" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "';", conn);

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

                SqlCommand command = new SqlCommand("SELECT serie, mastamp, design, marca, maquina, ref, no, estab, flno FROM ma where usrdata>='" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "';", conn);

                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstEquipamento.Add(new Equipamento()
                        {
                            IdEquipamento = result["mastamp"].ToString().Trim(),
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
        public List<FolhaObra> ObterFolhasObra(DateTime dataUltimaLeitura)
        {

            List<FolhaObra> LstFolhaObra = new List<FolhaObra>();
            //List<Equipamento> LstEquipamentos = FT_ManagementContext.ObterEquipamentos();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select pa.mastamp, u_intervencao.qassinou, u_intervencao.u_marcacaostamp, pa.nopat, pa.pdata, pa.no, pa.estab, pa.serie, pa.u_nincide, pa.situacao, pa.fechado, pa.problema from pa full outer join u_intervencao on pa.pastamp=u_intervencao.STAMP_DEST where pa.nopat is not null and pa.usrdata>='" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "' order by pa.nopat;", conn);

                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {

                            Cliente cliente = new Cliente()
                            {
                                IdCliente = int.Parse(result["no"].ToString().Trim()),
                                IdLoja = int.Parse(result["estab"].ToString().Trim())
                            };
                            Equipamento equipamento = new Equipamento()
                            {
                                IdEquipamento = result["mastamp"].ToString().Trim()
                            };
                        //string res = result["u_marcacaostamp"].ToString().Trim();
                        LstFolhaObra.Add(new FolhaObra()
                            {
                                IdFolhaObra = int.Parse(result["nopat"].ToString().Trim()),
                                DataServico = DateTime.Parse(result["pdata"].ToString().Trim()),
                                ReferenciaServico = result["u_nincide"].ToString().Trim(),
                                EstadoEquipamento = result["situacao"].ToString().Trim(),
                                //SituacoesPendentes = result["problema"].ToString().Trim(),
                                EquipamentoServico = equipamento,
                                ClienteServico = cliente,
                                ConferidoPor = result["qassinou"].ToString().Trim(),
                            IdCartao = result["u_marcacaostamp"].ToString().Trim()
                            });
                            //Console.WriteLine(LstFolhaObra.Count.ToString() + result["u_marcacaostamp"].ToString().Trim());
                    }
                }

                conn.Close();

                FT_ManagementContext.AtualizarUltimaModificacao("pa");

                Console.WriteLine("PAT's atualizados com sucesso! (PHC -> MYSQL)");

            }
            catch
            {
                Console.WriteLine("Não foi possivel ler os PAT's do PHC!");
            }

            return LstFolhaObra;
        }
        public List<Intervencao> ObterIntervencoes(DateTime dataUltimaLeitura)
        {

            List<Intervencao> LstIntervencao = new List<Intervencao>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select nopat, mhid, tecnico, tecnnm, data, hora, horaf, relatorio from mh where usrdata>='" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "' order by nopat;", conn);

                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstIntervencao.Add(new Intervencao()
                        {
                            IdIntervencao = int.Parse(result["mhid"].ToString().Trim()),
                            IdTecnico = int.Parse(result["tecnico"].ToString().Trim()),
                            IdFolhaObra = int.Parse(result["nopat"].ToString().Trim()),
                            NomeTecnico = result["tecnnm"].ToString().Trim(),
                            RelatorioServico = result["relatorio"].ToString().Trim(),
                            DataServiço = DateTime.Parse(result["data"].ToString().Trim())
                        });

                        DateTime.TryParse(result["hora"].ToString().Trim(), out DateTime horainicio);
                        LstIntervencao[^1].HoraInicio = horainicio;
                        DateTime.TryParse(result["horaf"].ToString().Trim(), out DateTime horafim);
                        LstIntervencao[^1].HoraFim = horafim;

                        //Console.WriteLine(result["nopat"].ToString().Trim());

                    }
                }

                conn.Close();

                FT_ManagementContext.AtualizarUltimaModificacao("mh");

                Console.WriteLine("Intervenções atualizadas com sucesso! (PHC -> MYSQL)");

            }
            catch
            {
                Console.WriteLine("Não foi possivel ler as intervenções do PHC!");
            }

            return LstIntervencao;
        }
        public List<Produto> ObterPecas(DateTime dataUltimaLeitura)
        {

            List<Produto> LstProduto = new List<Produto>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select pa.nopat, bi.ref, bi.design, bi.qtt from pa inner join bo on bo.pastamp=pa.pastamp inner join bi on bi.obrano=bo.obrano where ref!=''  and bo.ndos=49 and bo.usrdata>='" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "' order by nopat;", conn);

                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstProduto.Add(new Produto()
                        {
                            Armazem_ID = int.Parse(result["nopat"].ToString().Trim()),
                            Ref_Produto = result["ref"].ToString().Trim(),
                            Designacao_Produto = result["design"].ToString().Trim(),
                            TipoUn = "UN",
                            Stock_Fisico = double.Parse(result["qtt"].ToString().Trim())

                        });
                    }
                }

                conn.Close();

                FT_ManagementContext.AtualizarUltimaModificacao("bi");

                Console.WriteLine("Produtos de PAT's atualizados com sucesso! (PHC -> MYSQL)");

            }
            catch
            {
                Console.WriteLine("Não foi possivel ler as peças usadas pelos PAT's do PHC!");
            }

            return LstProduto;
        }
        public List<Marcacao> ObterMarcacoes(DateTime dataUltimaLeitura)
        {

            List<Marcacao> LstMarcacao = new List<Marcacao>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("SELECT num, data, no, estab, tecnno, tipoe, resumo, estado, prioridade, u_marcacaostamp, oficina FROM u_marcacao where usrdata>='" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "' order by num;", conn);

                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstMarcacao.Add(new Marcacao()
                        {
                            IdMarcacao = int.Parse(result["num"].ToString().Trim()),
                            DataMarcacao = DateTime.Parse(result["data"].ToString().Trim()),
                            Cliente = new Cliente { IdCliente = int.Parse(result["no"].ToString().Trim()), IdLoja = int.Parse(result["estab"].ToString().Trim()) },
                            //IdTecnico = int.Parse(result["tecnno"].ToString().Trim()),
                            ResumoMarcacao = result["resumo"].ToString().Trim(),
                            EstadoMarcacao = result["estado"].ToString().Trim(),
                            PrioridadeMarcacao = result["prioridade"].ToString().Trim(),
                            MarcacaoStamp = result["u_marcacaostamp"].ToString().Trim(),
                            TipoEquipamento = result["tipoe"].ToString().Trim(),
                            Oficina = result["oficina"].ToString().Trim() == "True" ? 1 : 0
                        });
                    }
                }

                conn.Close();

                FT_ManagementContext.AtualizarUltimaModificacao("u_marcacao");

                Console.WriteLine("Marcacoes atualizadas com sucesso! (PHC -> MYSQL)");

            }
            catch
            {
                Console.WriteLine("Não foi possivel ler as Marcacoes do PHC!");
            }

            return LstMarcacao;
        }
        public List<Utilizador> ObterTecnicosMarcacao(DateTime dataUltimaLeitura)
        {

            List<Utilizador> LstUtilizador = new List<Utilizador>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select u_mtecnicosstamp, marcacaostamp, tecnno,tecnnm from u_mtecnicos where marcado=1 AND usrdata>='" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "';", conn);

                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstUtilizador.Add(new Utilizador()
                        {
                            Id = int.Parse(result["tecnno"].ToString().Trim()),
                            NomeCompleto = result["tecnnm"].ToString().Trim(),
                            NomeUtilizador = result["u_mtecnicosstamp"].ToString().Trim(),
                            IdCartaoTrello = result["marcacaostamp"].ToString().Trim()
                        });
                    }
                }

                conn.Close();

                FT_ManagementContext.AtualizarUltimaModificacao("u_mtecnicos");

                Console.WriteLine("Tecnicos por marcacao atualizadas com sucesso! (PHC -> MYSQL)");

            }
            catch
            {
                Console.WriteLine("Não foi possivel ler as Marcacoes do PHC!");
            }

            return LstUtilizador;
        }
    }
}
