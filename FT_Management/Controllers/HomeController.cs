using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FT_Management.Models;
using System.Text.Json;
using System.Dynamic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;

namespace FT_Management.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        IHostApplicationLifetime applicationLifetime;

        public HomeController(ILogger<HomeController> logger, IHostApplicationLifetime appLifetime)
        {
            _logger = logger;
            applicationLifetime = appLifetime;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult ValidarCodigo(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            return View(context.ObterCodigo(id));
        }
        [Authorize(Roles = "Admin")]
        public IActionResult AprovarCodigo(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            context.AtualizarCodigo(id, 1, int.Parse(this.User.Claims.First().Value));
            return RedirectToAction("ValidarCodigo", "Home", new { id = id });
        }
        [Authorize(Roles = "Admin")]
        public IActionResult RejeitarCodigo(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            context.AtualizarCodigo(id, 1, int.Parse(this.User.Claims.First().Value));
            return RedirectToAction("ValidarCodigo", "Home", new { id = id });
        }
        public IActionResult AcessoNegado()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Restart()
        {
            applicationLifetime.StopApplication();
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
