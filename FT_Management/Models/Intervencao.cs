namespace FT_Management.Models
{
    public class Intervencao
    {
        [Display(Name = "Num. da Intervenção")]
        public string StampIntervencao { get; set; }
        [Display(Name = "Num. do Técnico")]
        public int IdTecnico { get; set; }
        [Display(Name = "Num. da Folha de Obra")]
        public int IdFolhaObra { get; set; }
        private string _RelatorioServico;
        [DataType(DataType.MultilineText)]
        [Display(Name = "Relatório do Serviço")]
        public string RelatorioServico { get { return _RelatorioServico ?? ""; } set { _RelatorioServico = value; } }
        private string _NomeTecnico;
        [Display(Name = "Técnico")]
        public string NomeTecnico { get { return _NomeTecnico ?? ""; } set { _NomeTecnico = value; } }
        [Display(Name = "Data do Serviço")]
        public DateTime DataServiço { get; set; }
        [Display(Name = "Hora de Inicio")]
        public DateTime HoraInicio { get; set; }
        [Display(Name = "Hora de Fim")]
        public DateTime HoraFim { get; set; }
    }

}
