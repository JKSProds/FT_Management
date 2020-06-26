using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string NomeCliente { get; set; }
        public string PessoaContatoCliente { get; set; }
        public string MoradaCliente { get; set; }
        public string EmailCliente { get; set; }
        public string NumeroContribuinteCliente { get; set; }
    }
}
