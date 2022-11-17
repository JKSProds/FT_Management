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
            List<Encomenda> LstEncomendas = phccontext.ObterEncomendas().Where(e => e.ExisteEncomenda(Encomenda.Tipo.TODAS)).ToList();

            if (Tipo > 0) LstEncomendas = LstEncomendas.Where(e => e.NumDossier == Tipo).ToList();
            if (!string.IsNullOrEmpty(NomeCliente)) LstEncomendas = LstEncomendas.Where(e => e.NomeCliente.ToUpper().Contains(NomeCliente.ToUpper())).ToList();
            if (IdEncomenda > 0) LstEncomendas = LstEncomendas.Where(e => e.Id.ToString().Contains(IdEncomenda.ToString())).ToList();

            return View(LstEncomendas);
        }

        public IActionResult Adicionar(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            string pi_stamp = phccontext.ObterEncomenda(id).PI_STAMP;

            if (string.IsNullOrEmpty(pi_stamp)) pi_stamp = phccontext.CriarPicking(id, this.User.ObterNomeCompleto());
            Picking p = phccontext.ObterPicking(pi_stamp);

            if (p.IdPicking == 0) 
            {
                pi_stamp = phccontext.CriarPicking(id, this.User.ObterNomeCompleto());
                p = phccontext.ObterPicking(pi_stamp);
            }

            return View(p);
        }
        public ActionResult Fechar(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Picking p = phccontext.ObterPicking(id);
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().ToString()));
            phccontext.FecharPicking(id, u.NomeCompleto);

            MailContext.EnviarEmailFechoPicking(u, p);

            return Content("Ok");
        }

        [HttpPost]
        public ActionResult ValidarPicking(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return Content(phccontext.ValidarPicking(id));
        }

        public JsonResult Validar(string stamp, int qtd, string serie, string bomastamp)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Linha_Picking linha_picking = new Linha_Picking()
            {
                Picking_Linha_Stamp = stamp,
                Qtd_Linha = qtd,
                Lista_Ref = new List<Ref_Linha_Picking>(),
                EditadoPor = this.User.ObterNomeCompleto()
            };
            if (serie != null || bomastamp != null)
            {
                linha_picking.Serie = true;
                linha_picking.Lista_Ref.Add(new Ref_Linha_Picking()
                {
                    BOMA_STAMP = bomastamp == null ? "" : bomastamp,
                    NumSerie = serie == null ? "" : serie
                });
            }

            return new JsonResult(phccontext.AtualizarLinhaPicking(linha_picking));
        }
    }
}
