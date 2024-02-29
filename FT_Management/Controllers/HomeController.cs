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

        [HttpGet]
        public IActionResult Codigo(string stamp, int pin)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter um código", u.NomeCompleto, u.Id);
            
            return context.ObterCodigo(stamp).ObsInternas == pin.ToString() ? StatusCode(200) : StatusCode(403);
        }
        [HttpPost]
        public IActionResult Codigo()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a gerar um código", u.NomeCompleto, u.Id);

            Random random = new Random();
            int codigo = random.Next(1000, 10000);
            string stamp = DateTime.Now.Ticks.ToString();
            
            context.CriarCodigo(new Models.Codigo() {Stamp = stamp, Estado=0, ObsInternas=codigo.ToString(), utilizador=u, ValidadeCodigo=DateTime.Now.AddMinutes(5) });
            
            foreach (var t in context.ObterListaUtilizadores(true, false).Where(v => v.Admin)) { MailContext.EnviarEmailCodigo(codigo.ToString(), t); }

            return Content(stamp);
        }

    }
}
