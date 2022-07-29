using Microsoft.AspNetCore.Http;
using System.IO;

namespace FT_Management.Models
{
    public static class FicheirosContext
    {

        //private static string CaminhoServerAnexos = "/server/Assistencias_Tecnicas/";
        private static string CaminhoImagensProduto = "/server/Imagens/EQUIPAMENTOS/";

        private static bool CriarFicheiro(string Caminho, IFormFile ficheiro)
        {
            try
            {
                using (Stream fileStream = new FileStream(Caminho, FileMode.Create))
                {
                    ficheiro.CopyTo(fileStream);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        private static bool ApagarFicheiro(string Caminho)
        {
            try
            {
                using (Stream fileStream = new FileStream(Caminho, FileMode.Create))
                {
                    File.Delete(Caminho);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static string FormatLinuxServer(string res)
        {
#if DEBUG 
    return res;
#endif
            return res.Replace("\\", "/").Replace("S:", "/server");
        }

        public static bool CriarAnexoMarcacao(Anexo a, IFormFile ficheiro)
        {
            return CriarFicheiro(FormatLinuxServer(a.NomeFicheiro), ficheiro);
        }
        public static bool ApagarAnexoMarcacao(Anexo a)
        {
            return ApagarFicheiro(FormatLinuxServer(a.NomeFicheiro));
        }
        public static string ObterCaminhoProdutoImagem(string Ref_Produto)
        {
            return CaminhoImagensProduto + Ref_Produto + ".jpg";
        }
        public static string ObterCaminhoAssinatura(string res)
        {
            return FormatLinuxServer(res);
        }
    }
}
