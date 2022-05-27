using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FT_Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FT_Management.Controllers
{
    [Authorize]
    public class ProdutosController : Controller
    {
        // GET: Produtos
        public ActionResult Index(int? page, string Ref, string Desig, int Armazem)
        {
            ViewData["Ref"] = Ref;
            ViewData["Desig"] = Desig;

            int pageSize = 100;
            var pageNumber = page ?? 1;

            if (Ref == null) { Ref = ""; }
            if (Desig == null) { Desig = ""; }
            if (Armazem == 0) { Armazem = 3; }

            ViewData["Armazem"] = Armazem;

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            var LstArmazens = context.ObterListaArmazens().ToList();

            ViewData["Armazens"] = new SelectList(LstArmazens, "ArmazemId", "ArmazemNome", Armazem);

            if (Armazem>9)
            {
                return View(phccontext.ObterProdutos(Ref, Desig, Armazem).Where(p => p.Stock_PHC - p.Stock_Res > 0).ToPagedList(pageNumber, pageSize));
            }
            return View(phccontext.ObterProdutos(Ref, Desig, Armazem).ToPagedList(pageNumber, pageSize));

        }

        public MemoryStream BitMapToMemoryStream(string filePath)
        {
            var ms = new MemoryStream();

            PdfDocument doc = new PdfDocument();
            PdfPage page = new PdfPage
            {
                Width = 810,
                Height = 504
            };

            XImage img = XImage.FromFile(filePath);
            img.Interpolate = false;

            doc.Pages.Add(page);

            XGraphics xgr = XGraphics.FromPdfPage(doc.Pages[0]);
            XRect box = new XRect(0, 0, 810, 504);
            xgr.DrawImage(img, box);

            doc.Save(ms, false);

            System.IO.File.Delete(filePath);

            return ms;

        }

        public ActionResult Print(string id, int armazemid)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            var filePath = Path.GetTempFileName();
            context.DesenharEtiqueta80x50(phccontext.ObterProduto(id,armazemid)).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            return File(BitMapToMemoryStream(filePath), "application/pdf");
        }

        public ActionResult PrintQr(string id, int armazemid)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            var filePath = Path.GetTempFileName();
            context.DesenharEtiqueta80x50QR(phccontext.ObterProduto(id, armazemid)).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            return File(BitMapToMemoryStream(filePath), "application/pdf");
        }

        public ActionResult PrintPeq(string id, int armazemid)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            var filePath = Path.GetTempFileName();
            context.DesenharEtiqueta80x25QR(phccontext.ObterProduto(id, armazemid)).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            return File(BitMapToMemoryStream(filePath), "application/pdf");
        }
        public ActionResult PrintPeqMulti(string id, int armazemid)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            var filePath = Path.GetTempFileName();
            context.DesenharEtiqueta40x25QR(phccontext.ObterProduto(id, armazemid)).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            return File(BitMapToMemoryStream(filePath), "application/pdf");
        }

        //Analisar
        //[HttpPost]
        //[Authorize(Roles = "Admin, Escritorio")]
        //public JsonResult EditarStockFisico(string refproduto, string stockfisico, int armazemid)
        //{
        //    FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
        //    Produto produtoFinal = context.ObterProduto(refproduto, armazemid);

        //    context.AdicionarLog(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).NomeUtilizador, "Foi alterado o stock fisico do produto " + refproduto + " de " + produtoFinal.Stock_Fisico+ " para " + stockfisico, 1);

        //    Double.TryParse(stockfisico, out double stock_fisico);
        //    produtoFinal.Stock_Fisico = stock_fisico;

        //    context.EditarArtigo(produtoFinal);
        //    return Json("ok");
        //}


        [Authorize(Roles = "Admin, Escritorio")]
        public ActionResult Detalhes(string id, int armazemid)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            ViewData["LstGuiasPecas"] = context.ObterListaMovimentosProduto(id);

            ViewData["LstProdutosArmazem"] = context.ObterListaProdutoArmazem(id);

            var LstArmazens = context.ObterListaArmazens().ToList();
            ViewData["Armazens"] = new SelectList(LstArmazens, "ArmazemId", "ArmazemNome", armazemid);
            
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;


            return View(phccontext.ObterProduto(id,armazemid));
        }
    }
}