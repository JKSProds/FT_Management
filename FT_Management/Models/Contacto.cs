using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FT_Management.Models
{
    public class Contacto
    {
        //ID | Nome | Morada | Pessoa Contacto | Email | Telemovel | NIF | Observacoes 
        [Display(Name = "ID")]
        public int IdContacto { get; set; }
        [Display(Name = "Nome da Empresa")]
        [Required]
        public string NomeContacto { get; set; }
        [Display(Name = "Morada")]
        public string MoradaContacto { get; set; }
        [Display(Name = "Pessoa de Contacto")]
        [Required]
        public string PessoaContacto { get; set; }
        [Display(Name = "Cargo")]
        public string CargoPessoaContacto { get; set; }
        [Display(Name = "Email")]
        [Required]
        public string EmailContacto { get; set; }
        [Display(Name = "Telemóvel")]
        [Required]
        public string TelefoneContacto { get; set; }
        [Display(Name = "Número de Contribuinte")]
        public string NIFContacto { get; set; }
        [Display(Name = "Observações")]
        public string Obs { get; set; }
        [Display(Name = "Tipo de Contacto")]
        public string TipoContacto { get; set; }
        public string URL { get; set; }
        public DateTime DataContacto { get; set; }
        public String AreaNegocio { get; set; }
        public bool ValidadoPorAdmin { get; set; }
        public int IdCliente { get; set; }
        public int IdLoja { get; set; }
        public Cliente Cliente { get; set; }
        public int IdUtilizador { get; set; }
        public int IdComercial { get; set; }
        public Utilizador Comercial { get; set; }
        public Utilizador Utilizador { get; set; }
        public List<HistoricoContacto> Historico { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Contacto()
        {
            NomeContacto = string.Empty;
        }


        public void CheckNull()
        {
            NomeContacto = NomeContacto is null ? "" : NomeContacto;
            PessoaContacto = PessoaContacto is null ? "" : PessoaContacto;
            TelefoneContacto = TelefoneContacto is null ? "" : TelefoneContacto;
            EmailContacto = EmailContacto is null ? "" : EmailContacto;
            NIFContacto = NIFContacto is null ? "" : NIFContacto;
            MoradaContacto = MoradaContacto is null ? "" : MoradaContacto;
            Obs = Obs is null ? "" : Obs;
        }

    }
    public class HistoricoContacto
    {

        public int Id { get; set; }
        public int IdContacto { get; set; }
        public Utilizador IdComercial { get; set; }
        public DateTime Data { get; set; }
        public string Obs { get; set; }

    }
}
