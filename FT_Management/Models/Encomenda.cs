using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Encomenda
    {
        public string BO_STAMP { get; set; }
        public string PI_STAMP { get; set; }
        [Display(Name = "Encomenda")]
        public int Id { get; set; }
        [Display(Name = "Dossier")]
        public string NomeDossier { get; set; }
        [Display(Name = "Num. Dossier")]
        public int NumDossier { get; set; }
        [Display(Name = "Cliente")]
        public string NomeCliente { get; set; }
        [Display(Name = "Dias")]
        public DateTime DataEnvio { get; set; }
        public DateTime Data { get { return (Total ? this.DataEnvio : ((this.LinhasEncomenda.Where(e => e.DataEnvio.Year > 1900).Count() > 0) ? this.LinhasEncomenda.Where(e => e.DataEnvio.Year > 1900).First().DataEnvio : this.DataEnvio) ); } }
        [Display(Name = "Data")]
        public DateTime DataDossier { get; set; }
        [Display(Name = "Linhas")]
        public List<Linha_Encomenda> LinhasEncomenda { get; set; }
        [Display(Name = "Tipo")]
        public bool Total { get { return this.LinhasEncomenda.Where(l => l.Total == false).Count() == 0; } }
        [Display(Name = "Items")]
        public double NItems { get { return this.LinhasEncomenda.Where(l => (l.DataEnvio.Year > 1900 || l.Total) && !l.Fornecido).Sum(l => l.Produto.Stock_Fisico); } }
        public bool Fornecido { get { return LinhasEncomenda.Where(l => l.Fornecido).Count() == LinhasEncomenda.Count(); } }
        public bool DespacharEncomenda { get; set; }
        public bool ExisteEncomenda(Tipo tipo)
        {
            if (Total)
            {
                if (DataEnvio <= DateTime.Now.AddDays(-1) && DataEnvio.Year > 1900 && tipo == Tipo.ATRASO) return true;
                if (DataEnvio.ToShortDateString() == DateTime.Now.ToShortDateString() && tipo == Tipo.HOJE) return true;
                if (DataEnvio.ToShortDateString() == DateTime.Now.AddDays(1).ToShortDateString() && tipo == Tipo.AMANHA) return true;
                if (DataEnvio >= DateTime.Now.AddDays(1) && tipo == Tipo.FUTURO) return true;
                if (DataEnvio.Year > 1900 && tipo == Tipo.TODAS) return true; 
            } else
            {
                if (LinhasEncomenda.Where(l => l.DataEnvio <= DateTime.Now.AddDays(-1) && DataEnvio.Year > 1900 && tipo == Tipo.ATRASO).Count() > 0) return true;
                if (LinhasEncomenda.Where(l => l.DataEnvio.ToShortDateString() == DateTime.Now.ToShortDateString() && tipo == Tipo.HOJE).Count() > 0) return true;
                if (LinhasEncomenda.Where(l => l.DataEnvio.ToShortDateString() == DateTime.Now.AddDays(1).ToShortDateString() && tipo == Tipo.AMANHA).Count() > 0) return true;
                if (LinhasEncomenda.Where(l => l.DataEnvio >= DateTime.Now.AddDays(1) && tipo == Tipo.FUTURO).Count() > 0) return true;
                if (LinhasEncomenda.Where(l => l.DataEnvio.Year > 1900 && tipo == Tipo.TODAS).Count() > 0) return true;
            }
            return false;
        }

        public enum Tipo
        {
            ATRASO,
            HOJE,
            AMANHA,
            FUTURO,
            TODAS
        }
    }
    public class Linha_Encomenda
    {
        [Display(Name = "ID da Encomenda")]
        public int IdEncomenda { get; set; }
        public string NomeCliente { get; set; }
        public DateTime DataEnvio { get; set; }
        public bool Total { get; set; }
        public Produto Produto { get; set; }
        public bool Fornecido { get; set; }
    }
}
