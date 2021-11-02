using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;

namespace FT_Management.Controllers
{
    public class PedidosDiariosViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Marcacao> model)
        {
            return View(model.Where(m => m.DataMarcacao == DateTime.Now.Date).OrderBy(e => e.EstadoMarcacao));
        }
    }
    public class PedidoOrcamentoViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Marcacao> model)
        {
            return View(model.Where(m => m.EstadoMarcacao == 8));
        }
    }
    public class PedidoPecasViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Marcacao> model)
        {
            return View(model.Where(m => m.EstadoMarcacao == 7));
        }
    }
    public class AguardarClienteViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Marcacao> model)
        {
            return View(model.Where(m => m.EstadoMarcacao == 10 || m.EstadoMarcacao == 6 || m.EstadoMarcacao == 13));
        }
    }
    public class OficinaViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Marcacao> model)
        {
            return View(model.Where(m => m.EstadoMarcacao == 9 || m.EstadoMarcacao == 12 || m.Oficina == 1));
        }
    }
        [Authorize(Roles = "Admin, Escritorio")]
        public class DashboardController : Controller
        {

            public IActionResult Index()
            {
                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            phccontext.AtualizarMarcacoes();

            return View(context.ObterListaMarcacoesPendentes());
            }
        }
}
