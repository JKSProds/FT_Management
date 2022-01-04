using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Acesso
    {
        public int Id { get; set; }
        public int IdUtilizador { get; set; }
        public Utilizador Utilizador { get; set; }
        public DateTime Data { get; set; }
        public string Tipo { get; set; }
        public string Temperatura { get; set; }
    }
}
