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
        [Display(Name = "Estado Atual")]
        public string EstadoMarcacaoDesc { get; set; }
        [Display(Name = "Periodo")]
        public string Periodo { get; set; }
        [Display(Name = "Criado em:")]
        public DateTime DataCriacao { get; set; }
        [Required]
        public DateTime DataPedido { get; set; }
        [Required]
        public DateTime DataMarcacao { get; set; }
        [Required]
        [Display(Name = "Prioridade")]
        public string PrioridadeMarcacao { get; set; }
        public string TipoServico { get; set; }
        public string TipoPedido { get; set; }
        [Required]
        [Display(Name = "Equipamento")]
        public string TipoEquipamento { get; set; }
        [Required]
        [Display(Name = "Incidente")]
        public string Referencia { get; set; }
        [Display(Name = "Hora")]
        public string Hora { get; set; }
        public string QuemPediuNome { get; set; }
        public string QuemPediuEmail { get; set; }
        public string QuemPediuTelefone { get; set; }
        [Required]
        public string ResumoMarcacao { get; set; }
        public int Oficina { get; set; }
        public int Piquete { get; set; }
        public string Utilizador { get; set; }
        public string MarcacaoStamp { get; set; }


        public int IdTecnico { get; set; }
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
