namespace FT_Management.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        IHostApplicationLifetime applicationLifetime;

        public HomeController(ILogger<HomeController> logger, IHostApplicationLifetime appLifetime)
        {
            _logger = logger;
            applicationLifetime = appLifetime;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AcessoNegado()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string id)
        {
            return View(new ErrorViewModel { RequestId = !string.IsNullOrEmpty(id) ? id : Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Restart()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a parar a aplicação manualmente!", u.NomeCompleto, u.Id);

            applicationLifetime.StopApplication();
            return View();
        }

    }
}
