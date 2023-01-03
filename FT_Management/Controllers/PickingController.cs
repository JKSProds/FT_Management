using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;

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
            List<Encomenda> LstEncomendas = phccontext.ObterEncomendas().Where(e => e.ExisteEncomenda(Encomenda.Tipo.TODAS) && e.NItems > 0).ToList();

            if (Tipo > 0) LstEncomendas = LstEncomendas.Where(e => e.NumDossier == Tipo).ToList();
            if (!string.IsNullOrEmpty(NomeCliente)) LstEncomendas = LstEncomendas.Where(e => e.NomeCliente.ToUpper().Contains(NomeCliente.ToUpper())).ToList();
            if (IdEncomenda > 0) LstEncomendas = LstEncomendas.Where(e => e.Id.ToString().Contains(IdEncomenda.ToString())).ToList();

            return View(LstEncomendas);
        }

        public IActionResult Adicionar(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            ViewBag.Armazens = phccontext.ObterArmazensFixos().Select(l => new SelectListItem() { Value = l.ArmazemStamp, Text = l.ArmazemNome, Selected = l.ArmazemId == 3 });

            Encomenda e = phccontext.ObterEncomenda(id);
            string pi_stamp = e.PI_STAMP;

            if (string.IsNullOrEmpty(pi_stamp))
            {
                pi_stamp = phccontext.CriarPicking(id, u.NomeCompleto);
                context.AdicionarLog(u.Id, "Criado um picking novo com sucesso! - Encomenda Nº " + e.Id  + ", " + e.NomeCliente + " pelo utilizador " + u.NomeCompleto, 6);

            }
            Picking p = phccontext.ObterPicking(pi_stamp);

            if (p.IdPicking == 0) 
            {
                pi_stamp = phccontext.CriarPicking(id, u.NomeCompleto);
                context.AdicionarLog(u.Id, "Criado um picking novo com sucesso! - Encomenda Nº " + e.Id + ", " + e.NomeCliente + " pelo utilizador " + u.NomeCompleto, 6);

                p = phccontext.ObterPicking(pi_stamp);
            }

            return View(p);
        }
        public ActionResult Fechar(string id, string obs, string armazem)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Picking p = phccontext.ObterPicking(id);
            p.EditadoPor = u.NomeCompleto;
            p.Obs = (string.IsNullOrEmpty(obs) ? "" : (obs + "\r\n\r\n")) + "<b>" + phccontext.ValidarPicking(p.Picking_Stamp) + "</b>";
            p.ArmazemDestino = p.Encomenda.NumDossier == 2 ? phccontext.ObterArmazem(armazem) : new Armazem();

            phccontext.FecharPicking(p);
            context.AdicionarLog(u.Id, "Foi fechado um picking com sucesso! - Picking Nº " + p.IdPicking + ", " + p.NomeCliente + " pelo utilizador " + u.NomeCompleto, 6);

            var filePath = Path.GetTempFileName();
            context.DesenharEtiquetaPicking(p).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            MailContext.EnviarEmailFechoPicking(u, p, new Attachment(context.BitMapToMemoryStream(filePath, 810, 504), "Picking_" + p.IdPicking + ".pdf"));

            return Content("Ok");
        }


        [HttpPost]
        public ActionResult ValidarPicking(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return Content(phccontext.ValidarPicking(id));
        }

        public JsonResult Validar(string stamp, Double qtd, string serie, string bomastamp)
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

        public JsonResult ObterEncomenda(string stamp)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Encomenda e = phccontext.ObterEncomenda(stamp);
            e.LinhasEncomenda = e.LinhasEncomenda.Where(l => l.DataEnvio.Year > 1900 && !l.Fornecido || e.Total).ToList();

            return new JsonResult(e);
        }
    }
}
