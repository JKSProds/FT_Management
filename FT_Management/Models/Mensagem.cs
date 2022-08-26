namespace FT_Management.Models
{
    public class Notificacao
    {
        public int ID { get; set; }
        public string Mensagem { get; set; }
        public string Destino { get; set; }
        public Utilizador Utilizador { get; set; }
        public int Tipo { get; set; }
        public bool Pendente { get; set; }
    }
}
