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
        private int TIMEOUT = 2;
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



        //Obter Referências
        #region REFERENCIA
        private List<Produto> ObterProdutos(string SQL_Query)
        {

            List<Produto> LstProdutos = new List<Produto>();

            try
            {

                SqlConnection conn = new SqlConnection(ConnectionString);

                conn.Open();

                SqlCommand command = new SqlCommand(SQL_Query, conn);
                command.CommandTimeout = TIMEOUT;
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

                FT_ManagementContext.AtualizarUltimaModificacao("sa", DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));

                Console.WriteLine("Stock's atualizados com sucesso! (PHC -> MYSQL)");
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
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    res = base64String;
                }
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

                SqlCommand command = new SqlCommand(SQL_Query, conn);
                command.CommandTimeout = TIMEOUT;
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

                FT_ManagementContext.AtualizarUltimaModificacao("cl", DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));

                Console.WriteLine("Clientes e Lojas atualizadas com sucesso! (PHC -> MYSQL)");
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
                return ObterClientes("SELECT no, estab, cl.nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, u_clresp.emailfo, tipo, vendedor, cl.usrdata, cl.usrhora FROM cl full outer join u_clresp on cl.clstamp=u_clresp.clstamp where cl.nome like '%"+filtro+ "%' and no is not null order by no, estab;", false, false, false, false);
            }
            return new List<Cliente>() { new Cliente() { } };
        }
        public Cliente ObterCliente(int IdCliente, int IdLoja)
        {
            return ObterCliente("SELECT no, estab, cl.nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, u_clresp.emailfo, tipo, vendedor, cl.usrdata, cl.usrhora FROM cl full outer join u_clresp on cl.clstamp=u_clresp.clstamp where cl.no=" + IdCliente + " and estab=" + IdLoja + " and no is not null;", true);
           
        }
        public Cliente ObterClienteSimples(int IdCliente, int IdLoja)
        {
            return ObterCliente("SELECT no, estab, cl.nome, ncont, telefone, contacto, CONCAT(morada, ' ' ,codpost) AS endereco, u_clresp.emailfo, tipo, vendedor, cl.usrdata, cl.usrhora FROM cl full outer join u_clresp on cl.clstamp=u_clresp.clstamp where cl.no=" + IdCliente + " and estab=" + IdLoja + " and no is not null;", false);
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

                    SqlCommand command = new SqlCommand("SELECT vendedor, vendnm FROM cl where cl.usrdata>='" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "' GROUP BY vendedor, vendnm order by vendedor;", conn);
                    command.CommandTimeout = TIMEOUT;
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

                    FT_ManagementContext.AtualizarUltimaModificacao("cl_1", DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));

                    Console.WriteLine("Vendedores atualizados com sucesso! (PHC -> MYSQL)");

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

                    SqlCommand command = new SqlCommand("SELECT no, nome, CONCAT(morada, ' ', local, ' ', codpost) as MoradaFornecedor, telefone, email, contacto, obs FROM fl where usrdata>='" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "';", conn);
                    command.CommandTimeout = TIMEOUT;
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

                    FT_ManagementContext.AtualizarUltimaModificacao("fl", DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));

                    Console.WriteLine("Fornecedores atualizados com sucesso! (PHC -> MYSQL)");
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

                    SqlCommand command = new SqlCommand(SQL_Query, conn);
                    command.CommandTimeout = TIMEOUT;
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

                    FT_ManagementContext.AtualizarUltimaModificacao("ma", DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));

                    Console.WriteLine("Equipamentos atualizados com sucesso! (PHC -> MYSQL)");
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
        private List<FolhaObra> ObterFolhasObra(string SQL_Query, bool LoadEquipamento, bool LoadCliente)
        {

            List<FolhaObra> LstFolhaObra = new List<FolhaObra>();

            try
            {

                    SqlConnection conn = new SqlConnection(ConnectionString);

                    conn.Open();

                    SqlCommand command = new SqlCommand(SQL_Query, conn);
                    command.CommandTimeout = TIMEOUT;
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
                        }
                    }

                    conn.Close();

                    FT_ManagementContext.AtualizarUltimaModificacao("pa", DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));

                    Console.WriteLine("PAT's atualizados com sucesso! (PHC -> MYSQL)");
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
                return ObterFolhasObra(SQL_Query, LoadAll, LoadAll).DefaultIfEmpty(new FolhaObra()).First();
        }

        public List<FolhaObra> ObterFolhasObra(int IdMarcacao)
        {
            return ObterFolhasObra("select *, (select TOP 1 qassinou from u_intervencao, u_marcacao where u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp and u_marcacao.num=" + IdMarcacao + ") as qassinou, (SELECT u_nincide from u_marcacao where num=" + IdMarcacao + ") as u_nincide, (SELECT u_marcacaostamp from u_marcacao where num=" + IdMarcacao + ") as u_marcacaostamp from pa where (select TOP 1 STAMP_DEST from u_intervencao, u_marcacao where u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp and u_marcacao.num=" + IdMarcacao+") = pastamp order by nopat", true, true);
 
        }
        public List<FolhaObra> ObterFolhasObra(Cliente c)
        {
            return ObterFolhasObra("select * from u_intervencao, pa, u_marcacao where u_intervencao.STAMP_DEST=pa.pastamp and u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp and pa.no='"+c.IdCliente+"' and pa.estab='"+c.IdLoja+"' order by nopat;", true, true);

        }
        public List<FolhaObra> ObterFolhasObra(DateTime Data)
        {
            return ObterFolhasObra("select * from u_intervencao, pa, u_marcacao where u_intervencao.STAMP_DEST=pa.pastamp and u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp and u_intervencao.data='"+Data.ToString("yyyy-MM-dd")+"' order by nopat;", true, true);

        }
        public FolhaObra ObterFolhaObra(int IdFolhaObra)
        {
            return ObterFolhaObra("select TOP 1 * from u_intervencao, pa, u_marcacao where u_intervencao.STAMP_DEST=pa.pastamp and u_intervencao.u_marcacaostamp=u_marcacao.u_marcacaostamp and pa.nopat=" + IdFolhaObra + " order by nopat;", true);

        }


        private List<Intervencao> ObterIntervencoes(string SQL_Query)
        {

            List<Intervencao> LstIntervencao = new List<Intervencao>();

            try
            {
                    SqlConnection conn = new SqlConnection(ConnectionString);

                    conn.Open();

                    SqlCommand command = new SqlCommand(SQL_Query, conn);
                    command.CommandTimeout = TIMEOUT;
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

                    FT_ManagementContext.AtualizarUltimaModificacao("mh", DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));

                    Console.WriteLine("Intervenções atualizadas com sucesso! (PHC -> MYSQL)");
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

                    SqlCommand command = new SqlCommand(SQL_Query, conn);
                    command.CommandTimeout = TIMEOUT;
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

                    FT_ManagementContext.AtualizarUltimaModificacao("bi", DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));

                    Console.WriteLine("Produtos de PAT's atualizados com sucesso! (PHC -> MYSQL)");
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
            List<Utilizador> LstUtilizadores = FT_ManagementContext.ObterListaTecnicos();
            List<EstadoMarcacao> LstEstadoMarcacao = this.ObterMarcacaoEstados();
            List<Marcacao> LstMarcacao = new List<Marcacao>();
            List<Cliente> LstClientes = this.ObterClientes();
            try
            {
                    SqlConnection conn = new SqlConnection(ConnectionString);

                    conn.Open();

                    SqlCommand command = new SqlCommand(SQL_Query, conn);
                    command.CommandTimeout = TIMEOUT;
                    using (SqlDataReader result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            LstMarcacao.Add(new Marcacao()
                            {
                                IdMarcacao = int.Parse(result["num"].ToString().Trim()),
                                DataMarcacao = DateTime.Parse(result["data"].ToString().Trim()),
                                ResumoMarcacao = result["resumo"].ToString().Trim(),
                                EstadoMarcacaoDesc = result["estado"].ToString().Trim(),
                                EstadoMarcacao = LstEstadoMarcacao.Where(e => e.EstadoMarcacaoDesc == result["estado"].ToString().Trim()).DefaultIfEmpty(new EstadoMarcacao()).First().IdEstado,
                                PrioridadeMarcacao = result["prioridade"].ToString().Trim(),
                                MarcacaoStamp = result["u_marcacaostamp"].ToString().Trim(),
                                TipoEquipamento = result["tipoe"].ToString().Trim(),
                                Oficina = result["oficina"].ToString().Trim() == "True" ? 1 : 0,
                                Instalacao = result["tipos"].ToString().Trim() == "Instalação" ? 1 : 0,
                                DataCriacao = DateTime.Parse(DateTime.Parse(result["ousrdata"].ToString().Trim()).ToShortDateString() + " " + result["ousrhora"].ToString()),
                           
                            }) ;

                            if(LoadCliente) LstMarcacao.Last().Cliente = LstClientes.Where(c => c.IdCliente == int.Parse(result["no"].ToString().Trim())).Where(c => c.IdLoja == int.Parse(result["estab"].ToString().Trim())).DefaultIfEmpty(new Cliente()).First();
                            if (LoadComentarios) LstMarcacao.Last().LstComentarios = ObterComentariosMarcacao(int.Parse(result["num"].ToString().Trim()));
                            if (LoadTecnico) LstMarcacao.Last().Tecnico = LstUtilizadores.Where(u => u.Id == int.Parse(result["tecnno"].ToString().Trim())).FirstOrDefault() ?? new Utilizador();
                            if (LoadFolhasObra) LstMarcacao.Last().LstFolhasObra = ObterFolhasObra(int.Parse(result["num"].ToString().Trim()));
                        }
                    conn.Close();

                    FT_ManagementContext.AtualizarUltimaModificacao("u_marcacao", DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));

                    Console.WriteLine("Marcacoes atualizadas com sucesso! (PHC -> MYSQL)");
                }
            }
            catch
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
            List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, prioridade, u_marcacao.u_marcacaostamp, oficina, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp  WHERE u_mtecnicos.tecnno='" + IdTecnico + "' and u_mdatas.data='" + DataMarcacoes.ToString("yyyy-MM-dd") + "' order by num;", true, true, true, true);
            LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, prioridade, u_marcacaostamp, oficina, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE u_mtecnicos.tecnno='" + IdTecnico + "' and data='" + DataMarcacoes.ToString("yyyy-MM-dd") + "' order by num;", true, true, true, true).AsEnumerable());
            return LstMarcacoes;
        }
        public List<Marcacao> ObterMarcacoes(DateTime DataInicio, DateTime DataFim)
        {
            
            List <Marcacao> LstMarcacoes = (ObterMarcacoes("SELECT num, data, u_mtecnicos.tecnno, no, estab, tipos, oficina, u_mtecnicos.tecnnm, nome, estado, prioridade, u_marcacao.ousrdata, u_marcacao.ousrhora, resumo FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp=u_marcacao.u_marcacaostamp and marcado=1 WHERE data>='" + DataInicio.ToString("yyyy-MM-dd") + "'  AND data<='" + DataFim.ToString("yyyy-MM-dd") + "';", false, false, false, false));
            LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, u_mdatas.data, u_mtecnicos.tecnno, no, estab, tipos, oficina, u_mtecnicos.tecnnm, nome, estado, prioridade, u_marcacao.ousrdata, u_marcacao.ousrhora, resumo  FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp WHERE u_mdatas.data>='" + DataInicio.ToString("yyyy-MM-dd") + "'  AND u_mdatas.data<='" + DataFim.ToString("yyyy-MM-dd") + "' ;", false, false, false, false).AsEnumerable());
            return LstMarcacoes;
        }
        public List<Marcacao> ObterMarcacoes(int IdTecnico, DateTime DataInicio, DateTime DataFim)
        {
            List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, prioridade, u_marcacao.u_marcacaostamp, oficina, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp  WHERE u_mtecnicos.tecnno='" + IdTecnico + "' and  u_mdatas.data>='" + DataInicio.ToString("yyyy-MM-dd") + "'  AND u_mdatas.data<='" + DataFim.ToString("yyyy-MM-dd") + "' order by u_mdatas.data;", true, true, true, true);
            LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, prioridade, u_marcacaostamp, oficina, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE u_mtecnicos.tecnno='" + IdTecnico + "' and  data>='" + DataInicio.ToString("yyyy-MM-dd") + "'  AND DataMarcacao<='" + DataFim.ToString("yyyy-MM-dd") + "' order by data;", true, true, true, true).AsEnumerable());
            return LstMarcacoes;
        }
        public List<Marcacao> ObterMarcacoes(Cliente c)
        {
            List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, prioridade, u_marcacao.u_marcacaostamp, oficina, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp  WHERE no='"+c.IdCliente+"' and estab='"+c.IdLoja+"' order by u_mdatas.data;", true, true, true, true);
            LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, prioridade, u_marcacaostamp, oficina, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE no='" + c.IdCliente + "' and estab='" + c.IdLoja + "' order by data;", true, true, true, true).AsEnumerable());
            return LstMarcacoes;
        }
        public List<Marcacao> ObterMarcacoesPendentes(int IdTecnico)
        {
            List<EstadoMarcacao> LstEstados = ObterMarcacaoEstados();
            List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, prioridade, u_marcacao.u_marcacaostamp, oficina, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp  WHERE u_mtecnicos.tecnno='" + IdTecnico + "' AND estado!='" + LstEstados[3].EstadoMarcacaoDesc + "' AND estado!='" + LstEstados[14].EstadoMarcacaoDesc + "' AND estado!='" + LstEstados[19].EstadoMarcacaoDesc + "' AND estado!='" + LstEstados[2].EstadoMarcacaoDesc + "' order by u_mdatas.data;", true, true, true, true);
            LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, prioridade, u_marcacaostamp, oficina, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 WHERE u_mtecnicos.tecnno='" + IdTecnico + "' AND estado!='" + LstEstados[3].EstadoMarcacaoDesc + "' AND estado!='" + LstEstados[14].EstadoMarcacaoDesc + "' AND estado!='" + LstEstados[19].EstadoMarcacaoDesc + "' AND estado!='" + LstEstados[2].EstadoMarcacaoDesc + "' order by data;", true, true, true, true).AsEnumerable());
            return LstMarcacoes;
        }
        public List<Marcacao> ObterMarcacoes()
        {
            List<EstadoMarcacao> LstEstados = ObterMarcacaoEstados();
            List<Marcacao> LstMarcacoes = ObterMarcacoes("SELECT num, u_mdatas.data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, prioridade, u_marcacao.u_marcacaostamp, oficina, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 inner join u_mdatas on u_mdatas.u_marcacaostamp=u_marcacao.u_marcacaostamp order by u_mdatas.data;", false, false, false, false);
            LstMarcacoes.AddRange(ObterMarcacoes("SELECT num, data, no, estab, u_mtecnicos.tecnno, tipoe, tipos, resumo, estado, prioridade, u_marcacaostamp, oficina, u_marcacao.ousrdata, u_marcacao.ousrhora FROM u_marcacao inner join u_mtecnicos on u_mtecnicos.marcacaostamp = u_marcacao.u_marcacaostamp and u_mtecnicos.marcado=1 order by data;", false, false, false, false).AsEnumerable());
            return LstMarcacoes;
        }
        public Marcacao ObterMarcacao(int IdMarcacao)
        {
           return ObterMarcacao("SELECT num, data, no, estab, tecnno, tipoe, tipos, resumo, estado, prioridade, u_marcacaostamp, oficina, ousrdata, ousrhora FROM u_marcacao where num='" + IdMarcacao + "' order by num;", true);
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

                    FT_ManagementContext.AtualizarUltimaModificacao("u_estados", DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));

                    Console.WriteLine("Estados marcação atualizados com sucesso! (PHC -> MYSQL)");
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

        private List<Comentario> ObterComentariosMarcacao(string SQL_Query)
        {

            List<Comentario> LstComentario = new List<Comentario>();

            try
            {
                    SqlConnection conn = new SqlConnection(ConnectionString);

                    conn.Open();

                    SqlCommand command = new SqlCommand(SQL_Query, conn);
                    command.CommandTimeout = TIMEOUT;
                    using (SqlDataReader result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            LstComentario.Add(new Comentario()
                            {
                                IdComentario = result["u_comentstamp"].ToString(),
                                Descricao = result["comentario"].ToString().Trim(),
                                IdMarcacao = result["marcacaostamp"].ToString().Trim(),
                                NomeUtilizador = result["ousrinis"].ToString().Trim()
                            });
                        }
                    }

                    conn.Close();

                    FT_ManagementContext.AtualizarUltimaModificacao("u_coment", DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));

                    Console.WriteLine("Comentários por marcacao atualizadas com sucesso! (PHC -> MYSQL)");
            }
            catch 
            {
                Console.WriteLine("Não foi possivel ler os comentarios das Marcacoes do PHC!");
            }

            return LstComentario;
        }
        public List<Comentario> ObterComentariosMarcacao(int IdMarcacao)
        {
            return ObterComentariosMarcacao("select u_comentstamp, num as marcacaostamp, comentario, u_coment.ousrinis from u_coment inner join u_marcacao on u_marcacao.u_marcacaostamp = u_coment.marcacaostamp WHERE num=" + IdMarcacao+";");
            
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

                    SqlCommand command = new SqlCommand("select cm, acao, data, hora, CONCAT('KM: ', km, ' | Obs:', obs) as obs from u_dias join cm4 on u_dias.tecnico = cm4.nome where u_dias.usrdata >= '" + dataUltimaLeitura.ToString("yyyy-MM-dd HH:mm:ss") + "' ; ", conn);
                    command.CommandTimeout = TIMEOUT;
                    using (SqlDataReader result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            LstAcessos.Add(new Acesso()
                            {
                                IdUtilizador = int.Parse(result["cm"].ToString()),
                                Tipo = result["acao"].ToString().Contains("Inicio") ? 1 : 2,
                                Data = DateTime.Parse(DateTime.Parse(result["data"].ToString()).ToShortDateString() + " " + DateTime.Parse(result["hora"].ToString().Substring(0,2) + ":" + result["hora"].ToString().Substring(2, 2) + ":" + result["hora"].ToString().Substring(4, 2)).ToLongTimeString()),
                                Temperatura = result["obs"].ToString()
                            });
                        }
                    }

                    conn.Close();

                    FT_ManagementContext.AtualizarUltimaModificacao("u_dias", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    Console.WriteLine("Acessos atualizados com sucesso! (PHC -> MYSQL)");
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
