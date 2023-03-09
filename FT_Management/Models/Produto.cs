namespace FT_Management.Models
{
    public class Produto
    {
        public string StampProduto { get; set; }
        [Display(Name = "Referência")]
        public string Ref_Produto { get; set; }
        [Display(Name = "Designação")]
        public string Designacao_Produto { get; set; }
        [Display(Name = "Fisico")]
        public double Stock_Fisico { get; set; }
        [Display(Name = "Stock")]
        public double Stock_PHC { get; set; }
        [Display(Name = "Reservado")]
        public double Stock_Res { get; set; }
        [Display(Name = "Receção")]
        public double Stock_Rec { get; set; }
        [Display(Name = "PHC")]
        public double Stock_Atual { get { return Stock_PHC + Stock_Rec - Stock_Res; } }
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
        public List<Equipamento> Equipamentos { get; set; }
        public double Valor { get; set; }
        public bool Servico { get { return this.Ref_Produto.StartsWith("SRV."); } }

        public Produto()
        {
            Equipamentos = new List<Equipamento>();
            Ref_Produto = "";
            TipoUn = "";
        }
    }

    public class Movimentos
    {
        [Display(Name = "Num. da Folha de Obra")]
        public int IdFolhaObra { get; set; }
        [Display(Name = "Num do Técnico")]
        public int IdTecnico { get; set; }
        [Display(Name = "Guia de Transporte")]
        public string GuiaTransporte { get; set; }
        [Display(Name = "Técnico")]
        public string NomeTecnico { get; set; }
        [Display(Name = "Referência")]
        public string RefProduto { get; set; }
        [Display(Name = "Designação")]
        public string Designacao { get; set; }
        [Display(Name = "Quantidade")]
        public float Quantidade { get; set; }
        [Display(Name = "Cliente")]
        public string NomeCliente { get; set; }
        [Display(Name = "Data")]
        public DateTime DataMovimento { get; set; }
    }
}
