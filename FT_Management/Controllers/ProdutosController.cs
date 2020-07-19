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

namespace FT_Management.Controllers
{
    [Authorize]
    public class ProdutosController : Controller
    {
        // GET: Produtos
        public ActionResult Index(int? page, string Ref, string Desig)
        {
            ViewData["Ref"] = Ref;
            ViewData["Desig"] = Desig;

            int pageSize = 100;
            var pageNumber = page ?? 1;

            if (Ref == null) { Ref = ""; }
            if (Desig == null) { Desig = ""; }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return View(context.ObterListaProdutos(Ref, Desig).ToPagedList(pageNumber, pageSize));

        }

        [HttpPost("FileUpload")]
        public async Task<IActionResult> FileUpload(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            var filePaths = new List<string>();
            foreach (var formFile in files)
            {
                if (formFile.Length > 0 && formFile.FileName.Contains(".xls"))
                {
                    // full path to file in temp location
                    var filePath = Path.GetTempFileName(); //we are using Temp file name just for the example. Add your own file path.
                    filePaths.Add(filePath);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await formFile.CopyToAsync(stream);
                }
            }

            foreach (var item in filePaths)
            {
                context.CarregarFicheiroDB(item);

            }

            return Redirect("~/Produtos");
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

        public ActionResult Print(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            var filePath = Path.GetTempFileName();
            context.DesenharEtiqueta80x50(context.ObterProduto(id)).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);


            //return File(outputStream, "image/bmp");
            return File(BitMapToMemoryStream(filePath), "application/pdf");
        }

        public ActionResult PrintQr(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            var filePath = Path.GetTempFileName();
            context.DesenharEtiqueta80x50QR(context.ObterProduto(id)).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);


            //return File(outputStream, "image/bmp");
            return File(BitMapToMemoryStream(filePath), "application/pdf");
        }

        public ActionResult PrintPeq(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            var filePath = Path.GetTempFileName();
            context.DesenharEtiqueta80x25QR(context.ObterProduto(id)).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);


            //return File(outputStream, "image/bmp");
            return File(BitMapToMemoryStream(filePath), "application/pdf");
        }

        // GET: Produtos/Create
        public ActionResult Criar()
        {
            return View();
        }

        // POST: Produtos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Criar(Produto produto)
        {
            try
            {
                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

                List<Produto> produtos = new List<Produto>
                {
                    produto
                };

                context.CriarArtigos(produtos);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public JsonResult EditarStockFisico(string refproduto, string stockfisico)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Produto produtoFinal = context.ObterProduto(refproduto);
            Double.TryParse(stockfisico, out double stock_fisico);
            produtoFinal.Stock_Fisico = stock_fisico;

            context.EditarArtigo(produtoFinal);

            return Json("ok");
        }

        // GET: Produtos/Edit/5
        public ActionResult Editar(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            ViewData["LstGuiasPecas"] = context.ObterListaMovimentosProduto(id);
            return View(context.ObterProduto(id));
        }

        // POST: Produtos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(string id, Produto produto)
        {
            try
            {
                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                produto.Ref_Produto = id;
                context.EditarArtigo(produto);

                return Redirect("~/Produtos/Editar/" + id);
            }
            catch
            {
                return View();
            }
        }

        // GET: Produtos/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return Content("Em desenvolvimento");
        //}

        // POST: Produtos/Delete/5
        [HttpPost]
        public ActionResult Apagar(string Id)
        {
            try
            {

                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                context.ApagarArtigo(context.ObterProduto(Id));
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}