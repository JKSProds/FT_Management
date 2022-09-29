using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Encomenda
    {
        [Display(Name = "ID da Encomenda")]
        public int Id { get; set; }
        public string NomeDossier { get; set; }
        public string NomeCliente { get; set; }
        public DateTime DataEnvio { get; set; }
        public DateTime DataDossier { get; set; }
        public List<Linha_Encomenda> LinhasEncomenda { get; set; }
        public bool Total { get { return this.LinhasEncomenda.Where(l => l.Total == false).Count() == 0; } }
        public double NItems { get { return this.LinhasEncomenda.Sum(l => l.Produto.Stock_Fisico); } }
    }
    public class Linha_Encomenda
    {
        [Display(Name = "ID da Encomenda")]
        public int IdEncomenda { get; set; }
        public string NomeCliente { get; set; }
        public DateTime DataEnvio { get; set; }
        public bool Total { get; set; }
        public Produto Produto { get; set; }
    }
}
