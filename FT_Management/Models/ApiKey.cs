using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FT_Management.Models
{
    public class ApiKey
    {
        public int Id { get; set; }
        public string Descricao { get; set; }
        public string Key { get; set; }
        public Utilizador Utilizador { get; set; }
    }
}
