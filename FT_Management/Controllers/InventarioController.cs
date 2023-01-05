using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        public ActionResult Dossier(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return View(phccontext.ObterInventario(id));
        }

        public JsonResult ObterDossiers(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(phccontext.ObterInventarios(id));
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
