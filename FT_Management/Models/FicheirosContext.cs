namespace FT_Management.Models
{
    public static class FicheirosContext
    {

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
        public static byte[] ObterFicheiro(string Caminho)
        {
            try
            {
                if (File.Exists(FormatLinuxServer(Caminho)))
                {
                    return File.ReadAllBytes(FormatLinuxServer(Caminho));
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

        public static byte[] ObterFicheiroTemporario(string Ficheiro)
        {
            try
            {
                return ObterFicheiro(CaminhoTemporario + Ficheiro);
            }
            catch
            {
                return null;
            }
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
            return res.Replace("\\", "/").Replace("S:", "/Volumes/PHC");
#else
            return res.Replace("\\", "/").Replace("S:", "/server");
#endif

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
            File.Move(source, dest, true);
        }

    }
}
