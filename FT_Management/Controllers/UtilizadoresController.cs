using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FT_Management.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FT_Management.Controllers
{
    public class UtilizadoresController : Controller
    {
        public IActionResult Login(string nome, string password, string ReturnUrl)
        {
            Utilizador utilizador = new Utilizador {NomeUtilizador = nome, Password = password};
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            if (nome != null && password != null)
            {

                List<Utilizador> LstUtilizadores = context.ObterListaUtilizadores(true).Where(u => u.NomeUtilizador == utilizador.NomeUtilizador).ToList();

                if (LstUtilizadores.Count == 0) ModelState.AddModelError("", "Não foram encontrados utlizadores com esse nome!");

                foreach (var user in LstUtilizadores)
                {
                    var passwordHasher = new PasswordHasher<string>();
                    if (passwordHasher.VerifyHashedPassword(null, user.Password, utilizador.Password) == PasswordVerificationResult.Success)
                    {
                        var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Id.ToString()),
                        new Claim(ClaimTypes.GivenName, user.NomeCompleto),
                        new Claim(ClaimTypes.Role, user.Admin ? "Admin" : "User"),
                        new Claim(ClaimTypes.Role, user.TipoUtilizador == 1 ? "Tech" : user.TipoUtilizador == 2 ? "Comercial" : "Escritorio")

                    };
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                        context.AdicionarLog(utilizador.NomeUtilizador, "LOGIN SUCESSO", 4);

                        if (ReturnUrl != "" && ReturnUrl != null)
                        {
                            Response.Redirect(ReturnUrl, true);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Password errada!");
                        context.AdicionarLog(utilizador.NomeUtilizador, "LOGIN SEM SUCESSO", 4);
                    }
                }
            }
                return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Utilizador utilizador, string ReturnUrl)
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            List<Utilizador> LstUtilizadores = context.ObterListaUtilizadores(true).Where(u => u.NomeUtilizador == utilizador.NomeUtilizador).ToList();

            if (LstUtilizadores.Count == 0) ModelState.AddModelError("", "Não foram encontrados utlizadores com esse nome!");

            foreach (var user in LstUtilizadores)
            {
                var passwordHasher = new PasswordHasher<string>();
                if (passwordHasher.VerifyHashedPassword(null, user.Password, utilizador.Password) == PasswordVerificationResult.Success)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Id.ToString()),
                        new Claim(ClaimTypes.GivenName, user.NomeCompleto),
                        new Claim(ClaimTypes.Role, user.Admin ? "Admin" : "User"),
                        new Claim(ClaimTypes.Role, user.TipoUtilizador == 1 ? "Tech" : user.TipoUtilizador == 2 ? "Comercial" : "Escritorio")

                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await  HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    context.AdicionarLog(utilizador.NomeUtilizador, "LOGIN SUCESSO", 4);

                    if (ReturnUrl != "" && ReturnUrl != null)
                    {
                        Response.Redirect(ReturnUrl, true);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Password errada!");
                    context.AdicionarLog(utilizador.NomeUtilizador, "LOGIN SEM SUCESSO", 4);
                }
            }
            return View();
        }

        [Authorize(Roles = "Admin, Tech, Escritorio, Comercial")]
        public IActionResult Editar()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return View(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value.ToString())));
        }

        public IActionResult AtualizarUtilizador(string name, string email)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value.ToString()));

            u.NomeCompleto = name;
            u.EmailUtilizador = email;

            return RedirectToAction("Editar");
        }
        public IActionResult AtualizarSenha(string password_current, string password, string password_confirmation)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value.ToString()));

            if (password != password_confirmation) ModelState.AddModelError("", "Passwords não condizem");

            if (ModelState.IsValid)
            {
                var passwordHasher = new PasswordHasher<string>();
                if (passwordHasher.VerifyHashedPassword(null, u.Password, password_current) == PasswordVerificationResult.Success)
                {
                    u.Password = passwordHasher.HashPassword(null, password);
                }
            }
            else
            {
                return View("Editar", u);
            }

            return RedirectToAction("Editar");
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
