using FT_Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio")]
    public class ViaturasController : Controller
    {
        public ActionResult Viagens(string id, string Data)
        {
            if (Data == null || Data == string.Empty) Data = DateTime.Now.ToString("dd-MM-yyyy");

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            ViewData["Data"] = Data;
            ViewData["Matricula"] = id;

            return View(context.ObterViagens(id, Data));
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Escritorio")]
        public List<Viatura> ObterViaturas()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return context.ObterViaturas();
        }
    }
   
}
