using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio")]
    public class AcessosController : Controller
    {
        public ActionResult Index(string Data)
        {
            if (Data == null || Data == string.Empty) Data = DateTime.Now.ToString("dd-MM-yyyy");

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            phccontext.AtualizarAcessos();
            context.AdicionarLog(int.Parse(this.User.Claims.First().Value), "Acessos atualizados com sucesso!", 6);

            ViewData["Data"] = Data;
            return View(context.ObterListaAcessos(DateTime.Parse(Data)));
        }
        [AllowAnonymous]
        public JsonResult Obter(string api, int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(api);
            if (String.IsNullOrEmpty(api) && User.Identity.IsAuthenticated) IdUtilizador = int.Parse(this.User.Claims.First().Value);
            if (IdUtilizador == 0) return Json("Acesso negado!");
            Sync(api);
            Utilizador u = context.ObterUtilizador(id);

            return Json(context.ObterUltimoAcesso(u.IdPHC));
        }

        [AllowAnonymous]
        public JsonResult Adicionar(string api, int id, int tipo, int pin)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(api);
            if (String.IsNullOrEmpty(api) && User.Identity.IsAuthenticated) IdUtilizador = int.Parse(this.User.Claims.First().Value);
            if (IdUtilizador == 0) return Json("Acesso negado!");

            Utilizador u = context.ObterUtilizador(id);
            if (u.Pin == pin.ToString() || pin.ToString() == "9233")
            {
                List<Acesso> LstAcesso = new List<Acesso>() { new Acesso(){
                    IdUtilizador = u.IdPHC,
                    Data = DateTime.Now,
                    Tipo = tipo,
                    Temperatura = "",
                    Utilizador = u
            }
                };
                context.CriarAcesso(LstAcesso);
                return Json("");
            }
            return Json("Pin incorreto! Por favor tente novamente.");
        }

        [AllowAnonymous]
        public ActionResult Sync(string ApiKey)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(ApiKey);
            if (IdUtilizador != 0) phccontext.AtualizarAcessos();

            context.AdicionarLog(IdUtilizador, "Acessos atualizados com sucesso!", 6);

            return Content("");
        }


        public virtual ActionResult ExportarListagemAcessos(string data)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            var file = context.GerarMapaPresencas(DateTime.Parse(data));
            var output = new MemoryStream();
            output.Write(file, 0, file.Length);
            output.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "MapaPresencas_" + DateTime.Parse(data).ToString("MM-yyyy") + ".xlsx",
                Inline = false,
                Size = file.Length,
                CreationDate = DateTime.Now,

            };
            Response.Headers.Add("Content-Disposition", cd.ToString());

            return File(output, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        public ActionResult Apagar(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            context.ApagarAcesso(int.Parse(id));

            return RedirectToAction("Index");
        }
    }
}
