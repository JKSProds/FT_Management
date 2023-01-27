using Microsoft.AspNetCore.Http;
using System.IO;

namespace FT_Management.Models
{
    public static class FicheirosContext
    {

        private static string CaminhoServerAnexos = "/server/Assistencias_Tecnicas/";
        private static string CaminhoImagensProduto = "/server/Imagens/EQUIPAMENTOS/";
        private static string CaminhoImagensUtilizador = "S:\\Imagens\\UTILIZADORES\\";

        private static bool CriarFicheiro(string Caminho, IFormFile ficheiro)
        {
            try
            {
                System.Console.WriteLine("A criar ficheiro na pasta: " + Caminho + " (" + ficheiro.FileName + "|" + ficheiro.Length + ")!");
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
#else
return res.Replace("\\", "/").Replace("S:", "/server");
#endif

        }

        public static bool CriarAnexoMarcacao(Anexo a, IFormFile ficheiro)
        {
            return CriarFicheiro(FormatLinuxServer(a.NomeFicheiro), ficheiro);
        }
        public static bool CriarAnexoAssinatura(Anexo a, IFormFile ficheiro)
        {
            return CriarFicheiro(FormatLinuxServer(CaminhoServerAnexos + a.NomeFicheiro), ficheiro);
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
        public static bool CriarImagemUtilizador(IFormFile ficheiro, string NomeUtilizador)
        {
            string FullPath = CaminhoImagensUtilizador + NomeUtilizador + "\\";
            if (!Directory.Exists(FormatLinuxServer(FullPath))) Directory.CreateDirectory(FormatLinuxServer(FullPath));
            return CriarFicheiro(FormatLinuxServer(FullPath + ficheiro.FileName), ficheiro);
        }

        public static void ObterImagensUtilizador()
        {
#if !DEBUG 
            if (Directory.Exists(FormatLinuxServer(CaminhoImagensUtilizador)))
            {
                CloneDirectory(FormatLinuxServer(CaminhoImagensUtilizador), FormatLinuxServer(Directory.GetCurrentDirectory() + "\\wwwroot\\img\\"));
            }
              
#endif
        }

        private static void CloneDirectory(string root, string dest)
        {
            foreach (var directory in Directory.GetDirectories(root))
            {
                //Get the path of the new directory
                var newDirectory = Path.Combine(dest, Path.GetFileName(directory));
                if (Directory.Exists(newDirectory)) Directory.Delete(newDirectory, true);
                //Create the directory if it doesn't already exist
                Directory.CreateDirectory(newDirectory);
                //Recursively clone the directory
                CloneDirectory(directory, newDirectory);
            }

            foreach (var file in Directory.GetFiles(root))
            {
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)));
            }
        }
    }
}
