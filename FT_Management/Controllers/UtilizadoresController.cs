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
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FT_Management.Controllers
{
    public class UtilizadoresController : Controller
    {
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return View(context.ObterListaUtilizadores(false, false));
        }
            public IActionResult Login(string nome, string password, string ReturnUrl)
        {
            Utilizador utilizador = new Utilizador {NomeUtilizador = nome, Password = password};
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            if (nome != null && password != null)
            {

                List<Utilizador> LstUtilizadores = context.ObterListaUtilizadores(true,false).Where(u => u.NomeUtilizador == utilizador.NomeUtilizador).ToList();

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
                        new Claim(ClaimTypes.Thumbprint, user.ImgUtilizador),
                        new Claim(ClaimTypes.Role, user.Id == 1 ? "Master" : ""),
                        new Claim(ClaimTypes.Role, user.Admin ? "Admin" : "User"),
                        new Claim(ClaimTypes.Role, user.TipoUtilizador == 1 ? "Tech" : user.TipoUtilizador == 2 ? "Comercial" : "Escritorio"),
                        new Claim(ClaimTypes.UserData, user.TipoMapa == 1 ? "Google Maps" : (user.TipoMapa == 2 ? "Waze" : "Apple"))

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
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            List<Utilizador> LstUtilizadores = context.ObterListaUtilizadores(true,false).Where(u => u.NomeUtilizador == utilizador.NomeUtilizador).ToList();

            if (LstUtilizadores.Count == 0) {
                Cliente c = phccontext.ObterClienteNIF(utilizador.NomeUtilizador);
                if (c.IdCliente != 0)
                {
                    if(c.Senha == utilizador.Password)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, c.NumeroContribuinteCliente),
                            new Claim(ClaimTypes.GivenName, c.NomeCliente),
                            new Claim(ClaimTypes.Role, "Cliente")
                        };
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                        context.AdicionarLog(c.IdCliente, "Cliente login com sucesso!", 4);
                        return RedirectToAction("Adicionar", "RMA");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Password errada!");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Não foram encontrados utlizadores com esse nome!");
                }
            }

            foreach (var user in LstUtilizadores)
            {
                var passwordHasher = new PasswordHasher<string>();
                if (passwordHasher.VerifyHashedPassword(null, user.Password, utilizador.Password) == PasswordVerificationResult.Success)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Id.ToString()),
                        new Claim(ClaimTypes.GivenName, user.NomeCompleto),
                        new Claim(ClaimTypes.Thumbprint, user.ImgUtilizador),
                        new Claim(ClaimTypes.Role, user.Id == 1 ? "Master" : ""),
                        new Claim(ClaimTypes.Role, user.Admin ? "Admin" : "User"),
                        new Claim(ClaimTypes.Role, user.TipoUtilizador == 1 ? "Tech" : user.TipoUtilizador == 2 ? "Comercial" : "Escritorio"),
                        new Claim(ClaimTypes.UserData, user.TipoMapa == 1 ? "Google Maps" : (user.TipoMapa == 2 ? "Waze" : "Apple"))

                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await  HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    context.AdicionarLog(user.Id, "Utilizador realizou um login com sucesso!", 4);

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
                    context.AdicionarLog(user.Id, "Utilizador tentou um login sem sucesso!", 4);
                }
            }
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Logs(int id, string Data)
        {
            if (Data == null || Data == string.Empty) Data = DateTime.Now.ToString("dd-MM-yyyy");
            ViewData["Data"] = Data;

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            ViewData["NomeUtilizador"] = context.ObterUtilizador(id).NomeUtilizador;

            return View(context.ObterListaLogs(id).Where(l => l.Data > DateTime.Parse(Data) && l.Data < DateTime.Parse(Data).AddDays(1)));
        }

        [Authorize(Roles = "Admin, Tech, Escritorio, Comercial")]
        public IActionResult Editar(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            if (id == 0) id = int.Parse(this.User.Claims.First().Value.ToString());
            if (!User.IsInRole("Admin") && id != int.Parse(this.User.Claims.First().Value)) return RedirectToAction("Editar", new { id = int.Parse(this.User.Claims.First().Value) });

            List<Viatura> LstViaturas = context.ObterViaturas();
            LstViaturas.Insert(0, new Viatura() { Matricula = "N/D" });
            ViewBag.Viaturas = LstViaturas.Select(l => new SelectListItem() { Value = l.Matricula, Text = l.Matricula });

            List<Zona> LstZonas = context.ObterZonas();
            LstZonas.Insert(0, new Zona() { Id = 0, Valor = "N/D" });
            ViewBag.Zonas = LstZonas.Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Valor });

            List<TipoTecnico> LstTipoTecnicos = context.ObterTipoTecnicos();
            LstTipoTecnicos.Insert(0, new TipoTecnico() { Id = 0, Valor = "N/D" });
            ViewBag.TipoTecnico = LstTipoTecnicos.Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Valor });

            return View(context.ObterUtilizador(id));
        }
        [Authorize(Roles = "Admin, Tech, Escritorio, Comercial")]
        public IActionResult AtualizarUtilizador(int id, Utilizador utilizador)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(id);

            u.Pin = utilizador.Pin;
            u.Iniciais = utilizador.Iniciais;
            u.CorCalendario = utilizador.CorCalendario;
            u.TipoMapa = utilizador.TipoMapa;
            u.Telemovel = utilizador.Telemovel;
            u.DataNascimento = utilizador.DataNascimento;
            u.Viatura.Matricula = utilizador.Viatura.Matricula == "N/D" ? "" : utilizador.Viatura.Matricula;
            u.TipoTecnico = utilizador.TipoTecnico;
            u.Zona = utilizador.Zona;

            context.NovoUtilizador(u);

            return RedirectToAction("Editar", new { id = u.Id});
        }
        [HttpPost]
        public IActionResult AtualizarImagem(int id, IFormFile file)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(id);

            if (file.Length > 0)
            {
                FicheirosContext.CriarImagemUtilizador(file, u.NomeUtilizador);
            }

            u.ImgUtilizador = "/img/" + u.NomeUtilizador + "/" + file.FileName;

            context.NovoUtilizador(u);
            FicheirosContext.ObterImagensUtilizador();

            return RedirectToAction("Logout");
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
        public IActionResult Apagar(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(id);

            if (!u.Admin || this.User.IsInRole("Master")) context.ApagarUtilizador(u);

            return RedirectToAction("Index");
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
            if (string.IsNullOrEmpty(senha)) return Content("Nok");
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
