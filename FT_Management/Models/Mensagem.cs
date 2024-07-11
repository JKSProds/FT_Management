namespace FT_Management.Models
{
    public class Notificacao
    {
        public int ID { get; set; }
        public string Stamp { get; set; }
        public string Assunto { get; set; }
        public string Mensagem { get; set; }        
        public List<String> Cc { get; set; }
        public Utilizador UtilizadorDestino { get; set; }
        public Utilizador UtilizadorOrigem { get; set; }
        public string Tipo { get; set; }
        public bool Pendente { get; set; }
    }
}
