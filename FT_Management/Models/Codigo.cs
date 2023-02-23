using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Codigo
    {
        public string Stamp { get; set; }
        public int Estado { get; set; }
        public string EstadoDescricao { get { return Estado == 1 ? "✅ Aprovado" : "❌ Rejeitado"; } }
        public string Obs { get; set; }
        public Utilizador utilizador { get; set; }
        public DateTime ValidadeCodigo { get; set; }
        public string GetUrl { get { return "http://webapp.food-tech.pt/Home/ValidarCodigo/" + Stamp; } }
        public bool Validado { get { return Estado != 0; } }
        public Utilizador ValidadoPor { get; set; }
    }
}