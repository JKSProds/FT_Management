using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;

namespace FT_Management.Controllers
{
    public class DashboardViewComponent : ViewComponent
    {
        [Authorize(Roles = "Admin, Escritorio")]
        public IViewComponentResult Invoke(List<Marcacao> model)
        {
            return View(model);
        }
    }
    public class PedidosDiariosViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Marcacao> model)
        {
            return View(model.Where(m => m.DataMarcacao == DateTime.Now.Date).OrderBy(e => e.EstadoMarcacao));
        }
    }
    public class InstalacoesViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Marcacao> model)
        {
            return View(model.Where(m => m.Instalacao == 1).OrderBy(e => e.EstadoMarcacao));
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
            return View(model.Where(m => m.EstadoMarcacao !=4 && m.Oficina == 1));
        }
    }
        [Authorize(Roles = "Admin, Escritorio")]
        public class DashboardController : Controller
        {

            public IActionResult Index()
            {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return View(phccontext.ObterMarcacoesPendentes());
            }

        public JsonResult ObterMarcacoesConcluidas30Dias()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            List<Marcacao> LstMarcacoes = context.ObterListaMarcacoesSimples(DateTime.Now.AddDays(-30), DateTime.Now).Where(m => m.EstadoMarcacao == 4).ToList();
            List<Utilizador> LstUtilizadores = context.ObterListaTecnicos();

            var data = LstMarcacoes.Select(m => m.DataMarcacao.ToShortDateString()).Distinct().ToList();
            var marcacoesFinalizadas = LstMarcacoes.GroupBy(i => i.DataMarcacao)
             .Select(i => i.Count());

            var tecnicosContagem = LstMarcacoes.GroupBy(i => i.IdTecnico).Select(group => new { tecnico = LstUtilizadores.Where(u => u.Id == group.Key).ToList(), marcacoesConcluidas = group.Count() }).OrderByDescending(i => i.marcacoesConcluidas);
            return new JsonResult(new { datas = data, contagem = marcacoesFinalizadas, tecnicosContagem });
        }
    }
}
