﻿namespace FT_Management.Models
{
    public class Viatura
    {
        public string Matricula { get; set; }
        public string LocalizacaoMorada { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string KmsAtuais { get; set; }
        public Utilizador Utilizador { get; set; }
    }
}