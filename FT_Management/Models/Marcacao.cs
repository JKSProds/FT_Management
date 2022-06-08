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
        public DateTime DataMarcacao { get; set; }
        public Cliente Cliente { get; set; }
        [Display(Name = "Criado em:")]
        public DateTime DataCriacao { get; set; }
        public int IdTecnico { get; set; }
        public Utilizador Tecnico { get; set; }
        public string ResumoMarcacao { get; set; }
        public int EstadoMarcacao { get; set; }
        [Display(Name = "Estado Atual")]
        public string EstadoMarcacaoDesc { get; set; }
        [Display(Name = "Prioridade")]
        public string PrioridadeMarcacao { get; set; }
        public string MarcacaoStamp { get; set; }
        public List<FolhaObra> LstFolhasObra { get; set; }
        public List<Comentario> LstComentarios { get; set; }
        public int Oficina { get; set; }
        public int Instalacao { get; set; }
        [Display(Name = "Equipamento")]
        public string TipoEquipamento { get; set; }
        [Display(Name = "Periodo")]
        public string Periodo { get; set; }
        [Display(Name = "Incidente")]
        public string Referencia { get; set; }
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
