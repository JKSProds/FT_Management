namespace FT_Management.Models
{
    public class Anexo
    {
        public string Stamp_Anexo { get; set; }
        public string Ecra { get; set; }
        public int Serie { get; set; }
        public string Stamp_Origem { get; set; }
        public string Resumo { get; set; }
        public string Nome { get; set; }
        public string LocalizacaoFicheiro { get; set; }
        public Utilizador Utilizador { get; set; }
    }
}