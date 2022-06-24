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
        private readonly int TIMEOUT = 2;
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
                Console.WriteLine("Connectado á Base de Dados PHC com sucesso!");
            }
            catch
            {
                Console.WriteLine("Não foi possivel conectar á BD PHC!");
            }
        }


        //EM DESENVOLVIMENTO
        public string ExecutarQuery(string SQL_Query)
        {
            string res = "";

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
                        res = result[0].ToString();
                    }
                }
                conn.Close();
            }

            catch
            {
                Console.WriteLine("Não foi possivel executar query");
            }

            return res;
        }
        public string ValidarMarcacao(Marcacao m)
        {
            //Tem de retornar 1 para poder proceder
            if (ExecutarQuery("SELECT 'valor' = COUNT(1) FROM bo (NOLOCK) INNER JOIN bi (NOLOCK) ON bo.bostamp = bi.bostamp WHERE bo.ndos = 45 AND bo.fechada = 0 AND bo.no = '"+m.Cliente.IdCliente+"' AND bo.estab = '"+m.Cliente.IdLoja+"' AND bi.ref IN ('SRV.101', 'SRV.102', 'SRV.103');") == "0") return "O Cliente escolhido na Marcação não tem uma tabela de preços definida! Por favor defina uma tabela de preços antes da marcação.";

            //Quando é cliente pingo doce tem obrigatoriamente de ter quem pediu para avancar
           if (m.Cliente.IdCliente == 878 && String.IsNullOrEmpty(m.QuemPediuNome)) return "Tem que indicar Quem Pediu!";

            //O cliente não pode ter as marcacoes canceladas
            if (ExecutarQuery("SELECT U_MARCCANC FROM cl (NOLOCK) WHERE no = '"+m.Cliente.IdCliente+"' AND estab = '"+m.Cliente.IdLoja+"'") == "True") return "O Cliente escolhido na Marcação tem as marcações canceladas. Não pode gravar.";

            //Caso esteja a 0 faz a validação da conta corrente
            if (ExecutarQuery("SELECT u_navdivma FROM CL(Nolock) WHERE cl.no='" + m.Cliente.IdCliente + "' AND cl.estab = 0") == "False")
            {
                //Verificar documentos vencidos
                if (ExecutarQuery("select 'valor' = COUNT(*) from cc (nolock) left join re (nolock) on cc.restamp = re.restamp where cc.no = "+m.Cliente.IdCliente+" and (case when cc.moeda ='EURO' or cc.moeda=space(11) then abs((cc.edeb-cc.edebf)-(cc.ecred-cc.ecredf)) else abs((cc.debm-cc.debfm)-(cc.credm-cc.credfm)) end) > (case when cc.moeda='EURO' or cc.moeda=space(11) then 0.010000 else 0 end) AND cc.dataven < GETDATE();") != "0") return "O Cliente escolhido na Marcação tem documentos não regularizados vencidos!!! Verifique por favor!";
            }
            foreach (var item in m.LstTecnicos)
            {
                if (FT_ManagementContext.VerificarFeriasUtilizador(item.Id, m.DataMarcacao)) return "O utilizador, " + item.NomeCompleto + ", encontra-se de férias nesta data! Por favor verifique.";
            }
            return "";
        }
        public int CriarMarcacao(Marcacao m)
        {
            int res = 0;
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                string Tecnicos = "";
                foreach (var item in m.LstTecnicos)
                {
                    if (Tecnicos.Length > 0) Tecnicos += ";";
                    Tecnicos += item.IdPHC;
                }

                SqlCommand command = new SqlCommand("Gera_Marcacao", conn)
                {
                    CommandTimeout = TIMEOUT,
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add(new SqlParameter("@NO", m.Cliente.IdCliente));
                command.Parameters.Add(new SqlParameter("@ESTAB", m.Cliente.IdLoja));
                command.Parameters.Add(new SqlParameter("@TECNICO", m.Tecnico.IdPHC));
                command.Parameters.Add(new SqlParameter("@TECNICOS", Tecnicos));
                command.Parameters.Add(new SqlParameter("@ESTADO", m.EstadoMarcacaoDesc));
                command.Parameters.Add(new SqlParameter("@PERIODO", m.Periodo));
                command.Parameters.Add(new SqlParameter("@DATAPEDIDO", m.DataPedido.ToString("yyyyMMdd")));
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
                command.Parameters.Add(new SqlParameter("@NOME_UTILIZADOR", m.Utilizador));

                using SqlDataReader result = command.ExecuteReader();
                result.Read();

                if (result[0].ToString() != "-1") res = int.Parse(result[3].ToString());

                conn.Close();

                Console.WriteLine("Enviada marcacao para o PHC pelo utilizador " + m.Utilizador);

            }

            catch
            {
                Console.WriteLine("Erro ao enviar marcacao para o PHC");
            }

            return res;
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
            catch
            {
                Console.WriteLine("Não foi possivel ler os reponsaveis do PHC!");
            }

            return m;
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
                            Ref_Produto = result["ref"].ToString(),
                            Designacao_Produto = result["design"].ToString(),
                            //Stock_Fisico = double.Parse(result["stock_fis"].ToString()),
                            Stock_PHC = double.Parse(result["stock"].ToString()),
                            Stock_Rec = double.Parse(result["qttrec"].ToString()),
                            Stock_Res = double.Parse(result["rescli"].ToString()),
                            Armazem_ID = int.Parse(result["armazem"].ToString()),
                            Pos_Stock = result["u_locpt"].ToString()
                        });
                    }
                }

                conn.Close();
            }

            catch
            {
                Console.WriteLine("Não foi possivel ler as referencias do PHC!");
            }

            return LstProdutos;
        }
        public List<Produto> ObterProdutos(string Referencia, string Designacao, int IdArmazem)
        {
            return ObterProdutos("SELECT sa.ref, st.design, sa.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec, stobs.u_locpt FROM sa inner join st on sa.ref=st.ref inner join stobs on sa.ref=stobs.ref where sa.ref like '%"+Referencia+"%' AND st.design like '%"+Designacao+"%' AND sa.armazem='"+IdArmazem+"' order by sa.ref;");
        }

        public Produto ObterProduto(string Referencia, int IdArmazem)
        {
            List<Produto> p = ObterProdutos("SELECT sa.ref, st.design, sa.stock, sa.armazem, sa.rescli, (sa.stock - sa.rescli) as stock_fis, sa.qttrec, stobs.u_locpt FROM sa inner join st on sa.ref=st.ref inner join stobs on sa.ref=stobs.ref where sa.ref='" + Referencia + "' AND sa.armazem='" + IdArmazem + "' order by sa.ref;");
            if (p.Count > 0)
            {
                p[0].ImgProduto = ObterProdutoImagem(p[0]);
                return p[0];
            }
            return new Produto();
        }
        private string ObterProdutoImagem(Produto p)
        {
            string img = "/server/Imagens/EQUIPAMENTOS/" + p.Ref_Produto + ".jpg";
            string res = "";
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
                    }
                }

                conn.Close();
            }
            catch
            {
                Console.WriteLine("Não foi possivel ler os clientes do PHC!");
            }

            return LstClientes;
        }
        private Cliente ObterCliente(string SQL_Query, bool LoadAll)
        {
                return ObterClientes(SQL_Query, LoadAll, LoadAll, LoadAll, LoadAll).DefaultIfEmpty(new Cliente()).First();
        }

        public List<Cliente> ObterClientes()
        {
            return ObterClientes("SELECT no, estab, cl.nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, u_clresp.emailfo, tipo, vendedor, cl.usrdata, cl.usrhora FROM cl full outer join u_clresp on cl.clstamp=u_clresp.clstamp where no is not null order by no, estab ;", false, false, false, false);
        }
        public List<Cliente> ObterClientes(string filtro, bool filtrar)
        {
            if (filtrar)
            {
                return ObterClientes("SELECT no, estab, cl.nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, (select TOP 1 emailfo from u_clresp where cl.clstamp=u_clresp.clstamp) as emailfo, tipo, vendedor, cl.usrdata, cl.usrhora FROM cl where cl.nome like '%"+filtro+"%' and no is not null order by no, estab;", false, false, false, false);
            }
            return new List<Cliente>() { new Cliente() { } };
        }
        public Cliente ObterCliente(int IdCliente, int IdLoja)
        {
            return ObterCliente("SELECT no, estab, cl.nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, u_clresp.emailfo, tipo, vendedor, cl.usrdata, cl.usrhora FROM cl full outer join u_clresp on cl.clstamp=u_clresp.clstamp where cl.no=" + IdCliente + " and estab=" + IdLoja + " and no is not null;", true);
           
        }
        public Cliente ObterClienteSimples(int IdCliente, int IdLoja)
        {
            return ObterCliente("SELECT TOP 1 no, estab, cl.nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, u_clresp.emailfo, tipo, vendedor, cl.usrdata, cl.usrhora FROM cl full outer join u_clresp on cl.clstamp=u_clresp.clstamp where cl.no=" + IdCliente + " and estab=" + IdLoja + " and no is not null;", false);
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
            catch
            {
                Console.WriteLine("Não foi possivel ler os vendedores do PHC!");
            }

            return LstVendedor;
        }
        #endregion

        //Obter Fornecedores
        #region FORNECEDORES
        public List<Fornecedor> ObterFornecedores(DateTime dataUltimaLeitura)
        {

            List<Fornecedor> LstFornecedor = new List<Fornecedor>();

                try
                {
                    SqlConnection conn = new SqlConnection(ConnectionString);

                    conn.Open();

                SqlCommand command = new SqlCommand("SELECT no, nome, CONCAT(morada, ' ', local, ' ', codpost) as MoradaFornecedor, telefone, email, contacto, obs FROM fl where usrdata>='" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "';", conn)
                {
                    CommandTimeout = TIMEOUT
                };
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
                }
            catch
            {
                Console.WriteLine("Não foi possivel ler os Fornecedores do PHC!");
            }

            return LstFornecedor;
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
            catch
            {
                Console.WriteLine("Não foi possivel ler os Equipamentos do PHC!");
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
                                ConferidoPor = result["qassinou"].ToString().Trim(),
                                IdCartao = result["u_marcacaostamp"].ToString().Trim()
                            });

                        if (LoadEquipamento) LstFolhaObra.Last().EquipamentoServico = ObterEquipamento(result["mastamp"].ToString().Trim());
                        if (LoadCliente) LstFolhaObra.Last().ClienteServico = ObterClienteSimples(int.Parse(result["no"].ToString().Trim()), int.Parse(result["estab"].ToString().Trim()));
                        if (LoadIntervencoes) { 
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
                        }
                    }
                }

                    conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler os PAT's do PHC!");
                throw new Exception(ex.Message);
            }

            return LstFolhaObra;
        }
        public FolhaObra ObterFolhaObra(string SQL_Query, bool LoadAll)
        {
                return ObterFolhasObra(SQL_Query, LoadAll, LoadAll, LoadAll, LoadAll, LoadAll).DefaultIfEmpty(new FolhaObra()).First();
        }

        public List<FolhaObra> ObterFolhasObra(int IdMarcacao)
        {
            return ObterFolhasObra("select *, (select TOP 1 qassinou from u_intervencao, u_marcacao where u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp and u_marcacao.num=" + IdMarcacao + ") as qassinou, (SELECT u_nincide from u_marcacao where num=" + IdMarcacao + ") as u_nincide, (SELECT u_marcacaostamp from u_marcacao where num=" + IdMarcacao + ") as u_marcacaostamp from pa where (select TOP 1 STAMP_DEST from u_intervencao, u_marcacao where u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp and u_marcacao.num=" + IdMarcacao+") = pastamp order by nopat", true, true, false, false, false);
 
        }
        public List<FolhaObra> ObterFolhasObra(Cliente c)
        {
            return ObterFolhasObra("select * from u_intervencao, pa, u_marcacao where u_intervencao.STAMP_DEST=pa.pastamp and u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp and pa.no='"+c.IdCliente+"' and pa.estab='"+c.IdLoja+"' order by nopat;", true, true, false, false, false);

        }
        public List<FolhaObra> ObterFolhasObra(DateTime Data)
        {
            return ObterFolhasObra("select * from u_intervencao, pa, u_marcacao where u_intervencao.STAMP_DEST=pa.pastamp and u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp and u_intervencao.data='"+Data.ToString("yyyy-MM-dd")+"' order by nopat;", true, true, true, false, false);

        }
        public FolhaObra ObterFolhaObra(int IdFolhaObra)
        {
            return ObterFolhaObra("select TOP 1 * from u_intervencao, pa, u_marcacao where u_intervencao.STAMP_DEST=pa.pastamp and u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp and pa.nopat=" + IdFolhaObra + " order by nopat;", true);

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
                    return result["u_vestigio"].ToString().Replace("\\", "/").Replace("S:", "/server");
                }
            }
            catch { 

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
            catch 
            {
                Console.WriteLine("Não foi possivel ler as intervenções do PHC!");
            }

            return LstIntervencao;
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
            catch
            {
                Console.WriteLine("Não foi possivel ler as peças usadas pelos PAT's do PHC!");
            }

            return LstProduto;
        }
        public List<Produto> ObterPecas(int IdFolhaObra)
        {
            return ObterPecas("select pa.nopat, bi.ref, bi.design, bi.qtt, (SELECT TOP 1 obrano from V_DOCS_GLOBAL WHERE ar2mazem=bi.armazem and dataobra<bi.dataobra and bi.ref not like '%SRV%' order by dataobra desc) as guiatransporte from pa inner join bo on bo.pastamp=pa.pastamp inner join bi on bi.obrano=bo.obrano where ref!=''  and bo.ndos=49 and pa.nopat=" + IdFolhaObra + " order by ref;");
        }
        public string ObterGuiaTransporte(List<Produto> LstProdutos)
        {
            string res = "";
            foreach (var item in LstProdutos)
            {
                if (!res.Contains(item.Pos_Stock) && !String.IsNullOrEmpty(item.Pos_Stock)) res += item.Pos_Stock;
            }
            return res;
        }
        #endregion

        //Obter Marcacoes
        #region MARCACOES
        private List<Marcacao> ObterMarcacoes(string SQL_Query, bool LoadComentarios, bool LoadCliente, bool LoadTecnico, bool LoadFolhasObra)
        {
            List<Utilizador> LstUtilizadores = FT_ManagementContext.ObterListaTecnicos(false);
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
                    LstMarcacao.Add(new Marcacao()
                    {
                        IdMarcacao = int.Parse(result["num"].ToString().Trim()),
                        DataMarcacao = DateTime.Parse(result["data"].ToString()),
                        ResumoMarcacao = result["resumo"].ToString().Trim(),
                        EstadoMarcacaoDesc = result["estado"].ToString().Trim(),
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
                        Hora = result["hora"].ToString().Length == 4 ? result["hora"].ToString()[..2] + ":" + result["hora"].ToString().Substring(2, 2) : ""
                    });

                    if (LoadCliente) LstMarcacao.Last().Cliente = LstClientes.Where(c => c.IdCliente == int.Parse(result["no"].ToString().Trim())).Where(c => c.IdLoja == int.Parse(result["estab"].ToString().Trim())).DefaultIfEmpty(new Cliente()).First();
                    if (LoadComentarios) LstMarcacao.Last().LstComentarios = ObterComentariosMarcacao(int.Parse(result["num"].ToString().Trim()));
                    if (LoadTecnico) LstMarcacao.Last().Tecnico = LstUtilizadores.Where(u => u.Id == int.Parse(result["tecnno"].ToString().Trim())).FirstOrDefault() ?? new Utilizador();
                    if (LoadFolhasObra) LstMarcacao.Last().LstFolhasObra = ObterFolhasObra(int.Parse(result["num"].ToString().Trim()));
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possivel ler as Marcacoes do PHC!");
            }

            return LstMarcacao;
        }
        private Marcacao ObterMarcacao(string SQL_Query, bool LoadAll)
        {
            return ObterMarcacoes(SQL_Query, LoadAll, LoadAll, LoadAll, LoadAll).DefaultIfEmpty(new Marcacao()).First();
        }

        public List<Marcacao> ObterMarcacoes(int IdTecnico, DateTime DataMarcacoes)
        {
            List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, u_mdatas.periodo, prioridade, u_marcacao.u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp  WHERE u_mtecnicos.tecnno='" + IdTecnico + "' and u_mdatas.data='" + DataMarcacoes.ToString("yyyy-MM-dd") + "' and estado!='Cancelado' order by num;", true, true, true, false);
            LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, u_marcacao.ousrdata, hora, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE u_mtecnicos.tecnno='" + IdTecnico + "' and data='" + DataMarcacoes.ToString("yyyy-MM-dd") + "' and estado!='Cancelado' order by num;", true, true, true, false).AsEnumerable());
            return LstMarcacoes;
        }
        public List<Marcacao> ObterMarcacoes(DateTime DataInicio, DateTime DataFim)
        {
            
            List <Marcacao> LstMarcacoes = (ObterMarcacoes("SELECT num, data, u_mtecnicos.tecnno, no, estab, tipos, tipoe, u_marcacao.u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, u_mtecnicos.tecnnm, nome, estado, hora, periodo, prioridade, u_marcacao.ousrdata, u_marcacao.ousrhora, resumo FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 WHERE data>='" + DataInicio.ToString("yyyy-MM-dd") + "'  AND data<='" + DataFim.ToString("yyyy-MM-dd") + "';", false, true, true, false));
            LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, u_mdatas.data, u_mtecnicos.tecnno,  u_marcacao.u_marcacaostamp, no, estab, tipos, tipoe, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, u_mtecnicos.tecnnm, nome, estado, hora, u_mdatas.periodo, prioridade, u_marcacao.ousrdata, u_marcacao.ousrhora, resumo  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp WHERE u_mdatas.data>='" + DataInicio.ToString("yyyy-MM-dd") + "'  AND u_mdatas.data<='" + DataFim.ToString("yyyy-MM-dd") + "' ;", false, true, true, false).AsEnumerable());
            return LstMarcacoes;
        }
        public List<Marcacao> ObterMarcacoes(int IdTecnico, DateTime DataInicio, DateTime DataFim)
        {
            List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, u_mdatas.periodo, prioridade, u_marcacao.u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp  WHERE u_mtecnicos.tecnno='" + IdTecnico + "' and  u_mdatas.data>='" + DataInicio.ToString("yyyy-MM-dd") + "'  AND u_mdatas.data<='" + DataFim.ToString("yyyy-MM-dd") + "' order by u_mdatas.data;", false, true, true, false);
            LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, u_marcacao.ousrdata, hora, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE u_mtecnicos.tecnno='" + IdTecnico + "' and  data>='" + DataInicio.ToString("yyyy-MM-dd") + "'  AND data<='" + DataFim.ToString("yyyy-MM-dd") + "' order by data;", false, true, true, false).AsEnumerable());
            return LstMarcacoes;
        }
        public List<Marcacao> ObterMarcacoes(Cliente c)
        {
            List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, u_mdatas.periodo, prioridade, u_marcacao.u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp  WHERE no='" + c.IdCliente+"' and estab='"+c.IdLoja+"' order by u_mdatas.data;", true, true, true, true);
            LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE no='" + c.IdCliente + "' and estab='" + c.IdLoja + "' order by data;", true, true, true, true).AsEnumerable());
            return LstMarcacoes;
        }
        public List<Marcacao> ObterMarcacoesPendentes(int IdTecnico)
        {
            List<EstadoMarcacao> LstEstados = ObterMarcacaoEstados();
            List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, u_mdatas.periodo, prioridade, u_marcacao.u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp  WHERE u_mtecnicos.tecnno='" + IdTecnico + "' AND estado!='" + LstEstados[3].EstadoMarcacaoDesc + "' AND estado!='" + LstEstados[14].EstadoMarcacaoDesc + "' AND estado!='" + LstEstados[19].EstadoMarcacaoDesc + "' AND estado!='" + LstEstados[2].EstadoMarcacaoDesc + "' order by u_mdatas.data;", true, true, true, false);
            LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE u_mtecnicos.tecnno='" + IdTecnico + "' AND estado!='" + LstEstados[3].EstadoMarcacaoDesc + "' AND estado!='" + LstEstados[14].EstadoMarcacaoDesc + "' AND estado!='" + LstEstados[19].EstadoMarcacaoDesc + "' AND estado!='" + LstEstados[2].EstadoMarcacaoDesc + "' order by data;", true, true, true, false).AsEnumerable());
            return LstMarcacoes;
        }
        public List<Marcacao> ObterMarcacoes()
        {
            List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, u_mdatas.periodo, prioridade, u_marcacao.u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp order by u_mdatas.data;", false, true, true, false);
            LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 order by data;", false, true, true, false).AsEnumerable());
            return LstMarcacoes;
        }
        public Marcacao ObterMarcacao(int IdMarcacao)
        {
           return ObterMarcacao("SELECT num, data, no, estab, tecnno, tipoe, tipos, resumo, estado, periodo, prioridade, u_marcacaostamp, oficina, piquete, nincidente, datapedido, tipopedido, qpediu, respemail, resptlm, hora, ousrdata, ousrhora FROM u_marcacao where num='" + IdMarcacao + "' order by num;", true);
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
            catch
            {
                Console.WriteLine("Não foi possivel ler os Estados do PHC!");
            }

            return LstEstadoMarcacao;
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
            catch
            {
                Console.WriteLine("Erro ao obter tipos de equipamento!");
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
            catch
            {
                Console.WriteLine("Erro ao obter tipos de serviço!");
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
            catch
            {
                Console.WriteLine("Erro ao obter periodos!");
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
            catch
            {
                Console.WriteLine("Erro ao obter prioridades!");
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
            catch
            {
                Console.WriteLine("Erro ao obter prioridades!");
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
                            LstComentario.Add(new Comentario()
                            {
                                IdComentario = result["u_comentstamp"].ToString(),
                                Descricao = result["comentario"].ToString().Trim(),
                                IdMarcacao = result["marcacaostamp"].ToString().Trim(),
                                NomeUtilizador = result["ousrinis"].ToString().Trim(),
                                DataComentario = DateTime.Parse(result["ousrdata"].ToString()[..10] + " " + result["ousrhora"].ToString())
                            });
                        }
                    }

                    conn.Close();
            }
            catch 
            {
                Console.WriteLine("Não foi possivel ler os comentarios das Marcacoes do PHC!");
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
            catch (Exception)
            {
                Console.WriteLine("Não foi possivel ler os Acessos do PHC");
            }

            return LstAcessos;
        }

        public void AtualizarAcessos()
        {
            FT_ManagementContext.CriarAcesso(ObterAcessos(FT_ManagementContext.ObterUltimaModificacaoPHC("u_dias")));
        }
        #endregion


    }
}
