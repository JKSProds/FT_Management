using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using FT_Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Custom;
using Microsoft.AspNetCore.Authorization;

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

            return File(context.BitMapToMemoryStream(filePath), "application/pdf");
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
            ViewData["Tecnicos"] = context.ObterListaUtilizadores(true).Where(u => u.TipoUtilizador != 3).ToList();

            return View(fo);
        }

        public JsonResult ObterHistorico(string NumeroSerieEquipamento)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return Json(new { json = phccontext.ObterHistorico(NumeroSerieEquipamento) });
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
                FileName = "FolhaObra_"+ id +".pdf",
                Inline = false,
                Size = file.Length,
                CreationDate = DateTime.Now,
                
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());

            return File(output, System.Net.Mime.MediaTypeNames.Application.Pdf);
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
