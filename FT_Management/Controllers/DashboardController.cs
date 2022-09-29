using System;
using System.Collections.Generic;
using System.Linq;
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
            return View(model.Where(m => m.DataMarcacao == DateTime.Now.Date).OrderByDescending(e => e.EstadoMarcacao));
        }
    }
    public class InstalacoesViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Marcacao> model)
        {
            return View(model.Where(m => m.TipoServico == "Instalação" && m.EstadoMarcacao != 4 && m.EstadoMarcacao != 3).OrderBy(e => e.EstadoMarcacao));
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
            return View(model.Where(m => m.EstadoMarcacao !=4 && m.Oficina));
        }
    }

    [Authorize(Roles = "Admin, Escritorio")]
    public class DashboardController : Controller
    {

        [AllowAnonymous]
        public IActionResult Encomendas(string ApiKey)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(ApiKey);
            if (String.IsNullOrEmpty(ApiKey) && User.Identity.IsAuthenticated) IdUtilizador = int.Parse(this.User.Claims.First().Value);
            if (IdUtilizador == 0) return Forbid();

            return View(phccontext.ObterEncomendas());
        }

        public JsonResult ObterMarcacoesConcluidas30Dias()
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            List<Marcacao> LstMarcacoes = phccontext.ObterMarcacoes(DateTime.Now.AddDays(-30), DateTime.Now).Where(m => m.EstadoMarcacao == 4).ToList();
            List<Utilizador> LstUtilizadores = context.ObterListaTecnicos(false);

            var data = LstMarcacoes.Select(m => m.DataMarcacao.ToShortDateString()).Distinct().ToList();
            var marcacoesFinalizadas = LstMarcacoes.GroupBy(i => i.DataMarcacao)
             .Select(i => i.Count());

            var tecnicosContagem = LstMarcacoes.GroupBy(i => i.IdTecnico).Select(group => new { tecnico = LstUtilizadores.Where(u => u.Id == group.Key).ToList(), marcacoesConcluidas = group.Count() }).OrderByDescending(i => i.marcacoesConcluidas);
            return new JsonResult(new { datas = data, contagem = marcacoesFinalizadas, tecnicosContagem });
        }
    }
}
