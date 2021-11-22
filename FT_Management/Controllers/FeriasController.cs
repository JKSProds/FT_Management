using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FT_Management.Models;
using Microsoft.AspNetCore.Mvc;

namespace FT_Management.Controllers
{
    public class FeriasController : Controller
    {

        public ActionResult Index()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            if (!User.IsInRole("Admin") && !User.IsInRole("Escritorio"))
            {
                return RedirectToAction("Detalhes", new { IdUtilizador = int.Parse(this.User.Claims.First().Value)});
            }

            return View(context.ObterListaUtilizadores());
        }

        public ActionResult Detalhes(int IdUtilizador)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            if (int.Parse(this.User.Claims.First().Value) != IdUtilizador)
            {
                return RedirectToAction("Detalhes", new { IdUtilizador = int.Parse(this.User.Claims.First().Value)});
            }
            ViewData["IdUtilizador"] = IdUtilizador;
            return View(context.ObterListaFerias(IdUtilizador));
        }

        public ActionResult Validar(Ferias ferias)
        {
            List<Ferias> LstFerias = new List<Ferias>();
            ferias.Validado = true;
            LstFerias.Add(ferias);

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            context.CriarFerias(LstFerias);

            return RedirectToAction("Detalhes", new { IdUtilizador = ferias.IdUtilizador });
        }

        public void AdicionarFerias(string datainicio, string datafim, int idutilizador) {
            List<Ferias> LstFerias = new List<Ferias>
            {
                new Ferias
                {
                    IdUtilizador = idutilizador,
                    DataInicio = DateTime.Parse(datainicio),
                    DataFim = DateTime.Parse(datafim)
                }
            };

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            context.CriarFerias(LstFerias);

        }
    }
}
