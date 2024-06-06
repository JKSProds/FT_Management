namespace FT_Management.Models
{
    public class Piquete
    {
        public string Stamp { get; set; }
        [Display(Name = "Num. do Utilizador")]
        public int IdUtilizador { get; set; }
        [Display(Name = "Utilizador")]
        public Utilizador Utilizador { get; set; }
        public DateTime Data { get {return Enumerable.Range(0, 7)
            .Select(days => new DateTime(int.Parse(Stamp.Split(",")[0]) , 1, 1).AddDays(days))
            .First(d => d.DayOfWeek == DayOfWeek.Monday)
            .AddDays((int.Parse(Stamp.Split(",")[1]) - 1) * 7); }}

        public string Semana { get {return System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(Data, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday).ToString();}}
        public int IdOrdem { get {return int.Parse(Stamp.Split(",")[2] + int.Parse(Stamp.Split(",")[3]));} }
        public int Zona { get {return int.Parse(Stamp.Split(",")[2]);} }
        public int Tipo { get {return int.Parse(Stamp.Split(",")[3]);} }
        public bool Valido { get; set; }
    }
}
