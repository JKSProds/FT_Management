using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FT_Management.Models
{
    public class Visita
    {
        public int IdVisita { get; set; }
        public DateTime DataVisita { get; set; }
        public Cliente Cliente { get; set; }
        public int IdComercial { get; set; }
        public string ResumoVisita { get; set; }
        public string EstadoVisita { get; set; }
        public string PrioridadeVisita { get; set; }
        public string VisitaStamp { get; set; }
        public string ObsVisita { get; set; }
        public List<Proposta> Propostas { get; set; }
        public Contacto Contacto { get; set; }
    }

}
