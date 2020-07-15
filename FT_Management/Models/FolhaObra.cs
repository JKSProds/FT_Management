using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class Intervencao
    {
        public int IdIntervencao { get; set; }
        public int IdTecnico { get; set; }
        public int IdFolhaObra { get; set; }
        [Display(Name = "Técnico")]
        public string NomeTecnico { get; set; }
        [Display(Name = "Data do Serviço")]
        public DateTime DataServiço { get; set; }
        [Display(Name = "Hora de Inicio")]
        public DateTime HoraInicio { get; set; }
        [Display(Name = "Hora de Fim")]
        public DateTime HoraFim { get; set; }
    }

    public class Equipamento
    {
        public int IdEquipamento { get; set; }
        [Display(Name = "Designação do Equipamento")]
        public string DesignacaoEquipamento { get; set; }
        [Display(Name = "Marca")]
        public string MarcaEquipamento { get; set; }
        [Display(Name = "Modelo")]
        public string ModeloEquipamento { get; set; }
        [Required(ErrorMessage = "Número de Série é Obrigatório")]
        [Display(Name = "Numero de Série")]
        public string NumeroSerieEquipamento { get; set; }
    }
    public class Cliente
    {
        public int IdCliente { get; set; }
        [Required(ErrorMessage = "Nome do Cliente é Obrigatório")]
        [Display(Name = "Nome do Cliente")]
        public string NomeCliente { get; set; }
        [Display(Name = "Pessoa de Contacto")]
        public string PessoaContatoCliente { get; set; }
        [Display(Name = "Morada")]
        public string MoradaCliente { get; set; }
        [Display(Name = "Email")]
        public string EmailCliente { get; set; }
        [Display(Name = "NIF")]
        public string NumeroContribuinteCliente { get; set; }
    }
    public class FolhaObra
    {
        public int IdFolhaObra { get; set; }
        [Display(Name = "Data")]
        public DateTime DataServico { get; set; }
        [Display(Name = "Referência")]
        public string ReferenciaServico { get; set; }
        [Display(Name = "Estado do Equipamento")]
        public string EstadoEquipamento { get; set; }
        [DataType(DataType.MultilineText)]
        [Display(Name = "Relatório do Serviço")]
        public string RelatorioServico { get; set; }
        [DataType(DataType.MultilineText)]
        [Display(Name = "Situações Pendentes")]
        public string SituacoesPendentes { get; set; }
        public List<Produto> PecasServico { get; set; }
        public List<Intervencao> IntervencaosServico { get; set; }
        public Equipamento EquipamentoServico { get; set; }
        public Cliente ClienteServico { get; set; }
        public string IdCartao { get; set; }
        [Display(Name = "Conferido por")]
        public string ConferidoPor { get; set; }
        [Display(Name = "Número da tua Guia de Transporte")]
        public string GuiaTransporteAtual { get; set; }
        [Display(Name = "Assistência Remota?")]
        public bool AssistenciaRemota { get; set; }
    }
}
