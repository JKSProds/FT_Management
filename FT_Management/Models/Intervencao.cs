using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Intervencao
    {
        public int IdIntervencao { get; set; }
        public int IdTecnico { get; set; }
        public int IdFolhaObra { get; set; }
        [Display(Name = "Técnico")]
        public string NomeTecnico { get; set; }
        [Display(Name = "Data do Serviço")]
        public DateTime DataServiço { get; set; }
        [Display(Name = "Hora de Inicio")]
        public DateTime HoraInicio { get; set; }
        [Display(Name = "Hora de Fim")]
        public DateTime HoraFim { get; set; }
    }
}
