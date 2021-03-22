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
        public string NomeCompleto { get; set; }
        public int TipoUtilizador { get; set; }
        public string EmailUtilizador { get; set; }
        public string IdCartaoTrello { get; set; }
        public bool Admin { get; set; }
        public bool Enable { get; set; }
    }
}
