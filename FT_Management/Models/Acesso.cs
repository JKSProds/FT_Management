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
        public int Tipo { get; set; }
        public bool App { get; set; }
        public string TipoAcesso { get { return !App ? (Tipo == 1 ? "Entrada" : "Saída") : (Tipo == 1 ? "Início de Dia" : "Fim de Dia"); } }
        public string Temperatura { get; set; }
    }
}
