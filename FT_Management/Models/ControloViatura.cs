using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class ControloViatura
    {
        public int Id{ get; set; }
        public string Nome_Tecnico { get; set; }
        public string MatriculaViatura { get; set; }
        public string KmsViatura { get; set; }
        public string KmsFinais { get; set; }
        public DateTime DataInicio { get; set; }
        public string Notas { get; set; }
        public DateTime DataFim { get; set; }

        bool EmUso()
        {
            return DataFim > DateTime.Now;
        }

    }
}
