using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FT_Management.Models
{
    public class Fornecedor
    {
        public int IdFornecedor { get; set; }
        public string NomeFornecedor { get; set; }
        public string MoradaFornecedor { get; set; }
        public string ContactoFornecedor { get; set; }
        public string ReferenciaFornecedor { get; set; }
        public string EmailFornecedor { get; set; }
        public string PessoaContactoFornecedor { get; set; }
        public string Obs { get; set; }

    }
}
