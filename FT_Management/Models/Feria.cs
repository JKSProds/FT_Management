namespace FT_Management.Models
{
    public class FeriasUtilizador
    {
        [Display(Name = "Utilizador")]
        public Utilizador utilizador { get; set; }
        [Display(Name = "Lista de Férias")]
        public List<Ferias> Ferias { get; set; }
        [Display(Name = "Dias Validados")]
        public int DiasMarcados { get; set; }
        [Display(Name = "Dias Marcados")]
        public int DiasTotais { get; set; }
        [Display(Name = "Dias por Marcar")]
        public int DiasDisponiveis { get; set; }
    }

    public class Ferias
    {
        public string Emoji { get { return Aniversario ? "🎂 " : (DataInicio.Month >= 3 && DataInicio.Month < 7 ? "🌻 " : (DataInicio.Month >= 7 && DataInicio.Month < 10 ? "🏖️ " : (DataInicio.Month >= 10 && DataInicio.Month < 12 ? "🍁 " : "❄ "))); } }
        [Display(Name = "Num. da Requisitação de Férias")]
        public int Id { get; set; }
        [Display(Name = "Num. do Utilizador")]
        public int IdUtilizador { get; set; }
        public Utilizador Utilizador { get; set; }
        [Display(Name = "Validado por")]
        public int ValidadoPor { get; set; }
        [Display(Name = "Validado por")]
        public string ValidadoPorNome { get; set; }
        [Display(Name = "Inicio")]
        public DateTime DataInicio { get; set; }
        [Display(Name = "Fim")]
        public DateTime DataFim { get; set; }
        [Display(Name = "Validado?")]
        public bool Validado { get; set; }
        [Display(Name = "Comentários")]
        public string Obs { get; set; }
        public bool Aniversario { get; set; }

        public string GetUrl { get { return "http://webapp.food-tech.pt/Ferias/Utilizador/" + IdUtilizador; } }
    }

    public class Feriado
    {
        public string Emoji { get { return "📅 "; } }
        [Display(Name = "Num. do Feriado")]
        public int Id { get; set; }
        [Display(Name = "Data")]
        public DateTime DataFeriado { get; set; }
        [Display(Name = "Nome do Feriado")]
        public string DescFeriado { get; set; }
    }
}
