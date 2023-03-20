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

            try
            {
                Console.WriteLine(SQL_Query);

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand(SQL_Query, conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    result.Read();
                    if (result.HasRows)
                    {
                        for (int i = 0; i < result.FieldCount; i++)
                        {
                            if (res.Count() <= i)
                            {
                                res.Add(result[i].ToString());
                            }
                            else
                            {
                                res[i] = result[i].ToString();
                            }
                        }
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
                            Stock_PHC = !string.IsNullOrEmpty(result["stock"].ToString()) ? double.Parse(result["stock"].ToString()) : 0,
                            Stock_Rec = !string.IsNullOrEmpty(result["qttrec"].ToString()) ? double.Parse(result["qttrec"].ToString()) : 0,
                            Stock_Res = !string.IsNullOrEmpty(result["rescli"].ToString()) ? double.Parse(result["rescli"].ToString()) : 0,
                            Armazem_ID = !string.IsNullOrEmpty(result["armazem"].ToString()) ? int.Parse(result["armazem"].ToString()) : 0,
                            Pos_Stock = result["u_locpt"].ToString(),
                            Serie = result["noserie"].ToString() == "True",
                            Valor = !string.IsNullOrEmpty(result["epv1"].ToString()) ? double.Parse(result["epv1"].ToString()) : 0,
                            TipoUn = result["unidade"].ToString()
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
        public Produto ObterProduto(string Referencia)
        {
            return ObterProdutos("SELECT unidade, ststamp, st.ref, st.design, st.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec, stobs.u_locpt, noserie, epv1 FROM st left outer join sa on sa.ref=st.ref inner join stobs on st.ref=stobs.ref where st.ref like '%" + Referencia + "%' order by st.ref;").DefaultIfEmpty(new Produto()).First();
        }
        public List<Produto> ObterProdutos(string Referencia, string Designacao, int IdArmazem, int IdFornecedor, string TipoEquipamento)
        {
            return ObterProdutos("SELECT TOP 500 unidade, ststamp, sa.ref, st.design, sa.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec, stobs.u_locpt, noserie, epv1 FROM sa inner join st on sa.ref=st.ref inner join stobs on sa.ref=stobs.ref where sa.ref like '%" + Referencia + "%' AND st.design like '%" + Designacao + "%' AND sa.armazem='" + IdArmazem + "' " + (IdFornecedor == 0 ? "" : " and fornec=" + IdFornecedor) + " and st.usr4 like '%" + TipoEquipamento + "%' AND st.inactivo=0 order by sa.ref;");
        }
        public List<Produto> ObterProdutosArmazem(string Referencia)
        {
            return ObterProdutos("SELECT unidade, ststamp, sa.ref, st.design, sa.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec, stobs.u_locpt, noserie, epv1 FROM sa inner join st on sa.ref=st.ref inner join stobs on sa.ref=stobs.ref where sa.ref like '" + Referencia + "' order by sa.armazem;");
        }
        public Produto ObterProdutoStamp(string Stamp)
        {
            return ObterProdutos("SELECT unidade, ststamp, sa.ref, st.design, sa.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec, stobs.u_locpt, noserie, epv1 FROM sa inner join st on sa.ref=st.ref inner join stobs on sa.ref=stobs.ref where ststamp like '%" + Stamp + "%' order by sa.armazem;").DefaultIfEmpty(new Produto()).First();
        }
        public List<Produto> ObterProdutosArmazem(int Armazem)
        {
            if (Armazem < 10) return ObterProdutos("SELECT unidade, ststamp, sa.ref, st.design, sa.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec, stobs.u_locpt, noserie, epv1 FROM sa inner join st on sa.ref=st.ref inner join stobs on sa.ref=stobs.ref where sa.armazem = '" + Armazem + "' order by sa.ref;").Where(p => !p.Ref_Produto.Contains("SRV")).ToList();
            return ObterProdutos("SELECT unidade, ststamp, sa.ref, st.design, sa.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec, stobs.u_locpt, noserie, epv1 FROM sa inner join st on sa.ref=st.ref inner join stobs on sa.ref=stobs.ref where sa.armazem = '" + Armazem + "' order by sa.ref;").Where(p => (p.Stock_PHC - p.Stock_Res + p.Stock_Rec) > 0 || p.Ref_Produto.Contains("SRV.15")).ToList();
        }
        public Produto ObterProduto(string Referencia, int IdArmazem)
        {
            List<Produto> p = ObterProdutos("SELECT unidade, ststamp, sa.ref, st.design, sa.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec, stobs.u_locpt, noserie, epv1 FROM sa inner join st on sa.ref=st.ref inner join stobs on sa.ref=stobs.ref where sa.ref='" + Referencia + "' AND sa.armazem='" + IdArmazem + "' order by sa.ref;");
            if (p.Count > 0)
            {
                p[0].ImgProduto = ObterProdutoImagem(p[0]);
                return p[0];
            }
            return new Produto();
        }
        private string ObterProdutoImagem(Produto p)
        {
            string img = FicheirosContext.ObterCaminhoProdutoImagem(p.Ref_Produto);
            if (!File.Exists(img)) img = "wwwroot/img/no_photo.png";
            byte[] imageBytes = File.ReadAllBytes(img);

            return Convert.ToBase64String(imageBytes); ;
        }

        public List<string> GerarGuiaGlobal(int IdArmazem)
        {
            List<string> res = new List<string>() { "-1", "Erro", "" };
            try
            {
                string SQL_Query = "EXEC WEB_Guia_Global_Gera ";

                SQL_Query += "@ARMAZEM = '" + IdArmazem + "'; ";

                res = ExecutarQuery(SQL_Query);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel gerar a guia global no PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
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
            return ObterArmazens("select * from sz where szstamp='" + stamp + "' order by no;").FirstOrDefault() ?? new Armazem();
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

        #endregion

        //Obter Clientes
        #region CLIENTES
        private List<Cliente> ObterClientes(string SQL_Query, bool LoadMarcacoes, bool LoadFolhasObra, bool LoadVisitas, bool LoadEquipamentos)
        {

            List<Cliente> LstClientes = new List<Cliente>();
            List<Cliente> LstClientesMySQL = FT_ManagementContext.ObterClientes();
            List<Utilizador> LstUtilizadores = FT_ManagementContext.ObterListaComerciais(true);
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
                            Vendedor = LstUtilizadores.Where(v => v.IdPHC == int.Parse(result["vendedor"].ToString().Trim())).DefaultIfEmpty(new Utilizador()).First(),
                            TipoCliente = result["tipo"].ToString().Trim()
                        });
                        if (LoadMarcacoes) LstClientes.Last().Marcacoes = ObterMarcacoesSimples(new Cliente() { IdCliente = int.Parse(result["no"].ToString()), IdLoja = int.Parse(result["estab"].ToString()) });
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
                return ObterClientes("SELECT TOP 100 cl.clstamp, no, estab, cl.nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, (select TOP 1 emailfo from u_clresp where cl.clstamp=u_clresp.clstamp) as emailfo, tipo, vendedor, cl.usrdata, cl.usrhora FROM cl where cl.nome like '%" + filtro + "%' and no is not null order by no, estab;", false, false, false, false);
            }
            return new List<Cliente>() { new Cliente() { } };
        }
        public Cliente ObterCliente(int IdCliente, int IdLoja)
        {
            return ObterCliente("SELECT cl.clstamp, no, estab, cl.nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, u_clresp.emailfo, tipo, vendedor, cl.usrdata, cl.usrhora FROM cl full outer join u_clresp on cl.clstamp=u_clresp.clstamp where cl.no=" + IdCliente + " and estab=" + IdLoja + " and no is not null;", true);

        }
        public Cliente ObterClienteSimples(string STAMP)
        {
            return ObterCliente("SELECT cl.clstamp, no, estab, cl.nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, u_clresp.emailfo, tipo, vendedor, cl.usrdata, cl.usrhora FROM cl full outer join u_clresp on cl.clstamp=u_clresp.clstamp where cl.clstamp='" + STAMP + "' and no is not null;", false);

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
        public List<Fornecedor> ObterFornecedores(string SQL_Query)
        {

            List<Fornecedor> LstFornecedor = new List<Fornecedor>();

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

        public List<Fornecedor> ObterFornecedores()
        {
            return ObterFornecedores("SELECT flstamp, no, nome, CONCAT(morada, ' ', local, ' ', codpost) as MoradaFornecedor, telefone, email, contacto, obs, u_numfart FROM fl order by nome;");
        }

        public Fornecedor ObterFornecedor(int id)
        {
            return ObterFornecedores("SELECT flstamp, no, nome, CONCAT(morada, ' ', local, ' ', codpost) as MoradaFornecedor, telefone, email, contacto, obs, u_numfart FROM fl where no=" + id + " order by nome;").DefaultIfEmpty(new Fornecedor()).First();
        }

        public Cliente ObterFornecedorCliente(int id)
        {

            Cliente f = new Cliente();

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("SELECT flstamp, no, nome, CONCAT(morada, ' ', local, ' ', codpost) as MoradaFornecedor, telefone, email, contacto, obs, u_numfart FROM fl where no=" + id + " order by nome;", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        f = new Cliente()
                        {
                            ClienteStamp = result["flstamp"].ToString(),
                            IdCliente = int.Parse(result["no"].ToString()),
                            NomeCliente = result["nome"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''"),
                            MoradaCliente = result["MoradaFornecedor"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''"),
                            TelefoneCliente = result["telefone"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''"),
                            EmailCliente = result["email"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''"),
                            PessoaContatoCliente = result["contacto"].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "''")
                        };
                    }
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os Fornecedores do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return f;
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
        private List<Equipamento> ObterEquipamentos(string SQL_Query, bool LoadCliente, bool LoadFolhasObra)
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
                            EquipamentoStamp = result["mastamp"].ToString().Trim(),
                            TipoEquipamento = result["tipo"].ToString().Trim(),
                            DesignacaoEquipamento = result["design"].ToString().Trim(),
                            MarcaEquipamento = result["marca"].ToString().Trim(),
                            ModeloEquipamento = result["maquina"].ToString().Trim(),
                            NumeroSerieEquipamento = result["serie"].ToString().Trim().Replace('\\', ' '),
                            RefProduto = result["ref"].ToString().Trim(),
                            IdCliente = int.Parse(result["no"].ToString()),
                            IdLoja = int.Parse(result["estab"].ToString()),
                            IdFornecedor = int.Parse(result["flno"].ToString()),
                            UltimoTecnico = !result.IsDBNull("utecnnm") ? result["utecnnm"].ToString() : "N/D",
                            DataCompra = DateTime.Parse(result["fldata"].ToString()),
                            DataVenda = DateTime.Parse(result["ftfdata"].ToString()),
                            Cliente = new Cliente() { NomeCliente = result["nome"].ToString() }
                        });

                        if (LoadCliente)
                        {
                            LstEquipamento.Last().Cliente = ObterClienteSimples(LstEquipamento.Last().IdCliente, LstEquipamento.Last().IdLoja);
                        }
                        if (LoadFolhasObra)
                        {
                            LstEquipamento.Last().FolhasObra = ObterFolhasObraEquipamento(LstEquipamento.Last().EquipamentoStamp);
                        }
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
            return ObterEquipamentos("SELECT * FROM ma;", false, false);
        }
        public List<Equipamento> ObterEquipamentos(Cliente c)
        {
            return ObterEquipamentos("SELECT * FROM ma where no='" + c.IdCliente + "' and estab='" + c.IdLoja + "';", false, false);
        }
        public List<Equipamento> ObterEquipamentosSerie(string NumeroSerie)
        {
            return ObterEquipamentos("SELECT TOP(100) * FROM ma where serie like '%" + NumeroSerie + "%';", false, false);
        }
        public Equipamento ObterEquipamento(string IdEquipamento)
        {
            List<Equipamento> e = ObterEquipamentos("SELECT * FROM ma where mastamp='" + IdEquipamento + "';", true, true);
            if (e.Count > 0) return e[0];
            return new Equipamento();
        }
        public Equipamento ObterEquipamentoSimples(string IdEquipamento)
        {
            List<Equipamento> e = ObterEquipamentos("SELECT * FROM ma where mastamp='" + IdEquipamento + "';", true, false);
            if (e.Count > 0) return e[0];
            return new Equipamento();
        }
        public bool AtualizarClienteEquipamento(Cliente c, Equipamento e, Utilizador u)
        {
            try
            {
                ExecutarQuery("UPDATE MA SET ma.no=cl.no, ma.nome=cl.nome, ma.estab=cl.estab, ma.morada=cl.morada, ma.local=cl.local, ma.codpost=cl.codpost, ma.contacto=cl.contacto, ma.email=cl.email, ma.telefone=cl.telefone, usrinis='" + u.Iniciais + "', usrdata='" + DateTime.Now.ToString("yyyy-MM-dd 00:00:00.000") + "', usrhora='" + DateTime.Now.ToString("HH:mm:ss") + "' FROM MA JOIN CL ON CL.NO=" + c.IdCliente + " AND CL.ESTAB=" + c.IdLoja + " WHERE ma.mastamp='" + e.EquipamentoStamp + "';");
                FT_ManagementContext.AdicionarLog(u.Id, "Foi atualizado o equipamento " + e.MarcaEquipamento + " " + e.ModeloEquipamento + " com número de serie: " + e.NumeroSerieEquipamento + " ao cliente: " + c.NomeCliente + " pelo utilizador " + u.NomeCompleto, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel associar o cliente no PHC!\r\n(Exception: " + ex.Message + ")");
                return false;
            }
            return true;
        }
        #endregion

        //Obter Folhas de Obra
        #region FOLHASOBRA

        //EM DESENVOLVIMENTO
        public List<string> CriarFolhaObra(FolhaObra fo)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };
            try
            {
                string SQL_Query = "EXEC WEB_PAT_Gera ";

                SQL_Query += "@U_MARCACAOSTAMP = '" + fo.Marcacao.MarcacaoStamp + "', ";
                SQL_Query += "@MASTAMP = '" + fo.EquipamentoServico.EquipamentoStamp + "', ";
                SQL_Query += "@ESTADO = '" + fo.EstadoFolhaObra + "', ";
                SQL_Query += "@OFICINA = '" + fo.RecolhaOficina + "', ";
                SQL_Query += "@TECNICO = '" + fo.Utilizador.IdPHC + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + fo.Utilizador.NomeCompleto + "'; ";

                res = ExecutarQuery(SQL_Query);

                if (res[0].ToString() != "-1")
                {
                    fo.StampFO = res[2].ToString();

                    //CRIAR INTERVENÇÔES
                    for (int i = 0; i < fo.IntervencaosServico.Count(); i++)
                    {
                        res = CriarIntervencao(fo.IntervencaosServico[i], fo);
                        if (res[0].ToString() != "-1")
                        {
                            fo.IntervencaosServico[i].StampIntervencao = res[2].ToString();

                            //ASSOCIAR PEÇAS INTERVENÇÃO
                            if (i == 0)
                            {
                                foreach (Produto p in fo.PecasServico)
                                {
                                    res = CriarPecaIntervencao(p, fo.IntervencaosServico[i], fo);
                                    if (res[0] == "-1") return res;
                                }
                            }
                        }
                        else
                        {
                            return res;
                        }
                    }

                    //GERAR AT
                    res = CriarAT(fo);

                    //Obter Folha de Obra
                    fo = ObterFolhaObra(fo.StampFO);
                    res[1] = fo.IdAT;
                    res[3] = fo.IdFolhaObra.ToString();

                    FT_ManagementContext.AdicionarLog(fo.Utilizador.Id, "Folha de Obra criada com sucesso! - Nº " + fo.IdFolhaObra.ToString() + ", " + fo.ClienteServico.NomeCliente + " pelo utilizador " + fo.Utilizador.NomeCompleto, 5);
                }
                else
                {
                    return res;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel enviar folha de obra para o PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }

        public List<string> CriarIntervencao(Intervencao i, FolhaObra fo)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };
            try
            {
                string SQL_Query = "EXEC WEB_Intervencao_Gera ";

                SQL_Query += "@U_MARCACAOSTAMP = '" + fo.Marcacao.MarcacaoStamp + "', ";
                SQL_Query += "@STAMP_PA = '" + fo.StampFO + "', ";
                SQL_Query += "@HORA_INI = '" + i.HoraInicio.ToShortTimeString() + "', ";
                SQL_Query += "@HORA_FIM = '" + i.HoraFim.ToShortTimeString() + "', ";
                SQL_Query += "@DATA = '" + i.DataServiço.ToString("yyyyMMdd") + "', ";
                SQL_Query += "@RELATORIO = '" + i.RelatorioServico + "', ";
                SQL_Query += "@QASSINOU = '" + fo.ConferidoPor + "', ";
                SQL_Query += "@TECNICO = '" + i.IdTecnico + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + i.NomeTecnico + "'; ";

                res = ExecutarQuery(SQL_Query);

                return res;
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel enviar a intervencao para o PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }
        public List<string> CriarPecaIntervencao(Produto p, Intervencao i, FolhaObra fo)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };
            try
            {
                string SQL_Query = "EXEC WEB_AT_INSERE_PECA ";

                SQL_Query += "@U_MARCACAOSTAMP = '" + fo.Marcacao.MarcacaoStamp + "', ";
                SQL_Query += "@STAMP_PA = '" + fo.StampFO + "', ";
                SQL_Query += "@STAMP_MH = '" + i.StampIntervencao + "', ";
                SQL_Query += "@REF = '" + p.Ref_Produto + "', ";
                SQL_Query += "@QTT = '" + p.Stock_Fisico + "', ";
                SQL_Query += "@SERIE = '" + "" + "', ";
                SQL_Query += "@TECNICO = '" + i.IdTecnico + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + i.NomeTecnico + "'; ";

                res = ExecutarQuery(SQL_Query);

                return res;
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel enviar a intervencao para o PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }

        public string CriarAssinatura(FolhaObra fo)
        {
            string res = "";
            if (string.IsNullOrEmpty(fo.RubricaCliente)) return res;
            try
            {
                byte[] bytes = Convert.FromBase64String(fo.RubricaCliente.Split(",").Last());
                MemoryStream stream = new MemoryStream(bytes);

                MarcacaoAnexo a = new MarcacaoAnexo()
                {
                    MarcacaoStamp = fo.Marcacao.MarcacaoStamp,
                    IdMarcacao = fo.Marcacao.IdMarcacao,
                    AnexoAssinatura = true,
                    NomeUtilizador = fo.Utilizador.NomeCompleto
                };
                a.NomeFicheiro = a.ObterNomeUnico() + ".png";
                IFormFile file = new FormFile(stream, 0, bytes.Length, a.NomeFicheiro.Split(".").First(), a.NomeFicheiro);

                if (FicheirosContext.CriarAnexoAssinatura(a, file)) res = CriarAnexoMarcacao(a);
                if (res.Length == 0) return "";
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel a assinatura para a folha de Obra!\r\n(Exception: " + ex.Message + ")");
            }
            return res;

        }
        public List<string> CriarAT(FolhaObra fo)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };
            try
            {
                string SQL_Query = "EXEC WEB_Gera_AT ";

                SQL_Query += "@U_MARCACAOSTAMP = '" + fo.Marcacao.MarcacaoStamp + "', ";
                SQL_Query += "@STAMP_PA = '" + fo.StampFO + "', ";
                SQL_Query += "@STAMPS_MH = '" + string.Join(",", fo.IntervencaosServico.Select(x => x.StampIntervencao)) + "', ";
                SQL_Query += "@STAMP_ASSINATURA = '" + CriarAssinatura(fo) + "', ";
                SQL_Query += "@QASSINOU = '" + fo.ConferidoPor + "', ";
                SQL_Query += "@GARANTIA = '" + (fo.EmGarantia ? "1" : "0") + "', ";
                SQL_Query += "@INSTALACAO = '" + (fo.Instalação ? "1" : "0") + "', ";
                SQL_Query += "@OFICINA = '" + (fo.RecolhaOficina ? "1" : "0") + "', ";
                SQL_Query += "@REMOTO = '" + (fo.AssistenciaRemota ? "1" : "0") + "', ";
                SQL_Query += "@PIQUETE = '" + (fo.Piquete ? "1" : "0") + "', ";
                SQL_Query += "@DESLOCACAO = '" + (fo.CobrarDeslocacao ? "1" : "0") + "', ";
                SQL_Query += "@OBS = '" + fo.SituacoesPendentes + "', ";
                SQL_Query += "@DATA = '" + fo.DataServico.ToString("yyyyMMdd") + "', ";
                SQL_Query += "@TECNICO = '" + fo.Utilizador.IdPHC + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + fo.Utilizador.NomeCompleto + "'; ";

                res = ExecutarQuery(SQL_Query);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel enviar a AT para o PHC!\r\n(Exception: " + ex.Message + ")");
            }
            return res;

        }

        public List<string> CriarAnexosFolhaObra(FolhaObra fo)
        {

            List<string> res = new List<string>() { "-1", "Erro", "", "" };
            if (string.IsNullOrEmpty(fo.FicheirosAnexo)) return res;

            foreach (string f in fo.FicheirosAnexo.Split(";"))
            {
                if (FicheirosContext.ExisteFicheiroTemporario(f))
                {
                    byte[] byteArray = FicheirosContext.ObterFicheiroTemporario(f);
                    var stream = new MemoryStream(byteArray);
                    IFormFile file = new FormFile(stream, 0, byteArray.Length, "file", f);

                    Anexo a = new Anexo()
                    {
                        Ecra = "BO",
                        Serie = 49,
                        Stamp_Origem = fo.StampFO,
                        Resumo = "FO (" + fo.IdAT + ") - " + fo.Utilizador.NomeCompleto,
                        Nome = "FO_" + fo.IdAT + "_" + fo.Utilizador.Iniciais + "_" + DateTime.Now.Ticks + (file.FileName.Split(".").Count() > 0 ? "." + file.FileName.Split(".").Last() : ""),
                        Utilizador = fo.Utilizador
                    };
                    res = this.CriarAnexo(a, file);
                    FicheirosContext.ApagarFicheiroTemporario(f);
                }
            }
            return res;
        }
        public bool FecharFolhaObra(FolhaObra fo)
        {
            if (fo.EnviarEmail && !string.IsNullOrEmpty(fo.EmailCliente))
            {
                MailContext.EnviarEmailFolhaObra(fo.EmailCliente + ";" + fo.Utilizador.EmailUtilizador, fo, new Attachment((new MemoryStream(FT_ManagementContext.PreencherFormularioFolhaObra(fo).ToArray())), "FO_" + fo.IdFolhaObra + ".pdf", System.Net.Mime.MediaTypeNames.Application.Pdf));
            }
            else
            {
                ChatContext.EnviarNotificacaoFolhaObraTecnico(fo, fo.Utilizador);
            }

            //PD
            if (fo.ClienteServico.IdCliente == 878 && fo.IntervencaosServico.Count > 0 && !fo.Avisar && fo.EstadoFolhaObra == 1) MailContext.EnviarEmailMarcacaoPD(fo, fo.Marcacao, 1);
            if (fo.ClienteServico.IdCliente == 878 && fo.IntervencaosServico.Count > 0 && fo.Avisar) MailContext.EnviarEmailMarcacaoPD(fo, fo.Marcacao, 2);
            if (fo.ClienteServico.IdCliente == 878 && fo.IntervencaosServico.Count > 0 && !fo.Avisar && fo.EstadoFolhaObra != 1) MailContext.EnviarEmailMarcacaoPD(fo, fo.Marcacao, 3);

            //SONAE
            if (fo.ClienteServico.IdCliente == 561 && fo.IntervencaosServico.Count > 0 && !fo.Avisar && fo.EstadoFolhaObra == 1) MailContext.EnviarEmailMarcacaoSONAE(fo, fo.Marcacao, 1);
            if (fo.ClienteServico.IdCliente == 561 && fo.IntervencaosServico.Count > 0 && fo.Avisar) MailContext.EnviarEmailMarcacaoSONAE(fo, fo.Marcacao, 2);
            if (fo.ClienteServico.IdCliente == 561 && fo.IntervencaosServico.Count > 0 && !fo.Avisar && fo.EstadoFolhaObra != 1) MailContext.EnviarEmailMarcacaoSONAE(fo, fo.Marcacao, 3);

            return true;
        }

        public string ValidarFolhaObra(FolhaObra fo)
        {
            string res = "";
            if (fo.Marcacao.DatasAdicionaisDistintas.Where(d => d.ToShortDateString() == fo.DataServico.ToShortDateString()).Count() == 0) res += "A data da intervenção é diferente da data da marcação!\r\n";
            if (fo.EquipamentoServico.EquipamentoStamp == null) res += "Não foi selecionado um equipamento!\r\n";
            if (fo.EquipamentoServico.EquipamentoStamp != null && fo.EquipamentoServico.Cliente.ClienteStamp != fo.ClienteServico.ClienteStamp) res += "O equipamento selecionado com o N/S " + fo.EquipamentoServico.NumeroSerieEquipamento + " pertence ao cliente " + fo.EquipamentoServico.Cliente.NomeCliente + ". Deseja proseguir e associar este equipamento ao cliente " + fo.ClienteServico.NomeCliente + "?\r\n";
            if (fo.IntervencaosServico.Where(i => i.DataServiço.ToShortDateString() != DateTime.Now.ToShortDateString()).Count() > 0) res += "A data escolhida para a intervenção é diferente da data atual. \r\n";
            if (fo.ValorTotal > 500) res += "O valor da reparação excede o valor máximo definido para esse cliente!\r\n";
            if (fo.IntervencaosServico.Where(i => i.HoraFim > DateTime.Now.AddHours(-2)).Count() == 0 && fo.IntervencaosServico.Count() > 0) res += "A intervenção adicionada excede o limite de 2 horas para criar uma folha de obra pelo que não pode proseguir!\r\n";
            foreach (Produto item in fo.PecasServico.Where(p => !p.Servico))
            {
                Produto p = ObterProdutosArmazem(fo.Utilizador.IdArmazem).Where(prod => prod.StampProduto == item.StampProduto).DefaultIfEmpty(new Produto()).First();
                if (p.Stock_Atual < item.Stock_Fisico) res += "Não tem stock suficiente da seguinte peça: " + p.Ref_Produto.Trim() + "!\r\n";
            }

            if (!string.IsNullOrEmpty(res)) res += "\r\nDeseja proseguir?";
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
                            StampFO = result["pastamp"].ToString(),
                            IdFolhaObra = int.Parse(result["nopat"].ToString().Trim()),
                            DataServico = DateTime.Parse(result["pdata"].ToString().Trim()),
                            ReferenciaServico = result["u_nincide"].ToString().Trim(),
                            EstadoEquipamento = result["situacao"].ToString().Trim(),
                            ConferidoPor = result.IsDBNull("qassinou") ? "" : result["qassinou"].ToString().Trim(),
                            IdCartao = result.IsDBNull("u_marcacaostamp") ? "" : result["u_marcacaostamp"].ToString().Trim(),
                            IdMarcacao = result.IsDBNull("num") ? 0 : int.Parse(result["num"].ToString()),
                            RubricaCliente = "",
                            EmGarantia = result["situacao"].ToString() == "Garantia",
                            Utilizador = FT_ManagementContext.ObterListaUtilizadores(true, false).Where(u => u.IdPHC.ToString() == result["tecnico"].ToString()).DefaultIfEmpty(new Utilizador()).First()
                        });

                        if (LoadEquipamento) LstFolhaObra.Last().EquipamentoServico = ObterEquipamentoSimples(result["mastamp"].ToString().Trim());
                        if (LoadCliente) LstFolhaObra.Last().ClienteServico = ObterClienteSimples(int.Parse(result["no"].ToString().Trim()), int.Parse(result["estab"].ToString().Trim()));
                        if (LoadIntervencoes)
                        {
                            LstFolhaObra.Last().AssistenciaRemota = result["logi2"].ToString() == "True";
                            LstFolhaObra.Last().Piquete = result.IsDBNull("piquete") ? false : result["piquete"].ToString().Trim() == "True";

                            LstFolhaObra.Last().IntervencaosServico = ObterIntervencoes(int.Parse(result["nopat"].ToString().Trim()));
                            if (LstFolhaObra.Last().IntervencaosServico.GroupBy(i => i.RelatorioServico).Select(i => i.FirstOrDefault()).Count() == 1) LstFolhaObra.Last().RelatorioServico = LstFolhaObra.Last().IntervencaosServico.First().RelatorioServico;
                            if (LstFolhaObra.Last().IntervencaosServico.GroupBy(i => i.RelatorioServico).Select(i => i.FirstOrDefault()).Count() > 1)
                            {
                                foreach (var item in LstFolhaObra.Last().IntervencaosServico.GroupBy(i => i.RelatorioServico).Select(i => i.FirstOrDefault()))
                                {
                                    LstFolhaObra.Last().RelatorioServico += item.DataServiço.ToShortDateString() + ": " + item.HoraInicio.ToShortTimeString() + " -> " + item.HoraFim.ToShortTimeString() + " - " + item.RelatorioServico + "\r\n";
                                }
                            }

                        }
                        if (LoadPecas)
                        {
                            LstFolhaObra.Last().PecasServico = ObterPecas(int.Parse(result["nopat"].ToString().Trim()));
                            LstFolhaObra.Last().GuiaTransporteAtual = LstFolhaObra.Last().PecasServico.Count() == 0 ? "" : (LstFolhaObra.Last().PecasServico.FirstOrDefault(p => p.Pos_Stock.Length > 0)?.Pos_Stock.ToString() ?? "");
                            LstFolhaObra.Last().SituacoesPendentes = result["obstab2"].ToString();
                        }

                        if (LoadRubrica)
                        {
                            LstFolhaObra.Last().IdAT = result["id_at"].ToString();
                            string img = ObterRubrica(LstFolhaObra.Last().IdFolhaObra);
                            if (File.Exists(img))
                            {
                                byte[] imageBytes = File.ReadAllBytes(img);

                                // Convert byte[] to Base64 String
                                string base64String = Convert.ToBase64String(imageBytes);
                                LstFolhaObra.Last().RubricaCliente = base64String;
                            }


                        }
                    }
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os PAT's do PHC!|" + LstFolhaObra.Last().IdFolhaObra + "|\r\n(Exception: " + ex.Message + ")");
            }

            return LstFolhaObra;
        }
        public FolhaObra ObterFolhaObra(string SQL_Query, bool LoadAll)
        {
            return ObterFolhasObra(SQL_Query, LoadAll, LoadAll, LoadAll, LoadAll, LoadAll).DefaultIfEmpty(new FolhaObra()).First();
        }

        public List<FolhaObra> ObterFolhasObra(int IdMarcacao)
        {
            return ObterFolhasObra("select *, (select TOP 1 logi2 from bo where pastamp = pa.pastamp) as logi2,(select TOP 1 qassinou from u_intervencao, u_marcacao where u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp and u_marcacao.num=" + IdMarcacao + ") as qassinou, (select u_marcacao.u_marcacaostamp from u_marcacao where num = " + IdMarcacao + ") as u_marcacaostamp, (SELECT u_nincide from u_marcacao where num=" + IdMarcacao + ") as u_nincide, (select '" + IdMarcacao + "' as num) as num from pa where pastamp in (select STAMP_DEST from u_intervencao where u_marcacaostamp = (select u_marcacao.u_marcacaostamp from u_marcacao where num = " + IdMarcacao + ")) order by nopat", true, true, false, false, false);

        }
        public List<FolhaObra> ObterFolhasObraEquipamento(string StampEquipamento)
        {
            return ObterFolhasObra("select (select TOP 1 logi2 from bo where pastamp = pa.pastamp) as logi2,* from pa full outer join u_intervencao on u_intervencao.STAMP_DEST=pa.pastamp full outer join u_marcacao on u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp where pa.mastamp='" + StampEquipamento + "' order by nopat;", true, true, false, false, false);

        }
        public List<FolhaObra> ObterFolhasObra(Cliente c)
        {
            return ObterFolhasObra("select (select TOP 1 logi2 from bo where pastamp = pa.pastamp) as logi2,* from pa full outer join u_intervencao on u_intervencao.STAMP_DEST=pa.pastamp full outer join u_marcacao on u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp where pa.no='" + c.IdCliente + "' and pa.estab='" + c.IdLoja + "' order by nopat;", true, false, false, false, false);

        }
        public List<FolhaObra> ObterFolhasObra(DateTime Data)
        {
            return ObterFolhasObra("select (select TOP 1 logi2 from bo where pastamp = pa.pastamp) as logi2,* from pa full outer join u_intervencao on u_intervencao.STAMP_DEST=pa.pastamp full outer join u_marcacao on u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp where pa.pdata='" + Data.ToString("yyyy-MM-dd") + "' order by nopat;", true, true, true, false, false).GroupBy(f => f.IdFolhaObra).Select(f => f.First()).ToList();

        }
        public List<FolhaObra> ObterFolhasObra(DateTime Data, Cliente c)
        {
            return ObterFolhasObra("select (select TOP 1 logi2 from bo where pastamp = pa.pastamp) as logi2,(select TOP 1 obrano from bo where orinopat=pa.nopat and ndos=49) as id_at, * from pa full outer join u_intervencao on u_intervencao.STAMP_DEST=pa.pastamp full outer join u_marcacao on u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp where pa.pdata='" + Data.ToString("yyyy-MM-dd") + "' and pa.no='" + c.IdCliente + "' and pa.estab='" + c.IdLoja + "' order by nopat;", true, true, true, false, true);

        }
        public FolhaObra ObterFolhaObra(int IdFolhaObra)
        {
            return ObterFolhaObra("select TOP 1 (select TOP 1 logi2 from bo where pastamp = pa.pastamp) as logi2,(select TOP 1 obrano from bo where orinopat=" + IdFolhaObra + " and ndos=49) as id_at, * from pa full outer join u_intervencao on u_intervencao.STAMP_DEST=pa.pastamp full outer join u_marcacao on u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp full outer join bo on bo.orinopat=pa.nopat where pa.nopat=" + IdFolhaObra + " order by pa.nopat;", true);

        }
        public FolhaObra ObterFolhaObra(string STAMP)
        {
            return ObterFolhaObra("select TOP 1 (select TOP 1 logi2 from bo where pastamp = pa.pastamp) as logi2,(select TOP 1 obrano from bo where orinopat=pa.nopat and ndos=49) as id_at, * from pa full outer join u_intervencao on u_intervencao.STAMP_DEST=pa.pastamp full outer join u_marcacao on u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp full outer join bo on bo.orinopat=pa.nopat where pa.pastamp='" + STAMP + "' order by pa.nopat;", true);

        }
        public FolhaObra ObterFolhaObraSimples(string STAMP)
        {
            return ObterFolhaObra("select TOP 1 (select TOP 1 logi2 from bo where pastamp = pa.pastamp) as logi2,(select TOP 1 obrano from bo where orinopat=nopat and ndos=49) as id_at, * from pa full outer join u_intervencao on u_intervencao.STAMP_DEST=pa.pastamp full outer join u_marcacao on u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp where pa.pastamp='" + STAMP + "' order by nopat;", false);

        }

        public string ObterRubrica(int IdFolhaObra)
        {
            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select u_vestigio from bo where pastamp = (select pastamp from pa where nopat=" + IdFolhaObra + ");", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    result.Read();
                    if (result.HasRows) return FicheirosContext.ObterCaminhoAssinatura(result["u_vestigio"].ToString());
                }
            }
            catch (Exception ex)
            {
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
                            StampIntervencao = result["mhstamp"].ToString().Trim(),
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
            return ObterIntervencoes("select nopat, mhstamp, tecnico, tecnnm, data, hora, horaf, relatorio from mh where nopat=" + IdFolhaObra + " order by nopat;");
        }
        public List<Intervencao> ObterHistorico(string stamp)
        {
            return ObterIntervencoes("select nopat, mhstamp, tecnico, tecnnm, data, hora, horaf, relatorio, serie from mh where mastamp='" + stamp + "' order by data;");
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
        public List<Movimentos> ObterPecasGuiaTransporte(string GuiaTransporte, Utilizador u)
        {
            return ObterListaMovimentos("select CONCAT(V_DOCS_GLOBAL.obrano, ' - AT ', V_DOCS_GLOBAL.atcodeid) as guiatransporte, * from V_DOCS_GLOBAL, pa inner join bo on bo.pastamp=pa.pastamp inner join bi on bi.obrano=bo.obrano where ref!='' and ref not like '%SRV%' and ref not like '%IMO%' and ref not like '%PAT%' and bo.ndos=49 and bi.armazem=" + u.IdArmazem + " and V_DOCS_GLOBAL.ar2mazem=bi.armazem and V_DOCS_GLOBAL.dataobra<=bi.ousrdata and pa.tecnico=" + u.IdPHC + " and V_DOCS_GLOBAL.obrano like '%" + GuiaTransporte + "%' order by ref;");
        }
        public List<String> ObterGuiasTransporte(int IdArmazem)
        {
            return ExecutarQuery("SELECT obrano from V_DOCS_GLOBAL where ar2mazem = " + IdArmazem + " order by dataobra desc");
        }
        #endregion

        //Obter Marcacoes
        #region MARCACOES
        public int CriarMarcacao(Marcacao m)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };
            try
            {
                m.DataMarcacao = m.DatasAdicionaisDistintas.First();
                string SQL_Query = "EXEC WEB_Marcacao_Gera ";

                SQL_Query += "@NO = '" + m.Cliente.IdCliente + "', ";
                SQL_Query += "@ESTAB = '" + m.Cliente.IdLoja + "', ";
                SQL_Query += "@TECNICO = '" + m.Tecnico.IdPHC + "', ";
                SQL_Query += "@TECNICOS = '" + string.Join(";", m.LstTecnicos.Select(x => x.IdPHC)) + "', ";
                SQL_Query += "@ESTADO = '" + m.EstadoMarcacaoDesc + "', ";
                SQL_Query += "@PERIODO = '" + m.Periodo + "', ";
                SQL_Query += "@DATAPEDIDO = '" + m.DataPedido.ToString("yyyyMMdd") + "', ";
                SQL_Query += "@DATA = '" + m.DataMarcacao.ToString("yyyyMMdd") + "', ";
                SQL_Query += "@MDATAS = '" + string.Join(";", m.DatasAdicionaisDistintas.Select(x => x.ToString("yyyyMMdd"))) + "', ";
                SQL_Query += "@HORA = '" + (!String.IsNullOrEmpty(m.Hora) ? (m.Hora == "00:00" ? "" : DateTime.Parse(m.Hora).ToString("HHmm")) : "") + "', ";
                SQL_Query += "@PRIORIDADE = '" + m.PrioridadeMarcacao + "', ";
                SQL_Query += "@TIPOS = '" + m.TipoServico + "', ";
                SQL_Query += "@TIPOE = '" + m.TipoEquipamento + "', ";
                SQL_Query += "@TIPOPEDIDO = '" + m.TipoPedido + "', ";
                SQL_Query += "@NINCIDENTE = '" + m.Referencia + "', ";
                SQL_Query += "@QPEDIU = '" + m.QuemPediuNome + "', ";
                SQL_Query += "@RESPTLM = '" + m.QuemPediuTelefone + "', ";
                SQL_Query += "@RESPEMAIL = '" + m.QuemPediuEmail + "', ";
                SQL_Query += "@RESUMO = '" + m.ResumoMarcacao + "', ";
                SQL_Query += "@PIQUETE = '" + (m.Piquete ? "1" : "0") + "', ";
                SQL_Query += "@OFICINA = '" + (m.Oficina ? "1" : "0") + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + m.Utilizador.NomeCompleto + "'; ";

                res = ExecutarQuery(SQL_Query);

                if (res[0].ToString() != "-1")
                {
                    m.IdMarcacao = int.Parse(res[3]);

                    if (NotificacaoContext.NotificacaoAutomaticaNextcloud(m.Tecnico) && m.LstTecnicos.Where(t => t.Id == m.Utilizador.Id).Count() == 0) ChatContext.EnviarNotificacaoMarcacaoTecnico(m, m.Tecnico);
                    if (NotificacaoContext.NotificacaoAutomaticaEmail(m.Tecnico)) MailContext.EnviarEmailMarcacaoTecnico(m.Tecnico.EmailUtilizador, m, m.Tecnico.NomeCompleto);
                    if (NotificacaoContext.NotificacaoClienteIndustrial(m.Cliente) && m.Cliente.Vendedor.Id > 0) MailContext.EnviarEmailMarcacaoCliente(m.Cliente.Vendedor.EmailUtilizador, m, null);
                    FT_ManagementContext.AdicionarLog(m.Utilizador.Id, "Marcação criada com sucesso! - Nº " + m.IdMarcacao + ", " + m.Cliente.NomeCliente + " pelo utilizador " + m.Utilizador.NomeCompleto, 5);
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel enviar marcacao para o PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return m.IdMarcacao;
        }
        public bool AtualizaMarcacao(Marcacao m)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };
            try
            {
                m.DataMarcacao = m.DatasAdicionaisDistintas.First();
                string SQL_Query = "EXEC WEB_Marcacao_Altera ";

                SQL_Query += "@U_MARCACAOSTAMP = '" + m.MarcacaoStamp + "', ";
                SQL_Query += "@NO = '" + m.Cliente.IdCliente + "', ";
                SQL_Query += "@ESTAB = '" + m.Cliente.IdLoja + "', ";
                SQL_Query += "@TECNICO = '" + m.Tecnico.IdPHC + "', ";
                SQL_Query += "@TECNICOS = '" + string.Join(";", m.LstTecnicos.Select(x => x.IdPHC)) + "', ";
                SQL_Query += "@ESTADO = '" + m.EstadoMarcacaoDesc + "', ";
                SQL_Query += "@PERIODO = '" + m.Periodo + "', ";
                SQL_Query += "@DATAPEDIDO = '" + m.DataPedido.ToString("yyyyMMdd") + "', ";
                SQL_Query += "@DATA = '" + m.DataMarcacao.ToString("yyyyMMdd") + "', ";
                SQL_Query += "@MDATAS = '" + string.Join(";", m.DatasAdicionaisDistintas.Select(x => x.ToString("yyyyMMdd"))) + "', ";
                SQL_Query += "@HORA = '" + (!String.IsNullOrEmpty(m.Hora) ? (m.Hora == "00:00" ? "" : DateTime.Parse(m.Hora).ToString("HHmm")) : "") + "', ";
                SQL_Query += "@PRIORIDADE = '" + m.PrioridadeMarcacao + "', ";
                SQL_Query += "@TIPOS = '" + m.TipoServico + "', ";
                SQL_Query += "@TIPOE = '" + m.TipoEquipamento + "', ";
                SQL_Query += "@TIPOPEDIDO = '" + m.TipoPedido + "', ";
                SQL_Query += "@NINCIDENTE = '" + m.Referencia + "', ";
                SQL_Query += "@QPEDIU = '" + m.QuemPediuNome + "', ";
                SQL_Query += "@RESPTLM = '" + m.QuemPediuTelefone + "', ";
                SQL_Query += "@RESPEMAIL = '" + m.QuemPediuEmail + "', ";
                SQL_Query += "@RESUMO = '" + m.ResumoMarcacao + "', ";
                SQL_Query += "@PIQUETE = '" + (m.Piquete ? "1" : "0") + "', ";
                SQL_Query += "@OFICINA = '" + (m.Oficina ? "1" : "0") + "', ";
                SQL_Query += "@JUSTFECHO = '" + (m.JustificacaoFecho.Length > 4000 ? m.JustificacaoFecho.Remove(4000) : m.JustificacaoFecho) + "', ";
                SQL_Query += "@TECFECHO = '" + m.Utilizador.NomeCompleto + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + m.Utilizador.NomeCompleto + "'; ";

                res = ExecutarQuery(SQL_Query);

                if (res[0] != "-1")
                {
                    if (NotificacaoContext.NotificacaoAutomaticaNextcloud(m.Tecnico) && m.LstTecnicos.Where(t => t.Id == m.Utilizador.Id).Count() == 0) ChatContext.EnviarNotificacaoAtualizacaoMarcacaoTecnico(m, m.Tecnico);
                    FT_ManagementContext.AdicionarLog(m.Utilizador.Id, "Marcação atualizada com sucesso! - Nº " + m.IdMarcacao + ", " + m.Cliente.NomeCliente + " pelo utilizador " + m.Utilizador.NomeCompleto, 5);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel atualizar a marcacao para o PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return false;
        }
        public List<string> ValidarMarcacao(Marcacao m)
        {
            List<string> res = new List<string>() { "0", "", "" };

            //Validacoes Opcionais

            //Ja existem marcacoes com essa referencia
            if (m.IdMarcacao == 0)
            {
                //Ja existe incedente
                if (ExecutarQuery("SELECT COUNT(*) FROM U_MARCACAO where nincidente='" + m.Referencia + "'").First() != "0")
                {
                    res[1] += "Já existe marcações com o mesmo numero de incidente. Tem a certeza que deseja proseguir?\r\n";
                }
            }

            //Caso esteja a 0 faz a validação da conta corrente
            if (ExecutarQuery("SELECT u_navdivma FROM CL(Nolock) WHERE cl.no='" + m.Cliente.IdCliente + "' AND cl.estab = 0").First() == "False")
            {
                //Verificar documentos vencidos
                if (ExecutarQuery("select 'valor' = COUNT(*) from cc (nolock) left join re (nolock) on cc.restamp = re.restamp where cc.no = " + m.Cliente.IdCliente + " and (case when cc.moeda ='EURO' or cc.moeda=space(11) then abs((cc.edeb-cc.edebf)-(cc.ecred-cc.ecredf)) else abs((cc.debm-cc.debfm)-(cc.credm-cc.credfm)) end) > (case when cc.moeda='EURO' or cc.moeda=space(11) then 0.010000 else 0 end) AND cc.dataven < GETDATE();").First() != "0")
                {
                    res[1] += "O Cliente escolhido na Marcação tem documentos não regularizados vencidos!!! Verifique por favor!\r\n";
                }
            }

            //Validar ferias do tecnico
            foreach (var item in m.LstTecnicosSelect)
            {
                if (FT_ManagementContext.VerificarFeriasUtilizador(item, m.DataMarcacao))
                {
                    res[1] += "O utilizador, " + FT_ManagementContext.ObterUtilizador(item).NomeCompleto + ", encontra-se de férias nesta data! Por favor verifique.\r\n";
                }
            }

            //Validar dia de feriados
            List<DateTime> LstFeriados = FT_ManagementContext.ObterListaFeriados(m.DataMarcacao.Year.ToString()).Select(f => f.DataFeriado).ToList();
            foreach (var item in m.DatasAdicionaisDistintas)
            {
                if (LstFeriados.Where(f => f.ToShortDateString() == item.ToShortDateString()).Count() > 0)
                {
                    res[1] += "O seguinte dia: " + item.ToShortDateString() + " é feriado! Por favor verifique.\r\n";
                }
            }

            if (!string.IsNullOrEmpty(res[1]))
            {
                res[0] = "1";
                res[1] += "\r\n";
            }

            //Validacoes Obrigatorias

            //Tem de retornar 1 para poder proceder
            if (ExecutarQuery("SELECT 'valor' = COUNT(1) FROM bo (NOLOCK) INNER JOIN bi (NOLOCK) ON bo.bostamp = bi.bostamp WHERE bo.ndos = 45 AND bo.fechada = 0 AND bo.no = '" + m.Cliente.IdCliente + "' AND bo.estab = '" + m.Cliente.IdLoja + "' AND bi.ref IN ('SRV.101', 'SRV.102', 'SRV.103');").First() == "0")
            {
                res[2] += "O Cliente escolhido na Marcação não tem uma tabela de preços definida! Por favor defina uma tabela de preços antes da marcação.\r\n";
            }

            //Quando é cliente pingo doce tem obrigatoriamente de ter quem pediu para avancar
            if (m.Cliente.IdCliente == 878 && String.IsNullOrEmpty(m.QuemPediuNome))
            {
                res[2] += "Tem que indicar Quem Pediu!\r\n";
            }

            //O cliente não pode ter as marcacoes canceladas
            if (ExecutarQuery("SELECT U_MARCCANC FROM cl (NOLOCK) WHERE no = '" + m.Cliente.IdCliente + "' AND estab = '" + m.Cliente.IdLoja + "'").First() == "True")
            {
                res[2] += "O Cliente escolhido na Marcação tem as marcações canceladas.\r\n";
            }

            if (!string.IsNullOrEmpty(res[2]))
            {
                res[0] = "2";
                res[2] += "Não pode gravar.";
            }
            return res;
        }
        public string CriarAnexoMarcacao(MarcacaoAnexo a)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };

            try
            {
                string SQL_Query = "EXEC WEB_Anexo_Gera ";

                SQL_Query += "@U_MARCACAOSTAMP = '" + a.MarcacaoStamp + "', ";
                SQL_Query += "@NOME_FICHEIRO = '" + a.NomeFicheiro + "', ";
                SQL_Query += "@MARCACAO = '" + (a.AnexoMarcacao ? "1" : "0") + "', ";
                SQL_Query += "@ASSINATURA = '" + (a.AnexoAssinatura ? "1" : "0") + "', ";
                SQL_Query += "@INSTALACAO = '" + (a.AnexoInstalacao ? "1" : "0") + "', ";
                SQL_Query += "@PECA = '" + (a.AnexoPeca ? "1" : "0") + "', ";
                SQL_Query += "@EMAIL = '" + (a.AnexoEmail ? "1" : "0") + "', ";
                SQL_Query += "@REF = '" + a.RefPeca + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + a.NomeUtilizador + "', ";
                SQL_Query += "@TITULO = '" + a.ObterNomeLegivel() + "', ";
                SQL_Query += "@OUSRDATA = '" + DateTime.Now.ToString("yyyyMMdd") + "', ";
                SQL_Query += "@OUSRHORA = '" + DateTime.Now.ToShortTimeString() + "'; ";

                res = ExecutarQuery(SQL_Query);

                if (res[2] != "-1") return res[2];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel criar o anexo da marcação do PHC!\r\n(Exception: " + ex.Message + ")");
            }
            return "";
        }
        public bool ApagarAnexoMarcacao(MarcacaoAnexo a)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };
            try
            {
                string SQL_Query = "EXEC WEB_Anexo_Apaga ";

                SQL_Query += "@STAMP = '" + a.AnexoStamp + "'; ";

                res = ExecutarQuery(SQL_Query);

                if (res[0] != "-1") return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel apagar o anexo da marcação do PHC!\r\n(Exception: " + ex.Message + ")");
            }
            return false;
        }
        public List<MarcacaoAnexo> ObterAnexos(Marcacao m)
        {
            List<MarcacaoAnexo> LstAnexos = new List<MarcacaoAnexo>();

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select * from u_anexos_mar where u_marcacaostamp='" + m.MarcacaoStamp + "'", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using SqlDataReader result = command.ExecuteReader();
                while (result.Read())
                {
                    LstAnexos.Add(new MarcacaoAnexo()
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
        public MarcacaoAnexo ObterAnexo(string AnexoStamp)
        {
            List<MarcacaoAnexo> LstAnexos = new List<MarcacaoAnexo>();

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
                    LstAnexos.Add(new MarcacaoAnexo()
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

            return new MarcacaoAnexo();
        }
        public List<MarcacaoAnexo> ObterAnexos()
        {
            List<MarcacaoAnexo> LstAnexos = new List<MarcacaoAnexo>();

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
                    LstAnexos.Add(new MarcacaoAnexo()
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
            List<Utilizador> LstUtilizadores = LoadTecnico ? FT_ManagementContext.ObterListaUtilizadores(false, false) : new List<Utilizador>();
            List<EstadoMarcacao> LstEstadoMarcacao = this.ObterMarcacaoEstados();
            List<Marcacao> LstMarcacao = new List<Marcacao>();
            List<Cliente> LstClientes = LoadCliente ? this.ObterClientes() : new List<Cliente>();
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
                        Cliente = new Cliente() { NomeCliente = result["nome"].ToString() }
                    });
                    if (LoadCliente) LstMarcacao.Last().Cliente = LstClientes.Where(c => c.IdCliente == int.Parse(result["no"].ToString().Trim())).Where(c => c.IdLoja == int.Parse(result["estab"].ToString().Trim())).DefaultIfEmpty(new Cliente()).First();
                    if (LoadComentarios) LstMarcacao.Last().LstComentarios = ObterComentariosMarcacao(int.Parse(result["num"].ToString().Trim()));
                    if (LoadAnexos) LstMarcacao.Last().LstAnexos = ObterAnexos(LstMarcacao.Last()).Where(a => a.ObterTipoFicheiro() != TipoFicheiro.Assinatura).ToList();
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

            return ObterMarcacoes("select * from v_marcacoes  where tecnno = " + IdTecnico + " and data = '" + DataMarcacoes.ToString("yyyyMMdd") + "'", true, true, true, false, true, false).OrderBy(m => m.IdMarcacao).ToList();
        }
        public List<Marcacao> ObterMarcacoes(int numMarcacao, string nomeCliente, string referencia, string tipoe, int idtecnico, string estado)
        {
            //string SQL_Query = "SELECT TOP 200 num, data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas,  no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, u_marcacao.ousrdata, hora, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao full outer join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 where " + (numMarcacao > 0 ? "num like '%" + numMarcacao + "%' and " : "") + (!string.IsNullOrEmpty(nomeCliente) ? "nome like '%" + nomeCliente + "%' and " : "") + (!string.IsNullOrEmpty(estado) && estado != "Todos" ? "estado = '" + estado + "' and " : "") + (!string.IsNullOrEmpty(referencia) ? "nincidente like '%" + referencia + "%' and " : "") + (!string.IsNullOrEmpty(tipoe) && tipoe != "Todos" ? "tipoe like '%" + tipoe + "%' and " : "") + (idtecnico > 0 ? "u_mtecnicos.tecnno=" + idtecnico + " and " : "") + " num is not null";

            //List<Marcacao> LstMarcacoes = ObterMarcacoes(SQL_Query + " order by num desc;", false, true, true, false, false, false);
            //return LstMarcacoes;

            string SQL_Query = "select TOP 200 * from v_marcacoes where ";
            SQL_Query += (numMarcacao > 0 ? "num like '%" + numMarcacao + "%' and " : "");
            SQL_Query += (idtecnico > 0 ? "tecnno=" + idtecnico + " and " : "");
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
            return ObterMarcacoes("select * from v_marcacoes where tecnno=" + IdTecnico + " and data between '" + DataInicio.ToString("yyyyMMdd") + "' and '" + DataFim.ToString("yyyyMMdd") + "'", false, false, true, false, false, false).OrderBy(m => m.IdMarcacao).ToList();
        }
        public List<Marcacao> ObterMarcacoes(Cliente c)
        {
            //List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, u_mdatas.periodo, prioridade, u_marcacao.u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp  WHERE no='" + c.IdCliente+"' and estab='"+c.IdLoja+"' order by u_mdatas.data;", true, true, true, true, false, false);
            //LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE no='" + c.IdCliente + "' and estab='" + c.IdLoja + "' order by data;", true, true, true, true, false, false).AsEnumerable());
            //return LstMarcacoes;

            return ObterMarcacoes("select * from v_marcacoes where no=" + c.IdCliente + " and estab=" + c.IdLoja, true, true, true, true, false, false).OrderBy(m => m.IdMarcacao).ToList();
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
        public List<Marcacao> ObterMarcacoesSimples(Cliente c)
        {
            //List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao full outer join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE estado not in ('AT Validada', 'Aguarda Ped. Compra') order by estado;", false, false, false, false, false, false);
            //return LstMarcacoes;

            return ObterMarcacoes("select * from v_marcacoes where no=" + c.IdCliente + " and estab=" + c.IdLoja, false, false, false, false, false, false).OrderBy(m => m.IdMarcacao).ToList();

        }
        public List<Marcacao> ObterMarcacoesCriadas()
        {
            //List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao full outer join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE estado not in ('AT Validada', 'Aguarda Ped. Compra') order by estado;", false, false, false, false, false, false);
            //return LstMarcacoes;

            return ObterMarcacoes("select * from v_marcacoes WHERE estado in ('Criado', 'Rececionado', 'Agendado') AND tecnno is null;", false, true, true, false, false, false).OrderBy(m => m.IdMarcacao).ToList();

        }
        public List<Marcacao> ObterMarcacoesPendentes(int IdTecnico)
        {
            //List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, u_mdatas.periodo, prioridade, u_marcacao.u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp  WHERE u_mtecnicos.tecnno='" + IdTecnico + "' AND estado not in ('Finalizado', 'Cancelado', 'AT Validada', 'Aguarda Ped. Compra') order by u_mdatas.data;", true, true, true, false, false, false);
            //LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora, u_marcacao.ousrinis  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE u_mtecnicos.tecnno='" + IdTecnico + "' AND estado not in ('Finalizado', 'Cancelado', 'AT Validada', 'Aguarda Ped. Compra') order by data;", true, true, true, false, false, false).AsEnumerable());
            //return LstMarcacoes;

            return ObterMarcacoes("select * from v_marcacoes WHERE tecnno='" + IdTecnico + "' AND estado not in ('Finalizado', 'Cancelado', 'AT Validada', 'Aguarda Ped. Compra') and data <= '" + DateTime.Now.ToString("yyyy-MM-dd") + "';", true, true, true, false, true, false).OrderBy(m => m.IdMarcacao).ToList();
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
        public Marcacao ObterMarcacao(string Stamp)
        {
            //return ObterMarcacao("SELECT num, data, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, estab, tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, ousrdata, ousrhora, u_marcacao.ousrinis  FROM u_marcacao where num='" + IdMarcacao + "' order by num;", true);
            return ObterMarcacao("select * from v_marcacoes where u_marcacaostamp='" + Stamp + "'", true);
        }
        public Marcacao ObterMarcacaoSimples(string Stamp)
        {
            //return ObterMarcacao("SELECT num, data, (select string_agg(CONVERT(VARCHAR(10),data,120), '|') from u_mdatas where u_mdatas.u_marcacaostamp = u_marcacao.u_marcacaostamp) as u_mdatas, no, (SELECT TOP 1 SUBSTRING((SELECT ';'+u_mtecnicos.tecnno  AS [text()] FROM u_mtecnicos WHERE u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 ORDER BY tecnno FOR XML PATH (''), TYPE).value('text()[1]','nvarchar(max)'), 2, 1000)FROM u_mtecnicos) as LstTecnicos, estab, tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, ousrdata, ousrhora, u_marcacao.ousrinis  FROM u_marcacao where num='" + IdMarcacao + "' order by num;", true);
            return ObterMarcacao("select * from v_marcacoes where u_marcacaostamp='" + Stamp + "'", false);
        }
        public List<int> ObterPercentagemMarcacoes(int IdTecnico)
        {
            List<int> res = new List<int>();

            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();
                SqlCommand command = new SqlCommand("select Count(*) as Total from V_Marcacoes where tecnno = '" + IdTecnico + "' union all select Count(*) as Completas  from V_Marcacoes where tecnno = '" + IdTecnico + "' and estado not like 'Agendado'", conn)
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
                        Tipo = int.Parse(result["ndos"].ToString()),
                        StampAtividade = result["bostamp"].ToString()
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

        public List<String> ObterTipoEquipamento()
        {

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
        public List<KeyValuePair<int, string>> ObterEstadoFolhaObra()
        {

            return new List<KeyValuePair<int, string>>() { new KeyValuePair<int, string>(1, "Concluído"), new KeyValuePair<int, string>(2, "Pedido de Peças"), new KeyValuePair<int, string>(3, "Pedido de Orçamento"), new KeyValuePair<int, string>(4, "Pendente"), };

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
            bool result = false;

            List<string> res = new List<string>() { "-1", "Erro", "", "" };

            try
            {
                string SQL_Query = "EXEC WEB_Comentario_Gera ";

                SQL_Query += "@U_MARCACAOSTAMP = '" + c.Marcacao.MarcacaoStamp + "', ";
                SQL_Query += "@COMENTARIO = '" + (c.Descricao.Length > 4000 ? c.Descricao.Remove(4000) : c.Descricao) + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + c.Utilizador.NomeCompleto + "'; ";

                res = ExecutarQuery(SQL_Query);

                result = (res[0].ToString() != "-1");
                FT_ManagementContext.AdicionarLog(c.Utilizador.Id, "Comentário adicionado com sucesso pelo utilizador " + c.Utilizador.NomeCompleto + " à marcação Nº " + c.Marcacao.IdMarcacao + " do cliente " + c.Marcacao.Cliente.NomeCliente, 5);

            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel criar o comentário para o PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return result;
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
            return ObterComentariosMarcacao("select u_comentstamp, num as marcacaostamp, comentario, u_coment.ousrinis, u_coment.ousrdata, u_coment.ousrhora from u_coment inner join u_marcacao on u_marcacao.u_marcacaostamp = u_coment.marcacaostamp WHERE num=" + IdMarcacao + ";").OrderBy(c => c.DataComentario).ToList();

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
                            NomeCliente = String.IsNullOrEmpty(result["Loja_Lin"].ToString()) ? result["Nome"].ToString() : result["Loja_Lin"].ToString(),
                            DataEnvio = DateTime.Parse(result["Data_Envio_Linha"].ToString()),
                            Total = result["Envio_Total"].ToString() == "True",
                            Produto = new Produto()
                            {
                                Ref_Produto = result["ref"].ToString().Trim(),
                                Designacao_Produto = result["design"].ToString(),
                                Stock_Fisico = result["Qtt_Separar"].ToString() == "0" ? double.Parse(result["Qtt_Envio"].ToString()) : double.Parse(result["Qtt_Separar"].ToString())
                            },
                            Fornecido = double.Parse(result["Qtt_Envio"].ToString()) <= 0
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
            List<string> res = new List<string>() { "-1", "Erro", "", "" };

            try
            {
                string SQL_Query = "EXEC WEB_Picking_Gera ";

                SQL_Query += "@STAMP = '" + BO_STAMP + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + NomeUtilizador + "'; ";

                res = ExecutarQuery(SQL_Query);

                if (res[0] != "-1") return res[2];
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel enviar picking para o PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return "0";
        }
        public string FecharPicking(Picking p)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };

            try
            {
                string SQL_Query = "EXEC WEB_Picking_Fecha ";

                SQL_Query += "@STAMP = '" + p.Picking_Stamp + "', ";
                SQL_Query += "@ARMAZEMSTAMP = '" + p.ArmazemDestino.ArmazemStamp + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + p.EditadoPor + "'; ";

                res = ExecutarQuery(SQL_Query);

                if (res[0] != "-1") return res[0];
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel fechar o picking no PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return "0";
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
                            EditadoPor = result["usrinis"].ToString().ToUpper(),
                            Anexos = ObterAnexosDossier(PI_STAMP)
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
                        if (LstPickingLinhas.Where(p => p.Ref_linha == result["ref"].ToString() && p.Serie == (result["USA_NSERIE"].ToString() == "True") && p.Nome_Loja == result["Loja_Lin"].ToString()).Count() == 0)
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
                                EditadoPor = result["usrinis"].ToString(),
                                Nome_Loja = result["Loja_Lin"].ToString()
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

            return LstPickingLinhas.OrderBy(p => p.Qtd_Separar).ToList();
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
            List<string> res = new List<string>() { "-1", "Erro", "", "" };

            try
            {
                string SQL_Query = "EXEC WEB_Picking_Atualiza_Linha ";

                SQL_Query += "@STAMP = '" + linha.Picking_Linha_Stamp + "', ";
                SQL_Query += "@QTT = '" + linha.Qtd_Linha + "', ";
                if (linha.Serie)
                {
                    SQL_Query += "@SERIE = '" + linha.Lista_Ref.First().NumSerie + "', ";
                    SQL_Query += "@BOMASTAMP = '" + linha.Lista_Ref.First().BOMA_STAMP + "', ";
                }
                SQL_Query += "@NOME_UTILIZADOR = '" + linha.EditadoPor + "'; ";

                res = ExecutarQuery(SQL_Query);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel atualizar o picking para o PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }
        public string ValidarPicking(Picking p)
        {
            string res = "";
            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select SUM(o.qtt-o.qtt2) from bi o(NOLOCK) where o.bostamp='" + p.Encomenda.BO_STAMP + "'", conn)
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

                command = new SqlCommand("select a.ref, a.serie, a.armazem, b.noarm  from boma a(nolock) join ma b(nolock) on a.mastamp = b.mastamp where a.bostamp = '" + p.Picking_Stamp + "' and a.armazem <> b.noarm", conn)
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

        //INVENTARIO
        #region Inventario

        public List<string> CriarInventario(int ID_ARMAZEM, string NomeUtilizador)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };

            try
            {
                string SQL_Query = "EXEC WEB_INV_Gera ";

                SQL_Query += "@ARMAZEM = '" + ID_ARMAZEM + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + NomeUtilizador + "'; ";

                res = ExecutarQuery(SQL_Query);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel criar o dossier de inventario no PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }
        public List<string> CriarLinhaInventario(Linha_Picking l)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };

            try
            {
                string SQL_Query = "EXEC WEB_INV_CRIA_LINHA ";

                SQL_Query += "@STAMP = '" + l.Picking_Linha_Stamp + "', ";
                SQL_Query += "@REF = '" + l.Ref_linha + "', ";
                SQL_Query += "@QTT = '" + l.Qtd_Linha + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + l.EditadoPor + "'; ";

                res = ExecutarQuery(SQL_Query);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel adicionar a linha ao dossier de inventario no PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }
        public List<string> CriarSerieLinhaInventario(Ref_Linha_Picking l)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };

            try
            {
                string SQL_Query = "EXEC WEB_INV_CRIA_LINHA_SERIE ";

                SQL_Query += "@STAMP_LIN = '" + l.Picking_Linha_Stamp + "', ";
                SQL_Query += "@STAMP = '" + l.BOMA_STAMP + "', ";
                SQL_Query += "@SERIE = '" + l.NumSerie + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + l.CriadoPor + "'; ";

                res = ExecutarQuery(SQL_Query);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel adicionar o numero de serie ao dossier de inventario no PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }

        public List<string> ApagarLinhaInventario(Ref_Linha_Picking l)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };

            try
            {
                string SQL_Query = "EXEC WEB_INV_Apaga_Linha ";

                SQL_Query += "@STAMP_LIN = '" + l.Picking_Linha_Stamp + "', ";
                SQL_Query += "@STAMP = '" + l.BOMA_STAMP + "'; ";

                res = ExecutarQuery(SQL_Query);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel apagar a linha do dossier de inventario no PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }

        public List<string> ApagarLinhaSerieInventario(string stamp, Ref_Linha_Picking l)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };

            try
            {
                string SQL_Query = "EXEC WEB_INV_APAGA_LINHA_SERIE ";

                SQL_Query += "@STAMP = '" + stamp + "', ";
                SQL_Query += "@STAMP_LIN = '" + l.Picking_Linha_Stamp + "', ";
                SQL_Query += "@STAMP_BOMA = '" + l.BOMA_STAMP + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + l.CriadoPor + "'; ";

                res = ExecutarQuery(SQL_Query);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel apagar o numero de serie ao dossier de inventario no PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }

        public List<Picking> ObterInventarios(int ID_ARMAZEM)
        {
            List<Picking> LstPicking = new List<Picking>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select * from V_Inv_Aberto WHERE armazem=" + ID_ARMAZEM + ";", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstPicking.Add(new Picking()
                        {
                            Picking_Stamp = result["bostamp"].ToString(),
                            NomeDossier = result["nmdos"].ToString(),
                            IdPicking = int.Parse(result["obrano"].ToString()),
                            DataDossier = DateTime.Parse(result["dataobra"].ToString()),
                            NomeCliente = result["Nome"].ToString(),
                            Obs = result["obs"].ToString(),
                            EditadoPor = result["ousrinis"].ToString(),
                        });
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os dossiers de inventario do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstPicking;
        }
        public Picking ObterInventario(string STAMP)
        {
            Picking p = new Picking();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("SELECT * from V_Inv_Cab WHERE INVSTAMP='" + STAMP + "'", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        p = new Picking()
                        {
                            Picking_Stamp = result["INVSTAMP"].ToString(),
                            NomeDossier = result["nmdos"].ToString(),
                            IdPicking = int.Parse(result["obrano"].ToString()),
                            DataDossier = DateTime.Parse(result["DATA"].ToString()),
                            NomeCliente = ObterArmazem(int.Parse(result["armazem"].ToString())).ArmazemNome,
                            EditadoPor = result["ousrinis"].ToString().ToUpper(),
                            Linhas = ObterLinhasInventario(STAMP)
                        };
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler o inventário do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return p;
        }

        public List<Linha_Picking> ObterLinhasInventario(string STAMP)
        {
            List<Linha_Picking> LstInventarioLinhas = new List<Linha_Picking>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select *, (select COUNT(*) from V_INV_SERIE where BISTAMP=V_INV_LIN.BISTAMP) as Qtd_Lida from V_INV_LIN WHERE INVSTAMP='" + STAMP + "'", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstInventarioLinhas.Add(new Linha_Picking()
                        {
                            Picking_Linha_Stamp = result["BISTAMP"].ToString().Trim(),
                            Ref_linha = result["ref"].ToString(),
                            Nome_Linha = result["design"].ToString(),
                            Qtd_Linha = Double.Parse(result["qtt"].ToString()),
                            Qtd_Separar = result["USA_NSERIE"].ToString() == "True" ? Double.Parse(result["Qtd_Lida"].ToString()) : Double.Parse(result["qtt"].ToString()),
                            TipoUnidade = result["UNIDADE"].ToString(),
                            Serie = result["USA_NSERIE"].ToString() == "True",
                            EditadoPor = result["usrinis"].ToString()
                        });
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler as linhas do inventario do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstInventarioLinhas.OrderBy(l => l.Ref_linha).ToList();
        }

        public Linha_Picking ObterLinhaInventario(string STAMP)
        {
            Linha_Picking Linha = new Linha_Picking();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select *, (select COUNT(*) from V_INV_SERIE where BISTAMP=V_INV_LIN.BISTAMP) as Qtd_Lida from V_INV_LIN WHERE BISTAMP='" + STAMP + "'", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        Linha = new Linha_Picking()
                        {
                            Picking_Linha_Stamp = result["BISTAMP"].ToString().Trim(),
                            Ref_linha = result["ref"].ToString().Trim(),
                            Nome_Linha = result["design"].ToString(),
                            Qtd_Linha = Double.Parse(result["qtt"].ToString()),
                            Qtd_Separar = result["USA_NSERIE"].ToString() == "True" ? Double.Parse(result["Qtd_Lida"].ToString()) : Double.Parse(result["qtt"].ToString()),
                            TipoUnidade = result["UNIDADE"].ToString(),
                            Serie = result["USA_NSERIE"].ToString() == "True",
                            EditadoPor = result["usrinis"].ToString()
                        };
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler a linha do inventario do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return Linha;
        }

        public List<Ref_Linha_Picking> ObterSerieLinhaInventario(string STAMP)
        {
            List<Ref_Linha_Picking> LstInventarioSerieLinhas = new List<Ref_Linha_Picking>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("SELECT * from V_Inv_Serie WHERE BISTAMP='" + STAMP + "'", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstInventarioSerieLinhas.Add(new Ref_Linha_Picking()
                        {
                            Picking_Linha_Stamp = result["BISTAMP"].ToString().Trim(),
                            Ref_linha = result["ref"].ToString().Trim(),
                            Nome_Linha = result["design"].ToString().Trim(),
                            Qtd_Separar = Double.Parse(result["qtt"].ToString()),
                            BOMA_STAMP = result["BOMASTAMP"].ToString(),
                            NumSerie = result["serie"].ToString(),
                            CriadoA = DateTime.Parse(DateTime.Parse(result["usrdata"].ToString()).ToShortDateString() + " " + DateTime.Parse(result["usrhora"].ToString()).ToShortTimeString()),
                            CriadoPor = result["usrinis"].ToString()
                        });
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler as linhas de serie do inventario do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstInventarioSerieLinhas;
        }

        public List<string> ValidarLinhaInventario(string STAMP)
        {
            List<string> res = new List<string>() { "-1", "Erro" };
            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select qtt, ISNULL((select COUNT(*) from boma(nolock) where bi.bistamp = bistamp),0) as contagem_sn from bi (nolock) where bistamp = '" + STAMP + "'", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        if (double.Parse(result[0].ToString()) > double.Parse(result[1].ToString()))
                        {
                            res[0] = "-1";
                            res[1] = "Os numeros de série lidos são inferiores á quantidade registada!";
                        }
                        else
                        {
                            res[0] = "1";
                            res[1] = "Está tudo OK";
                        }
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler as linhas de serie do inventario do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }

        public List<string> FecharInventario(Picking i)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };

            try
            {
                string SQL_Query = "EXEC WEB_INV_Fecha ";

                SQL_Query += "@STAMP = '" + i.Picking_Stamp + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + i.EditadoPor + "'; ";

                res = ExecutarQuery(SQL_Query);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel fechar o dossier de inventario no PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }


        #endregion

        //Dossiers
        #region Dossier

        public List<Dossier> ObterDossiers(string SQL_Query, bool LoadLinhas, bool LoadMarcacao, bool LoadFolhaObra, bool LoadAnexos)
        {
            List<Dossier> LstDossiers = new List<Dossier>();

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
                        LstDossiers.Add(new Dossier()
                        {
                            Ecra = "BO",
                            StampDossier = result["bostamp"].ToString().Trim(),
                            NomeDossier = result["nmdos"].ToString(),
                            IdDossier = int.Parse(result["obrano"].ToString()),
                            DataDossier = DateTime.Parse(result["dataobra"].ToString()),
                            Tecnico = int.Parse(result["tecnico"].ToString()) == 0 ? new Utilizador() { NomeCompleto = result["ousrinis"].ToString() } : FT_ManagementContext.ObterListaUtilizadores(false, false).Where(u => u.IdPHC.ToString() == result["tecnico"].ToString()).DefaultIfEmpty(new Utilizador()).First(),
                            Serie = int.Parse(result["ndos"].ToString()),
                            Cliente = int.Parse(result["ndos"].ToString()) == 2 || int.Parse(result["ndos"].ToString()) == 10 || int.Parse(result["no"].ToString()) == 1 ? new Cliente() { NomeCliente = result["nome"].ToString() } : ObterClienteSimples(int.Parse(result["no"].ToString()), int.Parse(result["estab"].ToString())),
                            Referencia = result["obranome"].ToString(),
                            Estado = result["u_estado"].ToString(),
                            Obs = result["obstab2"].ToString(),
                            DataCriacao = DateTime.Parse(DateTime.Parse(result["ousrdata"].ToString()).ToShortDateString() + " " + DateTime.Parse(result["ousrhora"].ToString()).ToShortTimeString()),
                            EditadoPor = result["usrinis"].ToString(),
                            Fechado = result["fechada"].ToString() == "True"
                        });
                        if (LoadFolhaObra) LstDossiers.Last().FolhaObra = ObterFolhaObra(result["pastamp"].ToString());
                        if (LoadMarcacao) LstDossiers.Last().Marcacao = ObterMarcacaoSimples(result["u_stampmar"].ToString());
                        if (LoadLinhas) LstDossiers.Last().Linhas = ObterLinhasDossier(LstDossiers.Last().StampDossier);
                        if (LoadAnexos) LstDossiers.Last().Anexos = ObterAnexosDossier(LstDossiers.Last().StampDossier);
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler o dossier do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstDossiers;
        }
        public List<Dossier> ObterDossiers(DateTime Data, string Filtro, int Serie)
        {
            return ObterDossiers("select top 100 * from bo (nolock) left join bo3 on bo.bostamp=bo3.bo3stamp where " + (string.IsNullOrEmpty(Filtro) ? "dataobra='" + Data.ToString("yyyy-MM-dd") + "'" : "(obrano like '%" + Filtro + "%' OR nome like '%" + Filtro + "%' OR tecnico like '%" + Filtro + "%' OR bo.ousrinis like '%" + Filtro + "%')") + (Serie > 0 ? " AND ndos=" + Serie : "") + " order by nmdos", false, false, false, false);
        }

        public List<Dossier> ObterDossierAberto(Utilizador u)
        {
            return ObterDossiers("select TOP 1 * from bo (nolock) left join bo3 on bo.bostamp=bo3.bo3stamp where bo.ndos in (36) and tecnico=" + u.IdPHC + " and fechada = 0 order by bo.ousrdata DESC;", false, false, false, false);
        }

        public Dossier ObterDossier(string STAMP)
        {
            return ObterDossiers("select * from bo (nolock) left join bo3 on bo.bostamp=bo3.bo3stamp where bostamp='" + STAMP + "';", true, true, true, true).DefaultIfEmpty(new Dossier()).First();
        }

        public List<Linha_Dossier> ObterLinhasDossier(string SQL_Query, bool LoadAll)
        {
            List<Linha_Dossier> LstLinhasDossier = new List<Linha_Dossier>();

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
                        if (!string.IsNullOrEmpty(result["design"].ToString()))
                        {
                            LstLinhasDossier.Add(new Linha_Dossier
                            {
                                Stamp_Dossier = result["bostamp"].ToString(),
                                Stamp_Linha = result["bistamp"].ToString(),
                                Referencia = result["ref"].ToString().Trim(),
                                Designacao = result["design"].ToString().Trim(),
                                Quantidade = Double.Parse(result["qtt"].ToString()),
                                CriadoPor = result["ousrinis"].ToString()
                            });
                        }

                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler as linhas do dossier do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstLinhasDossier;
        }


        public List<Linha_Dossier> ObterLinhasDossier(string STAMP)
        {
            return ObterLinhasDossier("select b.* from bo a(nolock) join bi b(nolock) on a.bostamp = b.bostamp where b.bostamp = '" + STAMP + "' order by lordem", true);
        }

        public Linha_Dossier ObterLinhaDossier(string STAMP)
        {
            return ObterLinhasDossier("select b.* from bo a(nolock) join bi b(nolock) on a.bostamp = b.bostamp where b.bistamp = '" + STAMP + "'", true).DefaultIfEmpty(new Linha_Dossier()).First();
        }

        public List<string> CriarDossier(Dossier d)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };

            try
            {
                string SQL_Query = "EXEC WEB_Dossier_Gera ";

                SQL_Query += "@SERIE = '" + d.Serie + "', ";
                SQL_Query += "@U_MARCACAOSTAMP = '" + (d.Marcacao.MarcacaoStamp == null ? "" : d.Marcacao.MarcacaoStamp) + "', ";
                SQL_Query += "@STAMP_PA = '" + (d.FolhaObra.StampFO == null ? "" : d.FolhaObra.StampFO) + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + d.EditadoPor + "', ";
                SQL_Query += "@IDTECNICO = '" + d.Tecnico.IdPHC + "'; ";

                res = ExecutarQuery(SQL_Query);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel criar o dossier no PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }

        public List<string> CriarLinhaDossier(Linha_Dossier l)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };

            try
            {
                string SQL_Query = "EXEC WEB_Dossier_Cria_Linha ";

                SQL_Query += "@STAMP = '" + l.Stamp_Dossier + "', ";
                SQL_Query += "@REF = '" + l.Referencia + "', ";
                SQL_Query += "@DESIGN = '" + l.Designacao + "', ";
                SQL_Query += "@QTT = '" + l.Quantidade + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + l.CriadoPor + "'; ";

                res = ExecutarQuery(SQL_Query);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel criar a linha do dossier no PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }
        public List<string> ApagarLinhaDossier(string STAMP_DOSSIER, string STAMP_LINHA)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };

            try
            {
                string SQL_Query = "EXEC WEB_Dossier_Apaga_Linha ";

                SQL_Query += "@STAMP = '" + STAMP_DOSSIER + "', ";
                SQL_Query += "@STAMP_LIN = '" + STAMP_LINHA + "'; ";

                res = ExecutarQuery(SQL_Query);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel remover a linha do dossier no PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }

        public List<string> CriarAnexo(Anexo a, IFormFile file)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };
            try
            {
                string SQL_Query = "EXEC WEB_Anexo_Gera_PHC ";

                SQL_Query += "@ECRA = '" + a.Ecra + "', ";
                SQL_Query += "@NDOC = '" + a.Serie + "', ";
                SQL_Query += "@STAMP_ORIGEM = '" + a.Stamp_Origem + "', ";
                SQL_Query += "@RESUMO = '" + a.Resumo + "', ";
                SQL_Query += "@NOME_FICHEIRO = '" + a.Nome + "', ";
                SQL_Query += "@NOME_UTILIZADOR = '" + a.Utilizador.Iniciais + "';";

                res = ExecutarQuery(SQL_Query);

                if (int.Parse(res[0]) < 0) return res;
                res[0] = string.IsNullOrEmpty(FicheirosContext.CriarAnexo(res[3], a.Nome, file)) ? "-1" : "1";
                if (res[0] == "-1") ApagarAnexo(res[2]);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel adicionar o anexo da folha de obra ao PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return res;
        }

        public bool ApagarAnexo(string Stamp_Anexo)
        {
            List<string> res = new List<string>() { "-1", "Erro", "", "" };
            try
            {
                string SQL_Query = "EXEC WEB_Anexo_Apaga ";

                SQL_Query += "@STAMP = '" + Stamp_Anexo + "'; ";

                res = ExecutarQuery(SQL_Query);

                return res[0] != "-1";

            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel apagar o anexo do PHC!\r\n(Exception: " + ex.Message + ")");
            }
            return false;
        }

        public List<Anexo> ObterAnexosDossier(string stamp)
        {
            List<Anexo> LstAnexosDossier = new List<Anexo>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select * from DBO.ANEXOS where recstamp='" + stamp + "';", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstAnexosDossier.Add(new Anexo
                        {
                            Stamp_Anexo = result["anexosstamp"].ToString(),
                            Ecra = result["oritable"].ToString(),
                            Serie = int.Parse(result["tpdos"].ToString()),
                            Stamp_Origem = result["recstamp"].ToString(),
                            Resumo = result["resumo"].ToString(),
                            Nome = result["fname"].ToString(),
                            LocalizacaoFicheiro = result["fullname"].ToString(),
                            Utilizador = new Utilizador() { NomeCompleto = result["ousrinis"].ToString() }
                        });
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os anexos do dossier do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstAnexosDossier;
        }

        public Anexo ObterAnexoDossier(string stamp)
        {
            Anexo a = new Anexo();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select * from DBO.ANEXOS where anexosstamp='" + stamp + "';", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        a = new Anexo
                        {
                            Stamp_Anexo = result["anexosstamp"].ToString(),
                            Ecra = result["oritable"].ToString(),
                            Serie = int.Parse(result["tpdos"].ToString()),
                            Stamp_Origem = result["recstamp"].ToString(),
                            Resumo = result["resumo"].ToString(),
                            Nome = result["fname"].ToString(),
                            LocalizacaoFicheiro = result["fullname"].ToString(),
                            Utilizador = new Utilizador() { NomeCompleto = result["ousrinis"].ToString() }
                        };
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler o anexo do dossier do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return a;
        }

        public List<KeyValuePair<int, string>> ObterSeriesDossiers()
        {
            List<KeyValuePair<int, string>> LstSeries = new List<KeyValuePair<int, string>>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select ndos, nmdos from ts order by nmdos;", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstSeries.Add(new KeyValuePair<int, string>(int.Parse(result["ndos"].ToString()), result["nmdos"].ToString()));
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel obter as series dos dossiers do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstSeries;

        }

        //FATURACAO

        public List<Dossier> ObterDossiersFaturacao(string SQL_Query, bool LoadLinhas, bool LoadAnexos)
        {
            List<Dossier> LstDossiers = new List<Dossier>();

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
                        LstDossiers.Add(new Dossier()
                        {
                            Ecra = "FT",
                            StampDossier = result["ftstamp"].ToString().Trim(),
                            NomeDossier = result["nmdoc"].ToString(),
                            IdDossier = int.Parse(result["fno"].ToString()),
                            Referencia = result["encomenda"].ToString(),
                            DataDossier = DateTime.Parse(result["fdata"].ToString()),
                            Serie = int.Parse(result["ndoc"].ToString()),
                            Cliente = ObterClienteSimples(int.Parse(result["no"].ToString()), int.Parse(result["estab"].ToString())),
                            DataCriacao = DateTime.Parse(DateTime.Parse(result["ousrdata"].ToString()).ToShortDateString() + " " + DateTime.Parse(result["ousrhora"].ToString()).ToShortTimeString()),
                            EditadoPor = result["usrinis"].ToString(),
                            Tecnico = new Utilizador() { NomeCompleto = result["usrinis"].ToString() },
                            Fechado = result["facturada"].ToString() == "True"
                        });
                        if (LoadLinhas) LstDossiers.Last().Linhas = ObterLinhasDossierFaturacao(LstDossiers.Last().StampDossier);
                        if (LoadAnexos) LstDossiers.Last().Anexos = ObterAnexosDossier(LstDossiers.Last().StampDossier);
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler o dossier do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstDossiers;
        }
        public List<Dossier> ObterDossiersFaturacao(DateTime Data, string Filtro, int Serie)
        {
            return ObterDossiersFaturacao("select top 100 * from ft (nolock) where " + (string.IsNullOrEmpty(Filtro) ? "fdata='" + Data.ToString("yyyy-MM-dd") + "'" : "fno like '%" + Filtro + "%' OR nome like '%" + Filtro + "%' OR usrinis like '%" + Filtro + "%' OR encomenda like '%" + Filtro + "%'") + (Serie > 0 ? " AND ndoc=" + Serie : "") + " order by nmdoc", false, false);
        }
        public Dossier ObterDossierFaturacao(string STAMP)
        {
            return ObterDossiersFaturacao("select * from ft (nolock) where ftstamp='" + STAMP + "';", true, true).DefaultIfEmpty(new Dossier()).First();
        }

        public List<Linha_Dossier> ObterLinhasDossierFaturacao(string SQL_Query, bool LoadAll)
        {
            List<Linha_Dossier> LstLinhasDossier = new List<Linha_Dossier>();

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
                        if (!string.IsNullOrEmpty(result["design"].ToString()))
                        {
                            LstLinhasDossier.Add(new Linha_Dossier
                            {
                                Stamp_Dossier = result["ftstamp"].ToString(),
                                Stamp_Linha = result["fistamp"].ToString(),
                                Referencia = result["ref"].ToString().Trim(),
                                Designacao = result["design"].ToString().Trim(),
                                Quantidade = Double.Parse(result["qtt"].ToString()),
                                CriadoPor = result["usrinis"].ToString()
                            });
                        }

                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler as linhas do dossier do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstLinhasDossier;
        }
        public List<Linha_Dossier> ObterLinhasDossierFaturacao(string STAMP)
        {
            return ObterLinhasDossierFaturacao("select * from fi where ftstamp='" + STAMP + "' order by lordem", true);
        }

        public List<KeyValuePair<int, string>> ObterSeriesFaturacao()
        {
            List<KeyValuePair<int, string>> LstSeries = new List<KeyValuePair<int, string>>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand("select ndoc, nmdoc from td order by nmdoc;", conn)
                {
                    CommandTimeout = TIMEOUT
                };
                using (SqlDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        LstSeries.Add(new KeyValuePair<int, string>(int.Parse(result["ndoc"].ToString()), result["nmdoc"].ToString()));
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel obter as series dos dossiers do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstSeries;

        }

        //COMPRAS
        public List<Dossier> ObterDossiersCompras(string SQL_Query, bool LoadLinhas, bool LoadAnexos)
        {
            List<Dossier> LstDossiers = new List<Dossier>();

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
                        LstDossiers.Add(new Dossier()
                        {
                            Ecra = "FO",
                            StampDossier = result["fostamp"].ToString().Trim(),
                            NomeDossier = "Compras",
                            IdDossier = int.Parse(result["foid"].ToString()),
                            Referencia = result["adoc"].ToString().Trim(),
                            DataDossier = DateTime.Parse(result["data"].ToString()),
                            Serie = 0,
                            Cliente = ObterFornecedorCliente(int.Parse(result["no"].ToString())),
                            DataCriacao = DateTime.Parse(DateTime.Parse(result["ousrdata"].ToString()).ToShortDateString() + " " + DateTime.Parse(result["ousrhora"].ToString()).ToShortTimeString()),
                            EditadoPor = result["usrinis"].ToString(),
                            Tecnico = new Utilizador() { NomeCompleto = result["usrinis"].ToString() },
                            Fechado = true
                        });
                        if (LoadLinhas) LstDossiers.Last().Linhas = ObterLinhasDossierCompras(LstDossiers.Last().StampDossier);
                        if (LoadAnexos) LstDossiers.Last().Anexos = ObterAnexosDossier(LstDossiers.Last().StampDossier);
                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler o dossier do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstDossiers;
        }
        public List<Dossier> ObterDossiersCompras(DateTime Data, string Filtro, int Serie)
        {
            return ObterDossiersCompras("select top 100 * from fo (nolock) where " + (string.IsNullOrEmpty(Filtro) ? " data='" + Data.ToString("yyyy-MM-dd") + "' " : " (foid like '%" + Filtro + "%' OR nome like '%" + Filtro + "%' OR usrinis like '%" + Filtro + "%' OR adoc like '%" + Filtro + "%')") + (Serie > 0 ? " AND doccode=" + Serie : "") + " order by doccode", false, false);
        }
        public Dossier ObterDossierCompras(string STAMP)
        {
            return ObterDossiersCompras("select * from fo (nolock) where fostamp='" + STAMP + "';", true, true).DefaultIfEmpty(new Dossier()).First();
        }

        public List<Linha_Dossier> ObterLinhasDossierCompras(string SQL_Query, bool LoadAll)
        {
            List<Linha_Dossier> LstLinhasDossier = new List<Linha_Dossier>();

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
                        if (!string.IsNullOrEmpty(result["design"].ToString()))
                        {
                            LstLinhasDossier.Add(new Linha_Dossier
                            {
                                Stamp_Dossier = result["fostamp"].ToString(),
                                Stamp_Linha = result["fnstamp"].ToString(),
                                Referencia = result["ref"].ToString().Trim(),
                                Designacao = result["design"].ToString().Trim(),
                                Quantidade = Double.Parse(result["qtt"].ToString()),
                                CriadoPor = result["usrinis"].ToString()
                            });
                        }

                    }
                }

                conn.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler as linhas do dossier do PHC!\r\n(Exception: " + ex.Message + ")");
            }

            return LstLinhasDossier;
        }
        public List<Linha_Dossier> ObterLinhasDossierCompras(string STAMP)
        {
            return ObterLinhasDossierCompras("select * from fn where fostamp='" + STAMP + "' order by lordem", true);
        }

        #endregion
    }
}
