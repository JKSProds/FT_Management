namespace FT_Management.Models
{
    public class Marcacao
    {
        public string EmojiEstado
        {
            get
            {
                return (this.Oficina || this.TipoServico == "Interno" ? "🏢 " : "") +
                (this.EstadoMarcacao == 4 || this.EstadoMarcacao == 9 || this.EstadoMarcacao == 10 ? "✅ " :
                this.EstadoMarcacaoDesc == "Em Curso" ? "🔧 " :
                this.EstadoMarcacaoDesc == "Cancelado" ? "🚫 " :
                this.EstadoMarcacaoDesc == "Reagendar" ? "📆 " :
                this.EstadoMarcacao != 1 && this.EstadoMarcacao != 26 ? "⌛️ " :
                this.EstadoMarcacaoDesc == "Criado" && this.Utilizador.NomeCompleto == "MailTrack" ? "🤖 " :
                this.DataMarcacao < DateTime.Now && this.EstadoMarcacaoDesc != "Criado" ? "❌ " : "");
            }
        }

        [Display(Name = "Cor")]
        public string Cor
        {
            get
            {
                return this.EstadoMarcacaoDesc == "Finalizado" || this.EstadoMarcacaoDesc == "Cancelado" ? "#23d160" :
                this.EstadoMarcacaoDesc != "Agendado" ? "#ffdd57" :
                this.DataMarcacao < DateTime.Now && this.EstadoMarcacaoDesc == "Agendado" && this.DataMarcacao.ToShortDateString() != DateTime.Now.ToShortDateString() ? "#ff3860" : "";
            }
        }
        [Display(Name = "Num. da Marcação")]
        public int IdMarcacao { get; set; }
        [Display(Name = "Cliente")]
        public Cliente Cliente { get; set; }
        public int EstadoMarcacao { get; set; }
        [Required]
        [Display(Name = "Estado")]
        public string EstadoMarcacaoDesc { get; set; }
        [Display(Name = "Periodo")]
        public string Periodo { get; set; }
        [Display(Name = "Criado em:")]
        public DateTime DataCriacao { get; set; }
        [Required]
        [Display(Name = "Data do Pedido")]
        public DateTime DataPedido { get; set; }
        [Required]
        [Display(Name = "Data da Marcação")]
        public DateTime DataMarcacao { get; set; }
        [Display(Name = "Datas")]
        public string DatasAdicionais { get; set; }
        public List<DateTime> DatasAdicionaisDistintas { get { return DatasAdicionais != null ? DatasAdicionais.Split(";").ToList().Where(d => d != "").Select(d => DateTime.Parse(d)).Distinct().ToList() : null; } }
        [Display(Name = "Num. de Dias")]
        public int DiffDias { get { return DateTime.Now.Subtract(this.DataMarcacao).Days; } }
        [Required]
        [Display(Name = "Prioridade")]
        public string PrioridadeMarcacao { get; set; }
        [Display(Name = "Tipo de Serviço")]
        public string TipoServico { get; set; }
        [Display(Name = "Tipo de Pedido")]
        public string TipoPedido { get; set; }
        [Required]
        [Display(Name = "Equipamento")]
        public string TipoEquipamento { get; set; }
        [Required]
        [Display(Name = "Incidente")]
        public string Referencia { get; set; }
        [Display(Name = "Hora")]
        public string Hora { get; set; }
        [Display(Name = "Nome")]
        public string QuemPediuNome { get; set; }
        [Display(Name = "Email")]
        public string QuemPediuEmail { get; set; }
        [Display(Name = "Telefone")]
        public string QuemPediuTelefone { get; set; }
        [Required]
        [Display(Name = "Resumo")]
        public string ResumoMarcacao { get; set; }
        [Display(Name = "Justificação Fecho")]
        public string JustificacaoFecho { get; set; }
        [Display(Name = "Fechado por")]
        public string FechadoPor { get; set; }
        [Display(Name = "Em Oficina?")]
        public bool Oficina { get; set; }
        [Display(Name = "Serviço de Piquete?")]
        public bool Piquete { get; set; }
        public Utilizador Utilizador { get; set; }
        public string MarcacaoStamp { get; set; }


        public int IdTecnico { get; set; }
        [Display(Name = "Técnico")]
        public Utilizador Tecnico { get; set; }
        public List<Utilizador> LstTecnicos { get; set; }
        public List<int> LstTecnicosSelect { get; set; }
        public List<FolhaObra> LstFolhasObra { get; set; }
        public List<Comentario> LstComentarios { get; set; }
        public List<Anexo> LstAnexos { get; set; }
        public List<Atividade> LstAtividade { get; set; }
        public string GetUrl { get { return "http://webapp.food-tech.pt/Pedidos/Pedido/" + IdMarcacao; } }

        public Marcacao()
        {
            this.LstTecnicos = new List<Utilizador>();
            this.DataPedido = DateTime.Now;
            this.DataMarcacao = DateTime.Now;
            this.DatasAdicionais = DateTime.Now.ToShortDateString();
            this.LstTecnicosSelect = new List<int> { };
            this.JustificacaoFecho = "";
            this.EstadoMarcacaoDesc = "Agendado";
        }
    }

    public class EstadoMarcacao
    {
        [Display(Name = "Num. do Estado")]
        public int IdEstado { get; set; }
        [Display(Name = "Estado")]
        public string EstadoMarcacaoDesc { get; set; }
        public EstadoMarcacao()
        {
            this.IdEstado = 0;
            this.EstadoMarcacaoDesc = "N/D";
        }
    }

    public class Comentario
    {
        [Display(Name = "Num. do Comentário")]
        public string IdComentario { get; set; }
        [Display(Name = "Comentário")]
        public string Descricao { get; set; }
        [Display(Name = "Num. da Marcação")]
        public string IdMarcacao { get; set; }
        [Display(Name = "Marcação")]
        public Marcacao Marcacao { get; set; }
        [Display(Name = "Utilizador")]
        public Utilizador Utilizador { get; set; }
        [Display(Name = "Data")]
        public DateTime DataComentario { get; set; }
    }

    public class Atividade
    {
        [Display(Name = "Id")]
        public string Id { get; set; }
        public string StampAtividade { get; set; }
        [Display(Name = "Tipo")]
        public int Tipo { get; set; }
        [Display(Name = "Nome")]
        public string Nome { get; set; }
        [Display(Name = "Criado por")]
        public string CriadoPor { get; set; }
        [Display(Name = "Data")]
        public DateTime Data { get; set; }
    }

}
