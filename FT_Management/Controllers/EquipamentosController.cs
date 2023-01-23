using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FT_Management.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio, Tech, Comercial")]
    public class EquipamentosController : Controller
    {

        public IActionResult Index(string Serie)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (string.IsNullOrEmpty(Serie)) Serie = "";
            ViewData["Serie"] = Serie;

            return View(phccontext.ObterEquipamentosSerie(Serie));
        }

        public IActionResult Detalhes(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return View(phccontext.ObterEquipamento(id));
        }

        public JsonResult ObterHistorico(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return Json(new { json = phccontext.ObterHistorico(id) });
        }

        [HttpGet]
        public JsonResult ObterEquipamentos(int no, int loja, string prefix)
        {
            if (no == 0) return Json("");
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return Json(phccontext.ObterEquipamentos(new Cliente() { IdCliente = no, IdLoja = loja }).Where(e => e.NumeroSerieEquipamento.ToLower().Contains(prefix.ToLower())).OrderBy(e => e.NumeroSerieEquipamento).ToList());
        }

        [HttpPost]
        public ActionResult CriarCodigo(string id, string obs)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Codigo c = new Codigo()
            {
                Stamp = id,
                Estado = 0,
                ValidadeCodigo = DateTime.Now.AddMinutes(10),
                utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value))
            };
            c.Obs = "O utilizador " + c.utilizador.NomeCompleto + " deseja associar um equipamento ao cliente: " + obs + "!";

            context.CriarCodigo(c);
            foreach (var u in context.ObterListaUtilizadores(false, false).Where(u => u.Admin))
            {
                ChatContext.EnviarNotificacaoCodigo(c, u);
            }
            return Content("OK");
        }
        [HttpPost]
        public ActionResult AssociarCliente(string id, string stamp)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Cliente c = phccontext.ObterClienteSimples(stamp);
            Equipamento e = phccontext.ObterEquipamento(id);
            return Content(phccontext.AtualizarClienteEquipamento(c, e).ToString());
        }
    }
}