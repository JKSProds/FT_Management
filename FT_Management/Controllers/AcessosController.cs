using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FT_Management.Controllers
{
    public class AcessosController : Controller
    {
        [Authorize(Roles = "Admin, Escritorio")]
        // GET: FolhasObraController
        public ActionResult Index(string Data)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            context.CriarAcessos();
           
            if (Data == null || Data == string.Empty) Data = DateTime.Now.ToString("dd-MM-yyyy");
            ViewData["Data"] = Data;        

            return View(context.ObterListaAcessos(DateTime.Parse(Data).ToString("yyyy-MM-dd")));
        }
    }
}
