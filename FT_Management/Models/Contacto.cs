using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FT_Management.Models
{
    public class Contacto
    {
        //ID | Nome | Morada | Pessoa Contacto | Email | Telemovel | NIF | Observacoes 
        [Display(Name = "Num. do Contacto")]
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
        [Display(Name = "Link/Anexos")]
        public string URL { get; set; }
        [Display(Name = "Data do Contacto")]
        public DateTime DataContacto { get; set; }
        [Display(Name = "Área de Negócio")]
        public string AreaNegocio { get; set; }
        [Display(Name = "Validado por Admin?")]
        public bool ValidadoPorAdmin { get; set; }
        [Display(Name = "Num. do Cliente")]
        public int IdCliente { get; set; }
        [Display(Name = "Ñum. do Estabelecimento")]
        public int IdLoja { get; set; }
        [Display(Name = "Cliente")]
        public Cliente Cliente { get; set; }
        [Display(Name = "Num. do Utilizador")]
        public int IdUtilizador { get; set; }
        [Display(Name = "Num. do Comercial")]
        public int IdComercial { get; set; }
        [Display(Name = "Comercial")]
        public Utilizador Comercial { get; set; }
        [Display(Name = "Criador por:")]
        public Utilizador Utilizador { get; set; }
        [Display(Name = "Histórico")]
        public List<HistoricoContacto> Historico { get; set; }
        [Display(Name = "Contactos Adicionais")]
        public List<ContactosAdicionais> ContactosAdicionais { get; set; }
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
        [Display(Name = "Num. da Observação")]
        public int Id { get; set; }
        [Display(Name = "Num. do Contacto")]
        public int IdContacto { get; set; }
        [Display(Name = "Comercial")]
        public Utilizador IdComercial { get; set; }
        [Display(Name = "Data")]
        public DateTime Data { get; set; }
        [Display(Name = "Comentário")]
        public string Obs { get; set; }

    }

    public class ContactosAdicionais
    {
        [Display(Name = "Num. do Contacto Adicional")]
        public int Id { get; set; }
        [Display(Name = "Num. do Contacto")]
        public int IdContacto { get; set; }
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
        public string CriadoPor { get; set; }
    }
}
