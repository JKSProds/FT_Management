using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class FolhaObra
    {
        public int IdFolhaObra { get; set; }
        public DateTime DataServico { get; set; }
        public string ReferenciaServico { get; set; }
        public string EstadoEquipamento { get; set; }
        public string RelatorioServico { get; set; }
        public string SituacoesPendentes { get; set; }
        public List<Produto> PecasServico { get; set; }
        public List<Intervencao> IntervencaosServico { get; set; }
        public Equipamento EquipamentoServico { get; set; }
        public Cliente ClienteServico { get; set; }
    }
}
