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
        public ActionResult Index(int? page, string Ref, string Desig, int Armazem, int Fornecedor, string TipoEquipamento)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            var LstArmazens = phccontext.ObterArmazens();
            var LstFornecedores = phccontext.ObterFornecedores().Where(p => !string.IsNullOrEmpty(p.CodigoIntermedio));
            var LstTiposEquipamento = phccontext.ObterTiposEquipamento();

            int pageSize = 100;
            var pageNumber = page ?? 1;

            if (Ref == null) { Ref = ""; }
            if (Desig == null) { Desig = ""; }
            if (Armazem == 0) { Armazem = 3; }
            if (TipoEquipamento == null) { TipoEquipamento = ""; }

            ViewData["Ref"] = Ref;
            ViewData["Desig"] = Desig;
            ViewData["Armazem"] = Armazem;
            ViewData["Fornecedor"] = Fornecedor;
            ViewData["TipoEquipamento"] = TipoEquipamento;
            ViewData["Armazens"] = new SelectList(LstArmazens, "ArmazemId", "ArmazemNome", Armazem);
            ViewData["Fornecedores"] = new SelectList(LstFornecedores, "IdFornecedor", "NomeFornecedor", Armazem);
            ViewData["TiposEquipamento"] = new SelectList(LstTiposEquipamento);

            if (Armazem>9)
            {
                return View(phccontext.ObterProdutos(Ref, Desig, Armazem, Fornecedor, TipoEquipamento).Where(p => p.Stock_PHC - p.Stock_Res > 0).ToPagedList(pageNumber, pageSize));
            }
            phccontext.ObterGuiasTransporte(32);
            return View(phccontext.ObterProdutos(Ref, Desig, Armazem, Fornecedor, TipoEquipamento).ToPagedList(pageNumber, pageSize));
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
            
            return File(context.BitMapToMemoryStream(filePath, 810, 504), "application/pdf");
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

            return File(context.BitMapToMemoryStream(filePath, 810, 504), "application/pdf");
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

            return File(context.BitMapToMemoryStream(filePath, 810, 504), "application/pdf");
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

            return File(context.BitMapToMemoryStream(filePath, 810, 504), "application/pdf");
        }


        [Authorize(Roles = "Admin, Escritorio")]
        public ActionResult Detalhes(string id, int armazemid)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            var LstArmazens = phccontext.ObterArmazens();

            if (armazemid == 0) { armazemid = 3; }

            ViewData["LstProdutosArmazem"] = phccontext.ObterProdutosArmazem(id);
            ViewData["Armazens"] = new SelectList(LstArmazens, "ArmazemId", "ArmazemNome", armazemid);
            
            return View(phccontext.ObterProduto(id,armazemid));
        }

        [HttpPost]
        public JsonResult ObterPecasUtilizador()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            int idArmazem = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).IdArmazem;

            return Json(phccontext.ObterProdutosArmazem(idArmazem).ToList());
        }
        [HttpPost]
        public JsonResult ObterPeca(string ref_produto)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return Json(phccontext.ObterProdutosArmazem(ref_produto).ToList().First());
        }
    }
}