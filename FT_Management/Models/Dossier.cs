using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FT_Management.Models
{
    public class Dossier
    {
        [Display(Name = "Núm. do Dossier")]
        public int IdDossier { get; set; }
        [Display(Name = "Tipo")]
        public int TipoDossier { get; set; }
        [Display(Name = "Dossier")]
        public string NomeDossier { get; set; }
        [Display(Name = "Criado por")]
        public string CriadoPor { get; set; }
        [Display(Name = "Data")]
        public DateTime DataDossier { get; set; }
    }
}
