using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio")]
    public class AcessosController : Controller
    {

        // GET: FolhasObraController
        public ActionResult Index(string Data)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            if (Data == null || Data == string.Empty) Data = DateTime.Now.ToString("dd-MM-yyyy");
            ViewData["Data"] = Data;

            return View(context.ObterListaAcessos(DateTime.Parse(Data)));
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
                FileName = "MapaPresencas.xlsx",
                Inline = false,
                Size = file.Length,
                CreationDate = DateTime.Now,

            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            context.AdicionarLog(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).NomeUtilizador, "Foi gerado um mapa de presenças", 5);

            return File(output, System.Net.Mime.MediaTypeNames.Application.Xml);
        }

        public ActionResult Apagar(string id)
        {

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            
            context.ApagarAcesso(int.Parse(id));

            return RedirectToAction("Index");
        }
    }
}
