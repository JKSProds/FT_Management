using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Comercial, Escritorio")]
    public class VisitasController : Controller
    {
        // GET: VisitasController
        public ActionResult Index()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return View(context.ObterListaComerciais());
        }

        public ActionResult ListaVisitas(string IdComercial, string DataVisitas)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            if (DataVisitas == null || DataVisitas == string.Empty) DataVisitas = DateTime.Now.ToString("dd-MM-yyyy");
            ViewData["DataVisitas"] = DataVisitas;

            List<Visita> ListaVisitas = context.ObterListaVisitas(int.Parse(IdComercial), DateTime.Parse(DataVisitas), DateTime.Parse(DataVisitas));
            ViewData["IdComercial"] = IdComercial;
            return View(ListaVisitas);
        }
    }
}
