using System;

namespace FT_Management.Models
{
    public enum TipoFicheiro
    {
        Marcacao, Instalacao, Peca, Assinatura

    }

    public class Anexo
    {
        public string AnexoStamp { get; set; }
        public int IdMarcacao { get; set; }
        public string MarcacaoStamp { get; set; }
        public string NomeFicheiro { get; set; }
        public bool AnexoMarcacao { get; set; }
        public bool AnexoAssinatura { get; set; }
        public bool AnexoInstalacao { get; set; }
        public bool AnexoPeca { get; set; }
        public string RefPeca { get; set; }
        public string NomeUtilizador { get; set; }
        public DateTime DataCriacao { get; set; }

        public TipoFicheiro ObterTipoFicheiro()
        {
            if (AnexoMarcacao) return TipoFicheiro.Marcacao;
            if (AnexoAssinatura) return TipoFicheiro.Assinatura;
            if (AnexoInstalacao) return TipoFicheiro.Instalacao;
            if (AnexoPeca) return TipoFicheiro.Peca;

            return TipoFicheiro.Marcacao;
        }

        public string ObterNomeLegivel()
        {
            string res = "MARC" + IdMarcacao;
            if (ObterTipoFicheiro() == TipoFicheiro.Instalacao) res += " - Anexo de Instalação";
            if (ObterTipoFicheiro() == TipoFicheiro.Assinatura) res += " - Anexo de Assinatura";
            if (ObterTipoFicheiro() == TipoFicheiro.Peca) res += " - Anexo de Peça";
            if (ObterTipoFicheiro() == TipoFicheiro.Marcacao) res += " - Anexo da Marcação";

            return res;
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
