namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio, Tech")]
    public class FormularioController : Controller
    {
        private readonly ILogger<FormularioController> _logger;

        public FormularioController(ILogger<FormularioController> logger)
        {
            _logger = logger;
        }

        //Obter todos os acessos de uma data em especifico
        [HttpGet]
        public ActionResult CertificaDetetorMetais(string id)
        {
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter formulario de certificação do detetor de metais: Marcacao ID: {3}", u.NomeCompleto, u.Id, id);
           
            return View(phccontext.ObterMarcacao(id));
        }
        [HttpPost]
        public ActionResult CertificaDetetorMetais(string email, string nome, string equipamento, string marcacao)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(equipamento) || string.IsNullOrEmpty(marcacao)) return StatusCode(500);

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a guardar formulario de certificação do detetor de metais: Equipamento Stamp: {3}", u.NomeCompleto, u.Id, equipamento);

            phccontext.CertificacaoDetetorMetais(email, nome, phccontext.ObterEquipamentoSimples(equipamento), phccontext.ObterMarcacao(marcacao), u);

            return RedirectToAction("Adicionar", "FolhasObra", new {id = marcacao });
        }
    }
}
