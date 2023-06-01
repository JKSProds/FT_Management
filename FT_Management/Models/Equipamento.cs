namespace FT_Management.Models
{
    public class Equipamento
    {
        [Required(ErrorMessage = "Falta escolher o equipamento!")]
        public string EquipamentoStamp { get; set; }
        public string TipoEquipamento { get; set; }
        public string DescricaoEquipamento { get { return this.NumeroSerieEquipamento + " - (" + MarcaEquipamento + " " + this.ModeloEquipamento + ")"; } }
        private string _DesignacaoEquipamento;
        [Display(Name = "Designação do Equipamento")]
        public string DesignacaoEquipamento { get { return _DesignacaoEquipamento ?? ""; } set { _DesignacaoEquipamento = value; } }
        private string _MarcaEquipamento;
        [Display(Name = "Marca")]
        public string MarcaEquipamento { get { return _MarcaEquipamento ?? ""; } set { _MarcaEquipamento = value; } }
        private string _ModeloEquipamento;
        [Display(Name = "Modelo")]
        public string ModeloEquipamento { get { return _ModeloEquipamento ?? ""; } set { _ModeloEquipamento = value; } }
        private string _NumeroSerieEquipamento;
        [Display(Name = "Numero de Série")]
        public string NumeroSerieEquipamento { get { return _NumeroSerieEquipamento ?? ""; } set { _NumeroSerieEquipamento = value; } }
        [Display(Name = "Num. do Cliente")]
        public int IdCliente { get; set; }
        [Display(Name = "Num. do Estabelecimento")]
        public int IdLoja { get; set; }
        [Display(Name = "Num. do Fornecedor")]
        public int IdFornecedor { get; set; }
        [Display(Name = "Referência do Equipamento")]
        public string RefProduto { get; set; }
        public Cliente Cliente { get; set; }
        public List<FolhaObra> FolhasObra { get; set; }
        public bool Garantia { get; set; }
        public bool Contrato { get; set; }
        public bool Inativo { get; set; }

        [Display(Name = "Técnico")]
        public string UltimoTecnico { get; set; }
        [Display(Name = "Data de Compra")]
        public DateTime DataCompra { get; set; }
        [Display(Name = "Data de Venda")]
        public DateTime DataVenda { get; set; }

        public Equipamento()
        {
            NumeroSerieEquipamento = "";
        }
    }

}
