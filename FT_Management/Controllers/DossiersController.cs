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

    }
}
