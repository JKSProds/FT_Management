using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FT_Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FT_Management.Controllers
{
    public class FolhasObraController : Controller
    {
        // GET: FolhasObraController
        public ActionResult Index()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return View(context.ObterListaFolhasObra("", ""));
        }

        

        // GET: FolhasObraController/Create
        public ActionResult Adicionar(string idCartao)
        {
            //FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            if (idCartao == null) idCartao = "";
            ViewData["IdCartao"] = idCartao;
            FolhaObra folha = new FolhaObra
            {
                EquipamentoServico = new Equipamento(),
                PecasServico = new List<Produto>(),
                IntervencaosServico = new List<Intervencao>(),
            };

            return View(folha);
        }

        // POST: FolhasObraController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Adicionar(FolhaObra folhaObra, string IdCartao)
        {
            if (folhaObra.EquipamentoServico.NumeroSerieEquipamento == null || folhaObra.ClienteServico.NomeCliente == null) return View(folhaObra);
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            folhaObra.DataServico = DateTime.Parse(DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day);
            folhaObra.IdCartao = IdCartao == null ? "" : IdCartao;
            return RedirectToAction("Editar", new { id = context.NovaFolhaObra(folhaObra) });

        }

        [HttpPost]
        public ActionResult AdicionarIntervencao(string data, string horainicio, string horafim, string tecnico, string idfolhaobra)
        {
 
                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                Intervencao intervencao = new Intervencao()
                {
                    IdFolhaObra = int.Parse(idfolhaobra),
                    NomeTecnico = tecnico,
                    DataServiço = DateTime.Parse(data),
                    HoraInicio = DateTime.Parse(horainicio),
                    HoraFim = DateTime.Parse(horafim)
                };
                context.NovaIntervencao(intervencao);

                return Content(string.Empty);

        }

        [HttpPost]
        public ActionResult AdicionarPeca(string referencia, string designacao, string quantidade, string idfolhaobra)
        {
                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                Produto produto = new Produto()
                {
                    Ref_Produto = referencia,
                    Designacao_Produto = designacao,
                    Stock_Fisico = int.Parse(quantidade)
                };
                context.NovaPecaIntervencao(produto, idfolhaobra);

                return Content(string.Empty);
        }

        public JsonResult ObterDesignacaoProduto (string RefProduto)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return Json(new { result = context.ObterProduto(RefProduto).Designacao_Produto });
        }

        public JsonResult ObterEquipamento(string NumeroSerieEquipamento)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return Json(new { json = context.ObterEquipamentoNS(NumeroSerieEquipamento) });
        }
        public JsonResult ObterCliente(string NomeCliente)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return Json(new { json = context.ObterListaClientes(NomeCliente).FirstOrDefault() }) ;
        }

        public virtual ActionResult PrintFolhaObra(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
           

            var file = context.PreencherFormularioFolhaObra(context.ObterFolhaObra(id)).ToArray();
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
        // GET: FolhasObraController/Edit/5
        public ActionResult Editar(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return View(context.ObterFolhaObra(id));
        }

        // POST: FolhasObraController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(int id, FolhaObra folhaObra)
        {
                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                folhaObra.IdFolhaObra = id;
                return RedirectToAction("Editar", new { id = context.NovaFolhaObra(folhaObra)});

        }


        // POST: FolhasObraController/Delete/5
        [HttpPost]
        public ActionResult ApagarFolhaObra(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            foreach (Intervencao intervencao in context.ObterFolhaObra(id).IntervencaosServico)
            {
                context.ApagarIntervencao(intervencao.IdIntervencao);
            }

            foreach (Produto peca in context.ObterFolhaObra(id).PecasServico)
            {
                context.ApagarPecaFolhaObra(peca.Ref_Produto, id);
            }

            context.ApagarFolhaObra(id);
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public ActionResult ApagarIntervencao(int id)
        {
                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                context.ApagarIntervencao(id);
                return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public ActionResult ApagarPeca(string Ref, string Id)
        {

                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                context.ApagarPecaFolhaObra(Ref, int.Parse(Id));
                return RedirectToAction(nameof(Index));

        }
    }
}
