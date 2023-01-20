using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FT_Management.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FT_Management.Controllers
{
    public class EquipamentosController : Controller
    {

        public IActionResult Index(string Serie)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (string.IsNullOrEmpty(Serie)) Serie = "";
            ViewData["Serie"] = Serie;

            return View(phccontext.ObterEquipamentosSerie(Serie));
        }

        public IActionResult Detalhes(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return View(phccontext.ObterEquipamento(id));
        }
    }
}