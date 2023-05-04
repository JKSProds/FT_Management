namespace FT_Management.Models
{
    public class Fornecedor
    {
        public string StampFornecedor { get; set; }
        [Display(Name = "Num. do Fornecedor")]
        public int IdFornecedor { get; set; }
        [Display(Name = "Nome do Fornecedor")]
        public string NomeFornecedor { get; set; }
        [Display(Name = "Morada")]
        public string MoradaFornecedor { get; set; }
        [Display(Name = "Telefone/Telemóvel")]
        public string ContactoFornecedor { get; set; }
        [Display(Name = "Referência do Fornecedor")]
        public string ReferenciaFornecedor { get; set; }
        [Display(Name = "Email")]
        public string EmailFornecedor { get; set; }
        [Display(Name = "Contacto")]
        public string PessoaContactoFornecedor { get; set; }
        [Display(Name = "Comentários")]
        public string Obs { get; set; }
        public string CodigoIntermedio { get; set; }
        public List<Encomenda> Encomendas { get; set; }

        public List<Picking> OrdensRececao { get; set; }

    }
}
