using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FT_Management.Models
{
    public class Visita
    {
        [Display(Name = "Num. da Visita")]
        public int IdVisita { get; set; }
        [Display(Name = "Data")]
        public DateTime DataVisita { get; set; }
        [Display(Name = "Cliente")]
        public Cliente Cliente { get; set; }
        [Display(Name = "Num. do Comercial")]
        public int IdComercial { get; set; }
        [Display(Name = "Descrição")]
        public string ResumoVisita { get; set; }
        [Display(Name = "Estado")]
        public string EstadoVisita { get; set; }
        [Display(Name = "Prioridade")]
        public string PrioridadeVisita { get; set; }
        [Display(Name = "Stamp")]
        public string VisitaStamp { get; set; }
        [Display(Name = "Comentários")]
        public string ObsVisita { get; set; }
        [Display(Name = "Propostas")]
        public List<Proposta> Propostas { get; set; }
        [Display(Name = "Contacto")]
        public Contacto Contacto { get; set; }
        public string GetUrl { get { return "http://webapp.food-tech.pt/Visitas/Visita/?idVisita=" + IdVisita; } }
        public string UrlAnexos { get; set; }
    }

}
