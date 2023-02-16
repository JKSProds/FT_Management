using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio, Tech")]
    public class DossiersController : Controller
    {
        [Authorize(Roles = "Admin, Escritorio")]
        public ActionResult Index(string Data)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (Data == null || Data == string.Empty) Data = DateTime.Now.ToString("dd-MM-yyyy");
            ViewData["Data"] = Data;

            return View(phccontext.ObterDossiers(DateTime.Parse(Data)));
        }


        public ActionResult Pedido(string id, string ReturnUrl)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Dossier d = phccontext.ObterDossier(id);
            ViewData["ReturnUrl"] = ReturnUrl;
            return View(d);
        }

        public ActionResult CriarDossier(string id, int serie, string ReturnUrl)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            FolhaObra fo = phccontext.ObterFolhaObra(id);
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Dossier d = new Dossier()
            {
                Serie = serie,
                FolhaObra = fo,
                Marcacao = phccontext.ObterMarcacao(fo.IdMarcacao),
                EditadoPor = u.NomeCompleto
            };

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && u.Id != d.Tecnico.Id) return Forbid();

            d.StampDossier = phccontext.CriarDossier(d)[2].ToString();

            //Criação de linhas por defeito
            phccontext.CriarLinhaDossier(new Linha_Dossier() { Stamp_Dossier = d.StampDossier, Designacao = "Pedido de Assistência Técnica N.º " + fo.IdFolhaObra, CriadoPor = d.EditadoPor });
            phccontext.CriarLinhaDossier(new Linha_Dossier() { Stamp_Dossier = d.StampDossier, Designacao = "Reparação de " + fo.EquipamentoServico.TipoEquipamento, CriadoPor = d.EditadoPor });
            phccontext.CriarLinhaDossier(new Linha_Dossier() { Stamp_Dossier = d.StampDossier, Designacao = fo.EquipamentoServico.MarcaEquipamento + " " + fo.EquipamentoServico.ModeloEquipamento + " N/S: " + fo.EquipamentoServico.NumeroSerieEquipamento, CriadoPor = d.EditadoPor });

            return RedirectToAction("Pedido", new { id = d.StampDossier, ReturnUrl = ReturnUrl });
        }

        public ActionResult CriarDossierTransferencia(string id, int armazem, int load)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Dossier d = new Dossier()
            {
                Serie = 36,
                Marcacao = new Marcacao(),
                FolhaObra = new FolhaObra(),
                EditadoPor = u.NomeCompleto
            };

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && u.Id != d.Tecnico.Id) return Forbid();

            d.StampDossier = phccontext.CriarDossier(d)[2].ToString();
            if (string.IsNullOrEmpty(d.StampDossier)) return Forbid();

            //Criação de linhas por defeito
            if (load == 1)
            {
                foreach (Movimentos m in phccontext.ObterPecasGuiaTransporte(id.Replace("|", "/"), armazem))
                {
                    phccontext.CriarLinhaDossier(new Linha_Dossier() { Stamp_Dossier = d.StampDossier, Referencia = m.RefProduto, Designacao = m.Designacao, Quantidade = m.Quantidade, CriadoPor = u.NomeCompleto });
                }
            }
            return RedirectToAction("Pedido", new { id = d.StampDossier });
        }


        public JsonResult CriarLinha(string id, string referencia, string design, double qtd)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Dossier d = phccontext.ObterDossier(id);
            Linha_Dossier l = new Linha_Dossier()
            {
                Stamp_Dossier = id,
                Referencia = string.IsNullOrEmpty(referencia) ? "" : referencia,
                Designacao = design,
                Quantidade = qtd,
                CriadoPor = u.NomeCompleto
            };
            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && u.Id != d.Tecnico.Id) return Json("");

            List<string> res = phccontext.CriarLinhaDossier(l);
            return Json(int.Parse(res[0].ToString()) > 0 ? phccontext.ObterLinhaDossier(res[3].ToString()) : new Linha_Dossier());
        }

        public JsonResult RemoverLinha(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Linha_Dossier l = phccontext.ObterLinhaDossier(id);
            return Json(phccontext.ApagarLinhaDossier(l.Stamp_Dossier, l.Stamp_Linha));
        }

        public ActionResult FecharDossier(string id, string ReturnUrl)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Dossier d = phccontext.ObterDossier(id);
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && u.Id != d.Tecnico.Id) return Forbid();

            MailContext.EnviarEmailFechoDossier(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)), d);

            if (ReturnUrl != "" && ReturnUrl != null)
            {
                return Redirect(ReturnUrl);
            }

            return RedirectToAction("Index", "Dossiers");
        }
    }
}
