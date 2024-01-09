namespace FT_Management.Models
{
    public class Armazem
    {
        public string ArmazemStamp { get; set; }
        [Display(Name = "Num. do Armazém")]
        public int ArmazemId { get; set; }
        [Display(Name = "Nome do Armazém")]
        public string ArmazemNome { get; set; }

        public Armazem()
        {
            ArmazemId = 0;
            ArmazemNome = "N/D";
        }
    }
}
