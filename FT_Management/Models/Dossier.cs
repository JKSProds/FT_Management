using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Dossier
    {
        public string StampDossier { get; set; }
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
    }
    public class Linha_Dossier
    {
        public string Referencia { get; set; }
        public string Designacao { get; set; }
        public double Quantidade { get; set; }
    }
}