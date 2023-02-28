using System.IO;
using System.Linq;
using FT_Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace FT_Management.Controllers
{
    [Authorize]
    public class ProdutosController : Controller
    {
        public ActionResult Index(string Ref, string Desig, int Armazem, int Fornecedor, string TipoEquipamento)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            var LstArmazens = phccontext.ObterArmazens();
            var LstFornecedores = phccontext.ObterFornecedores().Where(p => !string.IsNullOrEmpty(p.CodigoIntermedio));
            var LstTiposEquipamento = phccontext.ObterTiposEquipamento();
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

            if (Armazem > 9)
            {
                return View(phccontext.ObterProdutos(Ref, Desig, Armazem, Fornecedor, TipoEquipamento).Where(p => p.Stock_PHC - p.Stock_Res > 0));
            }
            //phccontext.ObterGuiasTransporte(32);
            return View(phccontext.ObterProdutos(Ref, Desig, Armazem, Fornecedor, TipoEquipamento));
        }

        public virtual ActionResult Print(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            var file = context.DesenharEtiquetaProduto(phccontext.ObterProduto(id, 3)).ToArray();
            var output = new MemoryStream();
            output.Write(file, 0, file.Length);
            output.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "Produto_" + id + ".pdf",
                Inline = true,
                Size = file.Length

            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            return new FileContentResult(output.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf);
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

            return View(phccontext.ObterProduto(id, armazemid));
        }

        [HttpPost]
        public JsonResult ObterPecasUtilizador(string filter)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            int idArmazem = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).IdArmazem;
            if (string.IsNullOrEmpty(filter)) filter = "";

            return Json(phccontext.ObterProdutosArmazem(idArmazem).Where(p => p.Ref_Produto.ToLower().Contains(filter.ToLower()) || p.Designacao_Produto.ToLower().Contains(filter.ToLower())).ToList());
        }
        [HttpPost]
        public JsonResult ObterPecas(string filter)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            int idArmazem = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).IdArmazem;
            if (string.IsNullOrEmpty(filter)) filter = "";

            return Json(phccontext.ObterProdutosArmazem(3).Where(p => p.Ref_Produto.ToLower().Contains(filter.ToLower()) || p.Designacao_Produto.ToLower().Contains(filter.ToLower())).ToList());
        }
        [HttpPost]
        public JsonResult ObterPeca(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            //return Json(phccontext.ObterProdutosArmazem(ref_produto).ToList().FirstOrDefault() ?? new Produto());
            return Json(phccontext.ObterProdutoStamp(id));
        }
        public JsonResult ObterDetalhes(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return Json(phccontext.ObterProdutoStamp(id));
        }


        public ActionResult Armazem(int id, string gt)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Utilizador u = context.ObterListaUtilizadores(false, false).Where(u => u.IdArmazem == id).DefaultIfEmpty().First();
            List<string> LstGuias = phccontext.ObterGuiasTransporte(u.IdArmazem);
            if (string.IsNullOrEmpty(gt)) gt = LstGuias.First();

            ViewData["Guias"] = new SelectList(LstGuias);
            ViewData["GT"] = gt;

            Armazem a = phccontext.ObterArmazem(id);
            a.LstMovimentos = phccontext.ObterPecasGuiaTransporte(gt, u).OrderBy(m => m.DataMovimento).ToList();
            return View(a);
        }


        public JsonResult GerarGuiaGlobal(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return Json(phccontext.GerarGuiaGlobal(id));
        }
    }
}