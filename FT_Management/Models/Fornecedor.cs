using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FT_Management.Models
{
    public class Fornecedor
    {
        [Display(Name = "Num. do Fornecedor")]
        public int IdFornecedor { get; set; }
        [Display(Name = "Nome do Fornecedor")]
        public string NomeFornecedor { get; set; }
        [Display(Name = "Morada")]
        public string MoradaFornecedor { get; set; }
        [Display(Name = "Telefone/Telemóvel")]
        public string ContactoFornecedor { get; set; }
        [Display(Name = "Referência do Fornecedor")]
        public string ReferenciaFornecedor { get; set; }
        [Display(Name = "Email")]
        public string EmailFornecedor { get; set; }
        [Display(Name = "Contacto")]
        public string PessoaContactoFornecedor { get; set; }
        [Display(Name = "Comentários")]
        public string Obs { get; set; }

    }
}
