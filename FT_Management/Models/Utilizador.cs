using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public static class ClaimsPrincipalExtension
    {
        public static string ObterNomeCompleto(this ClaimsPrincipal principal)
        {
            var firstName = principal.Claims.Where(c=> c.Type.Contains("givenname")).First();
            return firstName?.Value;
        }

      
    }
    public class Utilizador
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome de utilizador é obrigatório!")]
        [Display(Name = "Nome de Utilizador")]
        public string NomeUtilizador { get; set; }
        [Required(ErrorMessage = "A password é obrigatória!")]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; }
        [Display(Name = "Tipo de Utilizador")]
        public int TipoUtilizador { get; set; }
        [Display(Name = "Email")]
        public string EmailUtilizador { get; set; }
        [Display(Name = "Nº Telemóvel")]
        public string Telemovel { get { return ObterTelemovelLegivel(_Telemovel); } set { this._Telemovel = value; } }
        private string _Telemovel;
        [Display(Name = "Num. do PHC")]
        public int IdPHC { get; set; }
        [Display(Name = "Num. do Armazém")]
        public int IdArmazem { get; set; }
        [Display(Name = "Admin?")]
        public bool Admin { get; set; }
        [Display(Name = "Ativo?")]
        public bool Enable { get; set; }
        [Display(Name = "Cor")]
        public string CorCalendario { get; set; }
        [Display(Name = "Iniciais")]
        public string Iniciais { get; set; }
        [Display(Name = "Pin")]
        public string Pin { get; set; }
        [Display(Name = "Imagem")]
        public string ImgUtilizador { get; set; }
        [Display(Name = "Api Key")]
        public ApiKey ApiKey { get; set; }
        [Display(Name = "Mapa")]
        public int TipoMapa { get; set; }
        [Display(Name = "Data de Nascimento")]
        public DateTime DataNascimento { get; set; }
        public Utilizador()
        {
            this.Id = 0;
            this.NomeUtilizador = "---";
            this.NomeCompleto = "N/D";
            this.Iniciais = "??";
            this.Pin = "1111";
            this.IdPHC = 0;
            this.IdArmazem = 0;
            this.ApiKey = new ApiKey();
        }

        public string ObterTelemovelFormatado(bool Extensao)
        {
            string res = "";
            try
            {
                var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
                if (Extensao)
                {
                    if (phoneNumberUtil.IsValidNumberForRegion(phoneNumberUtil.Parse(this._Telemovel, "PT"), "PT")) res = "+" + phoneNumberUtil.Parse(this._Telemovel, "PT").CountryCode.ToString() + phoneNumberUtil.Parse(this._Telemovel, "PT").NationalNumber.ToString();
                }
                else
                {
                    if (phoneNumberUtil.IsValidNumberForRegion(phoneNumberUtil.Parse(this._Telemovel, "PT"), "PT")) res = phoneNumberUtil.Parse(this._Telemovel, "PT").NationalNumber.ToString();
                }
            }
            catch
            {
            }

            return res;
        }

        public string ObterTelemovelLegivel(string res)
        {
            try
            {
                var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
                var phone = Enumerable.Range(0, phoneNumberUtil.Parse(res, "PT").NationalNumber.ToString().Length / 3).Select(i => phoneNumberUtil.Parse(res, "PT").NationalNumber.ToString().Substring(i * 3, 3));
                res = "+" + phoneNumberUtil.Parse(res, "PT").CountryCode.ToString() + " " + String.Join(" ", phone.ToList());
            }
            catch
            {
            }

            return res;

        }

        public static string ObterLinkMapa(string morada, string valor)
        {
           if (valor == "Waze") return "https://waze.com/ul?q=" + morada + "&navigate=yes";

           return "https://maps.google.com/?daddr=" + morada;
        }
    }


}
