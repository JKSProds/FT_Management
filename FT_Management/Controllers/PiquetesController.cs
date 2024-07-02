
namespace FT_Management.Controllers
{

    [Authorize(Roles = "Admin")]
    public class PiquetesController : Controller
    {
        private readonly ILogger<PiquetesController> _logger;

        public PiquetesController(ILogger<PiquetesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
         public IActionResult Index(DateTime dInicio, DateTime dFim)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter os piquetes entre {3} e {4}.", u.NomeCompleto, u.Id, dInicio, dFim);

            if (dInicio == DateTime.MinValue) dInicio = DateTime.Now.AddDays(-((int)DateTime.Now.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7);
            if (dFim == DateTime.MinValue) dFim = DateTime.Now.AddDays(60).AddDays(-((int)DateTime.Now.AddDays(60).DayOfWeek - (int)DayOfWeek.Monday + 7) % 7);
            ViewBag.DataInicio = dInicio;
            ViewBag.DataFim = dFim;

            List<Utilizador> LstUtilizadores = context.ObterListaTecnicos(false, false).ToList();
            LstUtilizadores.Insert(0, new Utilizador() { Id = 0, NomeCompleto = "N/D" });
            ViewBag.ListaTecnicos = LstUtilizadores;

            ViewBag.Zonas = context.ObterZonas(true);
            ViewBag.TipoTecnico = context.ObterTipoTecnicos();

            return View(context.ObterPiquetes(dInicio, dFim));
        }

        [HttpPost]
         public IActionResult Piquete(string id, int t)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a atualizar o piquete com o STAMP - {3} para o utilizador - {4}.", u.NomeCompleto, u.Id, id, t);

            if (string.IsNullOrEmpty(id) || t == 0) return StatusCode(500);

            Piquete p = new Piquete() {
                Stamp = id,
                IdUtilizador = t,
                Utilizador = context.ObterUtilizador(t)
            };

            context.CriarPiquete(p);

            return StatusCode(200);
        }

        [HttpPost]
         public IActionResult Piquetes(DateTime dInicio, DateTime dFim)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a gerar os piquetes automaticos", u.NomeCompleto, u.Id);

            if (dInicio == DateTime.MinValue || dFim == DateTime.MinValue) return StatusCode(500);

            context.GerarPiquetes(dInicio, dFim);

            return StatusCode(200);
        }
    }
}
