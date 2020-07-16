using System;
using MySql.Simple;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using QRCoder;
using iTextSharp.text.pdf;
using OfficeOpenXml.FormulaParsing.Utilities;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Text.RegularExpressions;

namespace FT_Management.Models
{
    public class FT_ManagementContext
    {

        private string ConnectionString { get; set; }
        private string FT_Logo_Print { get; set; }
        private string PHC_DB_Conn { get; set; }

        public FT_ManagementContext(string connectionString, string FT_Logo, string PHC_DB)
        {
            this.ConnectionString = connectionString;
            this.FT_Logo_Print = FT_Logo;
            this.PHC_DB_Conn = PHC_DB;

            try
            {
                Database db = ConnectionString;
            }
            catch
            {
                throw new Exception("Não foi possivel conectar á BD! Verifique se a base de dados existe e o IP está correto. ");
            } 
        }

        public List<Produto> ObterListaProdutos(string referencia, string desig)
        {
            List<Produto> LstProdutos = new List<Produto>();

            Database db = ConnectionString;

            using (var result = db.Query("SELECT * FROM dat_produtos Where ref_produto like '%"+ referencia + "%' AND designacao_produto like '%"+desig+"%';"))
            {
                while (result.Read())
                {
                    LstProdutos.Add(new Produto()
                    {
                        Ref_Produto = result["ref_produto"],
                        Designacao_Produto = result["designacao_produto"],
                        Stock_Fisico = result["stock_fisico"],
                        Stock_PHC = result["stock_phc"],
                        Pos_Stock = result["pos_stock"],
                        Obs_Produto = result["obs"]
                    });
                }
            }
            db.Connection.Close();
            return LstProdutos;
        }
        public void CarregarFicheiroDB(string FilePath)
        {
            using ExcelPackage package = new ExcelPackage(new FileInfo(FilePath));
            //ExcelWorksheet workSheet = package.Workbook.Worksheets["Table1"];
            ExcelWorksheet workSheet = package.Workbook.Worksheets.First();
            int totalRows = workSheet.Dimension.Rows;

            var LstProdutos = new List<Produto>();

            for (int i = 1; i <= totalRows; i++)
            {
                string ref_prod = workSheet.Cells[i, 1].Value.ToString().Replace(" ", "");
                string desig = workSheet.Cells[i, 2].Value.ToString().Trim();
                double stock_Rececao = 0;
                double.TryParse(workSheet.Cells[i, 3].Value.ToString(), out double stock_PHC);
                if (workSheet.Dimension.End.Column == 4)
                {
                    double.TryParse(workSheet.Cells[i, 4].Value.ToString(), out stock_Rececao);
                }

                if (LstProdutos.Where(p => p.Ref_Produto == ref_prod).Count() == 0)
                {
                    LstProdutos.Add(new Produto
                    {
                        Ref_Produto = ref_prod,
                        Designacao_Produto = desig,
                        Stock_PHC = stock_PHC + stock_Rececao,
                        Stock_Fisico = 0.0,
                        Pos_Stock = "",
                        Obs_Produto = ""
                    });

                }
                else
                {
                    LstProdutos.Where(p => p.Ref_Produto == ref_prod).First().Stock_PHC += stock_PHC;
                    LstProdutos.Where(p => p.Ref_Produto == ref_prod).First().Designacao_Produto = desig;
                }
            }

            CriarArtigos(LstProdutos);
        }

        public Produto ObterProduto(string referencia)
        {
            Produto produto = new Produto();
            Database db = ConnectionString;

            var result = db.Query("SELECT * FROM dat_produtos Where ref_produto = '" + referencia + "';");

            while (result.Read())
            {

                produto = new Produto()
                {
                    Ref_Produto = result["ref_produto"],
                    Designacao_Produto = result["designacao_produto"],
                    Stock_Fisico = result["stock_fisico"],
                    Stock_PHC = result["stock_phc"],
                    Pos_Stock = result["pos_stock"],
                    Obs_Produto = result["obs"]
                };
            }
            db.Connection.Close();
            return produto;
        }
        public void CriarArtigos(List<Produto> LstProdutos)
        {
            string sql = "INSERT INTO dat_produtos (ref_produto, designacao_produto, stock_phc, stock_fisico, pos_stock, obs) VALUES ";

            foreach (var item in LstProdutos)
            {
                sql += ("('"+item.Ref_Produto+"', '"+item.Designacao_Produto+"', '"+item.Stock_PHC.ToString().Replace(",", ".") +"', '"+item.Stock_Fisico.ToString().Replace(",", ".") + "', '" +item.Pos_Stock+"', '"+item.Obs_Produto+"'), \r\n");
            }
            sql = sql.Remove(sql.Count() - 4); 
            sql += " ON DUPLICATE KEY UPDATE designacao_produto = VALUES(designacao_produto), stock_phc = VALUES(stock_phc);";

            Database db = ConnectionString;

            db.Execute(sql);
            db.Connection.Close();
        }
        public void EditarArtigo (Produto produto)
        {

            Database db = ConnectionString;
            String sql = "update dat_produtos set designacao_produto='"+ produto.Designacao_Produto + "', stock_fisico="+produto.Stock_Fisico+ ", stock_phc="+produto.Stock_PHC+ ", pos_stock='"+produto.Pos_Stock+ "', obs='"+produto.Obs_Produto+"' Where ref_produto='"+produto.Ref_Produto+"';";
            db.Execute(sql);
            db.Connection.Close();
        }
        public void ApagarArtigo(Produto produto)
        {
            Database db = ConnectionString;
            String sql = "delete from dat_produtos Where ref_produto='" + produto.Ref_Produto + "';";
            db.Execute(sql);
            db.Connection.Close();
        }

        public List<FolhaObra> ObterListaFolhasObra(string tecnico, string data)
        {
                List<FolhaObra> LstFolhasObra = new List<FolhaObra>();

            using (Database db = ConnectionString)
            {

                using (var result = db.Query("SELECT * FROM dat_folhas_obra Where dataservico like '%" + data + "%';"))
                {
                    while (result.Read())
                    {
                        LstFolhasObra.Add(new FolhaObra()
                        {
                            IdFolhaObra = result["IdFolhaObra"],
                            DataServico = result["DataServico"],
                            ReferenciaServico = result["ReferenciaServico"],
                            EstadoEquipamento = result["EstadoEquipamento"],
                            RelatorioServico = result["RelatorioServico"],
                            SituacoesPendentes = result["SituacoesPendentes"],
                            ConferidoPor = result["ConferidoPor"],
                            GuiaTransporteAtual = result["GuiaTransporteAtual"],
                            AssistenciaRemota = result["Remoto"] == 1 ? true : false,
                            //IdCartao = result.Reader.IsDBNull(result["IdCartaoTrello"]) ? "" : result["IdCartaoTrello"],
                            IdCartao = result["IdCartaoTrello"],
                            EquipamentoServico = ObterEquipamento(result["IdEquipamento"]),
                            ClienteServico = ObterCliente(result["IdCliente"]),
                            PecasServico = ObterListaProdutoIntervencao(result["IdFolhaObra"]),
                            IntervencaosServico = ObterListaIntervencoes(result["IdFolhaObra"])

                        });
                    }
                }
            }
            
            return LstFolhasObra;

            }
        public List<FolhaObra> ObterListaFolhasObraCartao(string IdCartao)
        {
            List<FolhaObra> LstFolhasObra = new List<FolhaObra>();

            using (Database db = ConnectionString)
            {

                using (var result = db.Query("SELECT * FROM dat_folhas_obra Where IdCartaoTrello='"+IdCartao+"';"))
                {
                    while (result.Read())
                    {
                        LstFolhasObra.Add(new FolhaObra()
                        {
                            IdFolhaObra = result["IdFolhaObra"],
                            DataServico = result["DataServico"],
                            ReferenciaServico = result["ReferenciaServico"],
                            EstadoEquipamento = result["EstadoEquipamento"],
                            RelatorioServico = result["RelatorioServico"],
                            SituacoesPendentes = result["SituacoesPendentes"],
                            ConferidoPor = result["ConferidoPor"],
                            GuiaTransporteAtual = result["GuiaTransporteAtual"],
                            AssistenciaRemota = result["Remoto"] == 1 ? true : false,
                            IdCartao = result["IdCartaoTrello"],
                            EquipamentoServico = ObterEquipamento(result["IdEquipamento"]),
                            ClienteServico = ObterCliente(result["IdCliente"]),
                            PecasServico = ObterListaProdutoIntervencao(result["IdFolhaObra"]),
                            IntervencaosServico = ObterListaIntervencoes(result["IdFolhaObra"])

                        });
                    }
                }
            }

            return LstFolhasObra;

        }
        public List<Intervencao> ObterListaIntervencoes(int idfolhaobra)
        {
            List<Intervencao> LstIntervencoes = new List<Intervencao>();

            using (Database db = ConnectionString)
            {

                using (var result = db.Query("SELECT * FROM dat_intervencoes_folha_obra where IdFolhaObra=" + idfolhaobra + ";"))
                {
                    while (result.Read())
                    {
                        LstIntervencoes.Add(new Intervencao()
                        {
                            IdFolhaObra = result["IdFolhaObra"],
                            IdIntervencao = result["IdIntervencao"],
                            IdTecnico = result["IdTecnico"],
                            NomeTecnico = result["NomeTecnico"],
                            DataServiço = DateTime.Parse(result["DataServico"]),
                            HoraInicio = DateTime.Parse(result["HoraInicio"]),
                            HoraFim = DateTime.Parse(result["HoraFim"])
                        });
                    }
                }
            }
            return LstIntervencoes;
        }
        public List<Produto> ObterListaProdutoIntervencao(int idfolhaobra)
        {
            List<Produto> LstProdutosIntervencao = new List<Produto>();

            using (Database db = ConnectionString)
            {
                using (var result = db.Query("SELECT * FROM dat_produto_intervencao where IdFolhaObra=" + idfolhaobra + ";"))
                {
                    while (result.Read())
                    {
                        LstProdutosIntervencao.Add(new Produto()
                        {
                            Ref_Produto = result["RefProduto"],
                            Designacao_Produto = result["Designacao"],
                            Stock_Fisico = result["Quantidade"]
                        });
                    }
                }
            }
            return LstProdutosIntervencao;
        }
        public FolhaObra ObterFolhaObra (int id)
        {
            using (Database db = ConnectionString)
            {
                using (var result = db.Query("SELECT * FROM dat_folhas_obra where IdFolhaObra=" + id + ";"))
                {

                    result.Read();

                    FolhaObra folhaObra = new FolhaObra()
                    {
                        IdFolhaObra = result["IdFolhaObra"],
                        DataServico = result["DataServico"],
                        ReferenciaServico = result["ReferenciaServico"],
                        EstadoEquipamento = result["EstadoEquipamento"],
                        RelatorioServico = result["RelatorioServico"],
                        SituacoesPendentes = result["SituacoesPendentes"],
                        ConferidoPor = result["ConferidoPor"],
                        GuiaTransporteAtual = result["GuiaTransporteAtual"],
                        AssistenciaRemota = result["Remoto"] == 1 ? true : false,
                        IdCartao = result["IdCartaoTrello"],
                        EquipamentoServico = ObterEquipamento(result["IdEquipamento"]),
                        ClienteServico = ObterCliente(result["IdCliente"]),
                        PecasServico = ObterListaProdutoIntervencao(id),
                        IntervencaosServico = ObterListaIntervencoes(id),
                        RubricaCliente = result["RubricaCliente"]
                    };
                return folhaObra;

                }
            }
        }
        public Equipamento ObterEquipamento(int id)
        {
            using (Database db = ConnectionString)
            {

                using (var result = db.Query("SELECT * FROM dat_equipamentos where IdEquipamento=" + id + ";"))
                {

                    result.Read();
                    if (result.Reader.HasRows)
                    {
                        Equipamento equipamento = new Equipamento()
                        {
                            IdEquipamento = result["IdEquipamento"],
                            DesignacaoEquipamento = result["DesignacaoEquipamento"],
                            MarcaEquipamento = result["MarcaEquipamento"],
                            ModeloEquipamento = result["ModeloEquipamento"],
                            NumeroSerieEquipamento = result["NumeroSerieEquipamento"]

                        };

                        return equipamento;
                    }

                }
            }
            return new Equipamento();
        }
        public Equipamento ObterEquipamentoNS(string NumeroSerie)
        {
            using (Database db = ConnectionString)
            {
                using (var result = db.Query("SELECT * FROM dat_equipamentos where NumeroSerieEquipamento='" + NumeroSerie + "';"))
                {
                    Equipamento equipamento = new Equipamento();

                    result.Read();

                    if (result.Reader.HasRows)
                    {
                        equipamento = new Equipamento()
                        {
                            IdEquipamento = result["IdEquipamento"],
                            DesignacaoEquipamento = result["DesignacaoEquipamento"],
                            MarcaEquipamento = result["MarcaEquipamento"],
                            ModeloEquipamento = result["ModeloEquipamento"],
                            NumeroSerieEquipamento = result["NumeroSerieEquipamento"]

                        };
                    }
                    else
                    {
                        equipamento = new Equipamento()
                        {
                            NumeroSerieEquipamento = NumeroSerie
                        };
                    }
                    return equipamento;
                }
             }
        }
        public List<FolhaObra> ObterHistorico (string NumeroSerie)
        {
        List<FolhaObra> LstFolhasObra = new List<FolhaObra>();

                using (Database db = ConnectionString)
                {

                    using (var result = db.Query("SELECT * FROM dat_folhas_obra inner join dat_equipamentos on dat_equipamentos.numeroserieequipamento='"+NumeroSerie+"' AND dat_folhas_obra.idequipamento=dat_equipamentos.idequipamento;"))
                    {
                        while (result.Read())
                        {
                            LstFolhasObra.Add(new FolhaObra()
                            {
                                IdFolhaObra = result["IdFolhaObra"],
                                DataServico = result["DataServico"],
                                ReferenciaServico = result["ReferenciaServico"],
                                EstadoEquipamento = result["EstadoEquipamento"],
                                RelatorioServico = result["RelatorioServico"],
                                SituacoesPendentes = result["SituacoesPendentes"],
                                ConferidoPor = result["ConferidoPor"],
                                //IdCartao = result.Reader.IsDBNull(result["IdCartaoTrello"]) ? "" : result["IdCartaoTrello"],
                                IdCartao = result["IdCartaoTrello"],
                                EquipamentoServico = ObterEquipamento(result["IdEquipamento"]),
                                ClienteServico = ObterCliente(result["IdCliente"]),
                                PecasServico = ObterListaProdutoIntervencao(result["IdFolhaObra"]),
                                IntervencaosServico = ObterListaIntervencoes(result["IdFolhaObra"])

                            });
                        }
                    }
                }

                return LstFolhasObra;
        }
        public List<Movimentos> ObterListaMovimentos(string NomeTecnico, string Guia)
        {
            List<Movimentos> LstGuias = new List<Movimentos>();

            using (Database db = ConnectionString)
            {

                using (var result = db.Query("SELECT dat_folhas_obra.IdFolhaObra, NomeTecnico, NomeCliente, dat_folhas_obra.DataServico, GuiaTransporteAtual, RefProduto, Designacao, Quantidade FROM dat_folhas_obra inner join dat_clientes, dat_produto_intervencao, dat_intervencoes_folha_obra where dat_produto_intervencao.idfolhaobra = dat_folhas_obra.idfolhaobra AND dat_intervencoes_folha_obra.idfolhaobra = dat_folhas_obra.idfolhaobra AND GuiaTransporteAtual != '' AND NomeTecnico like '"+NomeTecnico+"' AND GuiaTransporteAtual like '"+Guia+ "' AND dat_clientes.idcliente = dat_folhas_obra.idcliente GROUP BY dat_produto_intervencao.RefProduto, dat_folhas_obra.idfolhaobra;"))
                {
                    while (result.Read())
                    {
                        LstGuias.Add(new Movimentos()
                        {
                           IdFolhaObra = result["IdFolhaObra"],
                            NomeTecnico = result["NomeTecnico"],
                            GuiaTransporte = result["GuiaTransporteAtual"],
                            RefProduto = result["RefProduto"],
                            Designacao = result["Designacao"],
                            Quantidade = result["Quantidade"],
                            NomeCliente = result["NomeCliente"],
                            DataMovimento = result["DataServico"]
                        });
                    }
                }
            }

            return LstGuias;
        }
        public List<Movimentos> ObterListaMovimentosProduto(string Ref_Produto)
        {
            List<Movimentos> LstGuias = new List<Movimentos>();

            using (Database db = ConnectionString)
            {

                using (var result = db.Query("SELECT dat_folhas_obra.IdFolhaObra, NomeTecnico, NomeCliente, dat_folhas_obra.DataServico, GuiaTransporteAtual, RefProduto, Designacao, Quantidade FROM dat_folhas_obra inner join dat_clientes, dat_produto_intervencao, dat_intervencoes_folha_obra where dat_produto_intervencao.idfolhaobra = dat_folhas_obra.idfolhaobra AND dat_intervencoes_folha_obra.idfolhaobra = dat_folhas_obra.idfolhaobra AND RefProduto = '"+Ref_Produto+"' AND dat_clientes.idcliente = dat_folhas_obra.idcliente GROUP BY dat_produto_intervencao.RefProduto, dat_folhas_obra.idfolhaobra;"))
                {
                    while (result.Read())
                    {
                        LstGuias.Add(new Movimentos()
                        {
                            IdFolhaObra = result["IdFolhaObra"],
                            NomeTecnico = result["NomeTecnico"],
                            GuiaTransporte = result["GuiaTransporteAtual"],
                            RefProduto = result["RefProduto"],
                            Designacao = result["Designacao"],
                            Quantidade = result["Quantidade"],
                            NomeCliente = result["NomeCliente"],
                            DataMovimento = result["DataServico"]
                        });
                    }
                }
            }

            return LstGuias;
        }
        public List<Movimentos> ObterListaMovimentos(string NomeTecnico)
        {
            List<Movimentos> LstGuias = new List<Movimentos>();

            using (Database db = ConnectionString)
            {

                using (var result = db.Query("SELECT GuiaTransporteAtual FROM dat_folhas_obra inner join dat_produto_intervencao, dat_intervencoes_folha_obra where dat_produto_intervencao.idfolhaobra = dat_folhas_obra.idfolhaobra AND dat_intervencoes_folha_obra.idfolhaobra = dat_folhas_obra.idfolhaobra AND GuiaTransporteAtual != '' AND GuiaTransporteAtual != 'GT"+DateTime.Now.Year+"BO91/' AND NomeTecnico like '" + NomeTecnico + "' GROUP BY dat_folhas_obra.guiatransporteatual;"))
                {
                    while (result.Read())
                    {
                        LstGuias.Add(new Movimentos()
                        {
                            GuiaTransporte = result["GuiaTransporteAtual"]
                        });
                    }
                }
            }

            return LstGuias;
        }

        public Cliente ObterCliente(int id)
        {

            Cliente cliente = new Cliente();
            using (Database db = ConnectionString)
            {
                using (var result = db.Query("SELECT * FROM dat_clientes where IdCliente=" + id + ";"))
                {

                    result.Read();
                    if (result.Reader.HasRows)
                    {
                        cliente = new Cliente()
                        {
                            IdCliente = result["IdCliente"],
                            NomeCliente = result["NomeCliente"],
                            PessoaContatoCliente = result["PessoaContactoCliente"],
                            MoradaCliente = result["MoradaCliente"],
                            EmailCliente = result["EmailCliente"],
                            NumeroContribuinteCliente = result["NumeroContribuinteCliente"]

                        };
                    }
                    return cliente;

                }
            }
        }
        public Cliente ObterClienteNome(string nome)
        {
            using (Database db = ConnectionString)
            {
                Cliente cliente = new Cliente();
                using (var result = db.Query("SELECT * FROM dat_clientes where NomeCliente = '" + nome + "';"))
                {

                    result.Read();
                    if (result.Reader.HasRows)
                    {
                         cliente = new Cliente()
                        {
                            IdCliente = result["IdCliente"],
                            NomeCliente = result["NomeCliente"],
                            PessoaContatoCliente = result["PessoaContactoCliente"],
                            MoradaCliente = result["MoradaCliente"],
                            EmailCliente = result["EmailCliente"],
                            NumeroContribuinteCliente = result["NumeroContribuinteCliente"]

                        };
                    }
                    else
                    {
                        cliente = new Cliente()
                        {
                            NomeCliente = nome
                        };
                    }
                    return cliente;
                }
            }
        }
        public List<Cliente> ObterListaClientes(string NomeCliente, bool exact)
        {

            List<Cliente> LstClientes = new List<Cliente>();
            string sqlQuery = "SELECT * FROM dat_clientes where NomeCliente like '%" + NomeCliente + "%';";
            if (exact) sqlQuery = "SELECT * FROM dat_clientes where NomeCliente ='" + NomeCliente + "';"; 

            using (Database db = ConnectionString)
            {
                using (var result = db.Query(sqlQuery))
                {
                    while (result.Read())
                    {
                        LstClientes.Add(new Cliente()
                        {
                            IdCliente = result["IdCliente"],
                            NomeCliente = result["NomeCliente"],
                            PessoaContatoCliente = result["PessoaContactoCliente"],
                            MoradaCliente = result["MoradaCliente"],
                            EmailCliente = result["EmailCliente"],
                            NumeroContribuinteCliente = result["NumeroContribuinteCliente"]

                        });
                    }
                }
                if (LstClientes.Count == 0)
                {
                    LstClientes.Add(new Cliente()
                    {
                        NomeCliente = NomeCliente
                    });
                }
                return LstClientes;
            }
        }
        public int NovaFolhaObra(FolhaObra folhaObra)
        {
            folhaObra.IdFolhaObra = folhaObra.IdFolhaObra == 0 ? ObterUltimaEntrada("dat_folhas_obra", "IdFolhaObra") + 1 : folhaObra.IdFolhaObra;

            string sql = "INSERT INTO dat_folhas_obra (IdFolhaObra, DataServico, ReferenciaServico, EstadoEquipamento, RelatorioServico, ConferidoPor, SituacoesPendentes, IdCartaoTrello, IdEquipamento, IdCliente, GuiaTransporteAtual, Remoto, RubricaCliente) VALUES ";

            sql += ("('" + folhaObra.IdFolhaObra + "', '" + folhaObra.DataServico.ToString("yy-MM-dd") + "', '" + folhaObra.ReferenciaServico + "', '" + folhaObra.EstadoEquipamento + "', '" + folhaObra.RelatorioServico + "', '"+folhaObra.ConferidoPor+"', '" + folhaObra.SituacoesPendentes + "', '"+folhaObra.IdCartao+"', '" + NovoEquipamento(folhaObra.EquipamentoServico) + "', '" + NovoCliente(folhaObra.ClienteServico) + "', '"+folhaObra.GuiaTransporteAtual+"', '"+ (folhaObra.AssistenciaRemota ? 1 : 0) +"', '"+folhaObra.RubricaCliente+"') \r\n");
            
            sql += " ON DUPLICATE KEY UPDATE ReferenciaServico = VALUES(ReferenciaServico), EstadoEquipamento = VALUES(EstadoEquipamento), RelatorioServico = VALUES(RelatorioServico), ConferidoPor = VALUES(ConferidoPor), SituacoesPendentes = VALUES(SituacoesPendentes), IdEquipamento = VALUES(IdEquipamento), IdCliente = VALUES(IdCliente), GuiaTransporteAtual = VALUES(GuiaTransporteAtual), Remoto = VALUES(Remoto), RubricaCliente = VALUES(RubricaCliente);";

            using (Database db = ConnectionString)
            {
                db.Execute(sql);
            }
            return folhaObra.IdFolhaObra;
        }
        public int NovoCliente(Cliente cliente)
        {
            Cliente c = new Cliente();

            if (cliente.NomeCliente != null || cliente.NomeCliente == String.Empty) {  c = ObterClienteNome(cliente.NomeCliente); }
            if (c.IdCliente == 0) {
                cliente.IdCliente = ObterUltimaEntrada("dat_clientes", "IdCliente") + 1;
            }
            else
            {
                cliente.IdCliente = c.IdCliente;
            }

            string sql = "INSERT INTO dat_clientes (IdCliente, NomeCliente, PessoaContactoCliente, MoradaCliente, EmailCliente, NumeroContribuinteCliente) VALUES ";

            sql += ("('" + cliente.IdCliente + "', '" + cliente.NomeCliente + "', '" + cliente.PessoaContatoCliente + "', '" + cliente.MoradaCliente + "', '" + cliente.EmailCliente + "', '" + cliente.NumeroContribuinteCliente + "') \r\n");

            sql += " ON DUPLICATE KEY UPDATE PessoaContactoCliente = VALUES(PessoaContactoCliente), MoradaCliente = VALUES(MoradaCliente), EmailCliente = VALUES(EmailCliente), NumeroContribuinteCliente = VALUES(NumeroContribuinteCliente);";

            using (Database db = ConnectionString)
            {
                db.Execute(sql);
            }

            return cliente.IdCliente;

        }
        public int NovoEquipamento(Equipamento equipamento)
        {
 
            Equipamento e = ObterEquipamentoNS(equipamento.NumeroSerieEquipamento);
            if (e.IdEquipamento == 0)
            {
                equipamento.IdEquipamento = ObterUltimaEntrada("dat_equipamentos", "IdEquipamento") + 1;
            }
            else
            {
                equipamento.IdEquipamento = e.IdEquipamento;
            }

            string sql = "INSERT INTO dat_equipamentos (IdEquipamento, DesignacaoEquipamento, MarcaEquipamento, ModeloEquipamento, NumeroSerieEquipamento) VALUES ";

            sql += ("('" + equipamento.IdEquipamento + "',  '" + equipamento.DesignacaoEquipamento + "', '" + equipamento.MarcaEquipamento + "', '" + equipamento.ModeloEquipamento + "', '" + equipamento.NumeroSerieEquipamento + "') \r\n");

            sql += " ON DUPLICATE KEY UPDATE DesignacaoEquipamento = VALUES(DesignacaoEquipamento), MarcaEquipamento = VALUES(MarcaEquipamento), ModeloEquipamento = VALUES(ModeloEquipamento);";

            using (Database db = ConnectionString)
            {
                db.Execute(sql);
            }

            return equipamento.IdEquipamento;

        }
        public int NovaIntervencao(Intervencao intervencao)
        {
            intervencao.IdIntervencao = intervencao.IdIntervencao == 0 ? ObterUltimaEntrada("dat_intervencoes_folha_obra", "IdIntervencao") + 1 : intervencao.IdIntervencao;

            string sql = "INSERT INTO dat_intervencoes_folha_obra (IdIntervencao, IdFolhaObra,IdTecnico, NomeTecnico, DataServico, HoraInicio, HoraFim) VALUES ";

            sql += ("('" + intervencao.IdIntervencao + "',  '" + intervencao.IdFolhaObra + "', '0', '" + intervencao.NomeTecnico + "', '" + intervencao.DataServiço.ToString("yy-MM-dd") + "', '" + intervencao.HoraInicio.ToString("HH:mm") + "', '" + intervencao.HoraFim.ToString("HH:mm") + "') \r\n");

            sql += " ON DUPLICATE KEY UPDATE IdTecnico = VALUES(IdTecnico), NomeTecnico = VALUES(NomeTecnico), DataServico = VALUES(DataServico), HoraInicio = VALUES(HoraInicio), HoraFim = VALUES(HoraFim);";

            using (Database db = ConnectionString)
            {
                db.Execute(sql);
            }

            return intervencao.IdIntervencao;


        }
        public void NovaPecaIntervencao (Produto produto, string IdFolhaObra)
        {
            string sql = "INSERT INTO dat_produto_intervencao (RefProduto, Designacao,Quantidade, IdFolhaObra) VALUES ";

            sql += ("('" + produto.Ref_Produto + "',  '" + produto.Designacao_Produto + "', '"+produto.Stock_Fisico+"', '" + IdFolhaObra + "') \r\n");

            sql += " ON DUPLICATE KEY UPDATE Designacao = VALUES(Designacao), Quantidade = VALUES(Quantidade);";

            using (Database db = ConnectionString)
            {
                db.Execute(sql);
            }

        }
        public void ApagarFolhaObra(int id)
        {
            string sql = "DELETE FROM dat_folhas_obra where IdFolhaObra=" + id + ";";

            using (Database db = ConnectionString)
            {
                db.Execute(sql);
            }
        }
        public void ApagarIntervencao (int id)
        {
            string sql = "DELETE FROM dat_intervencoes_folha_obra where IdIntervencao="+id+";";

            using (Database db = ConnectionString)
            {
                db.Execute(sql);
            }
        }
        public void ApagarPecaFolhaObra(string Ref_Produto, int idFolhaObra)
        {
            string sql = "DELETE FROM dat_produto_intervencao where RefProduto='" + Ref_Produto + "' AND IdFolhaObra = "+idFolhaObra+";";

            using (Database db = ConnectionString)
            {
                db.Execute(sql);
            }
        }
        public int ObterUltimaEntrada (string NomeTabela, string CampoID)
        {
            using (Database db = ConnectionString)
            {
                using (var result = db.Query("SELECT Max(" + CampoID + ") FROM " + NomeTabela + ";"))
                {
                        while (result.Read())
                        {
                            return result.Reader.IsDBNull(0) ? 0 : result[0];
                        };

                }
            }
            return 0;

        }

        public Bitmap DesenharEtiqueta80x50 (Produto produto)
        {

            int x = 30;
            int y = 0;
            int width = 300;
            int height = 188;

            Bitmap bm = new Bitmap(width, height);

            Font fontHeader = new Font("Tahoma", 18, FontStyle.Bold);
            Font fontBody = new Font("Tahoma", 8, FontStyle.Regular);

            StringFormat format = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };

            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.White);

                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (File.Exists(FT_Logo_Print)) { Image img = System.Drawing.Image.FromFile(FT_Logo_Print, true); gr.DrawImage(img, x, y, 85, 50); }

                y += 10;
                gr.DrawString("Food-Tech", fontHeader, Brushes.Black, x + 85 + 10, y);
          
                x = 10;
                y += 40;
                gr.DrawString(produto.Designacao_Produto, fontBody, Brushes.Black, new Rectangle(x, y, width - (x*2), 35), format);


                y += 40;
                Barcode.Code93 code = new Barcode.Code93
                {
                    DrawText = false
                };

                gr.DrawImage(code.desenharBarcode(produto.Ref_Produto), 10, y, width - (x * 2), 90);
                y += 60;
                gr.DrawString(produto.Ref_Produto, fontHeader, new SolidBrush(Color.Black), new RectangleF(x, y, width - (x * 2), 20), format);

                gr.DrawString("geral@food-tech.pt", fontBody, Brushes.Black, new Rectangle(x, height-20, width - (x * 2), 20), format);

            }

            return bm;
        }
        public Bitmap DesenharEtiqueta80x50QR(Produto produto)
        {

            int x = 30;
            int y = 0;
            int width = 1024;
            int height = 641;

            Bitmap bm = new Bitmap(width, height);

            Font fontHeader = new Font("Tahoma", 70, FontStyle.Bold);
            Font fontBody = new Font("Tahoma", 40, FontStyle.Regular);
            Font fontFooter = new Font("Tahoma", 22, FontStyle.Regular);

            StringFormat format = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };

            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.White);

                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (File.Exists(FT_Logo_Print)) { Image img = System.Drawing.Image.FromFile(FT_Logo_Print, true); gr.DrawImage(img, x, y, 400, 235); }

                y += 65;
                gr.DrawString("Food-Tech", fontHeader, Brushes.Black, x + 400, y);

                x = 10;
                y += 165;
               
                gr.DrawString(produto.Designacao_Produto, fontBody, Brushes.Black, new Rectangle(x, y, width - (x * 2), 200), format);

                y += 250;
                gr.DrawString(produto.Ref_Produto, fontHeader, new SolidBrush(Color.Black), new RectangleF(x, y, width - (x * 2) - 200, 80), format);

                y += 95;

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(produto.Ref_Produto, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                gr.DrawImage(qrCodeImage, width - 220, height - 220, 200, 200);

                gr.DrawString("geral@food-tech.pt", fontFooter, Brushes.Black, new Rectangle(x, y, width - (x * 2) - 200, 30), format);

            }

            return bm;
        }
        public Bitmap DesenharEtiqueta80x25QR(Produto produto)
        {

            int x = 10;
            int y = 0;
            int width = 1024;
            int height = 320;

            Bitmap bm = new Bitmap(width, 641);
           
            Font fontHeader = new Font("Tahoma", 40, FontStyle.Bold);
            Font fontBody = new Font("Tahoma", 34, FontStyle.Bold);
            Font fontFooter = new Font("Tahoma", 16, FontStyle.Regular);

            StringFormat format = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };

            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.White);

                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (File.Exists(FT_Logo_Print)) { Image img = System.Drawing.Image.FromFile(FT_Logo_Print, true); gr.DrawImage(img, x + 10, height - 130, 200, 120); }


                y += 10;

                gr.DrawString(produto.Designacao_Produto, fontBody, Brushes.Black, new Rectangle(x, y, width - (x * 2) - 150, 160), format);

                y += 200;
                gr.DrawString(produto.Ref_Produto, fontHeader, new SolidBrush(Color.Black), new RectangleF(x + 220, y, width - (x * 2) - 520, 80), format);

                y += 70;

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(produto.Ref_Produto, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                gr.DrawImage(qrCodeImage, width - 310, height - 150, 150, 150);

                gr.DrawString("geral@food-tech.pt", fontFooter, Brushes.Black, new Rectangle(x, y, width - (x * 2) - 100, 30), format);

            }

            return bm;
        }

        public MemoryStream PreencherFormularioFolhaObra(FolhaObra folhaobra)
        {
            string pdfTemplate = AppDomain.CurrentDomain.BaseDirectory + "FT_FolhaObra.pdf";
            var outputPdfStream = new MemoryStream();
            PdfReader pdfReader = new PdfReader(pdfTemplate);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, outputPdfStream) { FormFlattening = true, FreeTextFlattening = true };
            AcroFields pdfFormFields = pdfStamper.AcroFields;

            if (folhaobra.RubricaCliente != null)
            {
                var fldPosition = pdfFormFields.GetFieldPositions("assinatura");
                Rectangle rectangle = new Rectangle((int)fldPosition[1], (int)fldPosition[2], (int)fldPosition[3], 20);
                folhaobra.RubricaCliente = folhaobra.RubricaCliente.Replace("data:image/png;base64,", "").Trim();

                if ((folhaobra.RubricaCliente.Length % 4 == 0) && Regex.IsMatch(folhaobra.RubricaCliente, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                {
                    byte[] imageBytes = Convert.FromBase64String(folhaobra.RubricaCliente);

                    iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageBytes);

                        image.ScaleToFit(rectangle.Width, rectangle.Height);
                        image.SetAbsolutePosition(rectangle.Left + 2, rectangle.Top - 2);

                        pdfStamper.GetOverContent((int)fldPosition[0]).AddImage(image);

                }
            }

            pdfFormFields.SetField("IdFolhaObra", "FO" + folhaobra.IdFolhaObra.ToString());
            if (folhaobra.AssistenciaRemota)
            {
                pdfFormFields.SetFieldProperty("Remoto", "textsize", 26f, null);
                pdfFormFields.SetFieldProperty("Remoto", "textcolor", iTextSharp.text.BaseColor.Red, null);
                pdfFormFields.SetField("Remoto", "REMOTO");
            }else if((folhaobra.DataServico.DayOfWeek == DayOfWeek.Sunday) || (folhaobra.DataServico.DayOfWeek == DayOfWeek.Saturday))
            {
                pdfFormFields.SetFieldProperty("Remoto", "textsize", 20f, null);
                pdfFormFields.SetFieldProperty("Remoto", "textcolor", iTextSharp.text.BaseColor.Red, null);
                pdfFormFields.SetField("Remoto", "FIM-DE-SEMANA");
            }

            if (folhaobra.PecasServico.Count() > 0 && folhaobra.GuiaTransporteAtual != "GT" + folhaobra.DataServico.Year + "BO91/" && folhaobra.GuiaTransporteAtual != string.Empty) pdfFormFields.SetField("GT", "Peça(s) retirada(s) da " + folhaobra.GuiaTransporteAtual);

            //Equipamento
            pdfFormFields.SetField("Designação", folhaobra.EquipamentoServico.DesignacaoEquipamento);
            pdfFormFields.SetField("Marca", folhaobra.EquipamentoServico.MarcaEquipamento);
            pdfFormFields.SetField("Mod", folhaobra.EquipamentoServico.ModeloEquipamento);
            pdfFormFields.SetField("NºSerie", folhaobra.EquipamentoServico.NumeroSerieEquipamento);
            pdfFormFields.SetField(folhaobra.EstadoEquipamento, "On");
            pdfFormFields.SetField("RGPD", "Yes");

            //Cliente
            pdfFormFields.SetField("Referencia", folhaobra.ReferenciaServico);
            pdfFormFields.SetField("Cliente", folhaobra.ClienteServico.NomeCliente);
            pdfFormFields.SetField("Contribuinte", folhaobra.ClienteServico.NumeroContribuinteCliente);

            //Assistencia
            pdfFormFields.SetField("trabalho efectuado", folhaobra.RelatorioServico);
            pdfFormFields.SetField("SituacoesPendentes", folhaobra.SituacoesPendentes);
            if (folhaobra.IntervencaosServico.Count > 0)
            {
                //Mao de Obra
                int i = 1;
                foreach (Intervencao intervencao in folhaobra.IntervencaosServico)
                {
                    pdfFormFields.SetField("DataRow" + i, intervencao.DataServiço.ToString("dd/MM"));
                    pdfFormFields.SetField("TécnicoRow" + i, intervencao.NomeTecnico);
                    pdfFormFields.SetField("InícioRow" + i, intervencao.HoraInicio.ToString("HH:mm"));
                    pdfFormFields.SetField("FimRow" + i, intervencao.HoraFim.ToString("HH:mm"));

                    var ts = new TimeSpan(intervencao.HoraFim.Hour - intervencao.HoraInicio.Hour, intervencao.HoraFim.Minute - intervencao.HoraInicio.Minute, 00);
                    ts = TimeSpan.FromMinutes(15 * Math.Ceiling(ts.TotalMinutes / 15));

                    pdfFormFields.SetField("HorasRow" + i, ts.Hours.ToString() + "." + (ts.Minutes * 100) / 60);
                    i++;
                    if (i == 9) break;
                }

                pdfFormFields.SetField("DataConclusao", folhaobra.IntervencaosServico.Last().DataServiço.ToString("dd/MM/yyyy"));
                pdfFormFields.SetField("o tecnico", folhaobra.IntervencaosServico.Last().NomeTecnico);
            }

            pdfFormFields.SetFieldProperty("Text5", "textsize", 9f, null);
            pdfFormFields.SetField("Text5", folhaobra.DataServico.ToString("dd/MM/yyyy"));

            if (folhaobra.PecasServico.Count > 0 )
            {

                //Peças
                int p = 1;
                foreach (Produto pecas in folhaobra.PecasServico)
                {
                    pdfFormFields.SetFieldProperty("ReferênciaRow" + p, "textsize", 6f, null);
                    pdfFormFields.SetField("ReferênciaRow" + p, pecas.Ref_Produto);
                    pdfFormFields.SetFieldProperty("DesignaçãoRow" + p, "textsize", 6f, null);
                    if (pecas.Designacao_Produto.Length > 50)
                    {
                        pdfFormFields.SetField("DesignaçãoRow" + p, pecas.Designacao_Produto.Substring(0,54));
                    }
                    else
                    {
                        pdfFormFields.SetField("DesignaçãoRow" + p, pecas.Designacao_Produto);
                    }
                    pdfFormFields.SetField("QuantRow" + p, pecas.Stock_Fisico.ToString());
                    p++;
                    if (p == 10) break;
                }
            }
            if (folhaobra.ConferidoPor == string.Empty) folhaobra.ConferidoPor = folhaobra.ClienteServico.PessoaContatoCliente;
            pdfFormFields.SetField("o cliente", folhaobra.ConferidoPor);

            pdfStamper.FormFlattening = true;
            pdfStamper.SetFullCompression();
            pdfStamper.Close();

            return outputPdfStream;
            
        }
    }
}
