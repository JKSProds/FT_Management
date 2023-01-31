using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class FolhaObra
    {
        public string StampFO { get; set; }
        [Display(Name = "Num. da Folha de Obra")]
        public int IdFolhaObra { get; set; }
        [Display(Name = "Num. da Assistência Técnica")]
        public string IdAT { get; set; }
        [Display(Name = "Num. da Marcação")]
        public int IdMarcacao { get; set; }
        public Marcacao Marcacao { get; set; }
        [Display(Name = "Data")]
        [Required]
        public DateTime DataServico { get; set; }
        private string _ReferenciaServico;
        [Display(Name = "Referência")]
        [Required]
        public string ReferenciaServico { get { return _ReferenciaServico ?? ""; } set { _ReferenciaServico = value; } }
        [Display(Name = "Estado do Equipamento")]
        public string EstadoEquipamento { get; set; }
        private string _RelatorioServico;
        [DataType(DataType.MultilineText)]
        [Display(Name = "Relatório do Serviço")]
        [Required]
        public string RelatorioServico { get { return _RelatorioServico ?? ""; } set { _RelatorioServico = value; } }
        private string _SituacoesPendentes;
        [DataType(DataType.MultilineText)]
        [Display(Name = "Observações Internas")]
        public string SituacoesPendentes { get { return _SituacoesPendentes ?? ""; } set { _SituacoesPendentes = value; } }
        [Display(Name = "Lista de Peças")]
        public string ListaPecas { get; set; }
        public List<Produto> PecasServico { get; set; }
        [Display(Name = "Lista de Intervenções")]
        [Required]
        public string ListaIntervencoes { get; set; }
        public List<Intervencao> IntervencaosServico { get; set; }
        [Required]
        public Equipamento EquipamentoServico { get; set; }
        [Required]
        public Cliente ClienteServico { get; set; }
        public Utilizador Utilizador { get; set; }
        public string IdCartao { get; set; }
        private string _ConferidoPor;
        [Display(Name = "Conferido por")]
        [Required]
        public string ConferidoPor { get { return _ConferidoPor ?? ""; } set { _ConferidoPor = value; } }
        private string _GuiaTransporteAtual;
        [Display(Name = "Número da tua Guia de Transporte")]
        public string GuiaTransporteAtual { get { return _GuiaTransporteAtual ?? ""; } set { _GuiaTransporteAtual = value; } }
        [Display(Name = "Assistência Remota?")]
        public bool AssistenciaRemota { get; set; }
        [Display(Name = "Rúbrica")]
        public string RubricaCliente { get; set; }
        public bool Piquete { get; set; }
        public string GetUrl { get { return "http://webapp.food-tech.pt/FolhasObra/Detalhes/" + IdFolhaObra; } }
        [Display(Name = "Estado do Serviço")]
        public int EstadoFolhaObra { get; set; }
        [Display(Name = "Tipo de Serviço")]
        public string TipoFolhaObra { get; set; }
        [Display(Name = "Em Garantia")]
        public bool EmGarantia { get; set; }
        [Display(Name = "Avisar Cliente")]
        public bool Avisar { get; set; }
        [Display(Name = "Recolher para Oficina")]
        public bool RecolhaOficina { get; set; }
        public bool CobrarDeslocacao { get; set; }
        public bool Instalação { get; set; }
        public bool EnviarEmail { get; set; }
        public string EmailCliente { get; set; }
        public bool GuardarLocalizacao { get; set; }
        public bool FecharMarcacao { get; set; }
        public double ValorTotal { get { return PecasServico.Sum(p => p.Valor); } }
        public double KmsDeslocacao { get; set; }

        public FolhaObra()
        {
            ListaIntervencoes = "";
            ListaPecas = "";
            EquipamentoServico = new Equipamento();
            this.PecasServico = new List<Produto>();
            this.IntervencaosServico = new List<Intervencao>()
            {
                new Intervencao()
                {
                    NomeTecnico = "N/D",
                    HoraInicio = DateTime.Parse(DateTime.Now.AddHours(-1).Hour.ToString() + ":00"),
                    HoraFim = DateTime.Parse(DateTime.Now.Hour.ToString() + ":00")
                }
            };
        }

        public FolhaObra PreencherDadosMarcacao(Marcacao m)
        {
            this.ClienteServico = m.Cliente;
            this.DataServico = m.DataMarcacao;
            this.ReferenciaServico = m.Referencia;
            this.IdMarcacao = m.IdMarcacao;
            this.EmailCliente = string.IsNullOrEmpty(m.Cliente.EmailCliente) ? m.QuemPediuEmail : m.Cliente.EmailCliente;
            this.Piquete = m.Piquete;
            this.TipoFolhaObra = m.TipoServico;
            return this;
        }

        public void PreencherViagem(Viagem v)
        {
            this.IntervencaosServico.First().HoraInicio = v.Fim_Viagem;
            this.IntervencaosServico.First().HoraFim = DateTime.Now;
            this.KmsDeslocacao = double.Parse(v.Distancia_Viagem);
        }

        public void ValidarIntervencoes()
        {
            this.IntervencaosServico.Clear();
            if (this.ListaIntervencoes == null) return;
            foreach (var item in this.ListaIntervencoes.Split(";"))
            {
                if (item != "")
                {
                    this.IntervencaosServico.Add(new Intervencao
                    {
                        HoraInicio = DateTime.Parse(item.Split("|").First()),
                        HoraFim = DateTime.Parse(item.Split("|").Last()),
                        DataServiço = this.DataServico,
                        RelatorioServico = this.RelatorioServico,
                        IdTecnico = this.Utilizador.IdPHC,
                        NomeTecnico = this.Utilizador.NomeCompleto,
                        IdFolhaObra = this.IdFolhaObra
                    });
                }
            }
        }
        public void ValidarTipoFolhaObra()
        {
            if (this.TipoFolhaObra == "Interno")
            {
                this.CobrarDeslocacao = false;
            }
            else if (this.TipoFolhaObra == "Externo")
            {
                this.CobrarDeslocacao = true;
            }
            else if (this.TipoFolhaObra == "Remoto")
            {
                this.CobrarDeslocacao = false;
                this.AssistenciaRemota = true;
            }
            else if (this.TipoFolhaObra == "Instalação")
            {
                this.CobrarDeslocacao = false;
                this.Instalação = true;
            }
        }
        public void ValidarPecas()
        {
            this.PecasServico.Clear();
            if (this.ListaPecas == null) return;
            foreach (var item in this.ListaPecas.Split(";"))
            {
                if (item != "")
                {
                    if (this.PecasServico.Where(p => p.Ref_Produto == item).Count() == 0)
                    {
                        this.PecasServico.Add(new Produto
                        {
                            Ref_Produto = item,
                            Stock_Fisico = 1
                        });
                    }
                    else
                    {
                        this.PecasServico.Where(p => p.Ref_Produto == item).First().Stock_Fisico += 1;
                    }

                }
            }
        }
    }
}
