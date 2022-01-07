using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Ferias
    {
        public int Id { get; set; }
        public int IdUtilizador { get; set; }
        public int ValidadoPor { get; set; }
        public string ValidadoPorNome { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public bool Validado { get; set; }
        public string Obs { get; set; }
        public int DiasMarcados { get; set; }
        public int DiasTotais { get; set; }
    }
}
