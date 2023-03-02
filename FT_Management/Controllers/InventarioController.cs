using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio")]
    public class InventarioController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return View(phccontext.ObterArmazensFixos());
        }

        [HttpGet]
        public JsonResult Dossiers(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(phccontext.ObterInventarios(id));
        }

        [HttpGet]
        public ActionResult Dossier(string id, string referencia)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (string.IsNullOrEmpty(referencia)) referencia = "";
            ViewData["Referencia"] = referencia;

            Picking p = phccontext.ObterInventario(id);
            p.Linhas = p.Linhas.Where(l => l.Ref_linha.Contains(referencia)).ToList();

            return View(p);
        }

        [HttpPost]
        public JsonResult Dossier(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            List<string> res = phccontext.CriarInventario(id, this.User.ObterNomeCompleto());
            res[2] = (!string.IsNullOrEmpty(res[2])) ? "/Inventario/Dossier/" + res[2] : "";

            return new JsonResult(res);
        }

        [HttpDelete]
        public JsonResult Dossier(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(phccontext.FecharInventario(new Picking() { Picking_Stamp = id, EditadoPor = this.User.ObterNomeCompleto() }));
        }

        [HttpPost]
        public JsonResult Linha(string stamp, string ref_produto, double qtd)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Linha_Picking l = new Linha_Picking()
            {
                Picking_Linha_Stamp = stamp,
                Ref_linha = ref_produto,
                Qtd_Linha = qtd,
                EditadoPor = this.User.ObterNomeCompleto()
            };

            return new JsonResult(phccontext.CriarLinhaInventario(l));
        }

        [HttpGet]
        public JsonResult Linha(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(phccontext.ObterLinhaInventario(id));
        }

        public JsonResult Linha(string stamp, string stamp_linha)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(phccontext.ApagarLinhaInventario(new Ref_Linha_Picking() { BOMA_STAMP = stamp, Picking_Linha_Stamp = stamp_linha }));
        }

        [HttpPost]
        public JsonResult Serie(string stamp, string stamp_linha, string serie)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Ref_Linha_Picking l = new Ref_Linha_Picking()
            {
                Picking_Linha_Stamp = stamp_linha,
                BOMA_STAMP = stamp,
                NumSerie = serie,
                CriadoPor = this.User.ObterNomeCompleto()
            };

            return new JsonResult(phccontext.CriarSerieLinhaInventario(l));
        }

        [HttpGet]
        public JsonResult Serie(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(phccontext.ObterSerieLinhaInventario(id).OrderBy(s => s.CriadoA).ToList());
        }

        [HttpDelete]
        public JsonResult ApagarSerie(string stamp, string stamp_boma, string stamp_linha)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(phccontext.ApagarLinhaSerieInventario(stamp, new Ref_Linha_Picking() { BOMA_STAMP = stamp_boma, Picking_Linha_Stamp = stamp_linha, CriadoPor = this.User.ObterNomeCompleto() }));
        }
    }

}
