using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FT_Management.Models
{
    public class Marcacao
    {
        [Display(Name = "Num. da Marcação")]
        public int IdMarcacao { get; set; }
        [Display(Name = "Agendado para:")]
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
        [Display(Name = "Em Oficina?")]
        public bool Oficina { get; set; }
        [Display(Name = "Serviço de Piquete?")]
        public bool Piquete { get; set; }
        public string Utilizador { get; set; }
        public string MarcacaoStamp { get; set; }


        public int IdTecnico { get; set; }
        [Display(Name = "Técnico")]
        public Utilizador Tecnico { get; set; }
        public List<Utilizador> LstTecnicos { get; set; }
        public List<FolhaObra> LstFolhasObra { get; set; }
        public List<Comentario> LstComentarios { get; set; }
    }

    public class EstadoMarcacao
    {
        public int IdEstado { get; set; }
        public string EstadoMarcacaoDesc { get; set; }
        public EstadoMarcacao()
        {
            this.IdEstado = 0;
            this.EstadoMarcacaoDesc = "N/D";
        }
    }

    public class Comentario
    {
        public string IdComentario { get; set; }
        public string Descricao { get; set; }
        public string IdMarcacao { get; set; }
        public string NomeUtilizador { get; set; }
    }

}
