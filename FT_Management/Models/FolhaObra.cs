using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class FolhaObra
    {
        [Display(Name = "Num. da Folha de Obra")]
        public int IdFolhaObra { get; set; }
        [Display(Name = "Num. da Assistência Técnica")]
        public string IdAT { get; set; }
        [Display(Name = "Num. da Marcação")]
        public int IdMarcacao { get; set; }
        [Display(Name = "Data")]
        [Required]
        public DateTime DataServico { get; set; }
        private string _ReferenciaServico;
        [Display(Name = "Referência")]
        [Required]
        public string ReferenciaServico { get {return _ReferenciaServico ?? ""; } set {_ReferenciaServico = value ;} }
        [Display(Name = "Estado do Equipamento")]
        public string EstadoEquipamento { get; set; }
        private string _RelatorioServico;
        [DataType(DataType.MultilineText)]
        [Display(Name = "Relatório do Serviço")]
        [Required]
        public string RelatorioServico { get {return _RelatorioServico ?? ""; } set {_RelatorioServico = value ;}  }
        private string _SituacoesPendentes;
        [DataType(DataType.MultilineText)]
        [Display(Name = "Observações Internas")]
        public string SituacoesPendentes { get {return _SituacoesPendentes ?? ""; } set {_SituacoesPendentes = value ;} }
        [Display(Name = "Lista de Peças")]
        public string ListaPecas { get; set; }
        public List<Produto> PecasServico { get; set; }
        [Display(Name = "Lista de Intervenções")]
        [Required]
        public string ListaIntervencoes { get; set; }
        public List<Intervencao> IntervencaosServico { get; set; }
        [Required]
        public Equipamento EquipamentoServico { get; set; }
        [Required]
        public Cliente ClienteServico { get; set; }
        public Utilizador Utilizador { get; set; }
        public string IdCartao { get; set; }
        private string _ConferidoPor;
        [Display(Name = "Conferido por")]
        [Required]
        public string ConferidoPor { get {return _ConferidoPor ?? ""; } set {_ConferidoPor = value ;} }
        private string _GuiaTransporteAtual; 
        [Display(Name = "Número da tua Guia de Transporte")]
        public string GuiaTransporteAtual { get {return _GuiaTransporteAtual ?? ""; } set {_GuiaTransporteAtual = value ;} }
        [Display(Name = "Assistência Remota?")]
        public bool AssistenciaRemota { get; set; }
        [Display(Name = "Rúbrica")]
        public string RubricaCliente { get; set; }
        public string GetUrl { get { return "http://webapp.food-tech.pt/FolhasObra/Detalhes/" + IdFolhaObra; } }
        [Display(Name = "Estado do Serviço")]
        public string EstadoFolhaObra { get; set; }
        [Display(Name = "Tipo de Serviço")]
        public string TipoFolhaObra { get; set; }
        [Display(Name = "Em Garantia")]
        public bool EmGarantia { get; set; }
        [Display(Name = "Avisar Cliente")]
        public bool Avisar { get; set; }
        [Display(Name = "Recolher para Oficina")]
        public bool RecolhaOficina { get; set; }
        public bool CobrarDeslocacao { get; set; }
        public bool EnviarEmail { get; set; }
        public string EmailCliente { get; set; }
        public bool GuardarLocalizacao { get; set; }
        public bool FecharMarcacao { get; set; }


        public FolhaObra()
        {
            ListaIntervencoes = "";
            ListaPecas = "";
            EquipamentoServico = new Equipamento();
            this.PecasServico = new List<Produto>();
            this.IntervencaosServico = new List<Intervencao>()
            {
                new Intervencao()
                {
                    NomeTecnico = "N/D",
                    HoraInicio = DateTime.Now.AddMinutes(-60),
                    HoraFim = DateTime.Now
                }
            };
        }

        public FolhaObra PreencherDadosMarcacao(Marcacao m)
        {
            this.ClienteServico = m.Cliente;
            this.DataServico = m.DataMarcacao;
            this.ReferenciaServico = m.Referencia;
            this.IdMarcacao = m.IdMarcacao;
            this.EmailCliente = m.Cliente.EmailCliente;

            return this;
        }
        public void ValidarIntervencoes()
        {
            this.IntervencaosServico.Clear();
            foreach (var item in this.ListaIntervencoes.Split(";"))
            {
                if (item != "")
                {
                    this.IntervencaosServico.Add(new Intervencao
                    {
                        HoraInicio = DateTime.Parse(item.Split("|").First()),
                        HoraFim = DateTime.Parse(item.Split("|").Last()),
                        DataServiço = this.DataServico
                    });
                }
            }
        }
        public void ValidarPecas()
        {
            this.PecasServico.Clear();
            foreach (var item in this.ListaPecas.Split(";"))
            {
                if (item != "")
                {
                    this.PecasServico.Add(new Produto
                    {
                        Ref_Produto = item
                    });
                }
            }
        }
    }
}
