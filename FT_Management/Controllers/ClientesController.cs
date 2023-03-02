namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio, Comercial")]
    public class ClientesController : Controller
    {
        //Obter listagem de todos os clientes com um filtro
        [HttpGet]
        public IActionResult Index(string Nome)
        {
            if (Nome == null) Nome = "";

            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            ViewData["Nome"] = Nome;

            return View(phccontext.ObterClientes(Nome, true));
        }

        //Obter um cliente em especifico
        [HttpGet]
        public IActionResult Cliente(int IdCliente, int IdLoja)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return View(phccontext.ObterCliente(IdCliente, IdLoja));
        }

        //Obter todos os clientes com base num filtro
        [HttpGet]
        public JsonResult Clientes(string prefix)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (prefix is null) prefix = "";

            return Json(phccontext.ObterClientes(prefix, true));
        }

        //Criar a senha para um cliente
        [HttpPost]
        public IActionResult Senha(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return Content(context.CriarSenhaCliente(id).ToString());
        }

        //Enviar o email com a senha de um cliente em especifico
        [HttpPost]
        public IActionResult EmailSenha(int id, int loja, string email)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return Content(MailContext.EnviarEmailSenhaCliente(email, phccontext.ObterClienteSimples(id, loja)) ? "1" : "");
        }
    }
}
