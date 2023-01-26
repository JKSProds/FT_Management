using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using FT_Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Custom;
using Microsoft.AspNetCore.Authorization;
using System.Drawing;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio, Tech")]
    public class FolhasObraController : Controller
    {
        [Authorize(Roles = "Admin, Escritorio, Tech")]
        // GET: FolhasObraController
        public ActionResult Index(string DataFolhasObra)
        {
            if (DataFolhasObra == null || DataFolhasObra == string.Empty) DataFolhasObra = DateTime.Now.ToString("dd-MM-yyyy");

            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            ViewData["DataFolhasObra"] = DataFolhasObra;

            return View(phccontext.ObterFolhasObra(DateTime.Parse(DataFolhasObra)));
        }
        [Authorize(Roles = "Admin")]
        public ActionResult Adicionar(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            FolhaObra fo = new FolhaObra().PreencherDadosMarcacao(phccontext.ObterMarcacao(id));
            fo.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            ViewBag.EstadoFolhaObra = phccontext.ObterEstadoFolhaObra().Select(l => new SelectListItem() { Value = l.Key.ToString(), Text = l.Value });
            ViewData["TipoFolhaObra"] = phccontext.ObterTipoFolhaObra();
            return View(fo);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Adicionar(FolhaObra fo)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            if (!User.IsInRole("Admin")) fo = fo.PreencherDadosMarcacao(phccontext.ObterMarcacao(fo.IdMarcacao));
            fo.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));


            if (ModelState.IsValid)
            {
                fo.ClienteServico = phccontext.ObterClienteSimples(fo.ClienteServico.IdCliente, fo.ClienteServico.IdLoja);
                fo.EquipamentoServico = phccontext.ObterEquipamentoSimples(fo.EquipamentoServico.EquipamentoStamp);
                fo.Marcacao = phccontext.ObterMarcacao(fo.IdMarcacao);
                fo.ValidarIntervencoes();
                fo.ValidarPecas();

                List<string> res = phccontext.CriarFolhaObra(fo);
                if (int.Parse(res[0]) > 0)
                {
                    fo = phccontext.ObterFolhaObra(int.Parse(res[1]));
                    phccontext.FecharFolhaObra(fo);

                    Marcacao m = phccontext.ObterMarcacao(fo.IdMarcacao);
                    if (fo.EstadoFolhaObra == 1) fo.Marcacao.EstadoMarcacaoDesc = "Finalizado";
                    if (fo.EstadoFolhaObra == 2) fo.Marcacao.EstadoMarcacaoDesc = "Pedido Peças";
                    if (fo.EstadoFolhaObra == 3) fo.Marcacao.EstadoMarcacaoDesc = "Pedido Orçamento";
                    phccontext.AtualizaMarcacao(fo.Marcacao);

                    return RedirectToAction("Detalhes", "FolhasObra", new { id = fo.IdFolhaObra });
                }


                ModelState.AddModelError("", res[1]);
            }

            ViewBag.EstadoFolhaObra = phccontext.ObterEstadoFolhaObra().Select(l => new SelectListItem() { Value = l.Key.ToString(), Text = l.Value });
            ViewData["TipoFolhaObra"] = phccontext.ObterTipoFolhaObra();

            return View(fo);
        }
        [HttpPost]
        public ActionResult ValidarCodigo(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            return Content(context.ValidarCodigo(id).ToString());
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
                utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)),
                Obs = obs
            };
            context.CriarCodigo(c);
            foreach (var u in context.ObterListaUtilizadores(false, false).Where(u => u.Admin))
            {
                ChatContext.EnviarNotificacaoCodigo(c, u);
            }
            return Content("OK");
        }
        [HttpPost]
        public ActionResult ValidarFolhaObra(FolhaObra fo)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            fo.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            fo.ValidarIntervencoes();
            fo.ValidarPecas();

#if DEBUG
            return Content(phccontext.ValidarFolhaObra(fo));
#else
                        return Content(User.IsInRole("Admin") ? "" : phccontext.ValidarFolhaObra(fo));
#endif
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult GuardarLocalizacao(int IdCliente, int IdLoja)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Cliente c = phccontext.ObterClienteSimples(IdCliente, IdLoja);
            Viatura v = context.ObterViatura(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)));
            c.Latitude = v.Latitude;
            c.Longitude = v.Longitude;

            context.GuardarLocalizacaoCliente(c);
            return new JsonResult("OK");
        }


        public ActionResult Print(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            FolhaObra fo = phccontext.ObterFolhaObra(int.Parse(id));

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && fo.IntervencaosServico.Where(i => i.IdTecnico == context.ObterUtilizador(int.Parse(this.User.Claims.First().Value.ToString())).IdPHC).Count() == 0) return Redirect("~/Home/AcessoNegado");

            var filePath = Path.GetTempFileName();
            context.DesenharEtiquetaFolhaObra(fo).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            return File(context.BitMapToMemoryStream(filePath, 810, 504), "application/pdf");
        }

        [Authorize(Roles = "Admin, Escritorio, Tech")]
        // GET: FolhasObraController
        public ActionResult Detalhes(int Id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FolhaObra fo = phccontext.ObterFolhaObra(Id);

            Utilizador user = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && fo.IntervencaosServico.Where(i => i.IdTecnico == context.ObterUtilizador(int.Parse(this.User.Claims.First().Value.ToString())).IdPHC).Count() == 0) return Redirect("~/Home/AcessoNegado");

            ViewData["SelectedTecnico"] = user.NomeCompleto;
            ViewData["Tecnicos"] = context.ObterListaTecnicos(false, false);

            return View(fo);
        }

        public JsonResult ObterEmailClienteFolhaObra(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return Json(phccontext.ObterFolhaObra(id).ClienteServico.EmailCliente);
        }

        public virtual ActionResult PrintFolhaObra(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FolhaObra fo = phccontext.ObterFolhaObra(id);

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && fo.IntervencaosServico.Where(i => i.IdTecnico == context.ObterUtilizador(int.Parse(this.User.Claims.First().Value.ToString())).IdPHC).Count() == 0) return Redirect("~/Home/AcessoNegado");

            var file = context.PreencherFormularioFolhaObra(fo).ToArray();
            var output = new MemoryStream();
            output.Write(file, 0, file.Length);
            output.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "FolhaObra_" + id + ".pdf",
                Inline = true,
                Size = file.Length,
                CreationDate = DateTime.Now,

            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            //Send the File to Download.
            return new FileContentResult(output.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        public virtual ActionResult PrintFolhaObraSimples(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FolhaObra fo = phccontext.ObterFolhaObra(id);

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && fo.IntervencaosServico.Where(i => i.IdTecnico == context.ObterUtilizador(int.Parse(this.User.Claims.First().Value.ToString())).IdPHC).Count() == 0) return Redirect("~/Home/AcessoNegado");

            var filePath = Path.GetTempFileName();
            Bitmap bm = context.DesenharFolhaObraSimples(fo);
            bm.Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "TicketFO_" + id + ".pdf",
                Inline = false,
                CreationDate = DateTime.Now
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            return new FileContentResult(context.BitMapToMemoryStream(filePath, bm.Width, bm.Height).ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf);
        }


        public ActionResult EmailFolhaObra(int id, string emailDestino)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FolhaObra fo = phccontext.ObterFolhaObra(id);

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && fo.IntervencaosServico.Where(i => i.IdTecnico == context.ObterUtilizador(int.Parse(this.User.Claims.First().Value.ToString())).IdPHC).Count() == 0) return Redirect("~/Home/AcessoNegado");

            if (MailContext.EnviarEmailFolhaObra(emailDestino, fo, new Attachment((new MemoryStream(context.PreencherFormularioFolhaObra(fo).ToArray())), "FO" + id + ".pdf", System.Net.Mime.MediaTypeNames.Application.Pdf))) return Content("Sucesso");

            return Content("Erro");

        }
    }
}
