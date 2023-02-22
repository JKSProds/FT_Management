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
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Dossier d = phccontext.ObterDossier(id);

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && u.Id != d.Tecnico.Id) return Forbid();

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
                EditadoPor = u.Iniciais
            };

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && u.Id != fo.Utilizador.Id) return Forbid();

            d.StampDossier = phccontext.CriarDossier(d)[2].ToString();
            MailContext.EnviarEmailPedidoTransferencia(u, d);

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
            Utilizador t = context.ObterListaUtilizadores(false, false).Where(u => u.IdArmazem == armazem).DefaultIfEmpty().First();
            Dossier d = phccontext.ObterDossierAberto(u).Where(d => !d.Fechado).DefaultIfEmpty(new Dossier()).Last();

            if (d.StampDossier == null)
            {
                d = new Dossier()
                {
                    Serie = 36,
                    Marcacao = new Marcacao(),
                    FolhaObra = new FolhaObra(),
                    EditadoPor = u.Iniciais
                };
                d.StampDossier = phccontext.CriarDossier(d)[2].ToString();
                d = phccontext.ObterDossier(d.StampDossier);
                if (string.IsNullOrEmpty(d.StampDossier)) return Forbid();
                MailContext.EnviarEmailPedidoTransferencia(u, d);
            }
            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && u.Id != d.Tecnico.Id) return Forbid();
            if (load == 1)
            {
                List<Linha_Dossier> Linhas = new List<Linha_Dossier>();
                foreach (Movimentos m in phccontext.ObterPecasGuiaTransporte(id.Replace("|", "/"), t))
                {
                    if (Linhas.Where(l => l.Referencia == m.RefProduto).Count() == 0)
                    {
                        Linhas.Add(new Linha_Dossier() { Stamp_Dossier = d.StampDossier, Referencia = m.RefProduto, Designacao = m.Designacao, Quantidade = m.Quantidade, CriadoPor = u.Iniciais });
                    }
                    else
                    {
                        Linhas.Where(l => l.Referencia == m.RefProduto).First().Quantidade += m.Quantidade;
                    }
                }

                foreach (Linha_Dossier l in Linhas)
                {
                    phccontext.CriarLinhaDossier(l);
                }

            }
            return RedirectToAction("Pedido", new { id = d.StampDossier, ReturnUrl = "/Produtos/Armazem/" + armazem });
        }


        public JsonResult CriarLinha(string id, string referencia, string design, double qtd)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            List<string> res = new List<string>();

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Dossier d = phccontext.ObterDossier(id);
            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && u.Id != d.Tecnico.Id) return Json("");
            if (d.Fechado) return Json("");

            for (int i = 0; i < design.Length; i += 60)
            {
                string s = i + 60 > design.Length ? design.Substring(i, design.Length - i) : design.Substring(i, 60);
                Linha_Dossier l = new Linha_Dossier()
                {
                    Stamp_Dossier = id,
                    Referencia = string.IsNullOrEmpty(referencia) ? "" : referencia,
                    Designacao = s,
                    Quantidade = qtd,
                    CriadoPor = u.Iniciais
                };
                res = phccontext.CriarLinhaDossier(l);
            }

            return Json(int.Parse(res[0].ToString()) > 0 ? phccontext.ObterLinhaDossier(res[3].ToString()) : new Linha_Dossier());
        }

        public JsonResult RemoverLinha(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Linha_Dossier l = phccontext.ObterLinhaDossier(id);
            Dossier d = phccontext.ObterDossier(l.Stamp_Dossier);

            if (d.Fechado) return Json("");

            return Json(phccontext.ApagarLinhaDossier(l.Stamp_Dossier, l.Stamp_Linha));
        }

        public ActionResult FecharDossier(string id, string ReturnUrl)
        {
            if (ReturnUrl != "" && ReturnUrl != null)
            {
                return Redirect(ReturnUrl);
            }

            return RedirectToAction("Index", "Dossiers");
        }
    }
}
