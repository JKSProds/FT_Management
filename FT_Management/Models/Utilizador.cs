namespace FT_Management.Models
{
    public static class ClaimsPrincipalExtension
    {
        public static string ObterNomeCompleto(this ClaimsPrincipal principal)
        {
            var firstName = principal.Claims.Where(c => c.Type.Contains("givenname")).First();
            return firstName?.Value;
        }
        public static string ObterImgUtilizador(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Where(c => c.Type.Contains("thumbprint")).Count() == 0) return "";
            var img = principal.Claims.Where(c => c.Type.Contains("thumbprint")).First();
            return img?.Value;
        }

    }

    public class Zona
    {
        public int Id { get; set; }
        public string Valor { get; set; }
    }
    public class TipoTecnico
    {
        public int Id { get; set; }
        public string Valor { get; set; }
    }
    public class Utilizador
    {
        public int Id { get; set; }
        public string StampMoradaCargaDescarga { get; set; }
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

        public int IdFuncionario { get; set; }  
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
        public bool Aniversario { get { return DataNascimento.ToString("ddMM") == DateTime.Now.ToString("ddMM"); } }
        [Display(Name = "Ultimo Acesso")]
        public DateTime UltimoAcesso { get; set; }
        public bool AcessoAtivo { get; set; }
        public bool Acessos { get; set; }
        public bool Dev { get; set; }
        public bool Dashboard { get; set; }
        public int TipoTecnico { get; set; }
        public int Zona { get; set; }
        public string ChatToken { get; set; }
        public string FaceRec { get; set; }
        public int NotificacaoAutomatica { get; set; } //0 Desativado > 1 Email > 2 Nextcloud > 3 Ambos
        public string SecondFactorAuthStamp { get; set; }
        public string SecondFactorImgUrl { get; set; }
        public string SecondFactorAuthCode { get; set; }
        public bool Ferias { get; set; }
        public bool IsencaoHorario { get; set; }
        public int BancoHoras {get; set;}
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
            this.UltimoAcesso = new DateTime();
            this.ImgUtilizador = "/img/user.png";
            this.AcessoAtivo = false;
        }

        public string ObterTelemovelFormatado(bool Indicativo)
        {
            string res = "";
            try
            {
                var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
                if (!string.IsNullOrEmpty(this._Telemovel) && phoneNumberUtil.IsValidNumberForRegion(phoneNumberUtil.Parse(this._Telemovel, "PT"), "PT"))
                {
                    if (Indicativo) res = "+" + phoneNumberUtil.Parse(this._Telemovel, "PT").CountryCode.ToString();
                    res += phoneNumberUtil.Parse(this._Telemovel, "PT").NationalNumber.ToString();
                }
            }
            catch
            {
                throw new Exception("Erro ao ObterTelemovelFormato");
            }

            return res;
        }

        public string ObterTelemovelLegivel(string res)
        {
            try
            {
                if (!string.IsNullOrEmpty(res))
                {
                    var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
                    var phone = Enumerable.Range(0, phoneNumberUtil.Parse(res, "PT").NationalNumber.ToString().Length / 3).Select(i => phoneNumberUtil.Parse(res, "PT").NationalNumber.ToString().Substring(i * 3, 3));
                    res = "+" + phoneNumberUtil.Parse(res, "PT").CountryCode.ToString() + " " + String.Join(" ", phone.ToList());
                }
            }
            catch
            {
                throw new Exception("Erro ao ObterTelemovelLegivel");
            }

            return res;

        }
    }


}
