using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FT_Management.Controllers
{
    [Authorize]
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
            ViewData["Ano"] = context.ObterAnoAtivo();
            if (!User.IsInRole("Admin") && !User.IsInRole("Escritorio"))
            {
                if (int.Parse(this.User.Claims.First().Value) != IdUtilizador)
                {
                    return RedirectToAction("Detalhes", new { IdUtilizador = int.Parse(this.User.Claims.First().Value) });
                }
            }

            ViewData["IdUtilizador"] = IdUtilizador;
            return View(context.ObterListaFerias(IdUtilizador));
        }

        public ActionResult Validar(Ferias ferias)
        {
            List<Ferias> LstFerias = new List<Ferias>();
            ferias.Validado = true;
            ferias.ValidadoPor = int.Parse(this.User.Claims.First().Value);
            LstFerias.Add(ferias);

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            context.CriarFerias(LstFerias);

            return RedirectToAction("Detalhes", new { IdUtilizador = ferias.IdUtilizador });
        }

        public ActionResult Apagar(Ferias ferias)
        {

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            context.ApagarFerias(ferias.Id);

            return RedirectToAction("Detalhes", new { IdUtilizador = ferias.IdUtilizador });
        }

        public void AdicionarFerias(string datainicio, string datafim, int idutilizador) {
            List<Ferias> LstFerias = new List<Ferias>
            {
                new Ferias
                {
                    IdUtilizador = idutilizador,
                    DataInicio = DateTime.Parse(datainicio),
                    DataFim = DateTime.Parse(datafim),
                    ValidadoPor = 0
                }
            };

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            context.CriarFerias(LstFerias);
        }
    }
}
