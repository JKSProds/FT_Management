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
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            phccontext.AtualizarClientes();

            ViewData["Nome"] = Nome;
            if (Nome == null) Nome = "";

            int pageSize = 100;
            var pageNumber = page ?? 1;


            return View(context.ObterListaClientes(Nome, false).ToPagedList(pageNumber, pageSize));
        }

        public IActionResult Cliente(int IdCliente, int IdLoja)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            phccontext.AtualizarClientes();
            return View(context.ObterClienteCompleto(IdCliente, IdLoja));
        }
    }
}
