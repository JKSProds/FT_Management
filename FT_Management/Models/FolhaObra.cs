namespace FT_Management.Models
{
    public class FolhaObra
    {
        public string EmojiFO { get { return (!string.IsNullOrEmpty(this.StampFO) && this.StampFO.Contains("WEBAPP") ? "⭐ " : "💩 ") + (this.Contrato ? "🤝 " : ""); } }
        public string StampFO { get; set; }
        public string StampPA { get; set; }
        [Display(Name = "Num. da Folha de Obra")]
        public int IdFolhaObra { get; set; }
        [Display(Name = "Num. da Assistência Técnica")]
        public string IdAT { get; set; }
        [Display(Name = "Num. da Marcação")]
        public int IdMarcacao { get; set; }
        public Marcacao Marcacao { get; set; }
        [Display(Name = "Data")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime DataServico { get; set; }
        private string _ReferenciaServico;
        [Display(Name = "Referência")]
        [Required]
        public string ReferenciaServico { get { return _ReferenciaServico ?? ""; } set { _ReferenciaServico = value; } }
        [Display(Name = "Estado do Equipamento")]
        public string EstadoEquipamento { get; set; }
        private string _RelatorioServico;
        [DataType(DataType.MultilineText)]
        [Display(Name = "Relatório do Serviço")]
        [Required(ErrorMessage = "Falta preencher o relatório do serviço!")]
        public string RelatorioServico { get { return _RelatorioServico ?? ""; } set { _RelatorioServico = value; } }
        private string _SituacoesPendentes;
        [DataType(DataType.MultilineText)]
        [Display(Name = "Observações Internas")]
        public string SituacoesPendentes { get { return _SituacoesPendentes ?? ""; } set { _SituacoesPendentes = value; } }
        [Display(Name = "Lista de Peças")]
        public string ListaPecas { get; set; }
        [Display(Name = "Peças")]
        public List<Produto> PecasServico { get; set; }
        [Display(Name = "Lista de Periodos")]
        [Required(ErrorMessage = "Tem de adicionar pelo menos 1 intervenção!")]
        public string ListaIntervencoes { get; set; }
        public List<Intervencao> IntervencaosServico { get; set; }
        [Required(ErrorMessage = "Falta selecionar o equipamento!")]
        [Display(Name = "Equipamento")]
        public Equipamento EquipamentoServico { get; set; }
        [Required(ErrorMessage = "Falta selecionar o cliente!")]
        public Cliente ClienteServico { get; set; }
        public Utilizador Utilizador { get; set; }
        public string IdCartao { get; set; }
        private string _ConferidoPor;
        [Display(Name = "Nome | Número")]
        [Required(ErrorMessage = "Falta preencher o campo Conferido por!")]
        public string ConferidoPor { get { return _ConferidoPor ?? ""; } set { _ConferidoPor = value; } }
        private string _GuiaTransporteAtual;
        [Display(Name = "Número da tua Guia de Transporte")]
        public string GuiaTransporteAtual { get { return _GuiaTransporteAtual ?? ""; } set { _GuiaTransporteAtual = value; } }
        [Display(Name = "Assistência Remota?")]
        public bool AssistenciaRemota { get; set; }
        [Display(Name = "Rúbrica")]
        public string RubricaCliente { get; set; }
        public bool Piquete { get; set; }
        public string GetUrl { get { return "http://webapp.food-tech.pt/FolhasObra/FolhaObra/" + IdFolhaObra; } }
        public string GetUrlAT { get { return "http://webapp.food-tech.pt/Dossiers/Dossier/" + IdAT + "?ecra=BO&anexar=1"; } }
        [Display(Name = "Estado do Serviço")]
        public int EstadoFolhaObra { get; set; }
        [Display(Name = "Tipo de Serviço")]
        public string TipoFolhaObra { get; set; }
        [Display(Name = "Garantia?")]
        public bool EmGarantia { get; set; }
        [Display(Name = "Encaminhar Pedido")]
        public bool Avisar { get; set; }
        [Display(Name = "Recolher para Oficina")]
        public bool RecolhaOficina { get; set; }
        public bool Oficina { get { return TipoFolhaObra == "Interno"; } }
        public bool CobrarDeslocacao { get; set; }
        public bool Instalação { get; set; }
        public bool EnviarEmail { get; set; }
        [Display(Name = "Email")]
        public string EmailCliente { get; set; }
        public bool GuardarLocalizacao { get; set; }
        public bool FecharMarcacao { get; set; }
        public double ValorTotal { get { return PecasServico.Sum(p => p.Valor * p.Stock_Fisico); } }
        public double KmsDeslocacao { get; set; }
        public string FicheirosAnexo { get; set; }
        public bool Guia { get; set; }
        [Display(Name = "Contrato?")]
        public bool Contrato { get; set; }
        [Display(Name = "Motivo - Fora de Contrato")]
        public string JustExtraContrato { get; set; }
        public string CheckList { get; set; }
        public bool EnviarEmailGuias { get; set; }
        public bool CategoriaResolucao { get { return (this.ClienteServico.IdCliente == 561 || this.ClienteServico.IdCliente==1568) && (Marcacao == null) ? false : Marcacao.TipoEquipamento == "Pesagem";} }
        public CategoriaResolucao CatResolucao { get; set; }

        public FolhaObra()
        {
            ListaIntervencoes = "";
            ListaPecas = "";
            EstadoEquipamento = "";
            EquipamentoServico = new Equipamento();
            ClienteServico = new Cliente() { NomeCliente = "N/D" };
            Utilizador = new Utilizador();
            this.PecasServico = new List<Produto>();
            this.CatResolucao = new CategoriaResolucao();
            this.IntervencaosServico = new List<Intervencao>()
            {
                new Intervencao()
                {
                    NomeTecnico = "N/D",
                    HoraInicio = DateTime.Parse(DateTime.Now.AddHours(-1).Hour.ToString() + ":00"),
                    HoraFim = DateTime.Parse(DateTime.Now.Hour.ToString() + ":00")
                }
            };
        }

        public FolhaObra PreencherDadosMarcacao(Marcacao m)
        {
            this.ClienteServico = m.Cliente;
            this.DataServico = DateTime.Now;
            if (m.Oficina) this.TipoFolhaObra = "Interno";
            if (m.Remoto) this.TipoFolhaObra = "Remoto";
            if (m.TipoServico == "Instalação") this.TipoFolhaObra = "Instalação";
            this.ReferenciaServico = m.Referencia;
            this.IdMarcacao = m.IdMarcacao;
            this.EmailCliente = string.IsNullOrEmpty(m.Cliente.EmailCliente) && (this.ClienteServico.IdCliente != 878 || m.Cliente.IdCliente == 297) ? m.QuemPediuEmail : m.Cliente.EmailCliente;
            this.Piquete = m.Piquete;
            if (!m.Oficina && !m.Remoto) this.TipoFolhaObra = m.TipoServico;
            this.Contrato = m.Contrato;
            this.Instalação = m.TipoServico == "Instalação";
            this.CobrarDeslocacao = this.Instalação;
            return this;
        }

        public void PreencherViagem(Viagem v)
        {
            this.IntervencaosServico.First().HoraInicio = v.Fim_Viagem;
            this.IntervencaosServico.First().HoraFim = DateTime.Now;
            this.KmsDeslocacao = double.Parse(v.Distancia_Viagem);
        }

        public void ValidarIntervencoes()
        {
            if (string.IsNullOrEmpty(this.FicheirosAnexo)) FicheirosAnexo = "";
            if (this.ListaIntervencoes == null)
            {
                ListaIntervencoes = "";
                return;
            }

            this.IntervencaosServico.Clear();
            foreach (var item in this.ListaIntervencoes.Split(";"))
            {
                if (item != "")
                {
                    foreach(var t in this.Marcacao.LstTecnicos) {
                        this.IntervencaosServico.Add(new Intervencao
                            {
                                DataServiço = DateTime.Parse(item.Split(" ").First()),
                                HoraInicio = DateTime.Parse(item.Split(" ").Last().Split("|").First()),
                                HoraFim = DateTime.Parse(item.Split(" ").Last().Split("|").Last()),
                                RelatorioServico = this.RelatorioServico,
                                IdTecnico = t.IdPHC,
                                NomeTecnico = t.NomeCompleto,
                                IdFolhaObra = this.IdFolhaObra
                            });
                    }
                    
                }
            }
            this.DataServico = DateTime.Now;
        }
        public void ValidarTipoFolhaObra()
        {
            if (this.TipoFolhaObra == "Interno")
            {
                this.CobrarDeslocacao = false;
            }
            else if (this.TipoFolhaObra == "Externo")
            {
                this.CobrarDeslocacao = true;
            }
            else if (this.TipoFolhaObra == "Remoto")
            {
                this.CobrarDeslocacao = false;
                this.AssistenciaRemota = true;
            }
            else if (this.TipoFolhaObra == "Instalação")
            {
                this.CobrarDeslocacao = false;
                this.Instalação = true;
            }
        }
        public void ValidarPecas(List<Produto> LstPecas)
        {
            this.PecasServico.Clear();
            if (this.ListaPecas == null) return;
            foreach (var item in this.ListaPecas.Split(";"))
            {
                if (item != "" && LstPecas.Where(p => p.StampProduto == item.Split("|").First()).Count() > 0)
                {
                    Produto p = LstPecas.Where(p => p.StampProduto == item.Split("|").First()).First();
                    this.PecasServico.Add(new Produto() { StampProduto = p.StampProduto, Ref_Produto = p.Ref_Produto, Designacao_Produto = p.Designacao_Produto });
                    this.PecasServico.Last().Stock_Fisico = Double.Parse(item.Split("|")[1]);
                    this.PecasServico.Last().Garantia = item.Split("|")[2] == "true";
                    this.PecasServico.Last().MotivoGarantia = item.Split("|")[3];
                    this.PecasServico.Last().ObsGarantia = item.Split("|")[4];
                }
            }
        }
    }

    public class CategoriaResolucao {
        public int Id_1 { get; set; }
        public int Id_2 { get; set; }
        [Required(ErrorMessage = "Falta selecionar a categoria de resolução!")]
        [Range(1, int.MaxValue, ErrorMessage = "Falta selecionar a categoria de resolução!")]
        public int Id_3 { get; set; }
        [Display(Name = "Categoria 1")]
        public string Categoria1 { get; set;}
        [Display(Name = "Categoria 2")]
        public string Categoria2 { get; set;}
        [Display(Name = "Categoria 3")]
        public string Categoria3 { get; set;}
        public string ExemploRelatorio { get; set;}

        public CategoriaResolucao() {
            Categoria1 = "Generic";
            Categoria2 = "Causa não identificada";
        }
    }
}

