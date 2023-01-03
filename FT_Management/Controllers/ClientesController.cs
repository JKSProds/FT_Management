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
            if (Nome == null) Nome = "";

            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            int pageSize = 100;
            var pageNumber = page ?? 1;

            ViewData["Nome"] = Nome;

            return View(phccontext.ObterClientes(Nome, true).ToPagedList(pageNumber, pageSize));
        }

        public IActionResult Cliente(int IdCliente, int IdLoja)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return View(phccontext.ObterCliente(IdCliente, IdLoja));
        }

        public IActionResult GerarSenha(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return Content(context.CriarSenhaCliente(id).ToString());
        }
        public IActionResult EmailSenhaCliente(int id, int loja, string email)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return Content(MailContext.EnviarEmailSenhaCliente(email, phccontext.ObterClienteSimples(id, loja)) ? "Sucesso" : "");
        }
        [HttpPost]
        public JsonResult ObterClientes(string prefix)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (prefix is null) prefix = "";

            return Json(phccontext.ObterClientes(prefix, true));
        }
    }
}
