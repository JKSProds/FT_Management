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

            context.CriarArtigos(phccontext.ObterProdutos(context.ObterUltimaModificacaoPHC("sa")));

            var LstArmazens = context.ObterListaArmazens().ToList();

            ViewData["Armazens"] = new SelectList(LstArmazens, "ArmazemId", "ArmazemNome", Armazem);

            return View(context.ObterListaProdutos(Ref, Desig, Armazem).ToPagedList(pageNumber, pageSize));

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

        public ActionResult Print(string id, int armazemid)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            var filePath = Path.GetTempFileName();
            context.DesenharEtiqueta80x50(context.ObterProduto(id,armazemid)).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            context.AdicionarLog(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).NomeUtilizador, "Impressa etiqueta EAN13 de produto: "+ id, 2);
            //return File(outputStream, "image/bmp");
            return File(BitMapToMemoryStream(filePath), "application/pdf");
        }

        public ActionResult PrintQr(string id, int armazemid)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            var filePath = Path.GetTempFileName();
            context.DesenharEtiqueta80x50QR(context.ObterProduto(id, armazemid)).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            context.AdicionarLog(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).NomeUtilizador, "Impressa etiqueta normal de produto: " + id, 2);
            //return File(outputStream, "image/bmp");
            return File(BitMapToMemoryStream(filePath), "application/pdf");
        }

        public ActionResult PrintPeq(string id, int armazemid)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            var filePath = Path.GetTempFileName();
            context.DesenharEtiqueta80x25QR(context.ObterProduto(id, armazemid)).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            context.AdicionarLog(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).NomeUtilizador, "Impressa etiqueta pequena de produto: " + id, 2);
            //return File(outputStream, "image/bmp");
            return File(BitMapToMemoryStream(filePath), "application/pdf");
        }

        // GET: Produtos/Create
        public ActionResult Criar()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            var LstArmazens = context.ObterListaArmazens().ToList();
            ViewData["Armazens"] = new SelectList(LstArmazens, "ArmazemId", "ArmazemNome", 3);
            
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
                context.AdicionarLog(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).NomeUtilizador, "Foi criado um novo artigo: " + produto.Ref_Produto, 1);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public JsonResult EditarStockFisico(string refproduto, string stockfisico, int armazemid)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Produto produtoFinal = context.ObterProduto(refproduto, armazemid);

            context.AdicionarLog(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).NomeUtilizador, "Foi alterado o stock fisico do produto " + refproduto + " de " + produtoFinal.Stock_Fisico+ " para " + stockfisico, 1);

            Double.TryParse(stockfisico, out double stock_fisico);
            produtoFinal.Stock_Fisico = stock_fisico;

            context.EditarArtigo(produtoFinal);
            return Json("ok");
        }

        // GET: Produtos/Edit/5
        public ActionResult Editar(string id, int armazemid)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            ViewData["LstGuiasPecas"] = context.ObterListaMovimentosProduto(id);

            ViewData["LstProdutosArmazem"] = context.ObterListaProdutoArmazem(id);

            var LstArmazens = context.ObterListaArmazens().ToList();
            ViewData["Armazens"] = new SelectList(LstArmazens, "ArmazemId", "ArmazemNome", armazemid);

            return View(context.ObterProduto(id,armazemid));
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
                context.AdicionarLog(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).NomeUtilizador, "Foi alterado o produto " + produto.Ref_Produto, 1);

                return Redirect("~/Produtos/Editar/" + id + "?armazemid=" + produto.Armazem_ID);
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
        public ActionResult Apagar(string Id, int armazemid)
        {
            try
            {

                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                context.ApagarArtigo(context.ObterProduto(Id,armazemid));
                context.AdicionarLog(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).NomeUtilizador, "Foi apagado o produto " + Id, 1);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}