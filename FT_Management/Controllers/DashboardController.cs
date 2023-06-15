namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio")]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        //Obter dashboard com todas as encomendas
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Encomendas(string Api)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(Api);
            if (String.IsNullOrEmpty(Api) && User.Identity.IsAuthenticated) IdUtilizador = int.Parse(this.User.Claims.First().Value);
            Utilizador u = context.ObterUtilizador(IdUtilizador);
            if (u.Id == 0 || (!u.Admin && u.TipoUtilizador != 3)) return Forbid();

            _logger.LogDebug("Utilizador {1} [{2}] a obter dashboard das encomendas.", u.NomeCompleto, u.Id);

            return View(phccontext.ObterEncomendas().Where(d => d.NumDossier != 2).Where(e => !e.Fornecido));
        }

        //Obter dashboard com todos os utilizadores realizarem acesso
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Utilizadores(string Api)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(Api);
            if (String.IsNullOrEmpty(Api) && User.Identity.IsAuthenticated) IdUtilizador = int.Parse(this.User.Claims.First().Value);
            Utilizador u = context.ObterUtilizador(IdUtilizador);
            if (u.Id == 0 || (!u.Admin && u.TipoUtilizador != 3)) return Forbid();

            _logger.LogDebug("Utilizador {1} [{2}] a obter dashboard dos Utilizadores.", u.NomeCompleto, u.Id);

            ViewData["API"] = Api;
            ViewData["Hora"] = context.ObterParam("ShowAcessTime");
            ViewData["Acesso"] = context.ObterParam("ShowAcess");
            List<Utilizador> LstUtilizadores = context.ObterListaUtilizadores(true, false).Where(u => u.Acessos).ToList();
            List<Ferias> LstFerias = context.ObterListaFerias(DateTime.Parse(DateTime.Now.ToLongDateString() + " 00:00:00"), DateTime.Parse(DateTime.Now.ToLongDateString() + " 23:59:59"));
            foreach (Ferias f in LstFerias)
            {
                if (LstUtilizadores.Where(u => u.Id == f.IdUtilizador).Count() > 0) LstUtilizadores.Where(u => u.Id == f.IdUtilizador).First().Ferias = true;
            }
            return View("UtilizadoresNew", LstUtilizadores);
        }

        //Obter dashboard das marcacoes pendentes
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Pendentes(string Api)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(Api);
            if (String.IsNullOrEmpty(Api) && User.Identity.IsAuthenticated) IdUtilizador = int.Parse(this.User.Claims.First().Value);
            Utilizador u = context.ObterUtilizador(IdUtilizador);
            if (u.Id == 0 || (!u.Admin && u.TipoUtilizador != 3)) return Forbid();

            _logger.LogDebug("Utilizador {1} [{2}] a obter dashboard dos pendentes.", u.NomeCompleto, u.Id);

            return View(phccontext.ObterMarcacoesPendentes());
        }

        //Obter dashboard das marcacoes atuais e estados
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Marcacoes(string Api)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(Api);
            if (String.IsNullOrEmpty(Api) && User.Identity.IsAuthenticated) IdUtilizador = int.Parse(this.User.Claims.First().Value);
            Utilizador u = context.ObterUtilizador(IdUtilizador);
            if (u.Id == 0 || (!u.Admin && u.TipoUtilizador != 3)) return Forbid();

            _logger.LogDebug("Utilizador {1} [{2}] a obter dashboard das Marcacoes.", u.NomeCompleto, u.Id);

            List<Utilizador> LstUtilizadores = context.ObterListaTecnicos(true, false).Where(u => u.Dashboard).ToList();
            List<Marcacao> LstMarcacao = phccontext.ObterMarcacoes(DateTime.Now.AddDays(-7), DateTime.Now);
            List<int> LstMarcacaosPendentes = phccontext.ObterPercentagemMarcacoes();

            for (int i = 0; i <= LstUtilizadores.Count() - 1; i++)
            {
                LstUtilizadores[i].LstMarcacoes = LstMarcacao.Where(m => m.Tecnico.Id == LstUtilizadores[i].Id).ToList();
            }

            ViewData["Pecas"] = LstMarcacaosPendentes[0];
            ViewData["Orcamento"] = LstMarcacaosPendentes[1];
            ViewData["Pendentes30"] = LstMarcacaosPendentes[2];
            ViewData["Finalizados30"] = LstMarcacaosPendentes[3];
            ViewData["Oficina"] = LstMarcacaosPendentes[4];
            ViewData["FinalizadosSemana"] = LstMarcacaosPendentes[5];
            ViewData["TotaisSemana"] = LstMarcacaosPendentes[6];
            ViewData["Pendentes90"] = LstMarcacaosPendentes[7];
            ViewData["Finalizados90"] = LstMarcacaosPendentes[8];


            return View(LstUtilizadores);
        }


    }
}
