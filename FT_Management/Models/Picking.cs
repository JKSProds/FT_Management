using System;
using System.Collections.Generic;

namespace FT_Management.Models
{
    public class Picking
    {
        public string Picking_Stamp { get; set; }
        public Encomenda Encomenda { get; set; }
        public string NomeCliente { get; set; }
        public DateTime DataDossier { get; set; }
        public List<Picking_Linha> Linhas { get; set; }
    }
    public class Picking_Linha
    {
        public string Picking_Linha_Stamp { get; set; }
        public string Ref_linha { get; set; }
        public string Nome_Linha { get; set; }
        public int Qtd_Linha { get; set; }
        public int Qtd_Fornecida { get; set; }
        public string Series { get; set; }
    }
}
