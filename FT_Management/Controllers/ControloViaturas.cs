using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FT_Management.Models;
using Microsoft.AspNetCore.Mvc;

namespace FT_Management.Controllers
{
    public class ControloViaturas : Controller
    {
        // GET: Viaturas
        public ActionResult Index()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            ViewData["Tecnicos"] = context.ObterListaUtilizadores().Where(u => u.TipoUtilizador != "3").ToList();

            return View(context.ObterViaturas());

        }

        [HttpPost]
        public ActionResult LevantarViatura(string data, string kms, string tecnico)
        {

            return Content("");

        }

    }
}
