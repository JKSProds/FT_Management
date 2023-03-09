namespace FT_Management.Models
{
    public class Viagem
    {
        [Display(Name = "Matricula")]
        public string Matricula { get; set; }
        [Display(Name = "Data de Inicio")]
        public DateTime Inicio_Viagem { get; set; }
        [Display(Name = "Data de Fim")]
        public DateTime Fim_Viagem { get; set; }
        [Display(Name = "Local de Inicio")]
        public string Inicio_Local { get; set; }
        [Display(Name = "Local de Fim")]
        public string Fim_Local { get; set; }
        [Display(Name = "Kms Iniciais")]
        public string Inicio_Kms { get; set; }
        [Display(Name = "Kms Finais")]
        public string Fim_Kms { get; set; }
        [Display(Name = "Distância da Viagem")]
        public string Distancia_Viagem { get; set; }
        [Display(Name = "Tempo de Viagem")]
        public string Tempo_Viagem { get; set; }
    }
}
