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
using System.Security.Cryptography.Xml;
using System.Configuration;
using OfficeOpenXml.Style;

namespace FT_Management.Models
{
    public class FT_ManagementContext
    {

        public string ConnectionString { get; set; }
        private string FT_Logo_Print { get; set; }

        public FT_ManagementContext(string connectionString, string FT_Logo)
        {
            this.ConnectionString = connectionString;
            this.FT_Logo_Print = FT_Logo;

            try
            {
                Database db = ConnectionString;
                Console.WriteLine("Connectado á Base de Dados MySQL com sucesso!");
            }
            catch
            {
                Console.WriteLine("Não foi possivel conectar á BD MySQL!");
            }
        }

        public void AdicionarLog(string user, string msg, int tipo)
        {
            string sql = "INSERT INTO dat_logs (user, msg_log, tipo_log) VALUES ('" + user + "', '" + msg + "', " + tipo + ");";

            Database db = ConnectionString;

            db.Execute(sql);
            db.Connection.Close();
        }

        public List<Visita> ObterListaVisitas(int IdComercial, DateTime DataInicial, DateTime DataFinal)
        {
            List<Visita> LstVisitas = new List<Visita>();
            String DataI = DataInicial.ToString("yyyy-MM-dd");
            String DataF = DataFinal.ToString("yyyy-MM-dd");
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_visitas where dat_visitas.idcomercial=" + IdComercial + " AND DataVisita>='" + DataI + "'  AND DataVisita<='" + DataF + "';");
                while (result.Read())
                {
                    LstVisitas.Add(new Visita()
                    {
                        IdVisita = result["IdVisita"],
                        DataVisita = DateTime.Parse(result["DataVisita"]),
                        Cliente = ObterCliente(result["IdCliente"], result["IdLoja"]),
                        ResumoVisita = result["ResumoVisita"],
                        EstadoVisita = result["EstadoVisita"],
                        ObsVisita = result["ObsVisita"],
                        VisitaStamp = result["VisitaStamp"],
                        IdComercial = result["idcomercial"]

                    });
                }
            }

            return LstVisitas;

        }
        public List<Visita> ObterListaVisitas(DateTime DataInicial, DateTime DataFinal)
        {
            List<Visita> LstVisitas = new List<Visita>();
            String DataI = DataInicial.ToString("yyyy-MM-dd");
            String DataF = DataFinal.ToString("yyyy-MM-dd");
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_visitas where DataVisita>='" + DataI + "'  AND DataVisita<='" + DataF + "' order by DataVisita, IdComercial;;");
                while (result.Read())
                {
                    LstVisitas.Add(new Visita()
                    {
                        IdVisita = result["IdVisita"],
                        DataVisita = DateTime.Parse(result["DataVisita"]),
                        Cliente = ObterCliente(result["IdCliente"], result["IdLoja"]),
                        ResumoVisita = result["ResumoVisita"],
                        EstadoVisita = result["EstadoVisita"],
                        ObsVisita = result["ObsVisita"],
                        VisitaStamp = result["VisitaStamp"],
                        IdComercial = result["idcomercial"]

                    });
                }
            }

            return LstVisitas;

        }
        public List<Visita> ObterListaVisitasCliente(int IdCliente, int IdLoja)
        {
            List<Visita> LstVisitas = new List<Visita>();
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_visitas where IdCliente='" + IdCliente + "'  AND IdLoja='" + IdLoja + "' order by DataVisita, IdComercial;;");
                while (result.Read())
                {
                    LstVisitas.Add(new Visita()
                    {
                        IdVisita = result["IdVisita"],
                        DataVisita = DateTime.Parse(result["DataVisita"]),
                        ResumoVisita = result["ResumoVisita"],
                        EstadoVisita = result["EstadoVisita"],
                        ObsVisita = result["ObsVisita"],
                        VisitaStamp = result["VisitaStamp"],
                        IdComercial = result["idcomercial"],
                        Propostas = ObterListaPropostasVisita(result["IdVisita"]) 
                        
                    });
                }
            }

            return LstVisitas;

        }

        public List<Proposta> ObterListaPropostasVisita(int IdVisita)
        {
            List<Proposta> LstPropostas = new List<Proposta>();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_propostas where dat_propostas.idvisita=" + IdVisita + ";");
                while (result.Read())
                {
                    LstPropostas.Add(new Proposta()
                    {
                        IdProposta = result["IdVisita"],
                        Comercial = ObterUtilizador(result["IdComercial"]),
                        //Visita = ObterVisita(result["IdVisita"]),
                        DataProposta = result["DataProposta"],
                        EstadoProposta = result["EstadoProposta"],
                        ValorProposta = result["ValorProposta"],
                        UrlAnexo = result["UrlAnexo"]

                    });
                }
            }
            return LstPropostas;
        }


        public Visita ObterVisita(int IdVisita)
        {
            Visita visita = new Visita();
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_visitas where IdVisita = "+ IdVisita + ";");
                result.Read();

                visita = new Visita()
                {
                    IdVisita = result["IdVisita"],
                    DataVisita = DateTime.Parse(result["DataVisita"]),
                    Cliente = ObterCliente(result["IdCliente"], result["IdLoja"]),
                    ResumoVisita = result["ResumoVisita"],
                    EstadoVisita = result["EstadoVisita"],
                    ObsVisita = result["ObsVisita"],
                    VisitaStamp = result["VisitaStamp"],
                    IdComercial = result["idcomercial"]
                    };
            }

            visita.Propostas = ObterListaPropostasVisita(IdVisita);
            return visita;
        }

        public Produto ObterProduto(string referencia, int armazemid)
        {
            Produto produto = new Produto();
            Database db = ConnectionString;

            var result = db.Query("SELECT * FROM dat_produtos Where Armazem_Id=" + armazemid + " and ref_produto = '" + referencia + "';");

            while (result.Read())
            {

                produto = new Produto()
                {
                    Ref_Produto = result["ref_produto"],
                    Designacao_Produto = result["designacao_produto"],
                    Stock_Fisico = result["stock_fisico"],
                    Stock_PHC = result["stock_phc"],
                    Pos_Stock = result["pos_stock"],
                    Stock_Rec = result["stock_rec"],
                    Stock_Res = result["stock_res"],
                    Armazem_ID = result["armazem_id"],
                    Obs_Produto = result["obs"],
                    ModificadoStock = result["modificado"]
                };
            }
            db.Connection.Close();
            return produto;
        }
        public List<Armazem> ObterListaArmazens()
        {
            List<Armazem> LstArmazens = new List<Armazem>();

            Database db = ConnectionString;

            using (var result = db.Query("SELECT * FROM sys_armazem;"))
            {
                while (result.Read())
                {
                    LstArmazens.Add(new Armazem()
                    {
                        ArmazemId = result["armazem_id"],
                        ArmazemNome = result["armazem_nome"]                       
                    });
                }
            }
            db.Connection.Close();

            return LstArmazens;
        }
        public List<Produto> ObterListaProdutoArmazem(string referencia)
        {
            List<Produto> LstProdutos = new List<Produto>();

            Database db = ConnectionString;

            using (var result = db.Query("SELECT * FROM dat_produtos Where ref_produto='" + referencia + "';"))
            {
                while (result.Read())
                {
                    LstProdutos.Add(new Produto()
                    {
                        Ref_Produto = result["ref_produto"],
                        Designacao_Produto = result["designacao_produto"],
                        Stock_Fisico = result["stock_fisico"],
                        Stock_PHC = result["stock_phc"],
                        Stock_Rec = result["stock_rec"],
                        Stock_Res = result["stock_res"],
                        Armazem_ID = result["armazem_id"],
                        Pos_Stock = result["pos_stock"],
                        Obs_Produto = result["obs"]
                    });
                }
            }
            db.Connection.Close();
            return LstProdutos;
        }
        public List<Produto> ObterListaProdutos(string referencia, string desig, int ArmazemId)
        {
            List<Produto> LstProdutos = new List<Produto>();

            Database db = ConnectionString;

            using (var result = db.Query("SELECT * FROM dat_produtos Where Armazem_Id=" + ArmazemId + " and ref_produto like '%" + referencia + "%' AND designacao_produto like '%" + desig + "%';"))
            {
                while (result.Read())
                {
                    LstProdutos.Add(new Produto()
                    {
                        Ref_Produto = result["ref_produto"],
                        Designacao_Produto = result["designacao_produto"],
                        Stock_Fisico = result["stock_fisico"],
                        Stock_PHC = result["stock_phc"],
                        Stock_Rec = result["stock_rec"],
                        Stock_Res = result["stock_res"],
                        Armazem_ID = result["armazem_id"],
                        Pos_Stock = result["pos_stock"],
                        Obs_Produto = result["obs"]
                    });
                }
            }
            db.Connection.Close();
            return LstProdutos;
        }
        public List<FolhaObra> ObterListaFolhasObra(string data)
        {
            List<FolhaObra> LstFolhasObra = new List<FolhaObra>();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_folhas_obra Where dataservico like '%" + data + "%';");
                while (result.Read())
                {
                    LstFolhasObra.Add(new FolhaObra()
                    {
                        IdFolhaObra = result["IdFolhaObra"],
                        DataServico = result["DataServico"],
                        ReferenciaServico = result["ReferenciaServico"],
                        EstadoEquipamento = result["EstadoEquipamento"],
                        SituacoesPendentes = result["SituacoesPendentes"],
                        ConferidoPor = result["ConferidoPor"],
                        GuiaTransporteAtual = result["GuiaTransporteAtual"],
                        AssistenciaRemota = result["Remoto"] == 1,
                        //IdCartao = result.Reader.IsDBNull(result["IdCartaoTrello"]) ? "" : result["IdCartaoTrello"],
                        Recibo = ObterReciboFolhaObra(result["IdFolhaObra"]),
                        IdCartao = result["IdCartaoTrello"],
                        EquipamentoServico = ObterEquipamento(result["IdEquipamento"]),
                        ClienteServico = ObterCliente(result["IdCliente"], result["IdLoja"]),
                        PecasServico = ObterListaProdutoIntervencao(result["IdFolhaObra"]),
                        IntervencaosServico = ObterListaIntervencoes(result["IdFolhaObra"])

                    });
                }
            }

            return LstFolhasObra;

        }
        public List<FolhaObra> ObterListaFolhasObraCliente(int IdCliente, int IdLoja)
        {
            List<FolhaObra> LstFolhasObra = new List<FolhaObra>();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_folhas_obra Where IdCliente = "+IdCliente+" AND IdLoja = "+IdLoja+" Order By DataServico DESC;");
                while (result.Read())
                {
                    LstFolhasObra.Add(new FolhaObra()
                    {
                        IdFolhaObra = result["IdFolhaObra"],
                        DataServico = result["DataServico"],
                        ReferenciaServico = result["ReferenciaServico"],
                        EstadoEquipamento = result["EstadoEquipamento"],
                        SituacoesPendentes = result["SituacoesPendentes"],
                        ConferidoPor = result["ConferidoPor"],
                        EquipamentoServico = ObterEquipamento(result["IdEquipamento"]),
                        GuiaTransporteAtual = result["GuiaTransporteAtual"],
                        AssistenciaRemota = result["Remoto"] == 1,
                        IdCartao = result["IdCartaoTrello"],

                    });
                }
            }

            return LstFolhasObra;

        }
        public List<Marcacao> ObterListaMarcacoesPendentes()
        {
            List<Marcacao> LstMarcacao = new List<Marcacao>();
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_marcacoes, dat_marcacoes_tecnico, dat_marcacoes_estado where dat_marcacoes_estado.idestado=dat_marcacoes.estadomarcacao and dat_marcacoes.marcacaostamp = dat_marcacoes_tecnico.marcacaostamp AND Marcado=1 AND (EstadoMarcacao!=4 OR DataMarcacao >= '" + DateTime.Now.ToString("yy-MM-dd") + "') order by DataMarcacao, IdTecnico;");
                while (result.Read())
                {
                    //DateTime d = DateTime.Parse(result["DataMarcacao"]);
                    LstMarcacao.Add(new Marcacao()
                    {
                        IdMarcacao = result["IdMarcacao"],
                        DataMarcacao = DateTime.Parse(result["DataMarcacao"]),
                        Cliente = ObterCliente(result["IdCliente"], result["IdLoja"]),
                        ResumoMarcacao = result["ResumoMarcacao"],
                        EstadoMarcacao = result["EstadoMarcacao"],
                        EstadoMarcacaoDesc = result["EstadoMarcacaoDesc"],
                        IdTecnico = result["IdTecnico"],
                        Tecnico = ObterTecnico(Int32.Parse(result["IdTecnico"])),
                        PrioridadeMarcacao = result["PrioridadeMarcacao"],
                        MarcacaoStamp = result["MarcacaoStamp"],
                        Oficina = result["Oficina"],
                        Instalacao = result["Instalacao"],
                        TipoEquipamento = result["TipoEquipamento"]

                    });
                }
            }

            return LstMarcacao;

        }

        public List<Marcacao> ObterListaMarcacoes(DateTime DataInicial, DateTime DataFinal)
        {
            List<Marcacao> LstMarcacao = new List<Marcacao>();
            String DataI = DataInicial.ToString("yyyy-MM-dd");
            String DataF = DataFinal.ToString("yyyy-MM-dd");
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_marcacoes, dat_marcacoes_tecnico, dat_marcacoes_estado where dat_marcacoes_estado.idestado=dat_marcacoes.estadomarcacao and dat_marcacoes.marcacaostamp = dat_marcacoes_tecnico.marcacaostamp AND DataMarcacao>='" + DataI + "'  AND DataMarcacao<='" + DataF + "' AND Marcado=1 order by DataMarcacao, IdTecnico;");
                while (result.Read())
                {
                    //DateTime d = DateTime.Parse(result["DataMarcacao"]);
                    LstMarcacao.Add(new Marcacao()
                    {
                        IdMarcacao = result["IdMarcacao"],
                        DataMarcacao = DateTime.Parse(result["DataMarcacao"]),
                        Cliente = ObterCliente(result["IdCliente"], result["IdLoja"]),
                        ResumoMarcacao = result["ResumoMarcacao"],
                        EstadoMarcacao = result["EstadoMarcacao"],
                        EstadoMarcacaoDesc = result["EstadoMarcacaoDesc"],
                        IdTecnico = result["IdTecnico"],
                        Tecnico = ObterTecnico(Int32.Parse(result["IdTecnico"])),
                        PrioridadeMarcacao = result["PrioridadeMarcacao"],
                        MarcacaoStamp = result["MarcacaoStamp"],
                        Oficina = result["Oficina"],
                        Instalacao = result["Instalacao"],
                        TipoEquipamento = result["TipoEquipamento"]

                    });
                }
            }

            return LstMarcacao;

        }
       public FeriasUtilizador ObterListaFeriasUtilizador(int IdUtilizador, string Ano)
        {
            FeriasUtilizador feriasUtilizador = new FeriasUtilizador();

            using (Database db = ConnectionString)
            {
                using var resultQuery = db.QueryValue("SELECT Count(*) from dat_ferias_utilizador where IdUtilizador='" + IdUtilizador + "' AND Ano='" + Ano + "';");
                if (resultQuery == 0) CriarFeriasUtilizador(IdUtilizador, Ano, 23);
            }

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_ferias_utilizador where IdUtilizador='"+IdUtilizador+"' AND Ano='"+Ano+"';");
                while (result.Read())
                {

                    feriasUtilizador.utilizador = ObterUtilizador(int.Parse(result["IdUtilizador"]));
                    feriasUtilizador.DiasMarcados = int.Parse(ObterFeriasMarcadas(IdUtilizador));
                    feriasUtilizador.DiasTotais = int.Parse(ObterFeriasDias(IdUtilizador));
                    feriasUtilizador.DiasDisponiveis = int.Parse(result["DiasDireito"]);
                    feriasUtilizador.Ferias = ObterListaFerias(IdUtilizador, Ano);
                }
            }

            return feriasUtilizador;

        }

        public List<Ferias> ObterListaFerias(int IdUtilizador, string Ano)
        {
            List<Ferias> LstFerias = new List<Ferias>();
            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM dat_ferias where IdUtilizador='" + IdUtilizador + "' AND DataInicio>='" + Ano + "-01-01' AND DataFim<='" + Ano + "-12-31' order by DataInicio;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstFerias.Add(new Ferias()
                    {
                        Id = result["Id"],
                        IdUtilizador = result["IdUtilizador"],
                        DataInicio = result["DataInicio"],
                        DataFim = result["DataFim"],
                        Validado = result["Validado"],
                        Obs = result["Obs"],
                        ValidadoPor = result["ValidadoPor"],
                        ValidadoPorNome = result["ValidadoPor"] == 0 ? "" : ObterUtilizador(result["ValidadoPor"]).NomeCompleto,
                    });
                }
            }

            return LstFerias;

        }
        public List<Ferias> ObterListaFerias(DateTime dataInicio, DateTime dataFim)
        {
            List<Ferias> LstFerias = new List<Ferias>();
            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM dat_ferias where DataInicio>='" + dataInicio.ToString("yyyy-MM-dd") + "' AND DataFim<='" + dataFim.ToString("yyyy-MM-dd") + "' order by DataInicio;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstFerias.Add(new Ferias()
                    {
                        Id = result["Id"],
                        IdUtilizador = result["IdUtilizador"],
                        DataInicio = result["DataInicio"],
                        DataFim = result["DataFim"],
                        Validado = result["Validado"],
                        Obs = result["Obs"],
                        ValidadoPor = result["ValidadoPor"],
                        ValidadoPorNome = result["ValidadoPor"] == 0 ? "" : ObterUtilizador(result["ValidadoPor"]).NomeCompleto,
                    });
                }
            }

            return LstFerias;

        }

        public Ferias ObterFerias(int Id)
        {
            List<Ferias> LstFerias = new List<Ferias>();
            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM dat_ferias where Id='" + Id + "';";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstFerias.Add(new Ferias()
                    {
                        Id = result["Id"],
                        IdUtilizador = result["IdUtilizador"],
                        DataInicio = result["DataInicio"],
                        DataFim = result["DataFim"],
                        Validado = result["Validado"],
                        Obs = result["Obs"],
                        ValidadoPor = result["ValidadoPor"],
                        ValidadoPorNome = result["ValidadoPor"] == 0 ? "" : ObterUtilizador(result["ValidadoPor"]).NomeCompleto,
                    });
                }
            }

            return LstFerias.Count() > 0 ? LstFerias.FirstOrDefault() : new Ferias();

        }
        public List<Acesso> ObterListaAcessos(DateTime Data)
        {
            List<Acesso> LstAcessos = new List<Acesso>();
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_acessos where DataHoraAcesso>'" + Data.ToString("yyyy-MM-dd") + " 00:00:00' AND DataHoraAcesso<'"+ Data.ToString("yyyy-MM-dd") + " 23:59:59' order by DataHoraAcesso;");
                while (result.Read())
                {
                    LstAcessos.Add(new Acesso()
                    {
                        Id = result["Id"],
                        Utilizador = ObterUtilizador(result["IdUtilizador"]),
                        Data = result["DataHoraAcesso"],
                        Temperatura = result["Temperatura"],
                        Tipo = result["TipoAcesso"]
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

                using var result = db.Query("SELECT * FROM dat_acessos where DataHoraAcesso>'" + Data.Year + "-" + Data.Month   + "-01 00:00:00' AND DataHoraAcesso<'" + Data.Year + "-" + Data.Month + "-31 23:59:59' order by DataHoraAcesso;");
                while (result.Read())
                {
                    LstAcessos.Add(new Acesso()
                    {
                        Id = result["Id"],
                        Utilizador = ObterUtilizador(result["IdUtilizador"]),
                        Data = result["DataHoraAcesso"],
                        Temperatura = result["Temperatura"],
                        Tipo = result["TipoAcesso"]
                    });
                }
            }

            return LstAcessos;

        }

        public List<Ferias> ObterListaFeriasValidar()
        {
            List<Ferias> LstFerias = new List<Ferias>();
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_ferias where Validado=0 order by DataInicio;");
                while (result.Read())
                {
                    LstFerias.Add(new Ferias()
                    {
                        Id = result["Id"],
                        IdUtilizador = result["IdUtilizador"],
                        DataInicio = result["DataInicio"],
                        DataFim = result["DataFim"],
                        Validado = result["Validado"],
                        Obs = result["Obs"],
                        ValidadoPor = result["ValidadoPor"],
                        ValidadoPorNome = result["ValidadoPor"] == 0 ? "" : ObterUtilizador(result["ValidadoPor"]).NomeCompleto,
                    });
                }
            }

            return LstFerias;

        }

        public void ValidarEmailEnviado()
        {
            string sql = "UPDATE dat_ferias set mail_validacao='1'";

            Database db = ConnectionString;

            db.Execute(sql);
            db.Connection.Close();
        }

        public string ObterFeriasMarcadas(int IdUtilizador)
        {

            using (Database db = ConnectionString)
            {
               
                using var result = db.QueryValue("SELECT COALESCE(SUM(DATEDIFF(DataFim, DataInicio) + 1),0) FROM dat_ferias where Validado=1 AND IdUtilizador ='" + IdUtilizador + "';");
                return result;
            }

        }
        public string ObterFeriasDias(int IdUtilizador)
        {

            using (Database db = ConnectionString)
            {

                using var result = db.QueryValue("SELECT COALESCE(SUM(DATEDIFF(DataFim, DataInicio) + 1),0) FROM dat_ferias where IdUtilizador ='" + IdUtilizador + "';");
                return result;
            }

        }

        public string ObterAnoAtivo()
        {

            using (Database db = ConnectionString)
            {

                using var result = db.QueryValue("SELECT IFNULL( (SELECT Ano from dat_ferias_ano where Active=1 LIMIT 1), '"+DateTime.Now.Year+"') as Ano;");
                return result;
            }

        }

        public List<Marcacao> ObterListaMarcacoesCliente(int IdCliente, int IdLoja)
        {
            List<Marcacao> LstMarcacao = new List<Marcacao>();
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_marcacoes, dat_marcacoes_tecnico, dat_marcacoes_estado where dat_marcacoes_estado.idestado=dat_marcacoes.estadomarcacao and dat_marcacoes.marcacaostamp = dat_marcacoes_tecnico.marcacaostamp AND IdCliente='" + IdCliente + "'  AND IdLoja='" + IdLoja + "' AND Marcado=1 order by DataMarcacao DESC, IdTecnico;");
                while (result.Read())
                {
                    //DateTime d = DateTime.Parse(result["DataMarcacao"]);
                    LstMarcacao.Add(new Marcacao()
                    {
                        IdMarcacao = result["IdMarcacao"],
                        DataMarcacao = DateTime.Parse(result["DataMarcacao"]),
                        Cliente = ObterCliente(result["IdCliente"], result["IdLoja"]),
                        ResumoMarcacao = result["ResumoMarcacao"],
                        EstadoMarcacao = result["EstadoMarcacao"],
                        EstadoMarcacaoDesc = result["EstadoMarcacaoDesc"],
                        IdTecnico = result["IdTecnico"],
                        Tecnico = ObterTecnico(Int32.Parse(result["IdTecnico"])),
                        PrioridadeMarcacao = result["PrioridadeMarcacao"],
                        MarcacaoStamp = result["MarcacaoStamp"],
                        Oficina = result["Oficina"],
                        Instalacao = result["Instalacao"],
                        TipoEquipamento = result["TipoEquipamento"]

                    });
                }
            }

            return LstMarcacao;

        }

        public List<Marcacao> ObterListaMarcacoesSimples(DateTime DataInicial, DateTime DataFinal)
        {
            List<Marcacao> LstMarcacao = new List<Marcacao>();
            String DataI = DataInicial.ToString("yyyy-MM-dd");
            String DataF = DataFinal.ToString("yyyy-MM-dd");
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_marcacoes, dat_marcacoes_tecnico, dat_marcacoes_estado where dat_marcacoes_estado.idestado=dat_marcacoes.estadomarcacao and dat_marcacoes.marcacaostamp = dat_marcacoes_tecnico.marcacaostamp AND DataMarcacao>='" + DataI + "'  AND DataMarcacao<='" + DataF + "' AND Marcado=1 order by DataMarcacao, IdTecnico;");
                while (result.Read())
                {
                    //DateTime d = DateTime.Parse(result["DataMarcacao"]);
                    LstMarcacao.Add(new Marcacao()
                    {
                        IdMarcacao = result["IdMarcacao"],
                        DataMarcacao = DateTime.Parse(result["DataMarcacao"]),
                        EstadoMarcacao = result["EstadoMarcacao"],
                        IdTecnico = result["IdTecnico"]                       

                    });
                }
            }

            return LstMarcacao;

        }
        public List<Marcacao> ObterListaMarcacoes(int IdTecnico, DateTime DataInicial, DateTime DataFinal)
        {
            List<Marcacao> LstMarcacao = new List<Marcacao>();
            String DataI = DataInicial.ToString("yyyy-MM-dd");
            String DataF = DataFinal.ToString("yyyy-MM-dd");
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_marcacoes, dat_marcacoes_tecnico, dat_marcacoes_estado where dat_marcacoes_estado.idestado=dat_marcacoes.estadomarcacao and dat_marcacoes.marcacaostamp = dat_marcacoes_tecnico.marcacaostamp AND dat_marcacoes_tecnico.idtecnico=" + IdTecnico+ " AND DataMarcacao>='" + DataI + "'  AND DataMarcacao<='" + DataF + "' AND Marcado=1;");
                while (result.Read())
                {
                    LstMarcacao.Add(new Marcacao()
                    {
                        IdMarcacao = result["IdMarcacao"],
                        DataMarcacao = DateTime.Parse(result["DataMarcacao"]),
                        Cliente = ObterCliente(result["IdCliente"], result["IdLoja"]),
                        ResumoMarcacao = result["ResumoMarcacao"],
                        EstadoMarcacao = result["EstadoMarcacao"],
                        EstadoMarcacaoDesc = result["EstadoMarcacaoDesc"],
                        PrioridadeMarcacao = result["PrioridadeMarcacao"],
                        MarcacaoStamp = result["MarcacaoStamp"],
                        Oficina = result["Oficina"],
                        Instalacao = result["Instalacao"],

                    });
                }
            }

            return LstMarcacao;

        }
        public Marcacao ObterMarcacao(int IdMarcacao)
        {
            Marcacao res = new Marcacao();
            using (Database db = ConnectionString)
            {

                
                using var result = db.Query("SELECT * FROM dat_marcacoes, dat_marcacoes_estado where dat_marcacoes_estado.idestado = dat_marcacoes.estadomarcacao AND IdMarcacao=" + IdMarcacao + ";");
                while (result.Read())
                {
                    res = new Marcacao()
                    {
                        IdMarcacao = result["IdMarcacao"],
                        DataMarcacao = DateTime.Parse(result["DataMarcacao"]),
                        Cliente = ObterCliente(result["IdCliente"], result["IdLoja"]),
                        ResumoMarcacao = result["ResumoMarcacao"],
                        EstadoMarcacao = result["EstadoMarcacao"],
                        EstadoMarcacaoDesc = result["EstadoMarcacaoDesc"],
                        PrioridadeMarcacao = result["PrioridadeMarcacao"],
                        MarcacaoStamp = result["MarcacaoStamp"]
                    };
                }
            }

            res.LstFolhasObra = ObterListaFolhasObraCartao(res.MarcacaoStamp);
            return res;

        }
        public List<FolhaObra> ObterListaFolhasObraCartao(string IdCartao)
        {
            List<FolhaObra> LstFolhasObra = new List<FolhaObra>();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_folhas_obra Where IdCartaoTrello='" + IdCartao + "';");
                while (result.Read())
                {
                    LstFolhasObra.Add(new FolhaObra()
                    {
                        IdFolhaObra = result["IdFolhaObra"],
                        DataServico = result["DataServico"],
                        ReferenciaServico = result["ReferenciaServico"],
                        EstadoEquipamento = result["EstadoEquipamento"],
                        SituacoesPendentes = result["SituacoesPendentes"],
                        ConferidoPor = result["ConferidoPor"],
                        GuiaTransporteAtual = result["GuiaTransporteAtual"],
                        AssistenciaRemota = result["Remoto"] == 1,
                        IdCartao = result["IdCartaoTrello"],
                        EquipamentoServico = ObterEquipamento(result["IdEquipamento"]),
                        Recibo = ObterReciboFolhaObra(result["IdFolhaObra"]),
                        ClienteServico = ObterCliente(result["IdCliente"], result["IdLoja"]),
                        PecasServico = ObterListaProdutoIntervencao(result["IdFolhaObra"]),
                        IntervencaosServico = ObterListaIntervencoes(result["IdFolhaObra"])

                    });
                }
            }

            return LstFolhasObra;

        }
        public List<Intervencao> ObterListaIntervencoes(int idfolhaobra)
        {
            List<Intervencao> LstIntervencoes = new List<Intervencao>();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_intervencoes_folha_obra where IdFolhaObra=" + idfolhaobra + ";");
                while (result.Read())
                {
                    LstIntervencoes.Add(new Intervencao()
                    {
                        IdFolhaObra = result["IdFolhaObra"],
                        IdIntervencao = result["IdIntervencao"],
                        IdTecnico = result["IdTecnico"],
                        NomeTecnico = result["NomeTecnico"],
                        RelatorioServico = result["RelatorioServico"],
                        DataServiço = DateTime.Parse(result["DataServico"]),
                        HoraInicio = DateTime.Parse(result["HoraInicio"]),
                        HoraFim = DateTime.Parse(result["HoraFim"])
                    });
                }
            }
            return LstIntervencoes;
        }
        public List<Produto> ObterListaProdutoIntervencao(int idfolhaobra)
        {
            List<Produto> LstProdutosIntervencao = new List<Produto>();

            using (Database db = ConnectionString)
            {
                using var result = db.Query("SELECT * FROM dat_produto_intervencao where IdFolhaObra=" + idfolhaobra + ";");
                while (result.Read())
                {
                    LstProdutosIntervencao.Add(new Produto()
                    {
                        Ref_Produto = result["RefProduto"],
                        Designacao_Produto = result["Designacao"],
                        Stock_Fisico = Math.Round(double.Parse(result["Quantidade"]), 2),
                        TipoUn = result["tipoun"]
                    });
                }
            }
            return LstProdutosIntervencao;
        }
        public FolhaObra ObterFolhaObra(int id)
        {
            FolhaObra folhaObra = new FolhaObra { IdFolhaObra = -1 };
            using Database db = ConnectionString;
            using var result = db.Query("SELECT * FROM dat_folhas_obra where IdFolhaObra=" + id + ";");
            result.Read();

            folhaObra = new FolhaObra()
            {
                IdFolhaObra = result["IdFolhaObra"],
                DataServico = result["DataServico"],
                ReferenciaServico = result["ReferenciaServico"],
                EstadoEquipamento = result["EstadoEquipamento"],
                SituacoesPendentes = result["SituacoesPendentes"],
                ConferidoPor = result["ConferidoPor"],
                GuiaTransporteAtual = result["GuiaTransporteAtual"],
                AssistenciaRemota = result["Remoto"] == 1,
                IdCartao = result["IdCartaoTrello"],
                EquipamentoServico = ObterEquipamento(result["IdEquipamento"]),
                ClienteServico = ObterCliente(result["IdCliente"], result["IdLoja"]),
                Recibo = ObterReciboFolhaObra(result["IdFolhaObra"]),
                PecasServico = ObterListaProdutoIntervencao(id),
                IntervencaosServico = ObterListaIntervencoes(id),
                RubricaCliente = result["RubricaCliente"]
            };

            if (folhaObra.IntervencaosServico.Count > 1)
            {
                foreach (var intervencao in folhaObra.IntervencaosServico)
                {
                    folhaObra.RelatorioServico += intervencao.DataServiço.ToShortDateString() + " - " + intervencao.HoraInicio.ToShortTimeString() + " -> " + intervencao.HoraFim.ToShortTimeString() + ": " + intervencao.RelatorioServico + " ";
                }
            }
            else if (folhaObra.IntervencaosServico.Count > 0)
            {
                folhaObra.RelatorioServico = folhaObra.IntervencaosServico.FirstOrDefault().RelatorioServico;
            }
            return folhaObra;
        }
        public List<Equipamento> ObterListaEquipamentos()
        {
            List<Equipamento> LstEquipamentos = new List<Equipamento>();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_equipamentos;");
                while (result.Read())
                {
                    LstEquipamentos.Add(new Equipamento()
                    {
                        IdEquipamento = result["IdEquipamento"],
                        DesignacaoEquipamento = result["DesignacaoEquipamento"],
                        MarcaEquipamento = result["MarcaEquipamento"],
                        ModeloEquipamento = result["ModeloEquipamento"],
                        NumeroSerieEquipamento = result["NumeroSerieEquipamento"]

                    });
                }
            }
            return LstEquipamentos;
        }
        public List<Equipamento> ObterListaEquipamentosCliente(int IdCliente, int IdLoja)
        {
            List<Equipamento> LstEquipamentos = new List<Equipamento>();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_equipamentos where IdCliente = "+IdCliente+" AND IdLoja = "+IdLoja+";");
                while (result.Read())
                {
                    LstEquipamentos.Add(new Equipamento()
                    {
                        IdEquipamento = result["IdEquipamento"],
                        DesignacaoEquipamento = result["DesignacaoEquipamento"],
                        MarcaEquipamento = result["MarcaEquipamento"],
                        ModeloEquipamento = result["ModeloEquipamento"],
                        NumeroSerieEquipamento = result["NumeroSerieEquipamento"]

                    });
                }
            }
            return LstEquipamentos;
        }
        public Equipamento ObterEquipamento(string id)
        {
            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_equipamentos where IdEquipamento='" + id + "';");
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
            return new Equipamento();
        }
        public Equipamento ObterEquipamentoNS(string NumeroSerie)
        {
            using Database db = ConnectionString;
            using var result = db.Query("SELECT * FROM dat_equipamentos where NumeroSerieEquipamento='" + NumeroSerie + "';");
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
        public List<FolhaObra> ObterHistorico(string NumeroSerie)
        {
            List<FolhaObra> LstFolhasObra = new List<FolhaObra>();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_folhas_obra inner join dat_equipamentos on dat_equipamentos.numeroserieequipamento='" + NumeroSerie + "' AND dat_folhas_obra.idequipamento=dat_equipamentos.idequipamento;");
                while (result.Read())
                {
                    LstFolhasObra.Add(new FolhaObra()
                    {
                        IdFolhaObra = result["IdFolhaObra"],
                        DataServico = result["DataServico"],
                        ReferenciaServico = result["ReferenciaServico"],
                        EstadoEquipamento = result["EstadoEquipamento"],
                        SituacoesPendentes = result["SituacoesPendentes"],
                        ConferidoPor = result["ConferidoPor"],
                        //IdCartao = result.Reader.IsDBNull(result["IdCartaoTrello"]) ? "" : result["IdCartaoTrello"],
                        IdCartao = result["IdCartaoTrello"],
                        EquipamentoServico = ObterEquipamento(result["IdEquipamento"]),
                        ClienteServico = ObterCliente(result["IdCliente"], result["IdLoja"]),
                        PecasServico = ObterListaProdutoIntervencao(result["IdFolhaObra"]),
                        IntervencaosServico = ObterListaIntervencoes(result["IdFolhaObra"])

                    });
                    if (LstFolhasObra.Last().IntervencaosServico.Count > 1)
                    {
                        foreach (var intervencao in LstFolhasObra.Last().IntervencaosServico)
                        {
                            LstFolhasObra.Last().RelatorioServico += intervencao.DataServiço.ToShortDateString() + " - " + intervencao.HoraInicio.ToShortTimeString() + " -> " + intervencao.HoraFim.ToShortTimeString() + ": " + intervencao.RelatorioServico + " ";
                        }
                    }
                    else if (LstFolhasObra.Last().IntervencaosServico.Count > 0)
                    {
                        LstFolhasObra.Last().RelatorioServico = LstFolhasObra.Last().IntervencaosServico.FirstOrDefault().RelatorioServico;
                    }
                }
            }

            return LstFolhasObra;
        }
        public List<Movimentos> ObterListaMovimentos(int IdTecnico, string Guia)
        {
            List<Movimentos> LstGuias = new List<Movimentos>();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT dat_folhas_obra.IdFolhaObra, NomeTecnico, NomeCliente, dat_folhas_obra.DataServico, GuiaTransporteAtual, RefProduto, Designacao, Quantidade FROM dat_folhas_obra inner join dat_clientes, dat_produto_intervencao, dat_intervencoes_folha_obra where dat_produto_intervencao.idfolhaobra = dat_folhas_obra.idfolhaobra AND dat_intervencoes_folha_obra.idfolhaobra = dat_folhas_obra.idfolhaobra AND GuiaTransporteAtual != '' AND IdTecnico=" + IdTecnico + " AND GuiaTransporteAtual like '" + Guia + "' AND dat_clientes.idcliente = dat_folhas_obra.idcliente GROUP BY dat_produto_intervencao.RefProduto, dat_folhas_obra.idfolhaobra;");
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

            return LstGuias;
        }
        public List<Movimentos> ObterListaMovimentosProduto(string Ref_Produto)
        {
            List<Movimentos> LstGuias = new List<Movimentos>();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT dat_folhas_obra.IdFolhaObra, NomeTecnico, NomeCliente, dat_folhas_obra.DataServico, GuiaTransporteAtual, RefProduto, Designacao, Quantidade FROM dat_folhas_obra inner join dat_clientes, dat_produto_intervencao, dat_intervencoes_folha_obra where dat_produto_intervencao.idfolhaobra = dat_folhas_obra.idfolhaobra AND dat_intervencoes_folha_obra.idfolhaobra = dat_folhas_obra.idfolhaobra AND RefProduto = '" + Ref_Produto + "' AND dat_clientes.idcliente = dat_folhas_obra.idcliente AND dat_clientes.idloja = dat_folhas_obra.idloja GROUP BY dat_produto_intervencao.RefProduto, dat_folhas_obra.idfolhaobra;");
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

            return LstGuias;
        }
        public List<Movimentos> ObterListaMovimentos(int IdTecnico)
        {
            List<Movimentos> LstGuias = new List<Movimentos>();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT GuiaTransporteAtual FROM dat_folhas_obra inner join dat_produto_intervencao, dat_intervencoes_folha_obra where dat_produto_intervencao.idfolhaobra = dat_folhas_obra.idfolhaobra AND dat_intervencoes_folha_obra.idfolhaobra = dat_folhas_obra.idfolhaobra AND GuiaTransporteAtual != '' AND GuiaTransporteAtual != 'GT" + DateTime.Now.Year + "BO91/' AND IdTecnico=" + IdTecnico + " GROUP BY dat_folhas_obra.guiatransporteatual;");
                while (result.Read())
                {
                    LstGuias.Add(new Movimentos()
                    {
                        GuiaTransporte = result["GuiaTransporteAtual"]
                    });
                }
            }

            return LstGuias;
        }
        public Cliente ObterCliente(int id, int est)
        {

            Cliente cliente = new Cliente();
            using Database db = ConnectionString;
            using var result = db.Query("SELECT * FROM dat_clientes where IdCliente=" + id + " AND IdLoja=" + est + ";");
            result.Read();
            if (result.Reader.HasRows)
            {
                cliente = new Cliente()
                {
                    IdCliente = result["IdCliente"],
                    IdLoja = result["IdLoja"],
                    NomeCliente = result["NomeCliente"],
                    PessoaContatoCliente = result["PessoaContactoCliente"],
                    MoradaCliente = result["MoradaCliente"],
                    EmailCliente = result["EmailCliente"],
                    NumeroContribuinteCliente = result["NumeroContribuinteCliente"],
                    TelefoneCliente = result["Telefone"]
            };
            }
           
            return cliente;
        }
        public Cliente ObterClienteCompleto(int id, int est)
        {

            Cliente cliente = new Cliente();
            using Database db = ConnectionString;
            using var result = db.Query("SELECT * FROM dat_clientes where IdCliente=" + id + " AND IdLoja=" + est + ";");
            result.Read();
            if (result.Reader.HasRows)
            {
                cliente = new Cliente()
                {
                    IdCliente = result["IdCliente"],
                    IdLoja = result["IdLoja"],
                    NomeCliente = result["NomeCliente"],
                    PessoaContatoCliente = result["PessoaContactoCliente"],
                    MoradaCliente = result["MoradaCliente"],
                    EmailCliente = result["EmailCliente"],
                    NumeroContribuinteCliente = result["NumeroContribuinteCliente"],
                    TelefoneCliente = result["Telefone"],
                    Marcacoes = ObterListaMarcacoesCliente(id, est),
                    FolhasObra = ObterListaFolhasObraCliente(id, est),
                    Visitas = ObterListaVisitasCliente(id, est),
                    Equipamentos = ObterListaEquipamentosCliente(id, est)
                };
            }

            return cliente;
        }
        public Cliente ObterClienteNome(string nome)
        {
            using Database db = ConnectionString;
            Cliente cliente = new Cliente();
            using var result = db.Query("SELECT * FROM dat_clientes where NomeCliente like '%" + nome.Replace("'", "''") + "%';");
            result.Read();
            if (result.Reader.HasRows)
            {
                cliente = new Cliente()
                {
                    IdCliente = result["IdCliente"],
                    IdLoja = result["IdLoja"],
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
        public Recibo ObterReciboFolhaObra(int IdFolhaObra)
        {
            using Database db = ConnectionString;
            Recibo recibo = new Recibo();
            using var result = db.Query("SELECT * FROM dat_recibos where IdFolhaObra = '" + IdFolhaObra + "';");
            result.Read();
            if (result.Reader.HasRows)
            {
                recibo = new Recibo()
                {
                    IdRecibo = result["IdRecibo"],
                    MaterialAplicado = result["MaterialAplicado"],
                    MaoObra = result["MaoObra"],
                    Deslocacao = result["Deslocacao"],
                    IdFolhaObra = result["IdFolhaObra"]
                };
            }
            else
            {
                recibo = new Recibo()
                {
                    IdRecibo = 0,
                    MaterialAplicado = 0.00,
                    MaoObra = 0.00,
                    Deslocacao = 0.00,
                    IdFolhaObra = IdFolhaObra
                };
            }
            return recibo;
        }
        public List<Cliente> ObterListaClientes(string NomeCliente, bool exact)
        {

            List<Cliente> LstClientes = new List<Cliente>();
            string sqlQuery = "SELECT * FROM dat_clientes where NomeCliente like '%" + NomeCliente.Replace("'", "''") + "%';";
            if (exact) sqlQuery = "SELECT * FROM dat_clientes where NomeCliente ='" + NomeCliente.Replace("'", "''") + "';";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstClientes.Add(new Cliente()
                    {
                        IdCliente = result["IdCliente"],
                        IdLoja = result["IdLoja"],
                        NomeCliente = result["NomeCliente"],
                        PessoaContatoCliente = result["PessoaContactoCliente"],
                        MoradaCliente = result["MoradaCliente"],
                        EmailCliente = result["EmailCliente"],
                        NumeroContribuinteCliente = result["NumeroContribuinteCliente"],
                        TelefoneCliente = result["Telefone"]
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
        public int ObterUltimaEntrada(string NomeTabela, string CampoID)
        {
            using (Database db = ConnectionString)
            {
                //using var result = db.Query("SELECT Max(" + CampoID + ") FROM " + NomeTabela + ";");
                using var result = db.Query("SELECT MIN(t1." + CampoID + " + 1) AS nextID FROM " + NomeTabela + " t1 LEFT JOIN " + NomeTabela + " t2 ON t1." + CampoID + " + 1 = t2." + CampoID + " WHERE t2." + CampoID + " IS NULL;");
                while (result.Read())
                {
                    return result.Reader.IsDBNull(0) ? 0 : result[0];
                };
            }
            return 0;

        }
        public List<Utilizador> ObterListaUtilizadores()
        {
            List<Utilizador> LstUtilizadores = new List<Utilizador>();
            string sqlQuery = "SELECT * FROM sys_utilizadores where enable=1;";

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
                        IdCartaoTrello = result["IdCartaoTrello"],
                        IdPHC = result["IdPHC"],
                        Admin = result["admin"]
                    });
                }
            }
            return LstUtilizadores;
        }
        public List<Utilizador> ObterListaTecnicos()
        {
            List<Utilizador> LstUtilizadores = new List<Utilizador>();
            string sqlQuery = "SELECT * FROM sys_utilizadores where TipoUtilizador = "+1+";";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstUtilizadores.Add(new Utilizador()
                    {
                        Id = result["IdPHC"],
                        NomeUtilizador = result["NomeUtilizador"],
                        NomeCompleto = result["NomeCompleto"],
                        EmailUtilizador = result["EmailUtilizador"]
                    });
                }
            }
            return LstUtilizadores;
        }
        public List<Utilizador> ObterListaTecnicosMarcacao(string MarcacaoStamp)
        {
            List<Utilizador> LstUtilizadores = new List<Utilizador>();
            string sqlQuery = "SELECT * FROM dat_marcacoes_tecnico where marcacaostamp = '" + MarcacaoStamp + "';";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstUtilizadores.Add(new Utilizador()
                    {
                        Id = result["IdTecnico"],
                        NomeCompleto = result["NomeTecnico"]
                    });
                }
            }
            return LstUtilizadores;
        }
        public List<Utilizador> ObterListaComerciais()
        {
            List<Utilizador> LstUtilizadores = new List<Utilizador>();
            string sqlQuery = "SELECT * FROM sys_utilizadores where TipoUtilizador = " + 2 + ";";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    LstUtilizadores.Add(new Utilizador()
                    {
                        Id = result["IdPHC"],
                        NomeUtilizador = result["NomeUtilizador"],
                        NomeCompleto = result["NomeCompleto"],
                        EmailUtilizador = result["EmailUtilizador"]
                    });
                }
            }
            return LstUtilizadores;
        }
        public Utilizador ObterUtilizador(int Id)
        {
            Utilizador utilizador = new Utilizador();
            string sqlQuery = "SELECT * FROM sys_utilizadores where IdUtilizador = " + Id + ";";

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
                        IdCartaoTrello = result["IdCartaoTrello"],
                        Admin = result["admin"] == 1,
                        Enable = result["enable"] == 1,
                        CorCalendario = result["CorCalendario"],
                        IdPHC = result["IdPHC"]
                    };
                }
            }
            return utilizador;
        }
        public Utilizador ObterTecnico(int Id)
        {
            Utilizador utilizador = new Utilizador();
            string sqlQuery = "SELECT * FROM sys_utilizadores where IdPHC = " + Id + ";";

            using Database db = ConnectionString;
            using (var result = db.Query(sqlQuery))
            {
                while (result.Read())
                {
                    utilizador = new Utilizador()
                    {
                        Id = result["IdPHC"],
                        NomeUtilizador = result["NomeUtilizador"],
                        Password = result["Password"],
                        NomeCompleto = result["NomeCompleto"],
                        TipoUtilizador = int.Parse(result["TipoUtilizador"]),
                        EmailUtilizador = result["EmailUtilizador"],
                        IdCartaoTrello = result["IdCartaoTrello"],
                        Admin = result["admin"] == 1,
                        Enable = result["enable"] == 1,
                        CorCalendario = result["CorCalendario"],
                        Iniciais = result["IniciaisUtilizador"]
                    };
                }
            }
            return utilizador;
        }
        public Utilizador ObterUtilizadorNome(string Nome)
        {
            Utilizador utilizador = new Utilizador();
            string sqlQuery = "SELECT * FROM sys_utilizadores where NomeCompleto like '" + Nome + "%';";

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
                        TipoUtilizador = result["TipoUtilizador"],
                        EmailUtilizador = result["EmailUtilizador"],
                        IdCartaoTrello = result["IdCartaoTrello"]
                    };
                }
            }
            return utilizador;
        }

        public List<CalendarioEvent> ConverterMarcacoesEventos(List<Marcacao> Marcacoes)
        {
            List<CalendarioEvent> LstEventos = new List<CalendarioEvent>();
            
            DateTime dataMarcacao = DateTime.Parse(DateTime.Now.ToShortDateString() + " 00:00:00");
            dataMarcacao.AddMinutes(5);
            foreach (var item in Marcacoes)
            {
                if (LstEventos.Count > 0 && LstEventos.Last().IdTecnico != item.IdTecnico) dataMarcacao = dataMarcacao.AddMinutes(5);
                if (dataMarcacao.ToShortDateString() != item.DataMarcacao.ToShortDateString()) dataMarcacao = DateTime.Parse(item.DataMarcacao.ToShortDateString() + " 00:00:00");

                LstEventos.Add(new CalendarioEvent
                {
                    id = item.IdMarcacao,
                    title = (item.EstadoMarcacao == 4 ? "✔ " : item.EstadoMarcacao != 1 && item.EstadoMarcacao != 5 ? "⌛ " : item.DataMarcacao < DateTime.Now ? "❌ " : "") + item.Tecnico.Iniciais + " - "  + item.Cliente.NomeCliente,
                    start = dataMarcacao,
                    end = dataMarcacao.AddMinutes(19),
                    IdTecnico = item.IdTecnico,
                    //color = ("#33FF77"),
                    url = "Pedido/?idMarcacao="+item.IdMarcacao+"&IdTecnico=" + (item.IdTecnico),
                    color = (item.Tecnico.CorCalendario == string.Empty ? "#3371FF" : item.Tecnico.CorCalendario)
                });
                dataMarcacao = dataMarcacao.AddMinutes(20);
            }

            return LstEventos;
        }

        public List<CalendarioEvent> ConverterFeriasEventos(List<Ferias> Ferias)
        {
            List<CalendarioEvent> LstEventos = new List<CalendarioEvent>();

            foreach (var item in Ferias)
            {
                Utilizador ut = ObterUtilizador(item.IdUtilizador);
                LstEventos.Add(new CalendarioEvent
                {
                    id = item.Id,
                    title = ut.NomeCompleto,
                    start = item.DataInicio,
                    end = item.DataInicio != item.DataFim ? item.DataFim.AddDays(1) : item.DataInicio.AddDays(1),
                    setAllDay = true,
                    IdTecnico = item.IdUtilizador,
                    //color = ("#33FF77"),
                    url = "Detalhes/?IdUtilizador=" + item.IdUtilizador,
                    color = (ut.CorCalendario == string.Empty ? "#3371FF" : ut.CorCalendario)
                });
            }

            return LstEventos;
        }


        public List<CalendarioEvent> ConverterVisitasEventos(List<Visita> Visitas)
        {
            List<CalendarioEvent> LstEventos = new List<CalendarioEvent>();

            DateTime dataMarcacao = DateTime.Parse(DateTime.Now.ToShortDateString() + " 00:00:00");
            dataMarcacao.AddMinutes(5);
            Utilizador comercial = new Utilizador();
            foreach (var item in Visitas)
            {
                if (LstEventos.Count > 0 && LstEventos.Last().IdTecnico != item.IdComercial) { dataMarcacao = dataMarcacao.AddMinutes(5); comercial = ObterTecnico(item.IdComercial); }
                if (dataMarcacao.ToShortDateString() != item.DataVisita.ToShortDateString()) dataMarcacao = DateTime.Parse(item.DataVisita.ToShortDateString() + " 00:00:00");
                if (LstEventos.Count == 0) comercial = ObterTecnico(item.IdComercial);

                LstEventos.Add(new CalendarioEvent
                {
                    id = item.IdVisita,
                    title = (item.EstadoVisita == "Finalizado" ? "✔ " : item.EstadoVisita != "Criado" && item.EstadoVisita != "Agendado" ? "⌛ " : item.DataVisita < DateTime.Now ? "❌ " : "") + comercial.Iniciais + " - " + item.Cliente.NomeCliente,
                    start = dataMarcacao,
                    end = dataMarcacao.AddMinutes(19),
                    IdTecnico = item.IdComercial,
                    //color = ("#33FF77"),
                    url = "Visita/?idVisita=" + item.IdVisita + "&IdComercial=" + (item.IdComercial),
                    color = (comercial.CorCalendario == string.Empty ? "#3371FF" : comercial.CorCalendario)
                });
                dataMarcacao = dataMarcacao.AddMinutes(20);
            }

            return LstEventos;
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

        public byte[] GerarMapaPresencas(DateTime Data)
        {
            using ExcelPackage package = new ExcelPackage(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "FT_Presencas.xlsx"));
            //ExcelWorksheet workSheet = package.Workbook.Worksheets["Table1"];
            ExcelWorksheet workSheet = package.Workbook.Worksheets.First();
            int totalRows = workSheet.Dimension.Rows;
            List<Utilizador> LstUtilizadores = ObterListaUtilizadores();
            List<Acesso> LstAcessos = ObterListaAcessosMes(Data);

            int y = 5;
            int x = 1;

            workSheet.Cells[4, 1].Value = Data.ToString("MMMM yyyy");


            foreach (var utilizador in LstUtilizadores)
            {
                workSheet.Cells[y, x].Value = utilizador.NomeCompleto;
                y += 4;
            }

            if (LstAcessos.Count > 0)
            {
                for (int i = 1; i < LstAcessos.Last().Data.Day + 1; i++)
                {
                    y = 5;

                    foreach (Utilizador utilizador in LstUtilizadores)
                    {
                        int j = y;
                        if (LstAcessos.Where(u => u.Data.Day == i).Count() == 0)
                        {
                            workSheet.Cells[j, i + 1].Value = utilizador.TipoUtilizador == 1 ? "E: 9:00 Externo" : utilizador.TipoUtilizador == 2 ? "E: 9:00 Comercial" : "E: 09:00";
                            workSheet.Cells[j, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            workSheet.Cells[j, i + 1].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                            workSheet.Cells[j + 1, i + 1].Value = utilizador.TipoUtilizador == 1 ? "S: 18:30 Externo" : utilizador.TipoUtilizador == 2 ? "S: 18:30 Comercial" : "S: 18:30 ";
                            workSheet.Cells[j + 1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            workSheet.Cells[j + 1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                        }
                        else
                        {
                            foreach (var acesso in LstAcessos.Where(u => u.Utilizador.Id == utilizador.Id))
                            {
                                workSheet.Cells[j, i + 1].Value = acesso.Tipo.Substring(0, 1) + ": " + acesso.Data.ToShortTimeString();
                                j++;
                            }
                        }
                        y += 4;
                    }
                }
            }

            return package.GetAsByteArray();
        }

        public DateTime ObterUltimaModificacaoPHC(string tabela)
        {
            DateTime res = new DateTime();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT ultimamodificacao FROM sys_tabelas where nometabela = '"+tabela+"'; ");
                while (result.Read())
                {
                    res = result[0];
                }
            }


            return res;
        }
        public void AtualizarUltimaModificacao(string tabela)
        {
            string sql = "UPDATE sys_tabelas set ultimamodificacao='"+DateTime.Now.ToString("yyyy-MM-dd 00:00:00")+"' where nometabela='"+tabela+"'";

            Database db = ConnectionString;

            db.Execute(sql);
            db.Connection.Close();
        }

        public void CriarIntervencoes(List<Intervencao> LstIntervencoes)
        {
            int max = 3000;
            int j = 0;
            for (int i = 0; j < LstIntervencoes.Count; i++)
            {
                if ((j + max) > LstIntervencoes.Count) max = (LstIntervencoes.Count - j);

                string sql = "INSERT INTO dat_intervencoes_folha_obra (IdIntervencao, IdFolhaObra,IdTecnico, RelatorioServico, NomeTecnico, DataServico, HoraInicio, HoraFim) VALUES ";

                foreach (var intervencao in LstIntervencoes.GetRange(j, max))
                {
                    sql += ("('" + intervencao.IdIntervencao + "', '" + intervencao.IdFolhaObra + "', '" + intervencao.IdTecnico + "', '"+intervencao.RelatorioServico.Replace("\r\n", "\n").Replace("'", "")+"', '" + intervencao.NomeTecnico.Replace("'", "''") + "', '" + intervencao.DataServiço.ToString("yy-MM-dd") + "', '" + intervencao.HoraInicio.ToString("HH:mm") + "', '" + intervencao.HoraFim.ToString("HH:mm") + "'), \r\n");
                    i++;
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE IdTecnico = VALUES(IdTecnico), NomeTecnico = VALUES(NomeTecnico), DataServico = VALUES(DataServico), RelatorioServico = VALUES(RelatorioServico), HoraInicio = VALUES(HoraInicio), HoraFim = VALUES(HoraFim);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();

                j += max;
                //Console.WriteLine("A ler FO: " + j + " de " + LstIntervencoes.Count());


            }
        }
        public void CriarFolhasObra(List<FolhaObra> LstFolhaObra)
        {
            int max = 1000;
            int j = 0;
            for (int i = 0; j < LstFolhaObra.Count; i++)
            {
                if ((j + max) > LstFolhaObra.Count) max = (LstFolhaObra.Count - j);

                string sql = "INSERT INTO dat_folhas_obra (IdFolhaObra, DataServico, ReferenciaServico, EstadoEquipamento, ConferidoPor, SituacoesPendentes, IdCartaoTrello, IdEquipamento, IdCliente, IdLoja, GuiaTransporteAtual, Remoto, RubricaCliente) VALUES ";

                foreach (var folhaObra in LstFolhaObra.GetRange(j, max))
                {
                    sql += ("('" + folhaObra.IdFolhaObra + "', '" + folhaObra.DataServico.ToString("yy-MM-dd") + "', '" + folhaObra.ReferenciaServico.Replace("'", "''").Replace("\\", "").ToString() + "', '" + folhaObra.EstadoEquipamento + "', '" + folhaObra.ConferidoPor.Replace("'", "''").ToString() + "', '" + folhaObra.SituacoesPendentes.Replace("'", "''").ToString() + "', '" + folhaObra.IdCartao + "', '" + folhaObra.EquipamentoServico.IdEquipamento + "', '" + folhaObra.ClienteServico.IdCliente + "', '" + folhaObra.ClienteServico.IdLoja + "', '" + folhaObra.GuiaTransporteAtual + "', '" + (folhaObra.AssistenciaRemota ? 1 : 0) + "', '" + folhaObra.RubricaCliente + "'), \r\n");
                    i++;
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE IdCartaoTrello=VALUES(IdCartaoTrello), DataServico = VALUES(DataServico), ReferenciaServico = VALUES(ReferenciaServico), EstadoEquipamento = VALUES(EstadoEquipamento), ConferidoPor = VALUES(ConferidoPor), SituacoesPendentes = VALUES(SituacoesPendentes), IdEquipamento = VALUES(IdEquipamento), IdCliente = VALUES(IdCliente), IdLoja = VALUES(IdLoja), GuiaTransporteAtual = VALUES(GuiaTransporteAtual), Remoto = VALUES(Remoto), RubricaCliente = VALUES(RubricaCliente);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();

                j += max;
                //Console.WriteLine("A ler FO: " + j + " de " + LstFolhaObra.Count());
            }
        }
        public void CriarPecasFolhaObra(List<Produto> LstProdutos)
        {
            int max = 7500;
            int j = 0;
            for (int i = 0; j < LstProdutos.Count; i++)
            {
                if ((j + max) > LstProdutos.Count) max = (LstProdutos.Count - j);

                string sql = "INSERT INTO dat_produto_intervencao (RefProduto, Designacao,Quantidade, IdFolhaObra, TipoUn) VALUES ";

                foreach (var produto in LstProdutos.GetRange(j, max))
                {
                    sql += ("('" + produto.Ref_Produto + "',  '" + produto.Designacao_Produto.Replace("'", "''") + "', '" + produto.Stock_Fisico + "', '" + produto.Armazem_ID + "', '" + produto.TipoUn + "'), \r\n");
                    i++;
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE Designacao = VALUES(Designacao), Quantidade = VALUES(Quantidade), TipoUn = VALUES(TipoUn);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();

                j += max;
                //Console.WriteLine("A ler peca: " + j + " de " + LstProdutos.Count());
            }
        }
        public void CriarArtigos(List<Produto> LstProdutos)
        {

            int max = 7500;
            int j = 0;
            for (int i = 0; j < LstProdutos.Count; i++)
            {
                if ((j + max) > LstProdutos.Count) max = (LstProdutos.Count - j);

                string sql = "INSERT INTO dat_produtos (ref_produto, designacao_produto, stock_phc, stock_rec, stock_res, armazem_id, stock_fisico, pos_stock, obs) VALUES ";

                foreach (var item in LstProdutos.GetRange(j, max))
                {
                    sql += ("('" + item.Ref_Produto + "', '" + item.Designacao_Produto + "', '" + item.Stock_PHC.ToString().Replace(",", ".") + "', '" + item.Stock_Rec.ToString().Replace(",", ".") + "', '" + item.Stock_Res.ToString().Replace(",", ".") + "', '" + item.Armazem_ID + "', '" + item.Stock_Fisico.ToString().Replace(",", ".") + "', '" + item.Pos_Stock + "', '" + item.Obs_Produto + "'), \r\n");
                    i++;
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE designacao_produto = VALUES(designacao_produto), stock_phc = VALUES(stock_phc), stock_rec = VALUES(stock_rec), stock_res = VALUES(stock_res), stock_fisico = VALUES(stock_fisico);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();

                j += max;
                //Console.WriteLine("A ler Marcacao: " + j + " de " + LstMarcacao.Count());
            }
        }
        public void CriarFornecedores(List<Fornecedor> LstFornecedor)
        {
            if (LstFornecedor.Count() > 0)
            {
              
                string sql = "INSERT INTO dat_fornecedores (IdFornecedor,NomeFornecedor, MoradaFornecedor, ContactoFornecedor, ReferenciaFornecedor, EmailFornecedor, PessoaContactoFornecedor, Obs) VALUES ";

                foreach (var fornecedor in LstFornecedor)
                {
                    sql += ("('" + fornecedor.IdFornecedor + "', '" + fornecedor.NomeFornecedor + "', '" + fornecedor.MoradaFornecedor + "', '" + fornecedor.ContactoFornecedor + "', '" + fornecedor.ReferenciaFornecedor + "', '" + fornecedor.EmailFornecedor + "', '" + fornecedor.PessoaContactoFornecedor + "', '" + fornecedor.Obs + "'), \r\n");
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE NomeFornecedor = VALUES(NomeFornecedor), MoradaFornecedor = VALUES(MoradaFornecedor), ContactoFornecedor = VALUES(ContactoFornecedor), ReferenciaFornecedor = VALUES(ReferenciaFornecedor), EmailFornecedor = VALUES(EmailFornecedor), PessoaContactoFornecedor = VALUES(PessoaContactoFornecedor), Obs = VALUES(Obs);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();

        }
        }
        public void CriarEquipamentos(List<Equipamento> LstEquipamento)
        {
            if (LstEquipamento.Count() > 0)
            {
                int max = 7500;
                int j = 0;
                for (int i = 0; j < LstEquipamento.Count; i++)
                {
                    if ((j + max) > LstEquipamento.Count) max = (LstEquipamento.Count - j);

                    string sql = "INSERT INTO dat_equipamentos (IdEquipamento, DesignacaoEquipamento, MarcaEquipamento, ModeloEquipamento, NumeroSerieEquipamento, IdCliente, IdLoja, IdFornecedor) VALUES ";

                    foreach (var equipamento in LstEquipamento.GetRange(j, max))
                    {
                        sql += ("('" + equipamento.IdEquipamento + "', '" + equipamento.DesignacaoEquipamento + "', '" + equipamento.MarcaEquipamento + "', '" + equipamento.ModeloEquipamento + "', '" + equipamento.NumeroSerieEquipamento + "', '" + equipamento.IdCliente + "', '" + equipamento.IdLoja + "', '" + equipamento.IdFornecedor + "'), \r\n");
                        i++;
                    }
                    sql = sql.Remove(sql.Count() - 4);

                    sql += " ON DUPLICATE KEY UPDATE DesignacaoEquipamento = VALUES(DesignacaoEquipamento), MarcaEquipamento = VALUES(MarcaEquipamento), ModeloEquipamento = VALUES(ModeloEquipamento), IdCliente = VALUES(IdCliente), IdLoja = VALUES(IdLoja), IdFornecedor = VALUES(IdFornecedor);";

                    Database db = ConnectionString;

                    db.Execute(sql);
                    db.Connection.Close();

                    j += max;
                    //Console.WriteLine("A ler equipamentos: " + j + " de " + LstEquipamento.Count());
                }
            }
        }
        public void CriarVendedores(List<Vendedor> LstVendedor)
        {
            if (LstVendedor.Count() > 0)
            {

                string sql = "INSERT INTO dat_vendedores (IdVendedor, NomeVendedor, uid) VALUES ";

                foreach (var vendedor in LstVendedor)
                {
                    sql += ("('" + vendedor.IdVendedor + "','" + vendedor.NomeVendedor + "', '" + vendedor.uid + "'), \r\n");
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE NomeVendedor = VALUES(NomeVendedor), uid = VALUES(uid);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();
            }
        }
        public void CriarClientes(List<Cliente> LstClientes)
        {
            if (LstClientes.Count() > 0)
            {

                string sql = "INSERT INTO dat_clientes (IdCliente, IdLoja, NomeCliente, PessoaContactoCliente, MoradaCliente, EmailCliente, Telefone, NumeroContribuinteCliente, IdVendedor, TipoCliente) VALUES ";

                foreach (var cliente in LstClientes)
                {
                    sql += ("('" + cliente.IdCliente + "','" + cliente.IdLoja + "', '" + cliente.NomeCliente.Replace("'", "''") + "', '" + cliente.PessoaContatoCliente.Replace("'", "''") + "', '" + cliente.MoradaCliente.Replace("'", "''") + "', '" + cliente.EmailCliente.Replace("'", "''") + "', '" + cliente.TelefoneCliente.Replace("'", "''") + "', '" + cliente.NumeroContribuinteCliente.Replace("'", "''") + "', '" + cliente.IdVendedor + "', '" + cliente.TipoCliente.Replace("'", "''") + "'), \r\n");
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE PessoaContactoCliente = VALUES(PessoaContactoCliente), Telefone = VALUES(Telefone), MoradaCliente = VALUES(MoradaCliente), EmailCliente = VALUES(EmailCliente), NumeroContribuinteCliente = VALUES(NumeroContribuinteCliente), IdVendedor = VALUES(IdVendedor), TipoCliente = VALUES(TipoCliente);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();
            }
        }
        public void CriarMarcacoes(List<Marcacao> LstMarcacao)
        {
            int max = 1000;
            int j = 0;
            for (int i = 0; j < LstMarcacao.Count; i++)
            {
                if ((j + max) > LstMarcacao.Count) max = (LstMarcacao.Count - j);

                string sql = "INSERT INTO dat_marcacoes (IdMarcacao,DataMarcacao,IdCliente,IdLoja,ResumoMarcacao,EstadoMarcacao,PrioridadeMarcacao,MarcacaoStamp, Oficina, Instalacao, TipoEquipamento) VALUES ";

                foreach (var marcacao in LstMarcacao.GetRange(j, max))
                {
                    sql += ("('" + marcacao.IdMarcacao + "', '" + marcacao.DataMarcacao.ToString("yy-MM-dd") + "', '" + marcacao.Cliente.IdCliente + "', '" + marcacao.Cliente.IdLoja + "', '" + marcacao.ResumoMarcacao.Replace("'", "''").Replace("\\", "").ToString() + "', (SELECT IdEstado FROM dat_marcacoes_estado where dat_marcacoes_estado.EstadoMarcacaoDesc='"+marcacao.EstadoMarcacaoDesc+"' LIMIT 1), '" + marcacao.PrioridadeMarcacao + "', '" + marcacao.MarcacaoStamp + "', '" + marcacao.Oficina + "', '" + marcacao.Instalacao + "', '" + marcacao.TipoEquipamento + "'), \r\n");
                    i++;
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE DataMarcacao=VALUES(DataMarcacao), IdCliente = VALUES(IdCliente), IdLoja = VALUES(IdLoja), ResumoMarcacao = VALUES(ResumoMarcacao), EstadoMarcacao = VALUES(EstadoMarcacao), PrioridadeMarcacao = VALUES(PrioridadeMarcacao), MarcacaoStamp = VALUES(MarcacaoStamp), Oficina = VALUES(Oficina), Instalacao = VALUES(Instalacao), TipoEquipamento = VALUES(TipoEquipamento);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();

                j += max;
                //Console.WriteLine("A ler Marcacao: " + j + " de " + LstMarcacao.Count());
            }
        }
        public void CriarAcessos()
        {
            List<Acesso> LstAcesso = new List<Acesso>();
            int max = 1000;
            int j = 0;
            bool entrada = true;

            try
            {
                using Database db_SDP = "server=192.168.102.201;uid=sdp2000;password=sdp2000;port=3358;database=sdp2000;SslMode=none;Connect Timeout=5";
                string sqlQuery = "SELECT * FROM t_snap where exception_type is null order by user_code, create_time;";

                using (var result = db_SDP.Query(sqlQuery))
                {
                    while (result.Read())
                    {
                        if (LstAcesso.Count > 0)
                        {
                            if (LstAcesso.Last().IdUtilizador != result["user_code"]) entrada = true;
                        }

                        LstAcesso.Add(new Acesso()
                        {
                            Id = result["s_id"],
                            IdUtilizador = result["user_code"],
                            Temperatura = result["temperature"],
                            Data = result["create_time"],
                            Tipo = entrada ? "Entrada" : "Saida"
                        });
                        entrada = !entrada;
                    }

                }

                for (int i = 0; j < LstAcesso.Count; i++)
                {
                    if ((j + max) > LstAcesso.Count) max = (LstAcesso.Count - j);

                    string sql = "INSERT INTO dat_acessos (Id,IdUtilizador,DataHoraAcesso,TipoAcesso,Temperatura) VALUES ";

                    foreach (var acesso in LstAcesso.GetRange(j, max))
                    {
                        sql += ("('" + acesso.Id + "', '" + acesso.IdUtilizador + "', '" + acesso.Data.ToString("yy-MM-dd HH:mm:ss") + "', '" + acesso.Tipo + "', '" + acesso.Temperatura + "'), \r\n");
                        i++;
                    }
                    sql = sql.Remove(sql.Count() - 4);

                    sql += " ON DUPLICATE KEY UPDATE IdUtilizador=VALUES(IdUtilizador), DataHoraAcesso = VALUES(DataHoraAcesso), TipoAcesso = VALUES(TipoAcesso), Temperatura = VALUES(Temperatura);";

                    using Database db = ConnectionString;
                    db.Execute(sql);
                    db.Connection.Close();

                    j += max;
                    //Console.WriteLine("A ler Marcacao: " + j + " de " + LstMarcacao.Count());
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void CriarFerias(List<Ferias> LstFerias)
        {
            int max = 1000;
            int j = 0;
            for (int i = 0; j < LstFerias.Count; i++)
            {
                if ((j + max) > LstFerias.Count) max = (LstFerias.Count - j);

                string sql = "INSERT INTO dat_ferias (Id,IdUtilizador,DataInicio,DataFim,Validado,Obs, ValidadoPor) VALUES ";

                foreach (var ferias in LstFerias.GetRange(j, max))
                {
                    sql += ("('" + ferias.Id + "', '" + ferias.IdUtilizador + "', '" + ferias.DataInicio.ToString("yy-MM-dd") + "', '" + ferias.DataFim.ToString("yy-MM-dd") + "', '" + (ferias.Validado ? "1" : "0") + "', '" + ferias.Obs + "', "+ferias.ValidadoPor+"), \r\n");
                    i++;
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE IdUtilizador=VALUES(IdUtilizador), DataInicio = VALUES(DataInicio), DataFim = VALUES(DataFim), Validado = VALUES(Validado), Obs = VALUES(Obs), ValidadoPor= VALUES(ValidadoPor);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();

                j += max;
                //Console.WriteLine("A ler Marcacao: " + j + " de " + LstMarcacao.Count());
            }
        }

        public void CriarFeriasUtilizador(int IdUtilizador, string Ano, int DiasDireito)
        {
            string sqlDelete = "Delete from dat_ferias_utilizador where IdUtilizador = '"+IdUtilizador+"' AND Ano='"+Ano+"';";

            string sqlInsert = "INSERT INTO dat_ferias_utilizador (IdUtilizador,Ano,DiasDireito) VALUES ";
            sqlInsert += ("('" + IdUtilizador + "', '" + Ano + "', '" + DiasDireito + "');");

                Database db = ConnectionString;

            db.Execute(sqlDelete);
            db.Execute(sqlInsert);
                db.Connection.Close();
        }
        public void CriarVisitas(List<Visita> LstVisita)
        {
            int max = 1000;
            int j = 0;
            for (int i = 0; j < LstVisita.Count; i++)
            {
                if ((j + max) > LstVisita.Count) max = (LstVisita.Count - j);

                string sql = "INSERT INTO dat_visitas (IdVisita,DataVisita,IdCliente,IdLoja,ResumoVisita,EstadoVisita,ObsVisita,IdComercial) VALUES ";

                foreach (var visita in LstVisita.GetRange(j, max))
                {
                    if (visita.ObsVisita is null) visita.ObsVisita = String.Empty;
                    sql += ("('" + visita.IdVisita + "', '" + visita.DataVisita.ToString("yy-MM-dd") + "', '" + visita.Cliente.IdCliente + "', '" + visita.Cliente.IdLoja + "', '" + visita.ResumoVisita.Replace("'", "''").Replace("\\", "").ToString() + "', '" + visita.EstadoVisita + "', '" + visita.ObsVisita.Replace("'", "''").Replace("\\", "").ToString() + "', '" + visita.IdComercial + "'), \r\n");
                    i++;
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE DataVisita=VALUES(DataVisita), IdCliente = VALUES(IdCliente), ResumoVisita = VALUES(ResumoVisita), EstadoVisita = VALUES(EstadoVisita), ObsVisita = VALUES(ObsVisita), IdComercial = VALUES(IdComercial);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();

                j += max;
            }
        }
        public void CriarPropostas(List<Proposta> LstPropostas)
        {
            int max = 1000;
            int j = 0;
            for (int i = 0; j < LstPropostas.Count; i++)
            {
                if ((j + max) > LstPropostas.Count) max = (LstPropostas.Count - j);

                string sql = "INSERT INTO dat_propostas (IdProposta,DataProposta,IdComercial,IdVisita,EstadoProposta,ValorProposta,UrlAnexo) VALUES ";

                foreach (var proposta in LstPropostas.GetRange(j, max))
                {
                    sql += ("('" + proposta.IdProposta + "', '" + proposta.DataProposta.ToString("yy-MM-dd") + "', '" + proposta.Comercial.Id + "', '" + proposta.Visita.IdVisita + "', '" + proposta.EstadoProposta.Replace("'", "''").Replace("\\", "").ToString() + "', '" + proposta.ValorProposta + "', '" + proposta.UrlAnexo + "'), \r\n");
                    i++;
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE DataProposta=VALUES(DataProposta), IdComercial = VALUES(IdComercial), EstadoProposta = VALUES(EstadoProposta), ValorProposta = VALUES(ValorProposta), UrlAnexo = VALUES(UrlAnexo);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();

                j += max;
            }
        }
        public void CriarTecnicosMarcacao(List<Utilizador> LstUtilizador)
        {
            int max = 5000;
            int j = 0;
            for (int i = 0; j < LstUtilizador.Count; i++)
            {
                if ((j + max) > LstUtilizador.Count) max = (LstUtilizador.Count - j);

                string sql = "INSERT INTO dat_marcacoes_tecnico (IdMarcacaoTecnico, MarcacaoStamp, IdTecnico, NomeTecnico, Marcado) VALUES ";

                foreach (var tecnico in LstUtilizador.GetRange(j, max))
                {
                    sql += ("('" + tecnico.NomeUtilizador + "', '" + tecnico.IdCartaoTrello + "', '" + tecnico.Id + "', '" + tecnico.NomeCompleto + "', '" + Convert.ToInt32(tecnico.Enable) + "'), \r\n");
                    i++;
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE MarcacaoStamp=VALUES(MarcacaoStamp), IdTecnico = VALUES(IdTecnico), NomeTecnico = VALUES(NomeTecnico), Marcado = VALUES(Marcado);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();

                j += max;
                //Console.WriteLine("A ler Marcacao: " + j + " de " + LstMarcacao.Count());
            }
        }

        public void CriarMarcacaoEstados(List<EstadoMarcacao> LstEstadoMarcacoes)
        {
            int max = 5000;
            int j = 0;
            for (int i = 0; j < LstEstadoMarcacoes.Count; i++)
            {
                if ((j + max) > LstEstadoMarcacoes.Count) max = (LstEstadoMarcacoes.Count - j);

                string sql = "INSERT INTO dat_marcacoes_estado (IdEstado, EstadoMarcacaoDesc) VALUES ";

                foreach (var estado in LstEstadoMarcacoes.GetRange(j, max))
                {
                    sql += ("('" + estado.IdEstado + "', '" + estado.EstadoMarcacaoDesc + "'), \r\n");
                    i++;
                }
                sql = sql.Remove(sql.Count() - 4);

                sql += " ON DUPLICATE KEY UPDATE EstadoMarcacaoDesc=VALUES(EstadoMarcacaoDesc);";

                Database db = ConnectionString;

                db.Execute(sql);
                db.Connection.Close();

                j += max;
                //Console.WriteLine("A ler Marcacao: " + j + " de " + LstMarcacao.Count());
            }
        }


        public void EditarArtigo(Produto produto)
        {

            Database db = ConnectionString;
            String sql = "update dat_produtos set modificado=1, designacao_produto='" + produto.Designacao_Produto + "', stock_fisico=" + produto.Stock_Fisico + ", pos_stock='" + produto.Pos_Stock + "', obs='" + produto.Obs_Produto + "' Where Armazem_Id=" + produto.Armazem_ID + " and ref_produto='" + produto.Ref_Produto + "';";
            db.Execute(sql);
            db.Connection.Close();
        }

        public int NovaFolhaObra(FolhaObra folhaObra)
        {


            folhaObra.IdFolhaObra = folhaObra.IdFolhaObra == 0 ? ObterUltimaEntrada("dat_folhas_obra", "IdFolhaObra") : folhaObra.IdFolhaObra;

            string sql = "INSERT INTO dat_folhas_obra (IdFolhaObra, DataServico, ReferenciaServico, EstadoEquipamento, RelatorioServico, ConferidoPor, SituacoesPendentes, IdCartaoTrello, IdEquipamento, IdCliente, IdLoja, GuiaTransporteAtual, Remoto, RubricaCliente) VALUES ";

            sql += ("('" + folhaObra.IdFolhaObra + "', '" + folhaObra.DataServico.ToString("yy-MM-dd") + "', '" + folhaObra.ReferenciaServico.Replace("'", "''").ToString() + "', '" + folhaObra.EstadoEquipamento + "', '" + folhaObra.RelatorioServico.Replace("'", "''").ToString() + "', '" + folhaObra.ConferidoPor.Replace("'", "''").ToString() + "', '" + folhaObra.SituacoesPendentes.Replace("'", "''").ToString() + "', '" + folhaObra.IdCartao + "', '" + NovoEquipamento(folhaObra.EquipamentoServico) + "', '" + NovoCliente(folhaObra.ClienteServico) + "', '" + folhaObra.ClienteServico.IdLoja + "', '" + folhaObra.GuiaTransporteAtual + "', '" + (folhaObra.AssistenciaRemota ? 1 : 0) + "', '" + folhaObra.RubricaCliente + "') \r\n");

            sql += " ON DUPLICATE KEY UPDATE ReferenciaServico = VALUES(ReferenciaServico), DataServico = VALUES(DataServico), IdLoja = VALUES(IdLoja), EstadoEquipamento = VALUES(EstadoEquipamento), RelatorioServico = VALUES(RelatorioServico), ConferidoPor = VALUES(ConferidoPor), SituacoesPendentes = VALUES(SituacoesPendentes), IdEquipamento = VALUES(IdEquipamento), IdCliente = VALUES(IdCliente), GuiaTransporteAtual = VALUES(GuiaTransporteAtual), Remoto = VALUES(Remoto), RubricaCliente = VALUES(RubricaCliente);";

            using (Database db = ConnectionString)
            {
                db.Execute(sql);
            }
            return folhaObra.IdFolhaObra;
        }
        public int NovoCliente(Cliente cliente)
        {
            Cliente c = new Cliente();

            if (cliente.NomeCliente != null || cliente.NomeCliente == String.Empty) { c = ObterClienteNome(cliente.NomeCliente); }
            if (c.IdCliente == 0)
            {
                cliente.IdCliente = ObterUltimaEntrada("dat_clientes", "IdCliente");
            }
            else
            {
                cliente = c;
            }

            string sql = "INSERT INTO dat_clientes (IdCliente, IdLoja, NomeCliente, PessoaContactoCliente, MoradaCliente, EmailCliente, NumeroContribuinteCliente) VALUES ";

            sql += ("('" + cliente.IdCliente + "','" + cliente.IdLoja + "', '" + cliente.NomeCliente.Replace("'", "''")  + "', '" + cliente.PessoaContatoCliente.Replace("'", "''")  + "', '" + cliente.MoradaCliente.Replace("'", "''")  + "', '" + cliente.EmailCliente.Replace("'", "''")  + "', '" + cliente.NumeroContribuinteCliente.Replace("'", "''")  + "') \r\n");

            sql += " ON DUPLICATE KEY UPDATE PessoaContactoCliente = VALUES(PessoaContactoCliente), MoradaCliente = VALUES(MoradaCliente), EmailCliente = VALUES(EmailCliente), NumeroContribuinteCliente = VALUES(NumeroContribuinteCliente);";

            using (Database db = ConnectionString)
            {
                db.Execute(sql);
            }

            return cliente.IdCliente;

        }
        public int NovoRecibo(Recibo recibo)
        {
            recibo.IdRecibo = ObterReciboFolhaObra(recibo.IdFolhaObra).IdRecibo;
            if (recibo.IdRecibo == 0) recibo.IdRecibo = ObterUltimaEntrada("dat_recibos", "IdRecibo");

            string sql = "INSERT INTO dat_recibos (IdRecibo, MaoObra, MaterialAplicado, Deslocacao, IdFolhaObra) VALUES ";

            sql += ("('" + recibo.IdRecibo + "', '" + recibo.MaoObra + "', '" + recibo.MaterialAplicado + "', '" + recibo.Deslocacao + "', '" + recibo.IdFolhaObra + "') \r\n");

            sql += " ON DUPLICATE KEY UPDATE MaoObra = VALUES(MaoObra), MaterialAplicado = VALUES(MaterialAplicado), Deslocacao = VALUES(Deslocacao);";

            using (Database db = ConnectionString)
            {
                db.Execute(sql);
            }

            return recibo.IdRecibo;

        }
        public string NovoEquipamento(Equipamento equipamento)
        {
            List<Equipamento> LstEquipamentos = new List<Equipamento>();
            LstEquipamentos.Add(equipamento);

            CriarEquipamentos(LstEquipamentos);

            return equipamento.IdEquipamento;
                       
        }
        public int NovaIntervencao(Intervencao intervencao)
        {
            intervencao.IdIntervencao = intervencao.IdIntervencao == 0 ? ObterUltimaEntrada("dat_intervencoes_folha_obra", "IdIntervencao") : intervencao.IdIntervencao;

            string sql = "INSERT INTO dat_intervencoes_folha_obra (IdIntervencao, IdFolhaObra,IdTecnico, NomeTecnico, DataServico, HoraInicio, HoraFim) VALUES ";

            sql += ("('" + intervencao.IdIntervencao + "',  '" + intervencao.IdFolhaObra + "', '" + intervencao.IdTecnico + "', '" + intervencao.NomeTecnico.Replace("'", "''")  + "', '" + intervencao.DataServiço.ToString("yy-MM-dd") + "', '" + intervencao.HoraInicio.ToString("HH:mm") + "', '" + intervencao.HoraFim.ToString("HH:mm") + "') \r\n");

            sql += " ON DUPLICATE KEY UPDATE IdTecnico = VALUES(IdTecnico), NomeTecnico = VALUES(NomeTecnico), DataServico = VALUES(DataServico), HoraInicio = VALUES(HoraInicio), HoraFim = VALUES(HoraFim);";

            using (Database db = ConnectionString)
            {
                db.Execute(sql);
            }

            return intervencao.IdIntervencao;


        }
        public void NovaPecaIntervencao(Produto produto, string IdFolhaObra)
        {
            string sql = "INSERT INTO dat_produto_intervencao (RefProduto, Designacao,Quantidade, IdFolhaObra, TipoUn) VALUES ";

            sql += ("('" + produto.Ref_Produto + "',  '" + produto.Designacao_Produto.Replace("'", "''")  + "', '" + produto.Stock_Fisico + "', '" + IdFolhaObra + "', '"+ produto.TipoUn +"') \r\n");

            sql += " ON DUPLICATE KEY UPDATE Designacao = VALUES(Designacao), Quantidade = VALUES(Quantidade), TipoUn = VALUES(TipoUn);";

            using Database db = ConnectionString;
            db.Execute(sql);

        }

        public void ApagarFolhaObra(int id)
        {
            string sql = "DELETE FROM dat_folhas_obra where IdFolhaObra=" + id + ";";

            using Database db = ConnectionString;
            db.Execute(sql);
        }
        public void ApagarIntervencao(int id)
        {
            string sql = "DELETE FROM dat_intervencoes_folha_obra where IdIntervencao=" + id + ";";

            using Database db = ConnectionString;
            db.Execute(sql);
        }
        public void ApagarPecaFolhaObra(string Ref_Produto, int idFolhaObra)
        {
            string sql = "DELETE FROM dat_produto_intervencao where RefProduto='" + Ref_Produto + "' AND IdFolhaObra = " + idFolhaObra + ";";

            using Database db = ConnectionString;
            db.Execute(sql);
        }

        public void ApagarFerias(int Id)
        {
            string sql = "DELETE FROM dat_ferias where Id="+Id+";";

            using Database db = ConnectionString;
            db.Execute(sql);
        }

        public void ApagarArtigo(Produto produto)
        {
            Database db = ConnectionString;
            String sql = "delete from dat_produtos Where Armazem_Id=" + produto.Armazem_ID + " and ref_produto='" + produto.Ref_Produto + "';";
            db.Execute(sql);
            db.Connection.Close();
        }
        public void ApagarVisita(int id)
        {
            Database db = ConnectionString;
            String sql = "delete from dat_visitas Where IdVisita='" + id + "';";
            db.Execute(sql);
            db.Connection.Close();
        }

        public Bitmap DesenharEtiqueta80x50(Produto produto)
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
                gr.DrawString(produto.Designacao_Produto, fontBody, Brushes.Black, new Rectangle(x, y, width - (x * 2), 35), format);


                y += 40;
                Barcode.Code93 code = new Barcode.Code93
                {
                    DrawText = false
                };

                gr.DrawImage(code.desenharBarcode(produto.Ref_Produto), 10, y, width - (x * 2), 90);
                y += 60;
                gr.DrawString(produto.Ref_Produto, fontHeader, new SolidBrush(Color.Black), new RectangleF(x, y, width - (x * 2), 20), format);

                gr.DrawString("geral@food-tech.pt", fontBody, Brushes.Black, new Rectangle(x, height - 20, width - (x * 2), 20), format);

            }

            return bm;
        }
        public Bitmap DesenharEtiqueta80x50QR(Produto produto)
        {

            int x = 0;
            int y = 0;
            int width = 1024;
            int height = 641;

            Bitmap bm = new Bitmap(width, height);

            Font fontHeader = new Font("Rubik", 70, FontStyle.Bold);
            Font fontBody = new Font("Tahoma", 40, FontStyle.Regular);
            Font fontFooter = new Font("Rubik", 22, FontStyle.Regular);

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

            Font fontHeader = new Font("Rubik", 40, FontStyle.Bold);
            Font fontBody = new Font("Tahoma", 34, FontStyle.Bold);
            Font fontFooter = new Font("Rubik", 16, FontStyle.Regular);

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

                gr.DrawString(produto.Designacao_Produto, fontBody, Brushes.Black, new Rectangle(x, y, width - (x * 2), 200), format);

                y += 200;
                gr.DrawString(produto.Ref_Produto, fontHeader, new SolidBrush(Color.Black), new RectangleF(x + 220, y, width - (x * 2) - 420, 80), format);

                y += 70;

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(produto.Ref_Produto, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                gr.DrawImage(qrCodeImage, width - 210, height - 150, 150, 150);

                gr.DrawString("geral@food-tech.pt", fontFooter, Brushes.Black, new Rectangle(x, y, width - (x * 2), 30), format);

            }

            return bm;
        }
        public Bitmap DesenharEtiquetaMarcacao(Marcacao marcacao)
        {

            int x = 0;
            int y = 0;
            int width = 1024;
            int height = 641;

            Bitmap bm = new Bitmap(width, height);

            Font fontHeader = new Font("Rubik", 70, FontStyle.Bold);
            Font fontBody = new Font("Tahoma", 40, FontStyle.Regular);
            Font fontFooter = new Font("Rubik", 22, FontStyle.Regular);

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

                gr.DrawString(marcacao.Cliente.NomeCliente + "\r\n(" + marcacao.EstadoMarcacaoDesc + ")", fontBody, Brushes.Black, new Rectangle(x, y, width - (x * 2), 200), format);

                y += 250;
                gr.DrawString("Marc. Nº " + marcacao.IdMarcacao.ToString(), fontHeader, new SolidBrush(Color.Black), new RectangleF(x, y, width - (x * 2) - 200, 80), format);

                y += 95;

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(marcacao.IdMarcacao.ToString(), QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                gr.DrawImage(qrCodeImage, width - 220, height - 220, 200, 200);

                gr.DrawString("geral@food-tech.pt", fontFooter, Brushes.Black, new Rectangle(x, y, width - (x * 2) - 200, 30), format);

            }

            return bm;
        }
        public Bitmap DesenharEtiquetaFolhaObra(FolhaObra fo)
        {

            int x = 0;
            int y = 0;
            int width = 1024;
            int height = 641;

            Bitmap bm = new Bitmap(width, height);

            Font fontHeader = new Font("Rubik", 70, FontStyle.Bold);
            Font fontBody = new Font("Tahoma", 22, FontStyle.Regular);
            Font fontFooter = new Font("Rubik", 22, FontStyle.Regular);

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

                gr.DrawString(fo.ClienteServico.NomeCliente + " (N/S: " + fo.EquipamentoServico.NumeroSerieEquipamento + ")\r\n " + fo.RelatorioServico, fontBody, Brushes.Black, new Rectangle(x, y, width - (x * 2), 200), format);

                y += 250;
                gr.DrawString("FO Nº " + fo.IdFolhaObra.ToString(), fontHeader, new SolidBrush(Color.Black), new RectangleF(x, y, width - (x * 2) - 200, 80), format);

                y += 95;

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(fo.IdFolhaObra.ToString(), QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                gr.DrawImage(qrCodeImage, width - 220, height - 220, 200, 200);

                gr.DrawString("geral@food-tech.pt", fontFooter, Brushes.Black, new Rectangle(x, y, width - (x * 2) - 200, 30), format);

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

            //if (folhaobra.RubricaCliente != null)
            //{
            //    var fldPosition = pdfFormFields.GetFieldPositions("assinatura");
            //    Rectangle rectangle = new Rectangle((int)fldPosition[1], (int)fldPosition[2] - 7, (int)fldPosition[3], 30);
            //    folhaobra.RubricaCliente = folhaobra.RubricaCliente.Replace("data:image/png;base64,", "").Trim();

            //    if ((folhaobra.RubricaCliente.Length % 4 == 0) && Regex.IsMatch(folhaobra.RubricaCliente, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
            //    {
            //        byte[] imageBytes = Convert.FromBase64String(folhaobra.RubricaCliente);

            //        iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageBytes);

            //        image.ScaleToFit(rectangle.Width, rectangle.Height);
            //        image.SetAbsolutePosition(rectangle.Left + 2, rectangle.Top - 2);

            //        pdfStamper.GetOverContent((int)fldPosition[0]).AddImage(image);

            //    }
            //}

            pdfFormFields.SetField("IdFolhaObra", folhaobra.IdFolhaObra.ToString());
            pdfFormFields.SetField("Marcação", "");
            pdfFormFields.SetField("Registado", "");
            if (folhaobra.AssistenciaRemota)
            {
                pdfFormFields.SetFieldProperty("Remoto", "textsize", 26f, null);
                pdfFormFields.SetFieldProperty("Remoto", "textcolor", iTextSharp.text.BaseColor.Red, null);
                pdfFormFields.SetField("Remoto", "REMOTO");
            }
            else if ((folhaobra.DataServico.DayOfWeek == DayOfWeek.Sunday) || (folhaobra.DataServico.DayOfWeek == DayOfWeek.Saturday))
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
            if (folhaobra.IntervencaosServico.Count > 1)
            {
                foreach (var intervencao in folhaobra.IntervencaosServico)
                {
                    folhaobra.RelatorioServico += intervencao.DataServiço.ToShortDateString() + " - " + intervencao.HoraInicio.ToShortTimeString() + " -> " + intervencao.HoraFim.ToShortTimeString() + ": " + intervencao.RelatorioServico + " ";
                }
            }
            else if (folhaobra.IntervencaosServico.Count > 0)
            {
                folhaobra.RelatorioServico = folhaobra.IntervencaosServico.FirstOrDefault().RelatorioServico;
            }
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

            if (folhaobra.PecasServico.Count > 0)
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
                        pdfFormFields.SetField("DesignaçãoRow" + p, pecas.Designacao_Produto.Substring(0, 50));
                    }
                    else
                    {
                        pdfFormFields.SetField("DesignaçãoRow" + p, pecas.Designacao_Produto);
                    }
                    pdfFormFields.SetFieldProperty("QuantRow" + p, "textsize", 6f, null);
                    pdfFormFields.SetField("QuantRow" + p, pecas.Stock_Fisico.ToString() + " " + pecas.TipoUn.ToString()); ;
                    p++;
                    if (p == 10) break;
                }
            }
            if (folhaobra.ConferidoPor == string.Empty) folhaobra.ConferidoPor = folhaobra.ClienteServico.PessoaContatoCliente;
            pdfFormFields.SetField("o cliente", folhaobra.ConferidoPor);

            if (folhaobra.Recibo != null)
            {
                double ValorFinal = Math.Round(folhaobra.Recibo.TotalRecibo, 2);
                if (ValorFinal > 0)
                {
                    pdfFormFields.SetField("Material", folhaobra.Recibo.MaterialAplicado.ToString() + " €");
                    pdfFormFields.SetField("mao-de-obra", folhaobra.Recibo.MaoObra.ToString() + " €");
                    pdfFormFields.SetField("deslocacoes", folhaobra.Recibo.Deslocacao.ToString() + " €");
                    pdfFormFields.SetField("subtotal", folhaobra.Recibo.SubTotalRecibo.ToString() + " €");
                    pdfFormFields.SetField("IVA", folhaobra.Recibo.IvaRecibo.ToString() + " €");
                    pdfFormFields.SetField("Total a pagar", ValorFinal.ToString().Split('.')[0].PadLeft(4, ' '));
                    pdfFormFields.SetField("undefined_3", ValorFinal.ToString().Split('.').Count() > 1 ? ValorFinal.ToString().Split('.')[1].PadRight(2, '0') : "00");
                    pdfFormFields.SetFieldProperty("recebiquantia", "textsize", 5f, null);
                    pdfFormFields.SetField("recebiquantia", folhaobra.Recibo.ConverterValorPalavras());
                    pdfFormFields.SetField("reciboprovi", folhaobra.Recibo.IdRecibo.ToString());
                    pdfFormFields.SetField("docliente", folhaobra.ClienteServico.NomeCliente);
                    pdfFormFields.SetField("contribuinte", folhaobra.ClienteServico.NumeroContribuinteCliente);
                    pdfFormFields.SetField("Referente à ATN", folhaobra.IdFolhaObra.ToString().PadLeft(5, ' '));
                    pdfFormFields.SetField("Data Pedido", folhaobra.DataServico.ToString("dd/MM/yyyy"));
                    pdfFormFields.SetField("responsaveltecnico", folhaobra.IntervencaosServico.Count() > 0 ? folhaobra.IntervencaosServico.Last().NomeTecnico : "");
                    pdfFormFields.SetField("Total recebido", ValorFinal.ToString().Split('.')[0].PadLeft(4, ' '));
                    pdfFormFields.SetField("undefined_4", ValorFinal.ToString().Split('.').Count() > 1 ? ValorFinal.ToString().Split('.')[1].PadRight(2, '0') : "00");
                }
            }
            pdfStamper.FormFlattening = true;
            pdfStamper.SetFullCompression();
            pdfStamper.Close();

            return outputPdfStream;

        }
        public MemoryStream AssinarDocumento(string nomecliente, string nometecnico, string tipodocumento, bool manualentregue, byte[] documento)
        {
            var outputPdfStream = new MemoryStream();
            PdfReader pdfReader = new PdfReader(documento);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, outputPdfStream) { FormFlattening = true, FreeTextFlattening = true };
            PdfContentByte canvas;

            switch (tipodocumento)
            {
                case "0":
                    for (int i = 3; i < pdfReader.NumberOfPages + 1; i++)
                    {
                        canvas = pdfStamper.GetOverContent(i);
                        ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_RIGHT, new iTextSharp.text.Phrase(nomecliente, new iTextSharp.text.Font(iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD))), pdfReader.GetPageSize(i).Width - 30, 350, 0);
                    }
                    break;
                case "1":
                    for (int i = 1; i < pdfReader.NumberOfPages + 1; i++)
                    {
                        canvas = pdfStamper.GetOverContent(i);
                        ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_LEFT, new iTextSharp.text.Phrase(nomecliente, new iTextSharp.text.Font(iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD))), pdfReader.GetPageSize(i).Width - 170, 160, 0);
                        ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_LEFT, new iTextSharp.text.Phrase(nometecnico, new iTextSharp.text.Font(iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD))), pdfReader.GetPageSize(i).Width - 360, 160, 0);

                        if (manualentregue)
                        {
                            ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_LEFT, new iTextSharp.text.Phrase("X", new iTextSharp.text.Font(iTextSharp.text.FontFactory.GetFont("Arial", 20, iTextSharp.text.Font.BOLD))), 63, 157, 0);
                        }
                        else
                        {
                            ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_LEFT, new iTextSharp.text.Phrase("X", new iTextSharp.text.Font(iTextSharp.text.FontFactory.GetFont("Arial", 20, iTextSharp.text.Font.BOLD))), 122, 157, 0);
                        }
                    }
                    break;
            }

            pdfStamper.FormFlattening = true;
            pdfStamper.SetFullCompression();
            pdfStamper.Close();

            return outputPdfStream;
        }

        public List<ControloViatura> ObterViaturas()
        {
            List<ControloViatura> lstViaturas = new List<ControloViatura>();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM sys_viaturas;");
                while (result.Read())
                {
                    lstViaturas.Add(ObterViatura(result["matricula_viatura"]));
                }
            }

            return lstViaturas;
        }
        public ControloViatura ObterViatura(string Matricula)
        {
            ControloViatura Viatura = new ControloViatura();

            using (Database db = ConnectionString)
            {

                using var result = db.Query("SELECT * FROM dat_controlo_viatura where matricula_viatura = '"+Matricula+"' order by data_inicio DESC LIMIT 1;");
                while (result.Read())
                {
                    Viatura = (new ControloViatura()
                    {
                        MatriculaViatura = result["matricula_viatura"],
                        KmsViatura = result["kms_viatura"],
                        KmsFinais = result["kms_finais"],
                        Id = result["id_reg"],
                        Nome_Tecnico = result["nome_tecnico"],
                        DataInicio = result["data_inicio"],
                        DataFim = result["data_fim"],
                        Notas = result["notas_viatura"]
                    });
                }
               
             }
            return Viatura;
        }
        public void LevantamentoViatura(ControloViatura viatura)
        {
            string sql = "INSERT INTO dat_controlo_viatura (nome_tecnico, matricula_viatura, kms_viatura, data_inicio, data_fim, notas_viatura, devolvida_viatura) VALUES ";

            sql += ("('" + viatura.Nome_Tecnico + "',  '" + viatura.MatriculaViatura + "', '" + viatura.KmsViatura + "', '" + viatura.DataInicio.ToString("yyyy-MM-dd HH:mm:ss") + "','0001-01-01 00:00:00', '" + viatura.Notas + "', 0);");

            using Database db = ConnectionString;
            db.Execute(sql);
        }
        public void DevolverViatura(ControloViatura viatura)
        {
            string sql = "UPDATE dat_controlo_viatura SET data_fim = '"+ viatura.DataFim.ToString("yyyy-MM-dd HH:mm:ss") + "', kms_finais= '"+viatura.KmsFinais+"', devolvida_viatura=1 WHERE matricula_viatura='"+viatura.MatriculaViatura+"' AND devolvida_viatura=0;";

            using Database db = ConnectionString;
            db.Execute(sql);
        }

    }
}
