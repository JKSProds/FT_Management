using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FT_Management.Models
{
    public class ApiKey
    {
        [Display(Name = "Num. da Api Key")]
        public int Id { get; set; }
        [Display(Name = "Descrição")]
        public string Descricao { get; set; }
        [Display(Name = "API Key")]
        public string Key { get; set; }
        [Display(Name = "Utilizador")]
        public Utilizador Utilizador { get; set; }

        public ApiKey()
        {
            Id = 0;
            Descricao = "N/D";
            Key = "";
        }
    }


}
