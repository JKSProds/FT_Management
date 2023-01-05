using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FT_Management.Models
{
    public class Picking
    {
        public string Picking_Stamp { get; set; }
        [Display(Name = "Núm. de Picking")]
        public int IdPicking { get; set; }
        [Display(Name = "Nome do Dossier")]
        public string NomeDossier { get; set; }
        public Encomenda Encomenda { get; set; }
        [Display(Name = "Cliente")]
        public string NomeCliente { get; set; }
        [Display(Name = "Data do Dossier")]
        public DateTime DataDossier { get; set; }
        [Display(Name = "Tipo de Envio")]
        public bool DespacharEncomenda { get; set; }
        public List<Linha_Picking> Linhas { get; set; }
        public string EditadoPor { get; set; }
        public string Obs { get; set; }
        public Armazem ArmazemDestino { get; set; }
        public string GetUrl { get { return "http://webapp.food-tech.pt/Picking/PrintPicking/" + Picking_Stamp; } }

    }
    public class Linha_Picking
    {
        [Display(Name = "Loja")]
        public string Nome_Loja { get; set; }
        public string Picking_Linha_Stamp { get; set; }
        [Display(Name = "Referência")]
        public string Ref_linha { get; set; }
        [Display(Name = "Designação")]
        public string Nome_Linha { get; set; }
        [Display(Name = "Quantidade Validada")]
        public double Qtd_Linha { get; set; }
        [Display(Name = "Quantidade a Separar")]
        public double Qtd_Separar { get; set; }
        public string TipoUnidade { get; set; }
        [Display(Name = "Núm. de Série")]
        public List<Ref_Linha_Picking> Lista_Ref { get; set; }
        public bool Serie { get; set; }
        public bool Validado { get { return Qtd_Linha >= Qtd_Separar; } }
        public string EditadoPor { get; set; }
    }

    public class Ref_Linha_Picking
    {
        public string Picking_Linha_Stamp { get; set; }
        [Display(Name = "Referência")]
        public string Ref_linha { get; set; }
        [Display(Name = "Designação")]
        public string Nome_Linha { get; set; }
        [Display(Name = "Quantidade a Separar")]
        public double Qtd_Separar { get; set; }
        public string BOMA_STAMP { get; set; }
        public string NumSerie { get; set; }
        public bool Validado { get { return (NumSerie != "" && BOMA_STAMP != ""); } }

        public Ref_Linha_Picking()
        {
            BOMA_STAMP = "";
            NumSerie = "";
        }
    }
}
