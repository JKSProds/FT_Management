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
        [Display(Name = "Data")]
        public DateTime DataServico { get; set; }
        private string _ReferenciaServico;
        [Display(Name = "Referência")]
        public string ReferenciaServico { get {return _ReferenciaServico ?? ""; } set {_ReferenciaServico = value ;} }
        [Display(Name = "Estado do Equipamento")]
        public string EstadoEquipamento { get; set; }
        private string _RelatorioServico;
        [DataType(DataType.MultilineText)]
        [Display(Name = "Relatório do Serviço")]
        public string RelatorioServico { get {return _RelatorioServico ?? ""; } set {_RelatorioServico = value ;}  }
        private string _SituacoesPendentes;
        [DataType(DataType.MultilineText)]
        [Display(Name = "Situações Pendentes")]
        public string SituacoesPendentes { get {return _SituacoesPendentes ?? ""; } set {_SituacoesPendentes = value ;} }
        public List<Produto> PecasServico { get; set; }
        public List<Intervencao> IntervencaosServico { get; set; }
        public Equipamento EquipamentoServico { get; set; }
        public Cliente ClienteServico { get; set; }
        public string IdCartao { get; set; }
        private string _ConferidoPor;
        [Display(Name = "Conferido por")]
        public string ConferidoPor { get {return _ConferidoPor ?? ""; } set {_ConferidoPor = value ;} }
        private string _GuiaTransporteAtual; 
        [Display(Name = "Número da tua Guia de Transporte")]
        public string GuiaTransporteAtual { get {return _GuiaTransporteAtual ?? ""; } set {_GuiaTransporteAtual = value ;} }
        [Display(Name = "Assistência Remota?")]
        public bool AssistenciaRemota { get; set; }
        [Display(Name = "Rúbrica")]
        public string RubricaCliente { get; set; }

        public FolhaObra()
        {
            this.IntervencaosServico = new List<Intervencao>()
            {
                new Intervencao()
                {
                    NomeTecnico = "N/D"
                }
            };
        }
    }
}
