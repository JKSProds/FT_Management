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
        public ActionResult Adicionar()
        {
            //FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

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
        public ActionResult Adicionar(FolhaObra folhaObra)
        {
            try
            {
                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                folhaObra.DataServico = DateTime.Parse(DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day);
                return RedirectToAction("Editar", new { id = context.NovaFolhaObra(folhaObra) });
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult AdicionarIntervencao(string data, string horainicio, string horafim, string tecnico, string idfolhaobra)
        {
            try
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
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult AdicionarPeca(string referencia, string designacao, string quantidade, string idfolhaobra)
        {
            try
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
            catch
            {
                return View();
            }
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

        public ActionResult PrintFolhaObra(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
           

            var file = context.FillForm(context.ObterFolhaObra(id)).ToArray();
            var output = new MemoryStream();
            output.Write(file, 0, file.Length);
            output.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "FolhaObra_"+ id +".pdf",
                Inline = false,
                Size = file.Length,
                CreationDate = DateTime.Now

            };

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
            try
            {
                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                folhaObra.IdFolhaObra = id;
                return RedirectToAction("Editar", new { id = context.NovaFolhaObra(folhaObra)});
            }
            catch
            {
                return View();
            }
        }

        // GET: FolhasObraController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: FolhasObraController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
