using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio, Comercial")]
    public class ClientesController : Controller
    {
        public IActionResult Index(int? page, string Nome)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            ViewData["Nome"] = Nome;
            if (Nome == null) Nome = "";

            int pageSize = 100;
            var pageNumber = page ?? 1;


            return View(phccontext.ObterClientes(Nome, true).ToPagedList(pageNumber, pageSize));
        }

        public IActionResult Cliente(int IdCliente, int IdLoja)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            return View(phccontext.ObterCliente(IdCliente, IdLoja));
        }
    }
}
