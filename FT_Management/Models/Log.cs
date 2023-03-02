namespace FT_Management.Models
{
    public class Log
    {
        [Display(Name = "Num. do Log")]
        public int Id { get; set; }
        [Display(Name = "Num. do Utilizador")]
        public int IdUtilizador { get; set; }
        [Display(Name = "Utilizador")]
        public Utilizador Utilizador { get; set; }
        [Display(Name = "Logs")]
        public string Descricao { get; set; }
        [Display(Name = "Tipo")]
        public int Tipo { get; set; }
        [Display(Name = "Data")]
        public DateTime Data { get; set; }

    }
}
