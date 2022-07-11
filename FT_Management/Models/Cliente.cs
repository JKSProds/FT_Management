using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FT_Management.Models
{
    public class Cliente
    {
        [Display(Name = "Num. do Cliente")]
        public int IdCliente { get; set; }
        [Display(Name = "Num. do Estabelecimento")]
        public int IdLoja { get; set; }
        private string _NomeCliente;
        [Required(ErrorMessage = "Nome do Cliente é Obrigatório")]
        [Display(Name = "Nome do Cliente")]
        public string NomeCliente { get { return _NomeCliente ?? ""; } set { _NomeCliente = value; } }
        private string _PessoaContactoCliente;
        [Display(Name = "Pessoa de Contacto")]
        public string PessoaContatoCliente { get { return _PessoaContactoCliente ?? ""; } set { _PessoaContactoCliente = value; } }
        private string _MoradaCliente;
        [Display(Name = "Morada")]
        public string MoradaCliente { get { return _MoradaCliente ?? ""; } set { _MoradaCliente = value; } }
        private string _EmailCliente;
        [Display(Name = "Email")]
        public string EmailCliente { get { return _EmailCliente ?? ""; } set { _EmailCliente = value; } }
        [Display(Name = "Telefone")]
        public string TelefoneCliente { get { return string.IsNullOrEmpty(_TelefoneCliente) ? "" : new string(_TelefoneCliente.Replace(" ", "").Take(9).ToArray()); } set { _TelefoneCliente = value; } }
        private string _TelefoneCliente;
        private string _NumeroContribuinteCliente;
        [Display(Name = "Numero de Contribuinte")]
        public string NumeroContribuinteCliente { get { return _NumeroContribuinteCliente ?? ""; } set { _NumeroContribuinteCliente = value; } }
        [Display(Name = "Num. do Vendedor")]
        public int IdVendedor { get; set; }
        [Display(Name = "Tipo de Cliente")]
        public string TipoCliente { get; set; }
        public List<Marcacao> Marcacoes { get; set; }
        public List<FolhaObra> FolhasObra { get; set; }
        public List<Visita> Visitas { get; set; }
        public List<Equipamento> Equipamentos { get; set; }
        public bool IsValidContrib()
        {
            string Contrib = NumeroContribuinteCliente;
            bool functionReturnValue = false;
            functionReturnValue = false;
            string[] s = new string[9];
            string Ss = null;
            string C = null;
            int i = 0;
            long checkDigit = 0;

            s[0] = Convert.ToString(Contrib[0]);
            s[1] = Convert.ToString(Contrib[1]);
            s[2] = Convert.ToString(Contrib[2]);
            s[3] = Convert.ToString(Contrib[3]);
            s[4] = Convert.ToString(Contrib[4]);
            s[5] = Convert.ToString(Contrib[5]);
            s[6] = Convert.ToString(Contrib[6]);
            s[7] = Convert.ToString(Contrib[7]);
            s[8] = Convert.ToString(Contrib[8]);

            if (Contrib.Length == 9)
            {
                C = s[0];
                if (s[0] == "1" || s[0] == "2" || s[0] == "5" || s[0] == "6" || s[0] == "9")
                {
                    checkDigit = Convert.ToInt32(C) * 9;
                    for (i = 2; i <= 8; i++)
                    {
                        checkDigit = checkDigit + (Convert.ToInt32(s[i - 1]) * (10 - i));
                    }
                    checkDigit = 11 - (checkDigit % 11);
                    if ((checkDigit >= 10))
                        checkDigit = 0;
                    Ss = s[0] + s[1] + s[2] + s[3] + s[4] + s[5] + s[6] + s[7] + s[8];
                    if ((checkDigit == Convert.ToInt32(s[8])))
                        functionReturnValue = true;
                }
            }
            return functionReturnValue;
        }
        public Cliente()
        {
            this.IdCliente = 0;
            this.IdLoja = 0;
            this.NomeCliente = "N/D";
        }
        public string ObterMoradaDirecoes()
        {
            if (IdCliente == 878) return "Pingo Doce " + NomeCliente.Replace("PD", "");
            if (IdCliente == 561) {
                if (NomeCliente.Contains("CNT")) return NomeCliente.Replace("CNT", "Continente");
                if (NomeCliente.Contains("MDL")) return NomeCliente.Replace("MDL", "Modelo");
                if (NomeCliente.Contains("BD")) return NomeCliente.Replace("BD", "Continete Bom Dia");
                if (NomeCliente.Contains("CBD")) return NomeCliente.Replace("CBD", "Continete Bom Dia");
                if (NomeCliente.Contains("BNJ")) return NomeCliente.Replace("BNJ", "Continete Bom Dia");
            };

            return MoradaCliente;
        }
    }
}
