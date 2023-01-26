using System;
using System.ComponentModel.DataAnnotations;

namespace FT_Management.Models
{
    public class Viatura
    {
        [Display(Name = "Matricula Viatura")]
        public string Matricula { get; set; }
        [Display(Name = "Morada")]
        public string LocalizacaoMorada { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        [Display(Name = "Kms Totais")]
        public string KmsAtuais { get; set; }
        [Display(Name = "Ignição")]
        public bool Ignicao { get; set; }
        [Display(Name = "Buzzer")]
        public bool Buzzer { get; set; }
        [Display(Name = "Ultimo Evento")]
        public DateTime UltimoUpdate { get; set; }
        public Utilizador Utilizador { get; set; }
    }
}
