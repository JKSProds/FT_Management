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
using System;

namespace FT_Management.Controllers
{
    public class UtilizadoresController : Controller
    {
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return View(context.ObterListaUtilizadores(false));
        }
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
                        new Claim(ClaimTypes.Role, user.Id == 1 ? "Master" : ""),
                        new Claim(ClaimTypes.Role, user.Admin ? "Admin" : "User"),
                        new Claim(ClaimTypes.Role, user.TipoUtilizador == 1 ? "Tech" : user.TipoUtilizador == 2 ? "Comercial" : "Escritorio")

                    };
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                        context.AdicionarLog(user.Id, "LOGIN SUCESSO", 4);

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
                        context.AdicionarLog(user.Id, "LOGIN SEM SUCESSO", 4);
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
                        new Claim(ClaimTypes.Role, user.Id == 1 ? "Master" : ""),
                        new Claim(ClaimTypes.Role, user.Admin ? "Admin" : "User"),
                        new Claim(ClaimTypes.Role, user.TipoUtilizador == 1 ? "Tech" : user.TipoUtilizador == 2 ? "Comercial" : "Escritorio")

                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await  HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    context.AdicionarLog(user.Id, "LOGIN SUCESSO", 4);

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
                    context.AdicionarLog(user.Id, "LOGIN SEM SUCESSO", 4);
                }
            }
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Logs(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            ViewData["NomeUtilizador"] = context.ObterUtilizador(id).NomeUtilizador;
            return View(context.ObterListaLogs(id));
        }

        [Authorize(Roles = "Admin, Tech, Escritorio, Comercial")]
        public IActionResult Editar(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            if (id == 0) id = int.Parse(this.User.Claims.First().Value.ToString());
            return View(context.ObterUtilizador(id));
        }
        [Authorize(Roles = "Admin, Tech, Escritorio, Comercial")]
        public IActionResult AtualizarUtilizador(int id, string name, string email, string pin, string iniciais, string cor)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(id);

            u.NomeCompleto = name;
            u.EmailUtilizador = email;
            u.Pin = pin;
            u.Iniciais = iniciais;
            u.CorCalendario = cor;

            context.NovoUtilizador(u);

            return View("Editar", u);
        }
        [Authorize(Roles = "Admin, Tech, Escritorio, Comercial")]
        public IActionResult AtualizarSenha(int id, string password_current, string password, string password_confirmation)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(id);

            if (password != password_confirmation) ModelState.AddModelError("", "Passwords não condizem");
            if (password.Length < 8) ModelState.AddModelError("", "Password demasiado pequena! Tem de ter pelo menos 8 digitos!");
            if (!password.Any(char.IsUpper)) ModelState.AddModelError("", "Tem de ter pelo menos uma letra maiscula!");
            if (!password.Any(char.IsNumber)) ModelState.AddModelError("", "Tem de ter pelo menos um número");

            if (ModelState.IsValid)
            {
                var passwordHasher = new PasswordHasher<string>();
                if (passwordHasher.VerifyHashedPassword(null, u.Password, password_current) == PasswordVerificationResult.Success)
                {
                    u.Password = passwordHasher.HashPassword(null, password);
                    context.NovoUtilizador(u);
                }
                else
                {
                    ModelState.AddModelError("", "Password atual incorreta!");
                    return View("Editar", u);
                }
            }
            else
            {
                return View("Editar", u);
            }

            return RedirectToAction("Logout");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult AlterarEstado(int id, bool estado)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(id);

            if (!u.Admin || this.User.IsInRole("Master")) u.Enable = estado;

            context.NovoUtilizador(u);

            return Content("Ok");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult AlterarAdmin(int id, bool admin)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(id);
            if (this.User.IsInRole("Master")) u.Admin = admin;

            context.NovoUtilizador(u);

            return Content("Ok");
        }
        public IActionResult GerarApiKey(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            if (!this.User.IsInRole("Admin")) id = int.Parse(this.User.Claims.First().Value.ToString());

            Utilizador u = context.ObterUtilizador(id);
            if ((u.Admin & !this.User.IsInRole("Master")) && u.Id != int.Parse(this.User.Claims.First().Value.ToString())) return Content("");
            return Content(context.NovaApiKey(u));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult ResetSenha(int id, string senha)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            var passwordHasher = new PasswordHasher<string>();

            Utilizador u = context.ObterUtilizador(id);
            if (!u.Admin || this.User.IsInRole("Master")) u.Password = passwordHasher.HashPassword(null, senha);

            context.NovoUtilizador(u);

            return Content("Ok");
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
