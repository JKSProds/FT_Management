using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        [Display(Name = "Nome do Cliente")]
        public string NomeCliente { get; set; }
        [Display(Name = "Pessoa de Contacto")]
        public string PessoaContatoCliente { get; set; }
        [Display(Name = "Morada")]
        public string MoradaCliente { get; set; }
        [Display(Name = "Email")]
        public string EmailCliente { get; set; }
        [Display(Name = "NIF")]
        public string NumeroContribuinteCliente { get; set; }
    }
}
