using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FT_Management.Models
{
    public enum TipoFicheiro
    {
        Marcacao, Instalacao, Peca, Assinatura, Email

    }

    public class Anexo
    {
        [Display(Name = "Stamp do Anexo")]
        public string AnexoStamp { get; set; }
        [Display(Name = "Num. da Marcação")]
        public int IdMarcacao { get; set; }
        [Display(Name = "Stamp da Marcação")]
        public string MarcacaoStamp { get; set; }
        [Display(Name = "Ficheiro")]
        public string NomeFicheiro { get; set; }
        [Display(Name = "Descrição")]
        public string DescricaoFicheiro { get; set; }
        public bool AnexoMarcacao { get; set; }
        public bool AnexoAssinatura { get; set; }
        public bool AnexoInstalacao { get; set; }
        public bool AnexoPeca { get; set; }
        public string RefPeca { get; set; }
        public bool AnexoEmail { get; set; }
        
        [Display(Name = "Criado por")]
        public string NomeUtilizador { get; set; }
        [Display(Name = "Criado em")]
        public DateTime DataCriacao { get; set; }

        public TipoFicheiro ObterTipoFicheiro()
        {
            if (AnexoMarcacao) return TipoFicheiro.Marcacao;
            if (AnexoAssinatura) return TipoFicheiro.Assinatura;
            if (AnexoInstalacao) return TipoFicheiro.Instalacao;
            if (AnexoPeca) return TipoFicheiro.Peca;
            if (AnexoEmail) return TipoFicheiro.Email;

            return TipoFicheiro.Marcacao;
        }

        public string ObterNomeLegivel()
        {
            string res = "MARC" + IdMarcacao;

            if (ObterTipoFicheiro() == TipoFicheiro.Email) res = this.DescricaoFicheiro;
            if (ObterTipoFicheiro() == TipoFicheiro.Instalacao) res += " - Anexo de Instalação";
            if (ObterTipoFicheiro() == TipoFicheiro.Assinatura) res += " - Anexo de Assinatura";
            if (ObterTipoFicheiro() == TipoFicheiro.Peca) res += " - Anexo de Peça";
            if (ObterTipoFicheiro() == TipoFicheiro.Marcacao) res += " - Anexo da Marcação";

            return res;
        }
        public string ObterNomeFicheiro()
        {
            return (NomeFicheiro.Contains("/") ? NomeFicheiro.Split("/").Last().ToString() : (NomeFicheiro.Contains("\\") ? NomeFicheiro.Split("\\").Last() : NomeFicheiro));
        }
        public string ObterNomeUnico()
        {
            string res = "MARC" + IdMarcacao + "_";
            if (ObterTipoFicheiro() == TipoFicheiro.Instalacao) res += "GT_";
            if (ObterTipoFicheiro() == TipoFicheiro.Assinatura) res += "SIGN_";
            if (ObterTipoFicheiro() == TipoFicheiro.Peca) res += "PECA_";

            res += DateTime.Now.ToString("yyyyMMddHHmmssf");

            return res;
        }
    }
}
