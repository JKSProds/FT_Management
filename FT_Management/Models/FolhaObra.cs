using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class FolhaObra
    {
        public int IdFolhaObra { get; set; }
        [Display(Name = "Data")]
        public DateTime DataServico { get; set; }
        [Display(Name = "Referência")]
        public string ReferenciaServico { get; set; }
        [Display(Name = "Estado do Equipamento")]
        public string EstadoEquipamento { get; set; }
        [DataType(DataType.MultilineText)]
        [Display(Name = "Relatório do Serviço")]
        public string RelatorioServico { get; set; }
        [DataType(DataType.MultilineText)]
        [Display(Name = "Situações Pendentes")]
        public string SituacoesPendentes { get; set; }
        public List<Produto> PecasServico { get; set; }
        public List<Intervencao> IntervencaosServico { get; set; }
        public Equipamento EquipamentoServico { get; set; }
        public Cliente ClienteServico { get; set; }
    }
}
