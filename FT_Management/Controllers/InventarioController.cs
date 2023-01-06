using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin")]
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

            return View(phccontext.ObterInventario(id));
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
            string res = phccontext.CriarInventario(id, this.User.ObterNomeCompleto());
            return new JsonResult((!string.IsNullOrEmpty(res)) ? "/Inventario/Dossier/" + res : "" );
        }
        public JsonResult ValidarQuantidade(string stamp, double qtt)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult("");
        }
        public JsonResult ValidarSerie(string stamp, string serie)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult("");
        }
    }
     
}
