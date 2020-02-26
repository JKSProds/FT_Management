using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Produto
    {
        public string Ref_Produto { get; set; }
        public string Designacao_Produto { get; set; }
        public double Stock_Fisico { get; set; }
        public double Stock_PHC { get; set; }
        public string Pos_Stock { get; set; }
        public string Obs_Produto { get; set; }
    }
}
