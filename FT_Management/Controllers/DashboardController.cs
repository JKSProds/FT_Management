namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio, Dashboard")]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        //Obter dashboard com todos os utilizadores realizarem acesso
        [HttpGet]
        public IActionResult Utilizadores(string Api)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            
            int IdUtilizador = int.Parse(this.User.Claims.First().Value);
            Utilizador u = context.ObterUtilizador(IdUtilizador);
            if (u.Id == 0) return Forbid();

            _logger.LogDebug("Utilizador {1} [{2}] a obter dashboard dos Utilizadores.", u.NomeCompleto, u.Id);

            ViewData["API"] = Api;
            ViewData["Hora"] = context.ObterParam("ShowAcessTime");
            ViewData["Acesso"] = context.ObterParam("ShowAcess");

            using (HttpClient wc = new HttpClient())
            {
                dynamic json = JsonConvert.DeserializeObject(wc.GetStringAsync("https://zenquotes.io/api/random").Result.Replace("[", "").Replace("]", ""));
                ViewData["Frase"] = json.q + "<br><b>" + json.a + "</b>";
            }

            List<Utilizador> LstUtilizadores = context.ObterListaUtilizadores(true, false).Where(u => u.Acessos).ToList();
            return View(LstUtilizadores);
        }


    }
}
