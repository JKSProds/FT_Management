using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class ControloViatura
    {
        public int Id{ get; set; }
        [Display(Name = "Ultimo Utilizador")]
        public string Nome_Tecnico { get; set; }
        [Display(Name = "Matricula")]
        public string MatriculaViatura { get; set; }
        public string KmsViatura { get; set; }
        public string KmsFinais { get; set; }
        [Display(Name = "Levantada a: ")]
        public DateTime DataInicio { get; set; }
        public string Notas { get; set; }
        [Display(Name = "Devolvida a:")]
        public DateTime DataFim { get; set; }

        bool EmUso()
        {
            return DataFim > DateTime.Now;
        }

    }
}
