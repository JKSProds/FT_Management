namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio, Tech, Comercial")]
    public class EquipamentosController : Controller
    {
        private readonly ILogger<EquipamentosController> _logger;

        public EquipamentosController(ILogger<EquipamentosController> logger)
        {
            _logger = logger;
        }

        //Obter lista de equipamentos com filtro do numero de serie
        [HttpGet]
        public IActionResult Index(string Serie)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter lista de todos os equipamento com base no seguinte filtro: S/N - {3}", u.NomeCompleto, u.Id, Serie);

            if (string.IsNullOrEmpty(Serie)) Serie = "";
            ViewData["Serie"] = Serie;

            return View(phccontext.ObterEquipamentosSerie(Serie));
        }

        //Obter um equipamento em especifico com base num stamp
        [HttpGet]
        public IActionResult Equipamento(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            Equipamento e = phccontext.ObterEquipamento(id);
            _logger.LogDebug("Utilizador {1} [{2}] a obter um equipamento em especifico: Marca - {3}, Modelo - {4}, S/N - {5}", u.NomeCompleto, u.Id, e.MarcaEquipamento, e.ModeloEquipamento, e.NumeroSerieEquipamento);

            return View(e);
        }

        //Obter todos os equipamento com base num filtro
        [HttpGet]
        public JsonResult Equipamentos(int no, int loja, string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) prefix = "";

            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter todos os equipamentos com base num filtro: No - {3}, Estab - {4}, Filtro - {5}", u.NomeCompleto, u.Id, no, loja, prefix);

            if (no == 0 && loja == 0) return Json(phccontext.ObterEquipamentosSerie(prefix));
            return Json(phccontext.ObterEquipamentos(new Cliente() { IdCliente = no, IdLoja = loja }).Where(e => e.NumeroSerieEquipamento.ToLower().Contains(prefix.ToLower())).OrderBy(e => e.NumeroSerieEquipamento).ToList());
        }

        //Obter historico de um equipamento em especifico
        [HttpGet]
        public JsonResult Historico(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            Equipamento e = phccontext.ObterEquipamentoSimples(id);
            _logger.LogDebug("Utilizador {1} [{2}] a obter histórico de um equipamento em especifico: Marca - {3}, Modelo - {4}, S/N - {5}", u.NomeCompleto, u.Id, e.MarcaEquipamento, e.ModeloEquipamento, e.NumeroSerieEquipamento);

            return Json(new { json = phccontext.ObterHistorico(id) });
        }

        //Criar Codigo para atualizar cliente
        [HttpPost]
        public ActionResult Codigo(string id, string equipamento, string cliente)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Cliente cl = phccontext.ObterClienteSimples(cliente);
            Equipamento e = phccontext.ObterEquipamento(equipamento);

            _logger.LogDebug("Utilizador {1} [{2}] a criar um codigo para atualizar o cliente de um equipamento em especifico: Marca - {3}, Modelo - {4}, S/N - {5}, Cliente - {6}", u.NomeCompleto, u.Id, e.MarcaEquipamento, e.ModeloEquipamento, e.NumeroSerieEquipamento, cl.NomeCliente);

            if (this.User.IsInRole("Admin"))
            {
                Cliente(e.EquipamentoStamp, cl.ClienteStamp, "");
                return Content("Refresh");
            }
            else
            {
                Codigo c = new Codigo()
                {
                    Stamp = id,
                    Estado = 0,
                    ValidadeCodigo = DateTime.Now.AddMinutes(10),
                    utilizador = u
                };
                c.Obs = "Deseja associar o equipamento " + e.MarcaEquipamento + " " + e.ModeloEquipamento + " com número de serie: " + e.NumeroSerieEquipamento + " ao cliente: " + cl.NomeCliente + "?";

                context.CriarCodigo(c);
                foreach (var utilizador in context.ObterListaUtilizadores(false, false).Where(u => u.Admin))
                {
                    ChatContext.EnviarNotificacaoCodigo(c, utilizador);
                }
            }

            return Content("OK");
        }

        //Associar cliente ao um equipamento
        [HttpPost]
        public ActionResult Cliente(string id, string stamp, string codigo)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            if (context.ValidarCodigo(codigo) == 1 || User.IsInRole("Admin"))
            {
                Cliente cl = phccontext.ObterClienteSimples(stamp);
                Equipamento e = phccontext.ObterEquipamento(id);

                _logger.LogDebug("Utilizador {1} [{2}] a atualizar o cliente de um equipamento em especifico: Marca - {3}, Modelo - {4}, S/N - {5}, Cliente - {6}", u.NomeCompleto, u.Id, e.MarcaEquipamento, e.ModeloEquipamento, e.NumeroSerieEquipamento, cl.NomeCliente);

                return Content(phccontext.AtualizarClienteEquipamento(cl, e, context.ObterUtilizador(int.Parse(this.User.Claims.First().Value))).ToString());
            }
            else
            {
                return Content("False");
            }

        }
    }
}