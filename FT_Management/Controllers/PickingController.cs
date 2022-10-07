using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio")]
    public class PickingController : Controller
    {
        public IActionResult Index(int IdEncomenda, int Tipo, string NomeCliente)
        {
            ViewData["IdEncomenda"] = (IdEncomenda == 0 ? "" : IdEncomenda.ToString());
            ViewData["NomeCliente"] = (string.IsNullOrEmpty(NomeCliente) ? "" : NomeCliente);
            ViewData["Tipo"] = Tipo;

            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            List<Encomenda> LstEncomendas = phccontext.ObterEncomendas().Where(e => e.DataEnvio.Year > 1900).ToList();

            if (Tipo > 0) LstEncomendas = LstEncomendas.Where(e => e.NumDossier == Tipo).ToList();
            if (!string.IsNullOrEmpty(NomeCliente)) LstEncomendas = LstEncomendas.Where(e => e.NomeCliente.ToUpper().Contains(NomeCliente.ToUpper())).ToList();
            if (IdEncomenda > 0) LstEncomendas = LstEncomendas.Where(e => e.Id.ToString().Contains(IdEncomenda.ToString())).ToList();

            return View(LstEncomendas);
        }
    }
}
