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
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Validar(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            return View(context.ObterCodigo(id));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ContentResult Aprovar(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            context.AtualizarCodigo(id, 1, int.Parse(this.User.Claims.First().Value));
            return Content("1");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ContentResult Rejeitar(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            context.AtualizarCodigo(id, 2, int.Parse(this.User.Claims.First().Value));
            return Content("1");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Restart()
        {
            applicationLifetime.StopApplication();
            return View();
        }

        [HttpPost]
        public JsonResult Sugestao(string Obs, string file)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            MailContext.EnviarEmailSugestao(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)), Obs, new System.Net.Mail.Attachment(new MemoryStream(Convert.FromBase64String(file.Split(',').Last())), "PrintScreen_" + DateTime.Now.ToString("ddMMyyyy_HHmmss") + ".png"));
            return Json("Ok");
        }
    }
}
