using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FT_Management.Models
{
    public class Proposta
    {
        public int IdProposta { get; set; }
        public Visita Visita { get; set; }
        public DateTime DataProposta { get; set; }
        public string EstadoProposta { get; set; }
        public Utilizador Comercial { get; set; }
        public string ValorProposta { get; set; }
        public string UrlAnexo { get; set; }
    }
}
