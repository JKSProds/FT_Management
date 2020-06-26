using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Intervencao
    {
        public int IdIntervencao { get; set; }
        public int IdTecnico { get; set; }
        public string NomeTecnico { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
    }
}
