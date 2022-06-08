using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FT_Management.Models
{
    public class Vendedor
    {
        public int IdVendedor { get; set; }
        private string _NomeVendedor;
        [Required(ErrorMessage = "Nome do Vendedor é Obrigatório")]
        [Display(Name = "Nome do Cliente")]
        public string NomeVendedor { get { return _NomeVendedor ?? ""; } set { _NomeVendedor = value; } }
        public string uid { get; set; }
    }
}
