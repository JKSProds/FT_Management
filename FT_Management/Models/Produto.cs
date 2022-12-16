using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Produto
    {
        public string StampProduto { get; set; }
        [Display(Name = "Referência")]
        public string Ref_Produto { get; set; }
        [Display(Name = "Designação")]
        public string Designacao_Produto { get; set; }
        [Display(Name = "Stock Fisico")]
        public double Stock_Fisico { get; set; }
        [Display(Name = "Stock")]
        public double Stock_PHC { get; set; }
        [Display(Name = "Reservado")]
        public double Stock_Res { get; set; }
        [Display(Name = "Receção")]
        public double Stock_Rec { get; set; }
        [Display(Name = "Armazém")]
        public int Armazem_ID { get; set; }
        [Display(Name = "Localização")]
        public string Pos_Stock { get; set; }
        public bool ModificadoStock { get; set; }
        [Display(Name = "Observações")]
        [DataType(DataType.MultilineText)]
        public string Obs_Produto { get; set; }
        [Display(Name = "Tipo de Unidade")]
        public string TipoUn { get; set; }
        [Display(Name = "Imagem")]
        public string ImgProduto { get; set; }
        public bool Serie { get; set; }
    }

    public class Movimentos
    {
        public int IdFolhaObra { get; set; }
        public int IdTecnico { get; set; }
        public string GuiaTransporte { get; set; }
        public string NomeTecnico { get; set; }
        public string RefProduto { get; set; }
        public string Designacao { get; set; }
        public float Quantidade { get; set; }
        public string NomeCliente { get; set; }
        public DateTime DataMovimento { get; set; }
    }
}
