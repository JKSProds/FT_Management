using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Log
    {
        public int Id { get; set; }
        public int IdUtilizador { get; set; }
        public Utilizador Utilizador { get; set; }
        public string Descricao { get; set; }
        public int Tipo { get; set; }
        public DateTime Data { get; set; }
        
    }
}
