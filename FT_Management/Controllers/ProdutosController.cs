using System.IO;
using System.Linq;
using FT_Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FT_Management.Controllers
{
    [Authorize]
    public class ProdutosController : Controller
    {
        public ActionResult Index(int? page, string Ref, string Desig, int Armazem)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            var LstArmazens = context.ObterListaArmazens().ToList();

            int pageSize = 100;
            var pageNumber = page ?? 1;

            if (Ref == null) { Ref = ""; }
            if (Desig == null) { Desig = ""; }
            if (Armazem == 0) { Armazem = 3; }

            ViewData["Ref"] = Ref;
            ViewData["Desig"] = Desig;
            ViewData["Armazem"] = Armazem;
            ViewData["Armazens"] = new SelectList(LstArmazens, "ArmazemId", "ArmazemNome", Armazem);

            if (Armazem>9)
            {
                return View(phccontext.ObterProdutos(Ref, Desig, Armazem).Where(p => p.Stock_PHC - p.Stock_Res > 0).ToPagedList(pageNumber, pageSize));
            }

            return View(phccontext.ObterProdutos(Ref, Desig, Armazem).ToPagedList(pageNumber, pageSize));
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

            return File(context.BitMapToMemoryStream(filePath), "application/pdf");
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

            return File(context.BitMapToMemoryStream(filePath), "application/pdf");
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

            return File(context.BitMapToMemoryStream(filePath), "application/pdf");
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

            return File(context.BitMapToMemoryStream(filePath), "application/pdf");
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
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            var LstArmazens = context.ObterListaArmazens().ToList();

            ViewData["LstGuiasPecas"] = context.ObterListaMovimentosProduto(id);
            ViewData["LstProdutosArmazem"] = context.ObterListaProdutoArmazem(id);
            ViewData["Armazens"] = new SelectList(LstArmazens, "ArmazemId", "ArmazemNome", armazemid);
            
            return View(phccontext.ObterProduto(id,armazemid));
        }
    }
}