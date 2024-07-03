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
        public bool Validado { get; set; }
        public int TipoFalta { get; set; }
        public int TipoHorasExtra { get; set; }

    }

        public class RegistroAcessos
    {
       public Utilizador Utilizador { get; set; }
       public Acesso E1 { get; set; }
       public Acesso S1 { get; set; }
       public Acesso E2 { get; set; }
       public Acesso S2 { get; set; }
       public DateTime Data { get; set; }
       public int TipoFalta { get { return E1.TipoFalta;} }

       public int TipoHorasExtra { get { return E1.TipoHorasExtra;} }
       public bool Valido { get { return E1.Id != 0 && S1.Id != 0;} }

       public bool Validado { get { return E1.Validado && S1.Validado;} }

       public int TotalMinutos { get { return (int)((S1.Data - E1.Data) + (S2.Data - E2.Data)).TotalMinutes;}}
       public bool ValidarPHC {get {return Math.Abs(TotalMinutos - 8*60) > 15 && this.Utilizador.TipoUtilizador != 1;}}     
       public string TotalHorasDesc() { 
            TimeSpan t1 = S1.Data - E1.Data;
            TimeSpan t2 = S2.Data - E2.Data;
            TimeSpan d = t1+t2;
            

            return d > TimeSpan.Zero ? $"{(int)d.TotalHours:D2}:{d.Minutes:D2}" : "--:--";
        }


        public int ObterHoras(int NHoras, int Margem) {
             double tempoTrabalhadoHoras = this.TotalMinutos / 60.0;
            double diferencaHoras = Math.Abs(NHoras - tempoTrabalhadoHoras);

            return (int)Math.Ceiling(diferencaHoras);
        }

        public RegistroAcessos() {
            E1 = new Acesso();
            S1 = new Acesso();
            E2 = new Acesso();
            S2 =    new Acesso();
        }

    }
}
