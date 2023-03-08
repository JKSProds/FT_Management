namespace FT_Management.Models
{
    public static class FicheirosContext
    {

        private static string CaminhoServerAnexos = "S:\\Assistencias_Tecnicas\\";
        private static string CaminhoImagensProduto = "S:\\Imagens\\EQUIPAMENTOS\\";
        private static string CaminhoImagensUtilizador = "S:\\Imagens\\UTILIZADORES\\";
        private static string CaminhoTemporario = "S:\\WebApp\\";

        private static bool CriarPasta(string Caminho)
        {
            try
            {
                if (!Directory.Exists(Caminho)) Directory.CreateDirectory(Caminho);
            }
            catch
            {
                return false;
            }
            return true;
        }
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
            return res.Replace("\\", "/").Replace("S:", "/Volumes/phc");
#else
            return res.Replace("\\", "/").Replace("S:", "/server");
#endif

        }

        public static bool CriarAnexoMarcacao(MarcacaoAnexo a, IFormFile ficheiro)
        {
            return CriarFicheiro(FormatLinuxServer(a.NomeFicheiro), ficheiro);
        }
        public static bool CriarAnexoAssinatura(MarcacaoAnexo a, IFormFile ficheiro)
        {
            CriarPasta(FormatLinuxServer(CaminhoServerAnexos));
            return CriarFicheiro(FormatLinuxServer(CaminhoServerAnexos + a.NomeFicheiro), ficheiro);
        }
        public static bool ApagarAnexoMarcacao(MarcacaoAnexo a)
        {
            return ApagarFicheiro(FormatLinuxServer(a.NomeFicheiro));
        }
        public static string ObterCaminhoProdutoImagem(string Ref_Produto)
        {
            return FormatLinuxServer(CaminhoImagensProduto) + Ref_Produto + ".jpg";
        }
        public static string ObterCaminhoAssinatura(string res)
        {
            return FormatLinuxServer(res);
        }
        public static bool CriarImagemUtilizador(IFormFile ficheiro, string NomeUtilizador)
        {
            string FullPath = CaminhoImagensUtilizador + NomeUtilizador + "\\";
            CriarPasta(FormatLinuxServer(FullPath));
            return CriarFicheiro(FormatLinuxServer(FullPath + ficheiro.FileName), ficheiro);
        }

        public static void GestaoFicheiros(bool ImagensUtilizador, bool Temp)
        {
#if !DEBUG
            if (Directory.Exists(FormatLinuxServer(CaminhoImagensUtilizador)) && ImagensUtilizador)
            {
                CloneDirectory(FormatLinuxServer(CaminhoImagensUtilizador), FormatLinuxServer(Directory.GetCurrentDirectory() + "\\wwwroot\\img\\"));
            }

            if (Directory.Exists(FormatLinuxServer(CaminhoTemporario)) && Temp)
            {
                foreach (FileInfo file in new DirectoryInfo(FormatLinuxServer(CaminhoTemporario)).EnumerateFiles())
                {
                    file.Delete();
                }
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

        private static void MoveFile(string source, string dest)
        {
            File.Move(source, dest);
        }

        public static string CriarFicheiroTemporario(string nome, IFormFile ficheiro)
        {
            string res = FormatLinuxServer(CaminhoTemporario + nome);
            CriarPasta(FormatLinuxServer(CaminhoTemporario));
            return CriarFicheiro(res, ficheiro) ? nome : "";
        }
        public static bool ExisteFicheiroTemporario(string nome)
        {
            return File.Exists(FormatLinuxServer(CaminhoTemporario + nome));
        }
        public static void MoverFicheiroTemporario(string source, string dest)
        {
            if (!string.IsNullOrEmpty(dest) && !string.IsNullOrEmpty(source))
            {
                CriarPasta(FormatLinuxServer(dest));
                MoveFile(FormatLinuxServer(CaminhoTemporario + source), FormatLinuxServer(dest + source));
            }
        }

        public static string CriarAnexo(string dest, string nome, IFormFile ficheiro)
        {
            string res = FormatLinuxServer(dest + nome);
            CriarPasta(FormatLinuxServer(dest));
            return CriarFicheiro(res, ficheiro) ? nome : "";
        }
    }
}
