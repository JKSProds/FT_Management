using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio")]
    public class DashboardController : Controller
    {

        [AllowAnonymous]
        public IActionResult Encomendas(string Api)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(Api);
            if (String.IsNullOrEmpty(Api) && User.Identity.IsAuthenticated) IdUtilizador = int.Parse(this.User.Claims.First().Value);
            if (IdUtilizador == 0) return Forbid();

            return View(phccontext.ObterEncomendas().Where(d => d.NumDossier != 2).Where(e => !e.Fornecido));
        }


        [AllowAnonymous]
        public IActionResult Utilizadores(string Api)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(Api);
            if (String.IsNullOrEmpty(Api) && User.Identity.IsAuthenticated) IdUtilizador = int.Parse(this.User.Claims.First().Value);
            if (IdUtilizador == 0) return Forbid();

            ViewData["API"] = Api;
            List<Utilizador> LstUtilizadores = context.ObterListaUtilizadores(true, false).Where(u => u.Acessos).ToList();
            List<Ferias> LstFerias = context.ObterListaFerias(DateTime.Parse(DateTime.Now.ToLongDateString() + " 00:00:00"), DateTime.Parse(DateTime.Now.ToLongDateString() + " 23:59:59"));
            foreach (Ferias f in LstFerias)
            {
                if (LstUtilizadores.Where(u => u.Id == f.IdUtilizador).Count() > 0) LstUtilizadores.Where(u => u.Id == f.IdUtilizador).First().Ferias = true;
            }
            return View(LstUtilizadores);
        }
        [AllowAnonymous]
        public IActionResult Pendentes(string Api)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(Api);
            if (String.IsNullOrEmpty(Api) && User.Identity.IsAuthenticated) IdUtilizador = int.Parse(this.User.Claims.First().Value);
            if (IdUtilizador == 0) return Forbid();

            return View(phccontext.ObterMarcacoesPendentes());
        }
        [AllowAnonymous]
        public IActionResult Marcacoes(string Api)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            List<Utilizador> LstUtilizadores = context.ObterListaTecnicos(true, true);
            List<Marcacao> LstMarcacao = phccontext.ObterMarcacoes(DateTime.Now, DateTime.Now);

            for (int i = 0; i <= LstUtilizadores.Count() - 1; i++)
            {
                LstUtilizadores[i].LstMarcacoes = LstMarcacao.Where(m => m.Tecnico.Id == LstUtilizadores[i].Id).ToList();
            }

            return View(LstUtilizadores);
        }


    }
}
