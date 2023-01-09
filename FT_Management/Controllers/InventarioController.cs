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
        public ActionResult Index()
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return View(phccontext.ObterArmazensFixos());
        }

        public ActionResult Dossier(string id, string referencia)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (string.IsNullOrEmpty(referencia)) referencia = "";
            ViewData["Referencia"] = referencia;

            Picking p = phccontext.ObterInventario(id);
            p.Linhas = p.Linhas.Where(l => l.Ref_linha.Contains(referencia)).ToList();

            return View(p);
        }

        public JsonResult ObterDossiers(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(phccontext.ObterInventarios(id));
        }
        [HttpPost]
        public JsonResult CriarDossier(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            List<string> res = phccontext.CriarInventario(id, this.User.ObterNomeCompleto());
            res[2] = (!string.IsNullOrEmpty(res[2])) ? "/Inventario/Dossier/" + res[2] : "";

            return new JsonResult(res);
        }
        public JsonResult ValidarQuantidade(string stamp, string ref_produto, double qtd)
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
        public JsonResult ValidarSerie(string stamp, string stamp_linha, string serie)
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
        public JsonResult ObterLinha(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(phccontext.ObterLinhaInventario(id));
        }
        public JsonResult ObterSerieLinha(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(phccontext.ObterSerieLinhaInventario(id));
        }
        public JsonResult ApagarLinha(string stamp, string stamp_linha)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(phccontext.ApagarLinhaInventario(new Ref_Linha_Picking() { BOMA_STAMP = stamp, Picking_Linha_Stamp = stamp_linha }));
        }

        public JsonResult FecharDossier(string id)
        {
            //DESENVOLVER
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult("Por desenvolver");
        }
    }

}
