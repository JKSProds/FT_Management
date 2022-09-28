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

        public ActionResult Adicionar(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Marcacao m = phccontext.ObterMarcacao(id);

            FolhaObra fo = new FolhaObra() { ClienteServico = m.Cliente };
            fo.DataServico = m.DataMarcacao;
            fo.ReferenciaServico = m.Referencia;
            fo.IdMarcacao = m.IdMarcacao;

            ViewData["EstadoFolhaObra"] = phccontext.ObterEstadoFolhaObra();
            ViewData["TipoFolhaObra"] = phccontext.ObterTipoFolhaObra();
            return View(fo);
        }
        [HttpPost]
        public ActionResult Adicionar(FolhaObra fo)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            if (ModelState.IsValid)
            {
            fo.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
                fo.ClienteServico = phccontext.ObterClienteSimples(fo.ClienteServico.IdCliente, fo.ClienteServico.IdLoja);
                fo.EquipamentoServico = phccontext.ObterEquipamento(fo.EquipamentoServico.IdEquipamento);
            fo.IntervencaosServico.Clear();
                foreach (var item in fo.ListaIntervencoes.Split(";"))
                {
                    if (item != "")
                    {
                        fo.IntervencaosServico.Add(new Intervencao
                        {
                            HoraInicio = DateTime.Parse(item.Split("|").First()),
                            HoraFim = DateTime.Parse(item.Split("|").Last()),
                            DataServiço = fo.DataServico
                        });
                    }
                }

            fo.PecasServico.Clear();
            foreach (var item in fo.ListaPecas.Split(";"))
            {
                    if (item != "")
                    {
                        fo.PecasServico.Add(new Produto
                        {
                            Ref_Produto = item
                        });
                    }
            }


                int idFolhaObra = phccontext.CriarFolhaObra(fo);
                if (idFolhaObra > 0) return RedirectToAction("Detalhes", "FolhasObra", new { id = idFolhaObra });
            }

            ViewData["EstadoFolhaObra"] = phccontext.ObterEstadoFolhaObra();
            ViewData["TipoFolhaObra"] = phccontext.ObterTipoFolhaObra();
            return View(fo);
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

        [HttpPost]
        public JsonResult ObterEquipamentos(string IdCliente, string IdLoja)
        {
            if (string.IsNullOrEmpty(IdCliente)) return Json("");
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return Json(phccontext.ObterEquipamentos(new Cliente() { IdCliente = int.Parse(IdCliente), IdLoja = int.Parse(IdLoja) }).OrderBy(e => e.NumeroSerieEquipamento).ToList());
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
                FileName = "FolhaObra_" + id + ".pdf",
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
