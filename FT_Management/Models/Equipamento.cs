using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Equipamento
    {
        public int IdEquipamento { get; set; }
        [Display(Name = "Designação do Equipamento")]
        public string DesignacaoEquipamento { get; set; }
        [Display(Name = "Marca")]
        public string MarcaEquipamento { get; set; }
        [Display(Name = "Modelo")]
        public string ModeloEquipamento { get; set; }
        [Display(Name = "Numero de Série")]
        public string NumeroSerieEquipamento { get; set; }
    }
}
