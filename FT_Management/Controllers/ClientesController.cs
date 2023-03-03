namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio, Comercial")]
    public class ClientesController : Controller
    {
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(ILogger<ClientesController> logger)
        {
            _logger = logger;
        }

        //Obter listagem de todos os clientes com um filtro
        [HttpGet]
        public IActionResult Index(string Nome)
        {
            if (Nome == null) Nome = "";

            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1}({2}) a obter todos os cliente com base no seguinte filtro: {3}", u.NomeCompleto, u.Id, Nome);

            ViewData["Nome"] = Nome;

            return View(phccontext.ObterClientes(Nome, true));
        }

        //Obter um cliente em especifico
        [HttpGet]
        public IActionResult Cliente(int IdCliente, int IdLoja)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Cliente c = phccontext.ObterCliente(IdCliente, IdLoja);

            _logger.LogDebug("Utilizador {1}({2}) a obter os dados do seguinte cliente: ID - {3}, Estab - {4}, Nome - {5}", u.NomeCompleto, u.Id, c.IdCliente, c.IdLoja, c.NomeCliente);

            return View(c);
        }

        //Obter todos os clientes com base num filtro
        [HttpGet]
        public JsonResult Clientes(string prefix)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            if (prefix is null) prefix = "";

            _logger.LogDebug("Utilizador {1}({2}) a obter todos os clientes com base no seguinte filtro: {3}", u.NomeCompleto, u.Id, prefix);

            return Json(phccontext.ObterClientes(prefix, true));
        }

        //Criar a senha para um cliente
        [HttpPost]
        public IActionResult Senha(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Cliente c = phccontext.ObterClienteSimples(id);

            _logger.LogDebug("Utilizador {1}({2}) a criar uma senha para o seguinte cliente: ID - {3}, Estab - {4}, Nome - {5}", u.NomeCompleto, u.Id, c.IdCliente, c.IdLoja, c.NomeCliente);

            return Content(context.CriarSenhaCliente(id).ToString());
        }

        //Enviar o email com a senha de um cliente em especifico
        [HttpPost]
        public IActionResult EmailSenha(int id, int loja, string email)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Cliente c = phccontext.ObterClienteSimples(id, loja);

            _logger.LogDebug("Utilizador {1}({2}) a enviar um email com a senha para o seguinte cliente: ID - {3}, Estab - {4}, Nome - {5}", u.NomeCompleto, u.Id, c.IdCliente, c.IdLoja, c.NomeCliente);

            return Content(MailContext.EnviarEmailSenhaCliente(email, c) ? "1" : "");
        }
    }
}
