namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio")]
    public class InventarioController : Controller
    {
        private readonly ILogger<InventarioController> _logger;

        public InventarioController(ILogger<InventarioController> logger)
        {
            _logger = logger;
        }

        //Obter todos os armazens
        [HttpGet]
        public ActionResult Index()
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter todos os armazens.", u.NomeCompleto, u.Id);

            return View(phccontext.ObterArmazensFixos());
        }

        //Obter todos os dossiers associados ao um armazem
        [HttpGet]
        public JsonResult Dossiers(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter todos os inventarios do armazém: {3}.", u.NomeCompleto, u.Id, id);

            return new JsonResult(phccontext.ObterInventarios(id));
        }

        //Obter as linhas de um dossier filtrando pela referencia
        [HttpGet]
        public ActionResult Dossier(string id, string referencia)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            if (string.IsNullOrEmpty(referencia)) referencia = "";
            ViewData["Referencia"] = referencia;

            Picking p = phccontext.ObterInventario(id);
            p.Linhas = p.Linhas.Where(l => l.Ref_linha.Contains(referencia)).ToList();

            _logger.LogDebug("Utilizador {1} [{2}] a obter um dossier de inventario: Id - {3}.", u.NomeCompleto, u.Id, p.IdPicking);

            return View(p);
        }

        //Criar um dossier de inventario se o mesmo nao existir
        [HttpPost]
        public JsonResult Dossier(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a criar um invetario novo para o armazém: {3}.", u.NomeCompleto, u.Id, id);

            List<string> res = phccontext.CriarInventario(id, this.User.ObterNomeCompleto());
            res[2] = (!string.IsNullOrEmpty(res[2])) ? "/Inventario/Dossier/" + res[2] : "";

            return new JsonResult(res);
        }

        //Fechar o dossier para previnir alterações futuras
        [HttpDelete]
        public JsonResult Dossier(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Picking i = phccontext.ObterInventario(id);

            _logger.LogDebug("Utilizador {1} [{2}] a fechar um inventario: {3}.", u.NomeCompleto, u.Id, i.IdPicking);

            i.EditadoPor = u.NomeCompleto;
            return new JsonResult(phccontext.FecharInventario(i));
        }

        //Obter linha baseado no stamp
        [HttpGet]
        public JsonResult Linha(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Linha_Picking l = phccontext.ObterLinhaInventario(id);

            _logger.LogDebug("Utilizador {1} [{2}] a obter uma linha do inventario: Ref - {3}, Qtd - {4},.", u.NomeCompleto, u.Id, l.Ref_linha, l.Qtd_Linha);

            return new JsonResult(l);
        }

        //Criar linha nova
        [HttpPost]
        public JsonResult Linha(string stamp, string ref_produto, double qtd)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            Linha_Picking l = new Linha_Picking()
            {
                Picking_Linha_Stamp = stamp,
                Ref_linha = ref_produto,
                Qtd_Linha = qtd,
                EditadoPor = u.NomeCompleto
            };

            _logger.LogDebug("Utilizador {1} [{2}] a criar uma linha novo no inventario: Ref - {3}, Qtd - {4},.", u.NomeCompleto, u.Id, l.Ref_linha, l.Qtd_Linha);

            return new JsonResult(phccontext.CriarLinhaInventario(l));
        }

        //Apagar linha
        [HttpDelete]
        public JsonResult Linha(string stamp, string stamp_linha)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a apagar uma linha do inventario: STAMP - {3}, STAMP_LINHA - {4}.", u.NomeCompleto, u.Id, stamp, stamp_linha);

            return new JsonResult(phccontext.ApagarLinhaInventario(new Ref_Linha_Picking() { BOMA_STAMP = stamp, Picking_Linha_Stamp = stamp_linha }));
        }

        //Obter todos os numeros de serie associados a uma linha
        [HttpGet]
        public JsonResult Serie(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a apagar uma linha do inventario: STAMP - {3}.", u.NomeCompleto, u.Id, id);

            return new JsonResult(phccontext.ObterSerieLinhaInventario(id).OrderBy(s => s.CriadoA).ToList());
        }

        //Criar numeros de serie associados a uma linha
        [HttpPost]
        public JsonResult Serie(string stamp, string stamp_linha, string serie)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a apagar uma linha do inventario: STAMP - {3}, LINHA - {4}, SERIE - {5}.", u.NomeCompleto, u.Id, stamp, stamp_linha, serie);

            Ref_Linha_Picking l = new Ref_Linha_Picking()
            {
                Picking_Linha_Stamp = stamp_linha,
                BOMA_STAMP = stamp,
                NumSerie = serie,
                CriadoPor = this.User.ObterNomeCompleto()
            };

            return new JsonResult(phccontext.CriarSerieLinhaInventario(l));
        }

        //Apagar numeros de serie baseado no stamp de numero de serie
        [HttpDelete]
        public JsonResult ApagarSerie(string stamp, string stamp_boma, string stamp_linha)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a apagar uma linha do inventario: STAMP - {3}, BOMA - {4}, STAMP LINHA - {5}.", u.NomeCompleto, u.Id, stamp, stamp_boma, stamp_linha);

            return new JsonResult(phccontext.ApagarLinhaSerieInventario(stamp, new Ref_Linha_Picking() { BOMA_STAMP = stamp_boma, Picking_Linha_Stamp = stamp_linha, CriadoPor = this.User.ObterNomeCompleto() }));
        }
    }

}
