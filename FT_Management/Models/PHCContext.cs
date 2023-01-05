using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;


namespace FT_Management.Models
{
    public class PHCContext
    {
        private string ConnectionString { get; set; }
        private readonly int TIMEOUT = 5;
        private FT_ManagementContext FT_ManagementContext { get; set; }

        public PHCContext(string connectionString, string mySqlConnectionString)
        {
            this.ConnectionString = connectionString;
            SqlConnection cnn;
            FT_ManagementContext = new FT_ManagementContext(mySqlConnectionString, "");

            try
            {
                cnn = new SqlConnection(connectionString);
                cnn.Open();
                FicheirosContext.ObterImagensUtilizador();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel conectar á BD PHC!\r\n(Exception: "+ex.Message+")");
            }
        }
        public List<string> ExecutarQuery(string SQL_Query)
        {
            List<string> res = new List<string>();

            try
            {
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
                        res.Add(result[0].ToString());
                    }
                }
                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel executar query.\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }

        //Obter Referências
        #region REFERENCIA
        private List<Produto> ObterProdutos(string SQL_Query)
        {

            List<Produto> LstProdutos = new List<Produto>();

            try
            {

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
                        LstProdutos.Add(new Produto()
                        {
                            StampProduto = result["ststamp"].ToString(),
                            Ref_Produto = result["ref"].ToString(),
                            Designacao_Produto = result["design"].ToString(),
                            //Stock_Fisico = double.Parse(result["stock_fis"].ToString()),
                            Stock_PHC = double.Parse(result["stock"].ToString()),
                            Stock_Rec = double.Parse(result["qttrec"].ToString()),
                            Stock_Res = double.Parse(result["rescli"].ToString()),
                            Armazem_ID = int.Parse(result["armazem"].ToString()),
                            Pos_Stock = result["u_locpt"].ToString(),
                            Serie = result["noserie"].ToString() == "True"
                        });
                        //LstProdutos.Last().ImgProduto = ObterProdutoImagem(LstProdutos.Last());
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler as referencias do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstProdutos;
        }
        public List<Produto> ObterProdutos(string Referencia, string Designacao, int IdArmazem)
        {
            return ObterProdutos("SELECT ststamp, sa.ref, st.design, sa.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec, stobs.u_locpt, noserie FROM sa inner join st on sa.ref=st.ref inner join stobs on sa.ref=stobs.ref where sa.ref like '%"+Referencia+"%' AND st.design like '%"+Designacao+"%' AND sa.armazem='"+IdArmazem+"' order by sa.ref;");
        }
        public List<Produto> ObterProdutos(string Referencia, string Designacao, int IdArmazem, int IdFornecedor, string TipoEquipamento)
        {
            return ObterProdutos("SELECT ststamp, sa.ref, st.design, sa.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec, stobs.u_locpt, noserie FROM sa inner join st on sa.ref=st.ref inner join stobs on sa.ref=stobs.ref where sa.ref like '%" + Referencia + "%' AND st.design like '%" + Designacao + "%' AND sa.armazem='" + IdArmazem + "' " + (IdFornecedor == 0 ? "" : " and fornec=" + IdFornecedor) + " and st.usr4 like '%"+TipoEquipamento+ "%' AND st.inactivo=0 order by sa.ref;");
        }
        public List<Produto> ObterProdutosArmazem(string Referencia)
        {
            return ObterProdutos("SELECT ststamp, sa.ref, st.design, sa.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec, stobs.u_locpt, noserie FROM sa inner join st on sa.ref=st.ref inner join stobs on sa.ref=stobs.ref where sa.ref like '%" + Referencia + "%' order by sa.armazem;");
        }
        public Produto ObterProdutoStamp(string Stamp)
        {
            return ObterProdutos("SELECT ststamp, sa.ref, st.design, sa.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec, stobs.u_locpt, noserie FROM sa inner join st on sa.ref=st.ref inner join stobs on sa.ref=stobs.ref where ststamp like '%" + Stamp + "%' order by sa.armazem;").DefaultIfEmpty(new Produto()).First();
        }
        public List<Produto> ObterProdutosArmazem(int Armazem)
        {
            return ObterProdutos("SELECT ststamp, sa.ref, st.design, sa.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec, stobs.u_locpt, noserie FROM sa inner join st on sa.ref=st.ref inner join stobs on sa.ref=stobs.ref where sa.armazem = '" + Armazem + "' order by sa.ref;").Where(p => (p.Stock_PHC - p.Stock_Res + p.Stock_Rec) > 0).ToList();
        }

        private List<Armazem> ObterArmazens(string SQL_Query)
        {

            List<Armazem> LstArmazens = new List<Armazem>();

            try
            {

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
                        LstArmazens.Add(new Armazem()
                        {
                            ArmazemStamp = result["szstamp"].ToString().Trim(),
                            ArmazemId = int.Parse(result["no"].ToString()),
                            ArmazemNome = result["nome"].ToString().Trim(),
                        });
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os armazens do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstArmazens;
        }


        public List<Armazem> ObterArmazens()
        {
            return ObterArmazens("select * from sz order by no;");
        }
        public List<Armazem> ObterArmazensFixos()
        {
            return ObterArmazens("select * from sz where nome not like '%Técnico%' and nome not like '%Comercial%' order by no;");
        }
        public Armazem ObterArmazem(string stamp)
        {
            return ObterArmazens("select * from sz where szstamp='"+stamp+"' order by no;").FirstOrDefault() ?? new Armazem();
        }
        public Armazem ObterArmazem(int num)
        {
            return ObterArmazens("select * from sz where no='" + num + "' order by no;").FirstOrDefault() ?? new Armazem();
        }

        //NAO FUNCIONA
        public List<Movimentos> ObterListaMovimentos(string SQL_Query)
        {
            List<Movimentos> LstGuias = new List<Movimentos>();

            try
            {
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
                        LstGuias.Add(new Movimentos()
                        {
                            IdFolhaObra = int.Parse(result["nopat"].ToString()),
                            IdTecnico = int.Parse(result["tecnico"].ToString()),
                            NomeTecnico = result["tecnnm"].ToString(),
                            GuiaTransporte = result["obrano"].ToString(),
                            RefProduto = result["ref"].ToString(),
                            Designacao = result["design"].ToString(),
                            Quantidade = float.Parse(result["qtt"].ToString()),
                            NomeCliente = result["nome"].ToString(),
                            DataMovimento = DateTime.Parse(DateTime.Parse(result["fdata"].ToString()).ToShortDateString() + " " + DateTime.Parse(result["fhora"].ToString()).ToShortTimeString()),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel obter a lista de movimentos do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstGuias;
        }
        public Produto ObterProduto(string Referencia, int IdArmazem)
        {
            List<Produto> p = ObterProdutos("SELECT ststamp, sa.ref, st.design, sa.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec, stobs.u_locpt, noserie FROM sa inner join st on sa.ref=st.ref inner join stobs on sa.ref=stobs.ref where sa.ref='" + Referencia + "' AND sa.armazem='" + IdArmazem + "' order by sa.ref;");
            if (p.Count > 0)
            {
                p[0].ImgProduto = ObterProdutoImagem(p[0]);
                return p[0];
            }
            return new Produto();
        }
        private string ObterProdutoImagem(Produto p)
        {
            string res = ""; string img = FicheirosContext.ObterCaminhoProdutoImagem(p.Ref_Produto);
            if (!File.Exists(img)) img = "wwwroot/img/no_photo.png";
            using (Image image = Image.FromFile(img))
            {
                using MemoryStream m = new MemoryStream();
                image.Save(m, image.RawFormat);
                byte[] imageBytes = m.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                res = base64String;
            }
            return res;
        }
        #endregion

        //Obter Clientes
        #region CLIENTES
        private List<Cliente> ObterClientes(string SQL_Query, bool LoadMarcacoes, bool LoadFolhasObra, bool LoadVisitas, bool LoadEquipamentos)
        {

            List<Cliente> LstClientes = new List<Cliente>();
            List<Cliente> LstClientesMySQL = FT_ManagementContext.ObterClientes();

            try
            {
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
                        LstClientes.Add(new Cliente()
                        {
                            ClienteStamp = result["clstamp"].ToString().Trim(),
                            IdCliente = int.Parse(result["no"].ToString()),
                            IdLoja = int.Parse(result["estab"].ToString()),
                            NomeCliente = result["nome"].ToString().Trim(),
                            NumeroContribuinteCliente = result["ncont"].ToString().Trim(),
                            TelefoneCliente = result["telefone"].ToString().Trim(),
                            PessoaContatoCliente = result["contacto"].ToString().Trim(),
                            MoradaCliente = result["endereco"].ToString().Trim(),
                            EmailCliente = result["emailfo"].ToString().Trim(),
                            IdVendedor = int.Parse(result["vendedor"].ToString().Trim()),
                            TipoCliente = result["tipo"].ToString().Trim()
                        });
                        if (LoadMarcacoes) LstClientes.Last().Marcacoes = ObterMarcacoes(new Cliente() { IdCliente = int.Parse(result["no"].ToString()), IdLoja = int.Parse(result["estab"].ToString()) });
                        if (LoadFolhasObra) LstClientes.Last().FolhasObra = ObterFolhasObra(new Cliente() { IdCliente = int.Parse(result["no"].ToString()), IdLoja = int.Parse(result["estab"].ToString()) });
                        if (LoadVisitas) LstClientes.Last().Visitas = FT_ManagementContext.ObterListaVisitasCliente(int.Parse(result["no"].ToString()), int.Parse(result["estab"].ToString()));
                        if (LoadEquipamentos) LstClientes.Last().Equipamentos = ObterEquipamentos(new Cliente() { IdCliente = int.Parse(result["no"].ToString()), IdLoja = int.Parse(result["estab"].ToString()) });
                        if (LstClientesMySQL.Where(c => c.ClienteStamp == LstClientes.Last().ClienteStamp).Count() > 0)
                        {
                            Cliente c = LstClientesMySQL.Where(c => c.ClienteStamp == LstClientes.Last().ClienteStamp).First();
                            LstClientes.Last().Latitude = c.Latitude;
                            LstClientes.Last().Longitude = c.Longitude;
                            LstClientes.Last().Senha = c.Senha;
                        }
                    }
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os clientes do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstClientes;
        }
        private Cliente ObterCliente(string SQL_Query, bool LoadAll)
        {
           return ObterClientes(SQL_Query, LoadAll, LoadAll, LoadAll, LoadAll).DefaultIfEmpty(new Cliente()).First();
           // return FT_ManagementContext.ObterCliente(c);
        }
        public List<Cliente> ObterClientes()
        {
            return ObterClientes("SELECT cl.clstamp, no, estab, cl.nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, u_clresp.emailfo, tipo, vendedor, cl.usrdata, cl.usrhora FROM cl full outer join u_clresp on cl.clstamp=u_clresp.clstamp where no is not null order by no, estab ;", false, false, false, false);
        }
        public List<Cliente> ObterClientes(string filtro, bool filtrar)
        {
            if (filtrar)
            {
                return ObterClientes("SELECT cl.clstamp, no, estab, cl.nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, (select TOP 1 emailfo from u_clresp where cl.clstamp=u_clresp.clstamp) as emailfo, tipo, vendedor, cl.usrdata, cl.usrhora FROM cl where cl.nome like '%" + filtro+"%' and no is not null order by no, estab;", false, false, false, false);
            }
            return new List<Cliente>() { new Cliente() { } };
        }
        public Cliente ObterCliente(int IdCliente, int IdLoja)
        {
            return ObterCliente("SELECT cl.clstamp, no, estab, cl.nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, u_clresp.emailfo, tipo, vendedor, cl.usrdata, cl.usrhora FROM cl full outer join u_clresp on cl.clstamp=u_clresp.clstamp where cl.no=" + IdCliente + " and estab=" + IdLoja + " and no is not null;", true);
           
        }
        public Cliente ObterClienteSimples(int IdCliente, int IdLoja)
        {
            return ObterCliente("SELECT TOP 1 cl.clstamp, no, estab, cl.nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, u_clresp.emailfo, tipo, vendedor, cl.usrdata, cl.usrhora FROM cl full outer join u_clresp on cl.clstamp=u_clresp.clstamp where cl.no=" + IdCliente + " and estab=" + IdLoja + " and no is not null;", false);
        }
        public Cliente ObterClienteNIF(string NIF)
        {
            return ObterCliente("SELECT cl.clstamp, no, estab, cl.nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, u_clresp.emailfo, tipo, vendedor, cl.usrdata, cl.usrhora FROM cl full outer join u_clresp on cl.clstamp=u_clresp.clstamp where cl.ncont='" + NIF + "' and no is not null;", true);
        }
        #endregion

        //Obter Vendedores
        #region VENDEDORES
        public List<Vendedor> ObterVendedores(DateTime dataUltimaLeitura)
        {

            List<Vendedor> LstVendedor = new List<Vendedor>();

                try
                {
                    SqlConnection conn = new SqlConnection(ConnectionString);

                    conn.Open();

                SqlCommand command = new SqlCommand("SELECT vendedor, vendnm FROM cl where cl.usrdata>='" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "' GROUP BY vendedor, vendnm order by vendedor;", conn)
                {
                    CommandTimeout = TIMEOUT
                };
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
                }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os vendedores do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstVendedor;
        }
        #endregion

        //Obter Fornecedores
        #region FORNECEDORES
        public List<Fornecedor> ObterFornecedores()
        {

            List<Fornecedor> LstFornecedor = new List<Fornecedor>();

                try
                {
                    SqlConnection conn = new SqlConnection(ConnectionString);

                    conn.Open();

                SqlCommand command = new SqlCommand("SELECT flstamp, no, nome, CONCAT(morada, ' ', local, ' ', codpost) as MoradaFornecedor, telefone, email, contacto, obs, u_numfart FROM fl order by nome;", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            LstFornecedor.Add(new Fornecedor()
                            {
                                StampFornecedor = result["flstamp"].ToString(),
                                IdFornecedor = int.Parse(result["no"].ToString()),
                                NomeFornecedor = result["nome"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''"),
                                MoradaFornecedor = result["MoradaFornecedor"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''"),
                                ContactoFornecedor = result["telefone"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''"),
                                EmailFornecedor = result["email"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''"),
                                PessoaContactoFornecedor = result["contacto"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''"),
                                Obs = result["obs"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''"),
                                CodigoIntermedio = result["u_numfart"].ToString(),
                                ReferenciaFornecedor = "N/D"
                            });
                        }
                    }

                    conn.Close();
                }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os Fornecedores do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstFornecedor;
        }
        public List<String> ObterTiposEquipamento()
        {

            List<String> LstTiposEquipamento = new List<string>();

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select CAMPO from dytable where entityname LIKE 'a_matipo'", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstTiposEquipamento.Add(result[0].ToString());
                    }
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os Tipos de Equipamento do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstTiposEquipamento;
        }
        #endregion

        //Obter Equipamentos
        #region EQUIPAMENTOS
        private List<Equipamento> ObterEquipamentos(string SQL_Query)
        {

            List<Equipamento> LstEquipamento = new List<Equipamento>();

            try
            {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os Equipamentos do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstEquipamento;
        }
        public List<Equipamento> ObterEquipamentos()
        {
            return ObterEquipamentos("SELECT serie, mastamp, design, marca, maquina, ref, no, estab, flno FROM ma;");
        }
        public List<Equipamento> ObterEquipamentos(Cliente c)
        {
            return ObterEquipamentos("SELECT serie, mastamp, design, marca, maquina, ref, no, estab, flno FROM ma where no='" + c.IdCliente + "' and estab='"+c.IdLoja+"';");
        }
        public Equipamento ObterEquipamento(string IdEquipamento)
        {
            List<Equipamento> e = ObterEquipamentos("SELECT serie, mastamp, design, marca, maquina, ref, no, estab, flno FROM ma where mastamp='"+ IdEquipamento +"';");
            if (e.Count > 0) return e[0];
            return new Equipamento();
        }
        #endregion

        //Obter Folhas de Obra
        #region FOLHASOBRA

        //EM DESENVOLVIMENTO
        public int CriarFolhaObra(FolhaObra fo)
        {
            int res = 0;
            try
            {
                //SqlConnection conn = new SqlConnection(ConnectionString);

                //conn.Open();

                //SqlCommand command = new SqlCommand("Gera_FolhaObra", conn)
                //{
                //    CommandTimeout = TIMEOUT,
                //    CommandType = CommandType.StoredProcedure
                //};

                //command.Parameters.Add(new SqlParameter("@NO", fo.ClienteServico.IdCliente));

                //using SqlDataReader result = command.ExecuteReader();
                //result.Read();

                //if (result[0].ToString() != "-1")
                //{
                //    res = int.Parse(result[3].ToString());
                //    FT_ManagementContext.AdicionarLog(fo.Utilizador.Id, "Folha de Obra criada com sucesso! - Nº " + res + ", " + fo.ClienteServico.NomeCliente + " pelo utilizador " + fo.Utilizador.NomeCompleto, 5);
                //}

                //conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel enviar folha de obra para o PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }
        public string ValidarFolhaObra(FolhaObra fo)
        {
            string res = "";
           
            return res;
        }
        private List<FolhaObra> ObterFolhasObra(string SQL_Query, bool LoadEquipamento, bool LoadCliente, bool LoadIntervencoes, bool LoadPecas, bool LoadRubrica)
        {

            List<FolhaObra> LstFolhaObra = new List<FolhaObra>();

            try
            {

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
                            LstFolhaObra.Add(new FolhaObra()
                            {
                                IdFolhaObra = int.Parse(result["nopat"].ToString().Trim()),
                                DataServico = DateTime.Parse(result["pdata"].ToString().Trim()),
                                ReferenciaServico = result["u_nincide"].ToString().Trim(),
                                EstadoEquipamento = result["situacao"].ToString().Trim(),
                                ConferidoPor = result.IsDBNull("qassinou") ? "" : result["qassinou"].ToString().Trim(),
                                IdCartao = result.IsDBNull("u_marcacaostamp") ? "" : result["u_marcacaostamp"].ToString().Trim(),
                                IdMarcacao = result.IsDBNull("num") ? 0 : int.Parse(result["num"].ToString()),
                            });

                        if (LoadEquipamento) LstFolhaObra.Last().EquipamentoServico = ObterEquipamento(result["mastamp"].ToString().Trim());
                        if (LoadCliente) LstFolhaObra.Last().ClienteServico = ObterClienteSimples(int.Parse(result["no"].ToString().Trim()), int.Parse(result["estab"].ToString().Trim()));
                        if (LoadIntervencoes) {
                            LstFolhaObra.Last().AssistenciaRemota = result.IsDBNull("tipo_int") ? false : result["tipo_int"].ToString() == "3";
                            LstFolhaObra.Last().Piquete = result.IsDBNull("piquete") ? false : result["piquete"].ToString().Trim() == "True";

                            LstFolhaObra.Last().IntervencaosServico = ObterIntervencoes(int.Parse(result["nopat"].ToString().Trim()));
                            if (LstFolhaObra.Last().IntervencaosServico.Count() == 1) LstFolhaObra.Last().RelatorioServico = LstFolhaObra.Last().IntervencaosServico.First().RelatorioServico;
                            if (LstFolhaObra.Last().IntervencaosServico.Count() > 1)
                            {
                                foreach (var item in LstFolhaObra.Last().IntervencaosServico)
                                {
                                    LstFolhaObra.Last().RelatorioServico += item.DataServiço.ToShortDateString() + ": " + item.HoraInicio.ToShortTimeString() + " -> " + item.HoraFim.ToShortTimeString() + " - " + item.RelatorioServico + "\r\n";
                                }
                            }

                        }
                        if (LoadPecas) {
                            LstFolhaObra.Last().PecasServico = ObterPecas(int.Parse(result["nopat"].ToString().Trim()));
                            LstFolhaObra.Last().GuiaTransporteAtual = LstFolhaObra.Last().PecasServico.Count() == 0 ? "" : (LstFolhaObra.Last().PecasServico.FirstOrDefault(p => p.Pos_Stock.Length > 0)?.Pos_Stock.ToString() ?? "");
                        }

                        if (LoadRubrica)
                        {
                            string img = ObterRubrica(LstFolhaObra.Last().IdFolhaObra);
                            if (!File.Exists(img)) img = "wwwroot/img/no_photo.png";
                            using Image image = Image.FromFile(img);
                            using MemoryStream m = new MemoryStream();
                            image.Save(m, image.RawFormat);
                            byte[] imageBytes = m.ToArray();

                            // Convert byte[] to Base64 String
                            string base64String = Convert.ToBase64String(imageBytes);
                            LstFolhaObra.Last().RubricaCliente = base64String;

                            LstFolhaObra.Last().IdAT = result["id_at"].ToString();
                        }
                    }
                }

                    conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os PAT's do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstFolhaObra;
        }
        public FolhaObra ObterFolhaObra(string SQL_Query, bool LoadAll)
        {
                return ObterFolhasObra(SQL_Query, LoadAll, LoadAll, LoadAll, LoadAll, LoadAll).DefaultIfEmpty(new FolhaObra()).First();
        }

        public List<FolhaObra> ObterFolhasObra(int IdMarcacao)
        {
            return ObterFolhasObra("select *, (select TOP 1 qassinou from u_intervencao, u_marcacao where u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp and u_marcacao.num=" + IdMarcacao + ") as qassinou, (select u_marcacao.u_marcacaostamp from u_marcacao where num = " + IdMarcacao + ") as u_marcacaostamp, (SELECT u_nincide from u_marcacao where num=" + IdMarcacao + ") as u_nincide, (select '"+IdMarcacao+"' as num) as num from pa where pastamp in (select STAMP_DEST from u_intervencao where u_marcacaostamp = (select u_marcacao.u_marcacaostamp from u_marcacao where num = " + IdMarcacao+ ")) order by nopat", true, true, false, false, false);
 
        }
        public List<FolhaObra> ObterFolhasObra(Cliente c)
        {
            return ObterFolhasObra("select * from pa full outer join u_intervencao on u_intervencao.STAMP_DEST=pa.pastamp full outer join u_marcacao on u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp where pa.no='" + c.IdCliente+"' and pa.estab='"+c.IdLoja+"' order by nopat;", true, true, false, false, false);

        }
        public List<FolhaObra> ObterFolhasObra(DateTime Data)
        {
            return ObterFolhasObra("select * from pa full outer join u_intervencao on u_intervencao.STAMP_DEST=pa.pastamp full outer join u_marcacao on u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp where pa.pdata='" + Data.ToString("yyyy-MM-dd")+"' order by nopat;", true, true, true, false, false);

        }
        public FolhaObra ObterFolhaObra(int IdFolhaObra)
        {
            return ObterFolhaObra("select TOP 1 (select TOP 1 obrano from bo where orinopat=" + IdFolhaObra + " and ndos=49) as id_at, * from pa full outer join u_intervencao on u_intervencao.STAMP_DEST=pa.pastamp full outer join u_marcacao on u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp where pa.nopat=" + IdFolhaObra + " order by nopat;", true);

        }

        public string ObterRubrica(int IdFolhaObra)
        {
            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select u_vestigio from bo where pastamp = (select pastamp from pa where nopat="+IdFolhaObra+");", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    result.Read();
                    return FicheirosContext.ObterCaminhoAssinatura(result["u_vestigio"].ToString());
                }
            }
            catch (Exception ex) {
                Console.WriteLine("Não foi possivel obter a rubrica!\r\n(Exception: " + ex.Message + ")");
            }
            return "wwwroot/img/no_photo.png";
        }

        private List<Intervencao> ObterIntervencoes(string SQL_Query)
        {

            List<Intervencao> LstIntervencao = new List<Intervencao>();

            try
            {
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
                            LstIntervencao.Add(new Intervencao()
                            {
                                IdIntervencao = int.Parse(result["mhid"].ToString().Trim()),
                                IdTecnico = int.Parse(result["tecnico"].ToString().Trim()),
                                IdFolhaObra = int.Parse(result["nopat"].ToString().Trim()),
                                NomeTecnico = result["tecnnm"].ToString().Trim(),
                                RelatorioServico = result["relatorio"].ToString().TrimEnd(),
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
                }
            catch (Exception ex) 
            {
                Console.WriteLine("Não foi possivel ler as intervenções do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstIntervencao.OrderBy(i => i.DataServiço).ToList();
        }
        public List<Intervencao> ObterIntervencoes(int IdFolhaObra)
        {
            return ObterIntervencoes("select nopat, mhid, tecnico, tecnnm, data, hora, horaf, relatorio from mh where nopat=" + IdFolhaObra + " order by nopat;");
        }
        public List<Intervencao> ObterHistorico(string NumeroSerie)
        {
            return ObterIntervencoes("select nopat, mhid, tecnico, tecnnm, data, hora, horaf, relatorio, serie from mh where serie='" + NumeroSerie + "' order by data;");
        }

        private List<Produto> ObterPecas(string SQL_Query)
        {

            List<Produto> LstProduto = new List<Produto>();

            try
            {
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
                            LstProduto.Add(new Produto()
                            {
                                Armazem_ID = int.Parse(result["nopat"].ToString().Trim()),
                                Ref_Produto = result["ref"].ToString().Trim(),
                                Designacao_Produto = result["design"].ToString().Trim(),
                                TipoUn = "UN",
                                Pos_Stock = result["guiatransporte"].ToString().Trim(),
                                Stock_Fisico = double.Parse(result["qtt"].ToString().Trim())
                            });
                        }
                    }

                    conn.Close();
                }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler as peças usadas pelos PAT's do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstProduto;
        }
        public List<Produto> ObterPecas(int IdFolhaObra)
        {
            return ObterPecas("select pa.nopat, bi.ref, bi.design, bi.qtt, (SELECT TOP 1 CONCAT(obrano, ' - AT ', atcodeid) from V_DOCS_GLOBAL WHERE ar2mazem=bi.armazem and dataobra<bi.dataobra and bi.ref not like '%SRV%' order by dataobra desc) as guiatransporte from pa inner join bo on bo.pastamp=pa.pastamp inner join bi on bi.obrano=bo.obrano where ref!=''  and bo.ndos=49 and pa.nopat=" + IdFolhaObra + " order by ref;");
        }
        public List<Movimentos> ObterPecasGuiaTransporte(string GuiaTransporte, int IdArmazem)
        {
            return ObterListaMovimentos("select CONCAT(V_DOCS_GLOBAL.obrano, ' - AT ', V_DOCS_GLOBAL.atcodeid) as guiatransporte, * from V_DOCS_GLOBAL, pa inner join bo on bo.pastamp=pa.pastamp inner join bi on bi.obrano=bo.obrano where ref!='' and ref not like '%SRV%' and ref not like '%IMO%' and ref not like '%PAT%' and bo.ndos=49 and bi.armazem=" + IdArmazem+" and V_DOCS_GLOBAL.ar2mazem=bi.armazem and V_DOCS_GLOBAL.dataobra<=bi.dataobra and V_DOCS_GLOBAL.obrano like '%"+GuiaTransporte+"%' order by ref;");
        }
        public List<String> ObterGuiasTransporte(int IdArmazem)
        {
            return ExecutarQuery("SELECT obrano from V_DOCS_GLOBAL where ar2mazem = "+IdArmazem+" order by dataobra desc");
        }
        #endregion

        //Obter Marcacoes
        #region MARCACOES
        public int CriarMarcacao(Marcacao m)
        {
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("Gera_Marcacao", conn)
                {
                    CommandTimeout = TIMEOUT,
                    CommandType = CommandType.StoredProcedure
                };
                m.DataMarcacao = m.DatasAdicionaisDistintas.First();

                command.Parameters.Add(new SqlParameter("@NO", m.Cliente.IdCliente));
                command.Parameters.Add(new SqlParameter("@ESTAB", m.Cliente.IdLoja));
                command.Parameters.Add(new SqlParameter("@TECNICO", m.Tecnico.IdPHC));
                command.Parameters.Add(new SqlParameter("@TECNICOS", string.Join(";", m.LstTecnicos.Select(x => x.IdPHC))));
                command.Parameters.Add(new SqlParameter("@ESTADO", m.EstadoMarcacaoDesc));
                command.Parameters.Add(new SqlParameter("@PERIODO", m.Periodo));
                command.Parameters.Add(new SqlParameter("@DATAPEDIDO", m.DataPedido.ToString("yyyyMMdd")));
                command.Parameters.Add(new SqlParameter("@DATA", m.DataMarcacao.ToString("yyyyMMdd")));
                command.Parameters.Add(new SqlParameter("@MDATAS", string.Join(";", m.DatasAdicionaisDistintas.Select(x => x.ToString("yyyyMMdd")))));
                command.Parameters.Add(new SqlParameter("@HORA", (!String.IsNullOrEmpty(m.Hora) ? (m.Hora == "00:00" ? "" : DateTime.Parse(m.Hora).ToString("HHmm")) : "")));
                command.Parameters.Add(new SqlParameter("@PRIORIDADE", m.PrioridadeMarcacao));
                command.Parameters.Add(new SqlParameter("@TIPOS", m.TipoServico));
                command.Parameters.Add(new SqlParameter("@TIPOE", m.TipoEquipamento));
                command.Parameters.Add(new SqlParameter("@TIPOPEDIDO", m.TipoPedido));
                command.Parameters.Add(new SqlParameter("@NINCIDENTE", m.Referencia));
                command.Parameters.Add(new SqlParameter("@QPEDIU", m.QuemPediuNome));
                command.Parameters.Add(new SqlParameter("@RESPTLM", m.QuemPediuTelefone));
                command.Parameters.Add(new SqlParameter("@RESPEMAIL", m.QuemPediuEmail));
                command.Parameters.Add(new SqlParameter("@RESUMO", m.ResumoMarcacao));
                command.Parameters.Add(new SqlParameter("@PIQUETE", m.Piquete ? 1 : 0));
                command.Parameters.Add(new SqlParameter("@OFICINA ", m.Oficina ? 1 : 0));
                command.Parameters.Add(new SqlParameter("@NOME_UTILIZADOR", m.Utilizador.NomeCompleto));

                using SqlDataReader result = command.ExecuteReader();
                result.Read();

                if (result[0].ToString() != "-1")
                {
                    m.IdMarcacao = int.Parse(result[3].ToString());

                    ChatContext.EnviarNotificacaoMarcacaoTecnico(m, m.Tecnico);
                    FT_ManagementContext.AdicionarLog(m.Utilizador.Id, "Marcação criada com sucesso! - Nº " + m.IdMarcacao + ", " + m.Cliente.NomeCliente + " pelo utilizador " + m.Utilizador.NomeCompleto, 5);
                }

                conn.Close();


            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel enviar marcacao para o PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return m.IdMarcacao;
        }
        public bool AtualizaMarcacao(Marcacao m)
        {
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("Altera_Marcacao", conn)
                {
                    CommandTimeout = TIMEOUT,
                    CommandType = CommandType.StoredProcedure
                };
                m.DataMarcacao = m.DatasAdicionaisDistintas.First();

                command.Parameters.Add(new SqlParameter("@U_MARCACAOSTAMP", m.MarcacaoStamp));
                command.Parameters.Add(new SqlParameter("@NO", m.Cliente.IdCliente));
                command.Parameters.Add(new SqlParameter("@ESTAB", m.Cliente.IdLoja));
                command.Parameters.Add(new SqlParameter("@TECNICO", m.Tecnico.IdPHC));
                command.Parameters.Add(new SqlParameter("@TECNICOS", string.Join(";", m.LstTecnicos.Select(x => x.IdPHC))));
                command.Parameters.Add(new SqlParameter("@ESTADO", m.EstadoMarcacaoDesc));
                command.Parameters.Add(new SqlParameter("@PERIODO", m.Periodo));
                command.Parameters.Add(new SqlParameter("@DATAPEDIDO", m.DataPedido.ToString("yyyyMMdd")));
                command.Parameters.Add(new SqlParameter("@MDATAS", string.Join(";", m.DatasAdicionaisDistintas.Select(x => x.ToString("yyyyMMdd")))));
                command.Parameters.Add(new SqlParameter("@DATA", m.DataMarcacao.ToString("yyyyMMdd")));
                command.Parameters.Add(new SqlParameter("@HORA", (!String.IsNullOrEmpty(m.Hora) ? (m.Hora == "00:00" ? "" : DateTime.Parse(m.Hora).ToString("HHmm")) : "")));
                command.Parameters.Add(new SqlParameter("@PRIORIDADE", m.PrioridadeMarcacao));
                command.Parameters.Add(new SqlParameter("@TIPOS", m.TipoServico));
                command.Parameters.Add(new SqlParameter("@TIPOE", m.TipoEquipamento));
                command.Parameters.Add(new SqlParameter("@TIPOPEDIDO", m.TipoPedido));
                command.Parameters.Add(new SqlParameter("@NINCIDENTE", m.Referencia));
                command.Parameters.Add(new SqlParameter("@QPEDIU", m.QuemPediuNome));
                command.Parameters.Add(new SqlParameter("@RESPTLM", m.QuemPediuTelefone));
                command.Parameters.Add(new SqlParameter("@RESPEMAIL", m.QuemPediuEmail));
                command.Parameters.Add(new SqlParameter("@RESUMO", m.ResumoMarcacao));
                command.Parameters.Add(new SqlParameter("@PIQUETE", m.Piquete ? 1 : 0));
                command.Parameters.Add(new SqlParameter("@OFICINA ", m.Oficina ? 1 : 0));
                command.Parameters.Add(new SqlParameter("@NOME_UTILIZADOR", m.Utilizador.NomeCompleto));
                command.Parameters.Add(new SqlParameter("@JUSTFECHO", m.JustificacaoFecho.Length > 4000 ? m.JustificacaoFecho.Remove(4000) : m.JustificacaoFecho)); 
                command.Parameters.Add(new SqlParameter("@TECFECHO", m.Utilizador.NomeCompleto));

                using SqlDataReader result = command.ExecuteReader();
                result.Read();

                string resp = result[0].ToString();

                if (resp != "-1")
                {
                    ChatContext.EnviarNotificacaoAtualizacaoMarcacaoTecnico(m, m.Tecnico);
                    FT_ManagementContext.AdicionarLog(m.Utilizador.Id, "Marcação atualizada com sucesso! - Nº " + m.IdMarcacao + ", " + m.Cliente.NomeCliente + " pelo utilizador " + m.Utilizador.NomeCompleto, 5);
                    conn.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel atualizar a marcacao para o PHC!\r\n(Exception: " + ex.Message + ")");
                return false;
            }

            return false;
        }
        public string ValidarMarcacao(Marcacao m)
        {
            string res = "";
            //Tem de retornar 1 para poder proceder
            if (ExecutarQuery("SELECT 'valor' = COUNT(1) FROM bo (NOLOCK) INNER JOIN bi (NOLOCK) ON bo.bostamp = bi.bostamp WHERE bo.ndos = 45 AND bo.fechada = 0 AND bo.no = '" + m.Cliente.IdCliente + "' AND bo.estab = '" + m.Cliente.IdLoja + "' AND bi.ref IN ('SRV.101', 'SRV.102', 'SRV.103');").First() == "0") res += "O Cliente escolhido na Marcação não tem uma tabela de preços definida! Por favor defina uma tabela de preços antes da marcação.\r\n";

            //Quando é cliente pingo doce tem obrigatoriamente de ter quem pediu para avancar
            if (m.Cliente.IdCliente == 878 && String.IsNullOrEmpty(m.QuemPediuNome)) res += "Tem que indicar Quem Pediu!\r\n";

            //O cliente não pode ter as marcacoes canceladas
            if (ExecutarQuery("SELECT U_MARCCANC FROM cl (NOLOCK) WHERE no = '" + m.Cliente.IdCliente + "' AND estab = '" + m.Cliente.IdLoja + "'").First() == "True") res += "O Cliente escolhido na Marcação tem as marcações canceladas. Não pode gravar.\r\n";

            //Caso esteja a 0 faz a validação da conta corrente
            if (ExecutarQuery("SELECT u_navdivma FROM CL(Nolock) WHERE cl.no='" + m.Cliente.IdCliente + "' AND cl.estab = 0").First() == "False")
            {
                //Verificar documentos vencidos
                if (ExecutarQuery("select 'valor' = COUNT(*) from cc (nolock) left join re (nolock) on cc.restamp = re.restamp where cc.no = " + m.Cliente.IdCliente + " and (case when cc.moeda ='EURO' or cc.moeda=space(11) then abs((cc.edeb-cc.edebf)-(cc.ecred-cc.ecredf)) else abs((cc.debm-cc.debfm)-(cc.credm-cc.credfm)) end) > (case when cc.moeda='EURO' or cc.moeda=space(11) then 0.010000 else 0 end) AND cc.dataven < GETDATE();").First() != "0") res += "O Cliente escolhido na Marcação tem documentos não regularizados vencidos!!! Verifique por favor!\r\n";
            }
            foreach (var item in m.LstTecnicosSelect)
            {
                if (FT_ManagementContext.VerificarFeriasUtilizador(item, m.DataMarcacao)) res += "O utilizador, " + FT_ManagementContext.ObterUtilizador(item).NomeCompleto + ", encontra-se de férias nesta data! Por favor verifique.\r\n";
            }
            List<DateTime> LstFeriados = FT_ManagementContext.ObterListaFeriados(m.DataMarcacao.Year.ToString()).Select(f => f.DataFeriado).ToList();
            foreach (var item in m.DatasAdicionaisDistintas)
            {
                if (LstFeriados.Where(f => f.ToShortDateString() == item.ToShortDateString()).Count() > 0) res += "O seguinte dia: " + item.ToShortDateString() + " é feriado! Por favor verifique.\r\n";
            }
            return res;
        }
        public string CriarAnexoMarcacao(Anexo a)
        {
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("Gera_Anexo", conn)
                {
                    CommandTimeout = TIMEOUT,
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add(new SqlParameter("@U_MARCACAOSTAMP", a.MarcacaoStamp));
                command.Parameters.Add(new SqlParameter("@NOME_FICHEIRO", a.NomeFicheiro));
                command.Parameters.Add(new SqlParameter("@MARCACAO", a.AnexoMarcacao ? "1" : "0"));
                command.Parameters.Add(new SqlParameter("@ASSINATURA", a.AnexoAssinatura ? "1" : "0"));
                command.Parameters.Add(new SqlParameter("@INSTALACAO", a.AnexoInstalacao ? "1" : "0"));
                command.Parameters.Add(new SqlParameter("@PECA", a.AnexoPeca ? "1" : "0"));
                command.Parameters.Add(new SqlParameter("@EMAIL", a.AnexoEmail ? "1" : "0"));
                command.Parameters.Add(new SqlParameter("@REF", a.RefPeca));
                command.Parameters.Add(new SqlParameter("@NOME_UTILIZADOR", a.NomeUtilizador));
                command.Parameters.Add(new SqlParameter("@TITULO", a.ObterNomeLegivel()));
                command.Parameters.Add(new SqlParameter("@OUSRDATA", DateTime.Now.ToString("yyyyMMdd")));
                command.Parameters.Add(new SqlParameter("@OUSRHORA", DateTime.Now.ToShortTimeString()));

                using SqlDataReader result = command.ExecuteReader();
                result.Read();

                string resp = result[2].ToString();

                if (resp != "-1") return resp;
            }
            catch (Exception ex) {
                Console.WriteLine("Não foi possivel criar o anexo da marcação do PHC!\r\n(Exception: " + ex.Message + ")");
            }
            return "";
        }
        public bool ApagarAnexoMarcacao(Anexo a)
        {
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("Apaga_Anexo", conn)
                {
                    CommandTimeout = TIMEOUT,
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add(new SqlParameter("STAMP", a.AnexoStamp));

                using SqlDataReader result = command.ExecuteReader();
                result.Read();

                string resp = result[0].ToString();

                if (resp != "-1") return true;

            }
            catch (Exception ex) {
                Console.WriteLine("Não foi possivel apagar o anexo da marcação do PHC!\r\n(Exception: " + ex.Message + ")");
            }
            return false;
        }
        public List<Anexo> ObterAnexos(Marcacao m)
        {
            List<Anexo> LstAnexos = new List<Anexo>();

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select * from u_anexos_mar where u_marcacaostamp='"+m.MarcacaoStamp+"'", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using SqlDataReader result = command.ExecuteReader();
                while (result.Read())
                {
                   LstAnexos.Add(new Anexo()
                    {
                        AnexoStamp = result["u_anexos_marstamp"].ToString(),
                        MarcacaoStamp = result["u_marcacaostamp"].ToString(),
                        IdMarcacao = int.Parse(result["num_marcacao"].ToString()),
                        NomeFicheiro = result["nome_ficheiro"].ToString(),
                        NomeUtilizador = result["ousrinis"].ToString(),
                        AnexoMarcacao = result["marcacao"].ToString() == "True",
                        AnexoAssinatura = result["assinatura"].ToString() == "True",
                        AnexoInstalacao = result["instalacao"].ToString() == "True",
                        AnexoPeca = result["peca"].ToString() == "1",
                        RefPeca = result["ref"].ToString(),
                        DataCriacao = DateTime.Parse(result["ousrdata"].ToString().Split(" ").First() + " " + result["ousrhora"].ToString()),
                        DescricaoFicheiro = result["Titulo"].ToString(),
                        AnexoEmail = result["email"].ToString() == "True"

                   });
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os anexos da marcacao do PHC!\r\n(Exception: " + ex.Message + ")");
            }
            return LstAnexos;
        }
        public Anexo ObterAnexo(string AnexoStamp)
        {
            List<Anexo> LstAnexos = new List<Anexo>();

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select * from u_anexos_mar where u_anexos_marstamp='" + AnexoStamp + "'", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using SqlDataReader result = command.ExecuteReader();
                while (result.Read())
                {
                    LstAnexos.Add(new Anexo()
                    {
                        AnexoStamp = result["u_anexos_marstamp"].ToString(),
                        MarcacaoStamp = result["u_marcacaostamp"].ToString(),
                        IdMarcacao = int.Parse(result["num_marcacao"].ToString()),
                        NomeFicheiro = result["nome_ficheiro"].ToString(),
                        NomeUtilizador = result["ousrinis"].ToString(),
                        AnexoMarcacao = result["marcacao"].ToString() == "1",
                        AnexoAssinatura = result["assinatura"].ToString() == "1",
                        AnexoInstalacao = result["instalacao"].ToString() == "1",
                        AnexoPeca = result["peca"].ToString() == "1",
                        RefPeca = result["ref"].ToString(),
                        DataCriacao = DateTime.Parse(DateTime.Parse(result["ousrdata"].ToString()).ToShortDateString() + " " + result["ousrhora"].ToString()),
                        DescricaoFicheiro = result["Titulo"].ToString(),
                        AnexoEmail = result["email"].ToString() == "1"
                    });
                }
                conn.Close();
                return LstAnexos.First();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os anexos da marcacao do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return new Anexo();
        }
        public List<Anexo> ObterAnexos()
        {
            List<Anexo> LstAnexos = new List<Anexo>();

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select * from u_anexos_mar;", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using SqlDataReader result = command.ExecuteReader();
                while (result.Read())
                {
                    LstAnexos.Add(new Anexo()
                    {
                        AnexoStamp = result["u_anexos_marstamp"].ToString(),
                        MarcacaoStamp = result["u_marcacaostamp"].ToString(),
                        IdMarcacao = int.Parse(result["num_marcacao"].ToString()),
                        NomeFicheiro = result["nome_ficheiro"].ToString(),
                        NomeUtilizador = result["ousrinis"].ToString(),
                        AnexoMarcacao = result["marcacao"].ToString() == "True",
                        AnexoAssinatura = result["assinatura"].ToString() == "True",
                        AnexoInstalacao = result["instalacao"].ToString() == "True",
                        AnexoPeca = result["peca"].ToString() == "1",
                        RefPeca = result["ref"].ToString(),
                        DataCriacao = DateTime.Parse(result["ousrdata"].ToString().Split(" ").First() + " " + result["ousrhora"].ToString()),
                        DescricaoFicheiro = result["Titulo"].ToString(),
                        AnexoEmail = result["email"].ToString() == "True"

                    });
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os anexos da marcacao do PHC!\r\n(Exception: " + ex.Message + ")");
            }
            return LstAnexos;
        }

        public Marcacao ObterResponsavelCliente(int IdCliente, int IdLoja, string TipoEquipamento)
        {
            Marcacao m = new Marcacao();

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select u_clresp.nome, u_clresp.email, u_clresp.tlmvl from u_clresp inner join cl on cl.clstamp=u_clresp.clstamp where cl.no=" + IdCliente + " and cl.estab=" + IdLoja + " and u_clresp.tipoe='" + TipoEquipamento + "'", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using SqlDataReader result = command.ExecuteReader();
                while (result.Read())
                {
                    m = new Marcacao()
                    {
                        QuemPediuNome = result["nome"].ToString(),
                        QuemPediuEmail = result["email"].ToString(),
                        QuemPediuTelefone = result["tlmvl"].ToString(),

                    };
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os reponsaveis do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return m;
        }

        private List<Marcacao> ObterMarcacoes(string SQL_Query, bool LoadComentarios, bool LoadCliente, bool LoadTecnico, bool LoadFolhasObra, bool LoadAnexos, bool LoadDossiers)
        {
            List<Utilizador> LstUtilizadores = FT_ManagementContext.ObterListaUtilizadores(false, false);
            List<EstadoMarcacao> LstEstadoMarcacao = this.ObterMarcacaoEstados();
            List<Marcacao> LstMarcacao = new List<Marcacao>();
            List<Cliente> LstClientes = this.ObterClientes();
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand(SQL_Query, conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using SqlDataReader result = command.ExecuteReader();
                while (result.Read())
                {
                    List<DateTime> LstDatasParse = new List<DateTime>() { DateTime.Parse(result["data"].ToString()) };

                    LstMarcacao.Add(new Marcacao()
                    {
                        IdMarcacao = int.Parse(result["num"].ToString().Trim()),
                        DatasAdicionais = result["u_mdatas"].ToString().Replace("|", ";"),
                        ResumoMarcacao = result["resumo"].ToString().Trim(),
                        EstadoMarcacaoDesc = result["estado"].ToString().Trim(),
                        DataMarcacao = DateTime.Parse(result["data"].ToString()),
                        EstadoMarcacao = LstEstadoMarcacao.Where(e => e.EstadoMarcacaoDesc == result["estado"].ToString().Trim()).DefaultIfEmpty(new EstadoMarcacao()).First().IdEstado,
                        PrioridadeMarcacao = result["prioridade"].ToString().Trim(),
                        MarcacaoStamp = result["u_marcacaostamp"].ToString().Trim(),
                        TipoEquipamento = result["tipoe"].ToString().Trim(),
                        Oficina = result["oficina"].ToString().Trim() == "True",
                        Piquete = result["piquete"].ToString().Trim() == "True",
                        TipoServico = result["tipos"].ToString().Trim(),
                        DataCriacao = DateTime.Parse(DateTime.Parse(result["ousrdata"].ToString().Trim()).ToShortDateString() + " " + result["ousrhora"].ToString()),
                        Periodo = result["Periodo"].ToString(),
                        Referencia = result["nincidente"].ToString(),
                        DataPedido = DateTime.Parse(result["datapedido"].ToString()),
                        TipoPedido = result["tipopedido"].ToString(),
                        QuemPediuNome = result["qpediu"].ToString(),
                        QuemPediuEmail = result["respemail"].ToString(),
                        QuemPediuTelefone = result["resptlm"].ToString(),
                        Utilizador = new Utilizador() { NomeCompleto = result["ousrinis"].ToString() },
                        Hora = result["hora"].ToString().Length == 4 ? result["hora"].ToString()[..2] + ":" + result["hora"].ToString().Substring(2, 2) : "",
                    });
                    if (LoadCliente) LstMarcacao.Last().Cliente = LstClientes.Where(c => c.IdCliente == int.Parse(result["no"].ToString().Trim())).Where(c => c.IdLoja == int.Parse(result["estab"].ToString().Trim())).DefaultIfEmpty(new Cliente()).First();
                    if (LoadComentarios) LstMarcacao.Last().LstComentarios = ObterComentariosMarcacao(int.Parse(result["num"].ToString().Trim()));
                    if (LoadAnexos) LstMarcacao.Last().LstAnexos = ObterAnexos(LstMarcacao.Last());
                    if (LoadTecnico)
                    {
                        LstMarcacao.Last().Tecnico = string.IsNullOrEmpty(result["tecnno"].ToString()) ? new Utilizador() : (LstUtilizadores.Where(u => u.IdPHC == int.Parse(result["tecnno"].ToString().Trim())).FirstOrDefault() ?? new Utilizador());

                        try
                        {
                            foreach (var item in result["LstTecnicos"].ToString().Split(";"))
                            {
                               if (!string.IsNullOrEmpty(item)) LstMarcacao.Last().LstTecnicos.Add(LstUtilizadores.Where(u => u.IdPHC == int.Parse(item)).FirstOrDefault() ?? new Utilizador());
                            }
                            foreach (var item in LstMarcacao.Last().LstTecnicos)
                            {
                                LstMarcacao.Last().LstTecnicosSelect.Add(item.Id);
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Não foi possivel obter a lista de técnicos do PHC!\r\n(Exception: " + ex.Message + ")");
                        }
                    }
                    if (LoadFolhasObra) LstMarcacao.Last().LstFolhasObra = ObterFolhasObra(int.Parse(result["num"].ToString().Trim()));
                    if (LoadDossiers) { LstMarcacao.Last().LstAtividade = ObterAtivivade(LstMarcacao.Last()); }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler as Marcacoes do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstMarcacao;
        }
        private Marcacao ObterMarcacao(string SQL_Query, bool LoadAll)
        {
            return ObterMarcacoes(SQL_Query, LoadAll, LoadAll, LoadAll, LoadAll, LoadAll, LoadAll).DefaultIfEmpty(new Marcacao()).First();
        }
        public List<Marcacao> ObterMarcacoes(int IdTecnico, DateTime DataMarcacoes)
        {
            //List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, u_mdatas.periodo, prioridade, u_marcacao.u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp WHERE u_mtecnicos.tecnno='" + IdTecnico + "' and u_mdatas.data='" + DataMarcacoes.ToString("yyyy-MM-dd") + "' and estado!='Cancelado' order by num;", true, true, true, false, false, false);
            //LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas,  no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, u_marcacao.ousrdata, hora, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE u_mtecnicos.tecnno='" + IdTecnico + "' and data='" + DataMarcacoes.ToString("yyyy-MM-dd") + "' and estado!='Cancelado' order by num;", true, true, true, false, false, false).AsEnumerable());
            //return LstMarcacoes;

            return ObterMarcacoes("select * from v_marcacoes  where tecnno = "+IdTecnico+" and data = '"+DataMarcacoes.ToString("yyyyMMdd")+"'", true, true, true, false, false, false).OrderBy(m => m.IdMarcacao).ToList();
        }
        public List<Marcacao> ObterMarcacoes(int numMarcacao, string nomeCliente, string referencia, string tipoe, int idtecnico, string estado)
        {
            //string SQL_Query = "SELECT TOP 200 num, data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas,  no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, u_marcacao.ousrdata, hora, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao full outer join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 where " + (numMarcacao > 0 ? "num like '%" + numMarcacao + "%' and " : "") + (!string.IsNullOrEmpty(nomeCliente) ? "nome like '%" + nomeCliente + "%' and " : "") + (!string.IsNullOrEmpty(estado) && estado != "Todos" ? "estado = '" + estado + "' and " : "") + (!string.IsNullOrEmpty(referencia) ? "nincidente like '%" + referencia + "%' and " : "") + (!string.IsNullOrEmpty(tipoe) && tipoe != "Todos" ? "tipoe like '%" + tipoe + "%' and " : "") + (idtecnico > 0 ? "u_mtecnicos.tecnno=" + idtecnico + " and " : "") + " num is not null";

            //List<Marcacao> LstMarcacoes = ObterMarcacoes(SQL_Query + " order by num desc;", false, true, true, false, false, false);
            //return LstMarcacoes;

            string SQL_Query = "select TOP 200 * from v_marcacoes where ";
            SQL_Query += (numMarcacao > 0 ? "num like '%" + numMarcacao + "%' and " : "");
            SQL_Query += (idtecnico > 0 ? "tecnno like '%" + idtecnico + "%' and " : "");
            SQL_Query += (nomeCliente != "" ? "nome like '%" + nomeCliente + "%' and " : "");
            SQL_Query += (referencia != "" ? "nincidente like '%" + referencia + "%' and " : "");
            SQL_Query += (tipoe != "" ? "tipoe like '%" + tipoe + "%' and " : "");
            SQL_Query += (estado != "" ? "estado like '%" + estado + "%' and " : "");
            SQL_Query = SQL_Query.Remove(SQL_Query.Length - 4, 4);
            SQL_Query += " order by data desc;";

            return ObterMarcacoes(SQL_Query, false, true, true, false, false, false).OrderByDescending(m => m.IdMarcacao).ToList();
        }
        public List<Marcacao> ObterMarcacoes(DateTime DataInicio, DateTime DataFim)
        {
            
            //List <Marcacao> LstMarcacoes = (ObterMarcacoes("SELECT num, data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, u_mtecnicos.tecnno, no, estab, tipos, tipoe, u_marcacao.u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, u_mtecnicos.tecnnm, nome, estado, hora, periodo, prioridade, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis , resumo FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 WHERE data>='" + DataInicio.ToString("yyyy-MM-dd") + "'  AND data<='" + DataFim.ToString("yyyy-MM-dd") + "';", false, true, true, false, false, false));
            //LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, u_mdatas.data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, u_mtecnicos.tecnno,  u_marcacao.u_marcacaostamp, no, estab, tipos, tipoe, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, u_mtecnicos.tecnnm, nome, estado, hora, u_mdatas.periodo, prioridade, u_marcacao.ousrdata, u_marcacao.ousrhora, resumo, u_marcacao.ousrinis   FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp WHERE u_mdatas.data>='" + DataInicio.ToString("yyyy-MM-dd") + "'  AND u_mdatas.data<='" + DataFim.ToString("yyyy-MM-dd") + "' ;", false, true, true, false, false, false).AsEnumerable());
            //return LstMarcacoes;

            return ObterMarcacoes("select * from v_marcacoes where data between '" + DataInicio.ToString("yyyyMMdd") + "' and '" + DataFim.ToString("yyyyMMdd") + "'", false, true, true, false, false, false).OrderBy(m => m.IdMarcacao).ToList();
        }
        public List<Marcacao> ObterMarcacoes(int IdTecnico, DateTime DataInicio, DateTime DataFim)
        {
            //List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, u_mdatas.periodo, prioridade, u_marcacao.u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp  WHERE u_mtecnicos.tecnno='" + IdTecnico + "' and  u_mdatas.data>='" + DataInicio.ToString("yyyy-MM-dd") + "'  AND u_mdatas.data<='" + DataFim.ToString("yyyy-MM-dd") + "' order by u_mdatas.data;", false, true, true, false, false, false);
            //LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, u_marcacao.ousrdata, hora, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE u_mtecnicos.tecnno='" + IdTecnico + "' and  data>='" + DataInicio.ToString("yyyy-MM-dd") + "'  AND data<='" + DataFim.ToString("yyyy-MM-dd") + "' order by data;", false, true, true, false, false, false).AsEnumerable());
            //return LstMarcacoes;
            return ObterMarcacoes("select * from v_marcacoes where tecnno="+IdTecnico+" and data between '" + DataInicio.ToString("yyyyMMdd") + "' and '" + DataFim.ToString("yyyyMMdd") + "'", false, true, true, false, false, false).OrderBy(m => m.IdMarcacao).ToList();
        }
        public List<Marcacao> ObterMarcacoes(Cliente c)
        {
            //List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, u_mdatas.periodo, prioridade, u_marcacao.u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp  WHERE no='" + c.IdCliente+"' and estab='"+c.IdLoja+"' order by u_mdatas.data;", true, true, true, true, false, false);
            //LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE no='" + c.IdCliente + "' and estab='" + c.IdLoja + "' order by data;", true, true, true, true, false, false).AsEnumerable());
            //return LstMarcacoes;

            return ObterMarcacoes("select * from v_marcacoes where no="+c.IdCliente+" and estab="+c.IdLoja, true, true, true, true, false, false).OrderBy(m => m.IdMarcacao).ToList();
        }
        public List<Marcacao> ObterMarcacoesPendentes()
        {
            //List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE estado in ('Pedido Orçamento', 'Pedido Peças', 'Pedido Cotação', 'Pedido Fornecedor', 'Pedido Garantia', 'Stock Maia', 'Enc. a Fornecedor', 'Enc. de Cliente', 'Peças na Maia') order by data;", false, true, true, false, false, true);
            //return LstMarcacoes;

            return ObterMarcacoes("select * from v_marcacoes where estado in ('Pedido Orçamento', 'Pedido Peças', 'Pedido Cotação', 'Pedido Fornecedor', 'Pedido Garantia', 'Stock Maia', 'Enc. a Fornecedor', 'Enc. de Cliente', 'Peças na Maia');", false, true, true, false, false, true).OrderBy(m => m.DataMarcacao).ToList();

        }
        public List<Marcacao> ObterMarcacoesSimples()
        {
            //List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao full outer join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE estado not in ('AT Validada', 'Aguarda Ped. Compra') order by estado;", false, false, false, false, false, false);
            //return LstMarcacoes;

            return ObterMarcacoes("select * from v_marcacoes WHERE estado not in ('AT Validada', 'Aguarda Ped. Compra') order by estado;", false, false, false, false, false, false).OrderBy(m => m.IdMarcacao).ToList();

        }
        public List<Marcacao> ObterMarcacoesCriadas()
        {
            //List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao full outer join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE estado not in ('AT Validada', 'Aguarda Ped. Compra') order by estado;", false, false, false, false, false, false);
            //return LstMarcacoes;

            return ObterMarcacoes("select * from v_marcacoes WHERE estado='Criado';", false, true, true, false, false, false).OrderBy(m => m.IdMarcacao).ToList();

        }
        public List<Marcacao> ObterMarcacoesPendentes(int IdTecnico)
        {
            //List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, u_mdatas.periodo, prioridade, u_marcacao.u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp  WHERE u_mtecnicos.tecnno='" + IdTecnico + "' AND estado not in ('Finalizado', 'Cancelado', 'AT Validada', 'Aguarda Ped. Compra') order by u_mdatas.data;", true, true, true, false, false, false);
            //LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE u_mtecnicos.tecnno='" + IdTecnico + "' AND estado not in ('Finalizado', 'Cancelado', 'AT Validada', 'Aguarda Ped. Compra') order by data;", true, true, true, false, false, false).AsEnumerable());
            //return LstMarcacoes;

            return ObterMarcacoes("select * from v_marcacoes WHERE tecnno='" + IdTecnico + "' AND estado not in ('Finalizado', 'Cancelado', 'AT Validada', 'Aguarda Ped. Compra') and data <= '"+DateTime.Now.ToString("yyyy-MM-dd")+"';", true, true, true, false, false, false).OrderBy(m => m.IdMarcacao).ToList();
        }
        public List<Marcacao> ObterMarcacoes()
        {
            //List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.dat, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatasa, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, u_mdatas.periodo, prioridade, u_marcacao.u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp order by u_mdatas.data;", false, true, true, false, false, false);
            //LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 order by data;", false, true, true, false, false, false).AsEnumerable());
            //return LstMarcacoes;

            return ObterMarcacoes("select * from v_marcacoes", false, true, true, false, false, false).OrderBy(m => m.IdMarcacao).ToList();
        }
        public Marcacao ObterMarcacao(int IdMarcacao)
        {
            //return ObterMarcacao("SELECT num, data, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, estab, tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, ousrdata, ousrhora, u_marcacao.ousrinis  FROM u_marcacao where num='" + IdMarcacao + "' order by num;", true);
            return ObterMarcacao("select * from v_marcacoes where num=" + IdMarcacao + "", true);
        }
        public List<int> ObterPercentagemMarcacoes(int IdTecnico)
        {
            List<int> res = new List<int>();

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("select Count(*) as Total from V_Marcacoes where tecnno = '" + IdTecnico + "' union all select Count(*) as Completas  from V_Marcacoes where tecnno = '" + IdTecnico+"' and estado not like 'Agendado'", conn)
                {
                    CommandTimeout = TIMEOUT
                };

                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        res.Add(int.Parse(result[0].ToString()));
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler as quantidades de marcações por técnico!\r\n(Exception: " + ex.Message + ")");
            }

            return res;

        }
        public List<Atividade> ObterAtivivade(Marcacao m)
        {
            List<Atividade> LstAtividade = new List<Atividade>();

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select *, bo.ousrhora as hora from bo3 inner join bo on bo.bostamp=bo3.bo3stamp where bo3.u_stampmar='" + m.MarcacaoStamp + "';", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using SqlDataReader result = command.ExecuteReader();
                while (result.Read())
                {
                    LstAtividade.Add(new Atividade()
                    {
                        Nome = result["nmdos"].ToString(),
                        CriadoPor = string.IsNullOrEmpty(result["inome"].ToString()) ? result["tabela1"].ToString() : result["inome"].ToString(),
                        Id = result["obrano"].ToString(),
                        Data = DateTime.Parse(DateTime.Parse(result["dataobra"].ToString()).ToShortDateString() + " " + result["hora"].ToString()),
                        Tipo = int.Parse(result["ndos"].ToString())
                    });
                }

                conn.Close();

                LstAtividade.AddRange(ObterAtividadeEmail(m).AsEnumerable());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os dossiers do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstAtividade.OrderBy(d => d.Data).ToList();
        }
        public List<Atividade> ObterAtividadeEmail(Marcacao m)
        {
            List<Atividade> LstAtividade = new List<Atividade>();

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select * from u_anexos_mar where u_marcacaostamp='" + m.MarcacaoStamp + "' and email=1;", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using SqlDataReader result = command.ExecuteReader();
                while (result.Read())
                {
                    LstAtividade.Add(new Atividade()
                    {
                        Nome = result["TITULO"].ToString(),
                        CriadoPor = result["ousrinis"].ToString(),
                        Id = "Email",
                        Data = DateTime.Parse(DateTime.Parse(result["ousrdata"].ToString()).ToShortDateString() + " " + result["ousrhora"].ToString()),
                        Tipo = 0
                    });
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os anexos de email do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstAtividade.OrderBy(d => d.Data).ToList();

        }

        private List<EstadoMarcacao> ObterMarcacaoEstados(string SQL_Query)
        {

            List<EstadoMarcacao> LstEstadoMarcacao = new List<EstadoMarcacao>();

            try
            {
                    SqlConnection conn = new SqlConnection(ConnectionString);

                    conn.Open();

                    SqlCommand command = new SqlCommand(SQL_Query, conn);
                    int i = 1;
                    command.CommandTimeout = TIMEOUT;
                    using (SqlDataReader result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            LstEstadoMarcacao.Add(new EstadoMarcacao()
                            {
                                IdEstado = int.Parse(result["id"].ToString().Trim()),
                                EstadoMarcacaoDesc = result["estado"].ToString().Trim()
                            });
                            i++;
                        }
                    }

                    conn.Close();
                }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os Estados do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstEstadoMarcacao.OrderBy(e => e.EstadoMarcacaoDesc).ToList();
        }
        public List<EstadoMarcacao> ObterMarcacaoEstados()
        {
            return ObterMarcacaoEstados("select ROW_NUMBER()  OVER (ORDER BY (Select 0)) as Id, * from u_estados;");
        }
        public EstadoMarcacao ObterMarcacaoEstado(string Estado)
        {
            List<EstadoMarcacao> e = ObterMarcacaoEstados("select ROW_NUMBER()  OVER (ORDER BY (Select 0)) as Id, * from u_estados;");
            return e.Where(e => e.EstadoMarcacaoDesc == Estado).FirstOrDefault();
        }

        public List<String> ObterTipoEquipamento() {

            List<String> res = new List<String>();

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("select tequip from u_tequip", conn)
                {
                    CommandTimeout = TIMEOUT
                };

                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        res.Add(result[0].ToString());
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os tipos de equipamento!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }
        public List<String> ObterTipoServico()
        {

            List<String> res = new List<String>
            {
                ""
            };

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("select tipo from u_tservico order by lordem", conn)
                {
                    CommandTimeout = TIMEOUT
                };

                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        res.Add(result[0].ToString());
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os tipos de serviço!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }
        public List<String> ObterPeriodo()
        {

            List<String> res = new List<String>
            {
                ""
            };

            try
            {
                res.Add("Manhã");
                res.Add("Tarde");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel obter periodos!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }
        public List<String> ObterPrioridade()
        {

            List<String> res = new List<String>();

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("select prioridade from u_prioridade order by lordem", conn)
                {
                    CommandTimeout = TIMEOUT
                };

                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        res.Add(result[0].ToString());
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler as prioridades!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }
        public List<String> ObterTipoPedido()
        {

            List<String> res = new List<String>
            {
                ""
            };

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("select tpedido from u_tpedido", conn)
                {
                    CommandTimeout = TIMEOUT
                };

                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        res.Add(result[0].ToString());
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel obter o tipo de pedido!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }
        public List<String> ObterEstadoFolhaObra()
        {

            List<String> res = new List<String>
            {
                "Concluído",
                "Pedido de Peças",
                "Pedido de Orçamento"
            };

            return res;
        }
        public List<String> ObterTipoFolhaObra()
        {

            List<String> res = new List<String>
            {
                "Externo",
                "Interno",
                "Remoto",
                "Instalação"
            };

            return res;
        }

        public bool CriarComentarioMarcacao(Comentario c)
        {
            bool res = false;

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("Gera_Comentario", conn)
                {
                    CommandTimeout = TIMEOUT,
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add(new SqlParameter("@U_MARCACAOSTAMP", c.Marcacao.MarcacaoStamp));
                command.Parameters.Add(new SqlParameter("@COMENTARIO", c.Descricao.Length > 4000 ? c.Descricao.Remove(4000) : c.Descricao));
                command.Parameters.Add(new SqlParameter("@NOME_UTILIZADOR", c.Utilizador.NomeCompleto));
                //command.Parameters.Add(new SqlParameter("@TECNICO", c.DataComentario.ToString()));

                using SqlDataReader result = command.ExecuteReader();
                result.Read();

                res = (result[0].ToString() != "-1");

                conn.Close();

                FT_ManagementContext.AdicionarLog(c.Utilizador.Id, "Comentário adicionado com sucesso pelo utilizador " + c.Utilizador.NomeCompleto + " à marcação Nº " + c.Marcacao.IdMarcacao + " do cliente " + c.Marcacao.Cliente.NomeCliente, 5);

            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel criar o comentário para o PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }
        private List<Comentario> ObterComentariosMarcacao(string SQL_Query)
        {

            List<Comentario> LstComentario = new List<Comentario>();

            try
            {
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
                        DateTime.TryParse(result["ousrdata"].ToString()[..10] + " " + result["ousrhora"].ToString(), out DateTime data);
                        LstComentario.Add(new Comentario()
                            {
                                IdComentario = result["u_comentstamp"].ToString(),
                                Descricao = result["comentario"].ToString().Trim(),
                                IdMarcacao = result["marcacaostamp"].ToString().Trim(),
                                Utilizador = new Utilizador() { NomeCompleto = result["ousrinis"].ToString().Trim() },
                                DataComentario = data
                            });
                        }
                }

                    conn.Close();
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Não foi possivel ler os comentarios das Marcacoes do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstComentario;
        }
        public List<Comentario> ObterComentariosMarcacao(int IdMarcacao)
        {
            return ObterComentariosMarcacao("select u_comentstamp, num as marcacaostamp, comentario, u_coment.ousrinis, u_coment.ousrdata, u_coment.ousrhora from u_coment inner join u_marcacao on u_marcacao.u_marcacaostamp = u_coment.marcacaostamp WHERE num=" + IdMarcacao+";").OrderBy(c => c.DataComentario).ToList();
            
        }
        #endregion

        //Obter Acessos
        #region ACESSOS
        public List<Acesso> ObterAcessos(DateTime dataUltimaLeitura)
        {

            List<Acesso> LstAcessos = new List<Acesso>();

            try
            {
                    SqlConnection conn = new SqlConnection(ConnectionString);

                    conn.Open();

                SqlCommand command = new SqlCommand("select cm, acao, data, hora, CONCAT('KM: ', km, ' | Obs:', obs) as obs from u_dias join cm4 on u_dias.tecnico = cm4.nome where u_dias.usrdata >= '" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "' ; ", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            LstAcessos.Add(new Acesso()
                            {
                                IdUtilizador = int.Parse(result["cm"].ToString()),
                                Tipo = result["acao"].ToString().Contains("Inicio") ? 1 : 2,
                                Data = DateTime.Parse(DateTime.Parse(result["data"].ToString()).ToShortDateString() + " " + DateTime.Parse(result["hora"].ToString()[..2] + ":" + result["hora"].ToString().Substring(2, 2) + ":" + result["hora"].ToString().Substring(4, 2)).ToLongTimeString()),
                                Temperatura = result["obs"].ToString()
                            });
                        }
                    }

                    conn.Close();

                    FT_ManagementContext.AtualizarUltimaModificacao("u_dias", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os Acessos do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstAcessos;
        }
        public void AtualizarAcessos()
        {
            FT_ManagementContext.CriarAcesso(ObterAcessos(FT_ManagementContext.ObterUltimaModificacaoPHC("u_dias")));
        }
        #endregion

        //Encomendas
        #region ENCOMENDAS
        private List<Encomenda> ObterEncomendas(string SQL_Query)
        {

            List<Encomenda> LstEncomenda = new List<Encomenda>();

            try
            {

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
                        if (LstEncomenda.Where(e => (e.Id == int.Parse(result["obrano"].ToString()) && e.NomeDossier == result["nmdos"].ToString())).Count() == 0)
                        {
                            LstEncomenda.Add(new Encomenda()
                            {
                                BO_STAMP = result["bostamp"].ToString(),
                                Id = int.Parse(result["obrano"].ToString()),
                                NomeDossier = result["nmdos"].ToString(),
                                NumDossier = int.Parse(result["ndos"].ToString()),
                                NomeCliente = result["Nome"].ToString(),
                                DataEnvio = DateTime.Parse(result["Data_Envio"].ToString()),
                                DataDossier = DateTime.Parse(result["dataobra"].ToString()),
                                DespacharEncomenda = result["Transportador"].ToString() == "True",
                                Prioritario = false,
                                Obs = result["obs"].ToString(),
                                PI_STAMP = !string.IsNullOrEmpty(result["NUM_PICKING"].ToString().Trim()) ? result["STAMP_PICKING"].ToString() : ""
                            });
                            LstEncomenda.Last().LinhasEncomenda = new List<Linha_Encomenda>();
                        }
                        LstEncomenda.Where(e => (e.Id == int.Parse(result["obrano"].ToString()) && e.NomeDossier == result["nmdos"].ToString())).First().LinhasEncomenda.Add(new Linha_Encomenda()
                        {
                            IdEncomenda = int.Parse(result["obrano"].ToString()),
                            NomeCliente = result["Nome"].ToString(),
                            DataEnvio = DateTime.Parse(result["Data_Envio_Linha"].ToString()),
                            Total = result["Envio_Total"].ToString() == "True",
                            Produto = new Produto()
                            {
                                Ref_Produto = result["ref"].ToString().Trim(),
                                Designacao_Produto = result["design"].ToString(),
                                Stock_Fisico = result["Qtt_Separar"].ToString() == "0" ? double.Parse(result["Qtt_Envio"].ToString()) : double.Parse(result["Qtt_Separar"].ToString())
                            },
                            Fornecido = double.Parse(result["Qtt_Envio"].ToString()) == 0
                        });
                        //Console.WriteLine(result["Nome"] + " - " + result["Envio_Total"]);
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler as encomendas do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstEncomenda;
        }
        public List<Encomenda> ObterEncomendas()
        {
            return ObterEncomendas("SELECT * FROM V_Enc_Aberto").OrderBy(e => e.Data).ToList();
        }
        public Encomenda ObterEncomenda(int IdEncomenda)
        {
            List<Encomenda> LstEncomendas = ObterEncomendas("SELECT * FROM V_Enc_Aberto WHERE OBRANO=" + IdEncomenda);
            return LstEncomendas.Count() == 0 ? new Encomenda() : LstEncomendas.FirstOrDefault();
        }
        public Encomenda ObterEncomenda(string Stamp_Encomenda)
        {
            List<Encomenda> LstEncomendas = ObterEncomendas("SELECT * FROM V_Enc_Aberto WHERE bostamp='" + Stamp_Encomenda + "';");
            return LstEncomendas.Count() == 0 ? new Encomenda() : LstEncomendas.FirstOrDefault();
        }
        public Encomenda ObterEncomendaPicking(string Stamp_Picking)
        {
            List<Encomenda> LstEncomendas = ObterEncomendas("SELECT * FROM V_Enc_Aberto WHERE STAMP_PICKING='" + Stamp_Picking + "';");
            return LstEncomendas.Count() == 0 ? new Encomenda() : LstEncomendas.FirstOrDefault();
        }
        #endregion

        //PICKING
        #region Picking
        public string CriarPicking(string BO_STAMP, string NomeUtilizador)
        {
            string res = "0";
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("Gera_Picking", conn)
                {
                    CommandTimeout = TIMEOUT,
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add(new SqlParameter("@STAMP", BO_STAMP));
                command.Parameters.Add(new SqlParameter("@NOME_UTILIZADOR", NomeUtilizador));

                using SqlDataReader result = command.ExecuteReader();
                result.Read();

                if (result[0].ToString() != "-1")
                {
                    res = result[2].ToString();
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel enviar picking para o PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }
        public string FecharPicking(Picking p)
        {
            string res = "0";
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("Fecha_Picking", conn)
                {
                    CommandTimeout = TIMEOUT,
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add(new SqlParameter("@STAMP", p.Picking_Stamp));
                command.Parameters.Add(new SqlParameter("@NOME_UTILIZADOR", p.EditadoPor));
                command.Parameters.Add(new SqlParameter("@ARMAZEMSTAMP", p.ArmazemDestino.ArmazemStamp));
                //command.Parameters.Add(new SqlParameter("@OBS", p.Obs));

                using SqlDataReader result = command.ExecuteReader();
                result.Read();

                if (result[0].ToString() != "-1")
                {
                    res = result[0].ToString();
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel fechar o picking no PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }
        public Picking ObterPicking(string PI_STAMP)
        {
            Picking p = new Picking();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("SELECT * from V_PICKING_CAB WHERE PISTAMP='" + PI_STAMP + "'", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        p = new Picking()
                        {
                            Picking_Stamp = result["PISTAMP"].ToString(),
                            NomeDossier = result["nmdos"].ToString(),
                            IdPicking = int.Parse(result["obrano"].ToString()),
                            DataDossier = DateTime.Parse(result["DATA"].ToString()),
                            NomeCliente = result["nome"].ToString(),
                            DespacharEncomenda = result["u_envio"].ToString() == "Transportadora",
                            Encomenda = this.ObterEncomendaPicking(PI_STAMP),
                            Linhas = this.ObterLinhasPicking(PI_STAMP),
                            EditadoPor = result["usrinis"].ToString().ToUpper()
                        };
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler o picking do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return p;
        }
        public List<Linha_Picking> ObterLinhasPicking(string PI_STAMP)
        {
            List<Linha_Picking> LstPickingLinhas = new List<Linha_Picking>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("SELECT * from V_PICKING_LIN WHERE PISTAMP='" + PI_STAMP + "'", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        if (LstPickingLinhas.Where(p => p.Ref_linha == result["ref"].ToString() && p.Serie == (result["USA_NSERIE"].ToString() == "True")).Count() == 0)
                        {
                            LstPickingLinhas.Add(new Linha_Picking()
                            {
                                Picking_Linha_Stamp = result["BISTAMP"].ToString().Trim(),
                                Ref_linha = result["ref"].ToString(),
                                Nome_Linha = result["design"].ToString(),
                                Qtd_Linha = Double.Parse(result["qtt"].ToString()),
                                Qtd_Separar = result["USA_NSERIE"].ToString() == "True" ? Double.Parse(result["QTT_SEPARAR"].ToString()) : Double.Parse(result["qtt"].ToString()) == Double.Parse(result["QTT_SEPARAR"].ToString()) ? Double.Parse(result["QTT_SEPARAR"].ToString()) : Math.Round(Double.Parse(result["QTT_SEPARAR"].ToString()) - Double.Parse(result["qtt"].ToString()), 3),
                                TipoUnidade = result["UNIDADE"].ToString(),
                                Serie = result["USA_NSERIE"].ToString() == "True",
                                Lista_Ref = ObterSerieLinhaPicking(result["BISTAMP"].ToString().Trim(), Double.Parse(result["QTT_SEPARAR"].ToString())),
                                EditadoPor = result["usrinis"].ToString()
                            });
                        }
                        else
                        {
                            Linha_Picking Linha = LstPickingLinhas.Where(p => p.Ref_linha == result["ref"].ToString()).First();
                            Linha.Qtd_Separar += Double.Parse(result["QTT_SEPARAR"].ToString());
                            Linha.Qtd_Linha += Double.Parse(result["qtt"].ToString());

                            Linha.Lista_Ref.AddRange(ObterSerieLinhaPicking(result["BISTAMP"].ToString().Trim(), Double.Parse(result["QTT_SEPARAR"].ToString())));
                        }
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler as linhas do picking do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstPickingLinhas;
        }
        public List<Ref_Linha_Picking> ObterSerieLinhaPicking(string BI_STAMP, Double Qtt)
        {
            List<Ref_Linha_Picking> Linha_Serie = new List<Ref_Linha_Picking>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("SELECT V_PICKING_LIN.BISTAMP, V_PICKING_LIN.ref, V_PICKING_LIN.design, V_PICKING_SERIE.BOMASTAMP, V_PICKING_SERIE.serie from V_PICKING_LIN full outer join V_PICKING_SERIE on V_PICKING_LIN.BISTAMP = V_PICKING_SERIE.BISTAMP WHERE V_PICKING_LIN.BISTAMP='" + BI_STAMP + "'", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        Linha_Serie.Add(new Ref_Linha_Picking()
                        {
                            Picking_Linha_Stamp = result["BISTAMP"].ToString().Trim(),
                            Ref_linha = result["ref"].ToString(),
                            Nome_Linha = result["design"].ToString(),
                            Qtd_Separar = 1,
                            BOMA_STAMP = result.IsDBNull("BOMASTAMP") ? "" : result["BOMASTAMP"].ToString().Trim(),
                            NumSerie = result.IsDBNull("serie") ? "" : result["serie"].ToString().Trim(),
                        });
                    }
                }

                conn.Close();

                for (int i = Linha_Serie.Count; i < Qtt; i++)
                {
                    Linha_Serie.Add(new Ref_Linha_Picking() { Picking_Linha_Stamp = Linha_Serie.First().Picking_Linha_Stamp });
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler as linhas de serie do picking do PHC!\r\n(Exception: " + ex.Message + ")");
            }
            return Linha_Serie;

        }
        public List<string> AtualizarLinhaPicking(Linha_Picking linha)
        {
            List<string> res = new List<string>() { "-1", "Erro", "" };

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("Atualiza_Linha_Picking", conn)
                {
                    CommandTimeout = TIMEOUT,
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add(new SqlParameter("@STAMP", linha.Picking_Linha_Stamp));
                command.Parameters.Add(new SqlParameter("@QTT", linha.Qtd_Linha));
                if (linha.Serie)
                {
                    command.Parameters.Add(new SqlParameter("@SERIE", linha.Lista_Ref.First().NumSerie));
                    command.Parameters.Add(new SqlParameter("@BOMASTAMP", linha.Lista_Ref.First().BOMA_STAMP));
                }
                command.Parameters.Add(new SqlParameter("@NOME_UTILIZADOR", linha.EditadoPor));


                using SqlDataReader result = command.ExecuteReader();
                result.Read();

                res[0] = result[0].ToString();
                res[1] = res[0] == "1" ? "" : result[1].ToString();
                res[2] = result[2].ToString();

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel atualizar o picking para o PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }
        public string ValidarPicking(string PI_STAMP)
        {
            string res = "";
            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select completo from V_PICKING_CAB where PISTAMP = '" + PI_STAMP+"'", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        res = result[0].ToString() == "0" ? "Ainda falta validar referências!\r\n\r\n" : "";
                    }
                }

                command = new SqlCommand("select SUM(o.qtt-o.qtt2) from bi o join bi b on o.bistamp = b.oobistamp	where b.bostamp = '" + PI_STAMP + "'", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        double qtt = double.Parse(result[0].ToString());
                        res += qtt == 0 ? "As referências lidas satisfazem a encomenda na totalidade e a encomenda será fechada!\r\n\r\n" : "";
                        res += qtt > 0 ? "As referências lidas não satisfazem a encomenda na totalidade, por esse motivo a encomenda manter-se-á em aberto!\r\n\r\n" : "";
                        res += qtt < 0 ? "As referências lidas são superiores á quantidade encomendada, por esse motivo a encomenda será fechada!\r\n\r\n" : "";
                    }
                }

                command = new SqlCommand("select a.ref, a.serie, a.armazem, b.noarm  from boma a(nolock) join ma b(nolock) on a.mastamp = b.mastamp where a.bostamp = '"+PI_STAMP+"' and a.armazem <> b.noarm", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    if (result.HasRows) res += "Os seguintes equipamentos encontram-se associados a outro armazem. Deseja proseguir com a transferência entre armazens destes equipamentos?\r\n";
                    while (result.Read())
                    {
                        res += "S/N: " + result["serie"].ToString().Trim() + " (" + result["noarm"].ToString() + " -> " + result["armazem"].ToString() + ")\r\n";
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel validar o picking do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }

        #endregion
    }
}
