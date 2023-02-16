using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio")]
    public class DossiersController : Controller
    {
        public ActionResult Pedido(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Dossier d = phccontext.ObterDossier(id);
            return View(d);
        }

        public ActionResult CriarDossier(string id, int serie)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            FolhaObra fo = phccontext.ObterFolhaObra(id);
            Dossier d = new Dossier()
            {
                Serie = serie,
                FolhaObra = fo,
                Marcacao = fo.Marcacao,
                EditadoPor = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).NomeCompleto
            };

            return RedirectToAction("Pedido", new { id = phccontext.CriarDossier(d)[2].ToString() });
        }

        public JsonResult CriarLinha(string id, string referencia, string design, double qtd)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Linha_Dossier l = new Linha_Dossier()
            {
                Stamp_Dossier = id,
                Referencia = referencia,
                Designacao = design,
                Quantidade = qtd,
                CriadoPor = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).NomeCompleto
            };

            return Json(phccontext.CriarLinhaDossier(l));
        }

    }
}
