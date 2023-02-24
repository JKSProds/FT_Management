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
        public string LocalizacaoCidade
        {
            get
            {
                return this.LocalizacaoMorada.Split(",").Length > 1 ? this.LocalizacaoMorada.Split(",")[this.LocalizacaoMorada.Split(",").Length - 2].Trim() : "";
            }
        }
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
        public int Velocidade { get; set; }
        public int Combustivel { get; set; }
        public Utilizador Utilizador { get; set; }
        public string GetUrl { get { return "http://www.google.com/maps/search/?api=1&query=" + this.Latitude + "," + this.Longitude; } }
    }
}
