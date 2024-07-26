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
            var LstFornecedores = phccontext.ObterFornecedores(false).Where(p => !string.IsNullOrEmpty(p.CodigoIntermedio));
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

        //Obter Transferencia em Viagem do Técnico
        [HttpGet]
        public ActionResult Viagens(int id) {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Utilizador t = context.ObterListaUtilizadores(true, false).Where(u => u.IdArmazem == id).DefaultIfEmpty(new Utilizador()).First();

            if (!u.Admin && u.IdArmazem != id) return StatusCode(500);

            _logger.LogDebug("Utilizador {1} [{2}] a obter todas as transferencias em viagem em especifico: Filtro - {3}, Armazem - {4}.", u.NomeCompleto, u.Id, u.IdArmazem, id);

            return Json(phccontext.ObterTransferenciaViagemAbertas(t));
        }

        //validar Transferencia em Viagem do Técnico
        [HttpGet]
        public ActionResult Viagem(string id) {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter uma transferencia em viagem em especifico: Armazem - {3}, Linhas - {4}.", u.NomeCompleto, u.Id, u.IdArmazem, id);
            
            Dossier d = phccontext.ObterDossier(id);
            d.Linhas = phccontext.ObterLinhasViagem(id);

            if (d.Linhas.Count() == 0) return Forbid();
            return View(d);
        }

                //validar Transferencia em Viagem do Técnico
        [HttpPost]
        public ActionResult Viagem(string id, string linhas) {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a validar uma transferencia em viagem em especifico: Armazem - {3}, ID TV - {4}.", u.NomeCompleto, u.Id, u.IdArmazem, id);
            
            Dossier d = phccontext.ObterDossier(id);

            List<Linha_Dossier> LstLinhas = linhas.Split(';')
            .Select(l => l.Split('|'))
            .Where(p => p.Length == 2)
            .Select(p => new Linha_Dossier() { Stamp_Linha = p[0], Quantidade = int.Parse(p[1]), Stamp_Dossier = d.StampDossier })
            .ToList();
            
            d.Linhas = LstLinhas;

            foreach (var l in d.Linhas) {
                phccontext.ValidarTransferenciaViagem(l, u);
            }

            phccontext.FecharTransferenciaViagem(d, u);
            return StatusCode(200);
        }
        
        //Obter peças em uso num armazem
        [HttpGet]
        public ActionResult Armazem(int id, string gt)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            if (id == 0) id = u.IdArmazem;
            Utilizador t = context.ObterListaUtilizadores(false, false).Where(u => u.IdArmazem == id).DefaultIfEmpty().First();

            _logger.LogDebug("Utilizador {1} [{2}] a obter todas as peças em uso de um armazem em especifico: Armazem - {3}, GT - {4}.", u.NomeCompleto, u.Id, id, gt);

            List<string> LstGuias = phccontext.ObterGuiasTransporte(id);
            if (string.IsNullOrEmpty(gt)) gt = LstGuias.Count() == 0 ? "Sem Guias Globais" : LstGuias.First();

            ViewData["Guias"] = new SelectList(LstGuias);
            ViewData["GT"] = gt;

            Armazem a = phccontext.ObterArmazem(id);
            if (t != null) a.LstMovimentos = phccontext.ObterPecasGuiaTransporte(gt, t).OrderBy(m => m.DataMovimento).ToList();
            return View(a);
        }

        //Obter peças em garantia num armazem
        [HttpGet]
        public ActionResult Garantia(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Utilizador t = context.ObterListaUtilizadores(false, false).Where(u => u.IdArmazem == id).DefaultIfEmpty().First();

            _logger.LogDebug("Utilizador {1} [{2}] a obter todas as garantias pendentes. Tecnico: {3}", u.NomeCompleto, u.Id, t.IdPHC);
            if (t.IdPHC == 0) return StatusCode(500);

            return Json(phccontext.ObterDossiersRMATecnico(t));
        }

        //Obter peças em garantia
        [HttpGet]
        [Authorize(Roles = "Admin, Escritorio, Outros")]
        public ActionResult Garantias(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter todas as garantias pendentes de todos os tecnicos!", u.NomeCompleto, u.Id);

            return View(phccontext.ObterDossiersRMA());
        }

        //Atualizar estado do RMAF
        [HttpPut]
        public ActionResult Garantia(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            if (id == null)
            {
                return StatusCode(500);
            }

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a atualizar o estado do RMAF: Id - {3}", u.NomeCompleto, u.Id, id);

            return phccontext.AtualizarEstadoRMAF(u.Zona == 1 ? "Maia" : u.Zona == 2 ? "Alverca" : "Outros", id)[0] != "-1" ? StatusCode(200) : StatusCode(500);
        }

        //Gerar guia global
        [HttpGet]
        public virtual ActionResult GuiaGlobal(int id, string Api)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            int IdUtilizador = int.Parse(this.User.Claims.First().Value);
            Utilizador u = context.ObterUtilizador(IdUtilizador);
            if (u.Id == 0) return Json("Acesso Negado - " + Api);

            _logger.LogDebug("Utilizador {1} [{2}] a gerar a guia global do armazem: {3}.", u.NomeCompleto, u.Id, id);

            return File(context.MemoryStreamToPDF(context.DesenharDossier(phccontext.ObterDossier("phccontext.GerarGuiaGlobal(id, u)[2]")), 2480, 3508), "application/pdf");  
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

        //Imprimir etiqueta de garantia de uma peça de uma folha de obra
        [HttpGet]
        public ActionResult EtiquetaGarantia(string id, string peca)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Dossier d = phccontext.ObterDossier(id);
            d.FolhaObra.PecasServico = phccontext.ObterPecas(d.FolhaObra.IdFolhaObra);

            _logger.LogDebug("Utilizador {1} [{2}] a imprimir uma etiqueta de garantia de peca: FO - {3}, Ref. {4}.", u.NomeCompleto, u.Id, d.FolhaObra.IdFolhaObra, peca);

            return File(context.MemoryStreamToPDF(context.DesenharEtiquetaPecaGarantia(d, d.FolhaObra.PecasServico.Where(p => p.Ref_Produto == peca).First()), 801, 504), "application/pdf");
        }

        //Adiciona um anexo
        [HttpPost]
        [Authorize(Roles = "Admin, Escritorio, Outros")]
        public IActionResult Anexo(string id, IFormFile file)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            List<string> res = new List<string>() { "-1", "Erro", "", "" };
            if (file == null) return StatusCode(500);

            if (!phccontext.AtualizarProdutoImagem(phccontext.ObterProduto(id), file)) return StatusCode(500);
            return Ok();
        }
    }
}