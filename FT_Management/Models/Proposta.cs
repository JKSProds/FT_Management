namespace FT_Management.Models
{
    public class Proposta
    {
        [Display(Name = "Num. da Proposta")]
        public int IdProposta { get; set; }
        [Display(Name = "Visita")]
        public Visita Visita { get; set; }
        [Display(Name = "Num. da Visita")]
        public int IdVisita { get; set; }
        [Display(Name = "Data")]
        public DateTime DataProposta { get; set; }
        [Display(Name = "Estado")]
        public string EstadoProposta { get; set; }
        [Display(Name = "Comercial")]
        public Utilizador Comercial { get; set; }
        [Display(Name = "Valor")]
        public string ValorProposta { get; set; }
        [Display(Name = "Url/Anexo")]
        public string UrlProposta { get; set; }
        [Display(Name = "Observações")]
        public string ObsProposta { get; set; }
    }
}
