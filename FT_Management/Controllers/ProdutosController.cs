namespace FT_Management.Controllers
{
    [Authorize]
    public class ProdutosController : Controller
    {
        private readonly ILogger<ProdutosController> _logger;

        public ProdutosController(ILogger<ProdutosController> logger)
        {
            _logger = logger;
        }

        //Obter todas as referencias baseadas num filtro
        [HttpGet]
        public ActionResult Index(string Ref, string Desig, int Armazem, int Fornecedor, string TipoEquipamento)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter todas as referencias baseadas num filtro: Ref - {3}, Desig - {4}, Armazem - {5}, Fornecedor - {6}, TipoEquipamento - {6}.", u.NomeCompleto, u.Id, Ref, Desig, Armazem, Fornecedor, TipoEquipamento);

            var LstArmazens = phccontext.ObterArmazens();
            var LstFornecedores = phccontext.ObterFornecedores().Where(p => !string.IsNullOrEmpty(p.CodigoIntermedio));
            var LstTiposEquipamento = phccontext.ObterTiposEquipamento();
            if (Ref == null) { Ref = ""; }
            if (Desig == null) { Desig = ""; }
            if (Armazem == 0) { Armazem = 3; }
            if (TipoEquipamento == null) { TipoEquipamento = ""; }

            ViewData["Ref"] = Ref;
            ViewData["Desig"] = Desig;
            ViewData["Armazem"] = Armazem;
            ViewData["Fornecedor"] = Fornecedor;
            ViewData["TipoEquipamento"] = TipoEquipamento;
            ViewData["Armazens"] = new SelectList(LstArmazens, "ArmazemId", "ArmazemNome", Armazem);
            ViewData["Fornecedores"] = new SelectList(LstFornecedores, "IdFornecedor", "NomeFornecedor", Armazem);
            ViewData["TiposEquipamento"] = new SelectList(LstTiposEquipamento);

            if (Armazem > 9)
            {
                return View(phccontext.ObterProdutos(Ref, Desig, Armazem, Fornecedor, TipoEquipamento).Where(p => p.Stock_PHC - p.Stock_Res > 0));
            }

            return View(phccontext.ObterProdutos(Ref, Desig, Armazem, Fornecedor, TipoEquipamento));
        }

        //Obter detalhes de um produto em especifo num armazem
        [HttpGet]
        [Authorize(Roles = "Admin, Escritorio")]
        public ActionResult Produto(string id, int armazemid)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            var LstArmazens = phccontext.ObterArmazens();

            if (armazemid == 0) { armazemid = 3; }

            Produto p = phccontext.ObterProduto(id, armazemid);

            _logger.LogDebug("Utilizador {1} [{2}] a obter os detalhes de um produto em especifico: Ref - {3}, Desig - {4}, Armazem - {5}.", u.NomeCompleto, u.Id, p.Ref_Produto, p.Designacao_Produto, p.Armazem_ID);

            ViewData["LstProdutosArmazem"] = phccontext.ObterProdutosArmazem(id);
            ViewData["Armazens"] = new SelectList(LstArmazens, "ArmazemId", "ArmazemNome", armazemid);

            return View(p);
        }

        //Obter uma peca através do stamp ou ref
        [HttpGet]
        public JsonResult Peca(string id, string ref_produto, int armazem)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            if (armazem == 0) armazem = 3;
            _logger.LogDebug("Utilizador {1} [{2}] a obter uma peça em especifico: Ref - {3}, Stamp - {4}.", u.NomeCompleto, u.Id, ref_produto, id);

            if (!string.IsNullOrEmpty(id)) return Json(phccontext.ObterProdutoStamp(id));
            if (!string.IsNullOrEmpty(ref_produto)) return Json(phccontext.ObterProduto(ref_produto, armazem));

            return Json("");
        }

        //Obter todas as pecas baseadas num filtro e que estejam num armazem
        [HttpGet]
        public JsonResult Pecas(string filter, int armazem)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter todas as peças de um armazem em especifico: Filtro - {3}, Armazem - {4}.", u.NomeCompleto, u.Id, filter, armazem);

            if (armazem == 0) armazem = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).IdArmazem;
            if (string.IsNullOrEmpty(filter)) filter = "";

            return Json(phccontext.ObterProdutosArmazem(armazem).Where(p => p.Ref_Produto.ToLower().Contains(filter.ToLower()) || p.Designacao_Produto.ToLower().Contains(filter.ToLower())).ToList());
        }

        //Obter peças em uso num armazem
        [HttpGet]
        public ActionResult Armazem(int id, string gt)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Utilizador t = context.ObterListaUtilizadores(false, false).Where(u => u.IdArmazem == id).DefaultIfEmpty().First();

            _logger.LogDebug("Utilizador {1} [{2}] a obter todas as peças em uso de um armazem em especifico: Armazem - {3}, GT - {4}.", u.NomeCompleto, u.Id, id, gt);

            List<string> LstGuias = phccontext.ObterGuiasTransporte(t.IdArmazem);
            if (string.IsNullOrEmpty(gt)) gt = LstGuias.First();

            ViewData["Guias"] = new SelectList(LstGuias);
            ViewData["GT"] = gt;

            Armazem a = phccontext.ObterArmazem(id);
            a.LstMovimentos = phccontext.ObterPecasGuiaTransporte(gt, t).OrderBy(m => m.DataMovimento).ToList();
            return View(a);
        }

        //Gerar guia global
        [HttpPost]
        public JsonResult GuiaGlobal(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a gerar a guia global do armazem: {3}.", u.NomeCompleto, u.Id, id);

            return Json(phccontext.GerarGuiaGlobal(id));
        }

        //Imprimir etiqueta normal
        [HttpGet]
        public virtual ActionResult Etiqueta(string id, int armazemid)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a imprimir uma etiqueta normal: Id - {3}, Armazem {4}.", u.NomeCompleto, u.Id, id, armazemid);

            return File(context.MemoryStreamToPDF(context.DesenharEtiquetaProduto(phccontext.ObterProduto(id, armazemid)), 801, 504), "application/pdf");
        }

        //Imprimir etiqueta pequena
        [HttpGet]
        public virtual ActionResult EtiquetaPequena(string id, int armazemid)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a imprimir uma etiqueta normal: Id - {3}, Armazem {4}.", u.NomeCompleto, u.Id, id, armazemid);

            return File(context.MemoryStreamToPDF(context.DesenharEtiquetaProdutoPequena(phccontext.ObterProduto(id, armazemid)), 801, 504), "application/pdf");
        }

        //Imprimir multiplas etiquetas
        [HttpGet]
        public ActionResult EtiquetaMultipla(string id, int armazemid)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a imprimir uma etiqueta multipla: Id - {3}, Armazem {4}.", u.NomeCompleto, u.Id, id, armazemid);

            return File(context.MemoryStreamToPDF(context.DesenharEtiquetaMultipla(phccontext.ObterProduto(id, armazemid)), 801, 504), "application/pdf");
        }

    }
}