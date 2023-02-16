using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Dossier
    {
        public enum TipoDossier
        {
            Pecas, Orcamento, Transferencia

        }
        public string StampDossier { get; set; }
        public int Serie { get; set; }
        public string SerieNome { get { return this.Serie == 96 ? "Pedido de Peças" : this.Serie == 97 ? "Pedido de Orçamento" : this.Serie == 36 ? "Pedido de Transferência" : "N/D"; } }
        public string NomeDossier { get; set; }
        public int IdDossier { get; set; }
        public DateTime DataDossier { get; set; }
        public Cliente Cliente { get; set; }
        public string Referencia { get; set; }
        public Utilizador Tecnico { get; set; }
        public FolhaObra FolhaObra { get; set; }
        public Marcacao Marcacao { get; set; }
        public string Estado { get; set; }
        public string Obs { get; set; }
        public DateTime DataCriacao { get; set; }
        public string EditadoPor { get; set; }
        public List<Linha_Dossier> Linhas { get; set; }
        public bool Fechado { get; set; }

        public void DefinirSerie(TipoDossier tp)
        {
            if (tp == TipoDossier.Pecas) this.Serie = 96;
            if (tp == TipoDossier.Orcamento) this.Serie = 97;
            if (tp == TipoDossier.Transferencia) this.Serie = 36;
        }
    }
    public class Linha_Dossier
    {
        public string Stamp_Dossier { get; set; }
        public string Stamp_Linha { get; set; }
        public string Referencia { get; set; }
        public string Designacao { get; set; }
        public double Quantidade { get; set; }
        public string CriadoPor { get; set; }
    }
}