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

namespace FT_Management.Controllers
{
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

        public ActionResult Print(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var ms = new MemoryStream();

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            var filePath = Path.GetTempFileName();
            context.DesenharEtiqueta80x50QR(context.ObterProduto(id)).Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

            PdfDocument doc = new PdfDocument();
            doc.Info.Title = context.ObterProduto(id).Designacao_Produto;
            PdfPage page = new PdfPage
            {
                Width = 225,
                Height = 140
            };

                       XImage img = XImage.FromFile(filePath);
            img.Interpolate = false;

            doc.Pages.Add(page);

            XGraphics xgr = XGraphics.FromPdfPage(doc.Pages[0]);
            xgr.DrawImage(img, 0, 0);

            doc.Save(ms, false);

            System.IO.File.Delete(filePath);
            //return File(outputStream, "image/bmp");
            return File(ms, "application/pdf");
        }

        // GET: Produtos/Create
        public ActionResult Create()
        {
            return Content ("Em desenvolvimento");
        }

        // POST: Produtos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Produtos/Edit/5
        public ActionResult Edit(int id)
        {
            return Content("Em desenvolvimento");
        }

        // POST: Produtos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Produtos/Delete/5
        public ActionResult Delete(int id)
        {
            return Content("Em desenvolvimento");
        }

        // POST: Produtos/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}