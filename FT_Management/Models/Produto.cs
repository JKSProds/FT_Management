using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Produto
    {
        [Display(Name = "Referência")]
        public string Ref_Produto { get; set; }
        [Display(Name = "Designação")]
        public string Designacao_Produto { get; set; }
        [Display(Name = "Stock Fisico")]
        public double Stock_Fisico { get; set; }
        [Display(Name = "PHC")]
        public double Stock_PHC { get; set; }
        [Display(Name = "Localização")]
        public string Pos_Stock { get; set; }
        [Display(Name = "Observações")]
        [DataType(DataType.MultilineText)]
        public string Obs_Produto { get; set; }
        public string TipoUn { get; set; }
    }
}
