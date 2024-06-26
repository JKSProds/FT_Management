﻿namespace FT_Management.Controllers
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
        public IActionResult Validar(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a aceder um codigo para validação: Codigo - {3}.", u.NomeCompleto, u.Id, id);

            return View(context.ObterCodigo(id));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ContentResult Aprovar(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a aprovar um codigo: Codigo - {3}.", u.NomeCompleto, u.Id, id);

            context.AtualizarCodigo(id, 1, u.Id);
            return Content("1");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ContentResult Rejeitar(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a rejeitar um codigo: Codigo - {3}.", u.NomeCompleto, u.Id, id);

            context.AtualizarCodigo(id, 2, u.Id);
            return Content("1");
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
        [Authorize]
        [HttpPost]
        public JsonResult Sugestao(string Obs, string file)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a enviar uma sugestão nova.", u.NomeCompleto, u.Id);

            MailContext.EnviarEmailSugestao(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)), Obs, new System.Net.Mail.Attachment(new MemoryStream(Convert.FromBase64String(file.Split(',').Last())), "PrintScreen_" + DateTime.Now.ToString("ddMMyyyy_HHmmss") + ".png"));
            return Json("Ok");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Notificacoes(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter as suas notificacoes.", u.NomeCompleto, u.Id);

            return Json(context.ObterNotificacoesPendentes(id).ToList());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Notificacoes(int id, string notificacao, string tipo)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a criar uma notificacao.", u.NomeCompleto, u.Id);

            Notificacao n = new Notificacao()
            {
                Mensagem = notificacao,
                UtilizadorDestino = context.ObterUtilizador(id),
                UtilizadorOrigem = u,
                Tipo = tipo
            };

            return Json(context.CriarNotificacao(n) ? StatusCode(200) : StatusCode(500));
        }

        [Authorize]
        [HttpDelete]
        public IActionResult Notificacoes(int id, bool apagar)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a apagar uma notificacao.", u.NomeCompleto, u.Id);

            return Json(context.ApagarNotificacao(id) ? StatusCode(200) : StatusCode(500));
        }

        [Authorize]
        [HttpGet]
        public IActionResult Codigo(string stamp, int pin)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter um código", u.NomeCompleto, u.Id);
            
            return context.ObterCodigo(stamp).ObsInternas == pin.ToString() ? StatusCode(200) : StatusCode(403);
        }
        [Authorize]
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
            
            foreach (Utilizador t in context.ObterListaUtilizadores(true, false).Where(o => o.Admin)) { ChatContext.EnviarNotificacaoCodigoSimples(codigo.ToString(), t, u); }

            return Content(stamp);
        }
    }
}
