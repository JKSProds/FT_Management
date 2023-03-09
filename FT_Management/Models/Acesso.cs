namespace FT_Management.Models
{
    public class Acesso
    {
        [Display(Name = "Num. do Acesso")]
        public int Id { get; set; }
        [Display(Name = "Num. do Utilizador")]
        public int IdUtilizador { get; set; }
        [Display(Name = "Utilizador")]
        public Utilizador Utilizador { get; set; }
        [Display(Name = "Hora de Acesso")]
        public DateTime Data { get; set; }
        [Display(Name = "Tipo")]
        public int Tipo { get; set; }
        [Display(Name = "Entrada pela App")]
        public bool App { get; set; }
        [Display(Name = "Tipo")]
        public string TipoAcesso { get { return !App ? (Tipo == 1 ? "Entrada" : "Saída") : (Tipo == 1 ? "Início de Dia" : "Fim de Dia"); } }
        [Display(Name = "Temperatura")]
        public string Temperatura { get; set; }
    }
}
