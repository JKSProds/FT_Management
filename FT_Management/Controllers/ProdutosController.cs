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
            var LstArmazens = phccontext.ObterArmazens();

            if (armazemid == 0) { armazemid = 3; }

            ViewData["LstProdutosArmazem"] = phccontext.ObterProdutosArmazem(id);
            ViewData["Armazens"] = new SelectList(LstArmazens, "ArmazemId", "ArmazemNome", armazemid);

            return View(phccontext.ObterProduto(id, armazemid));
        }

        //Obter uma peca através do stamp ou ref
        [HttpGet]
        public JsonResult Peca(string id, string ref_produto)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (!string.IsNullOrEmpty(id)) return Json(phccontext.ObterProdutoStamp(id));
            if (!string.IsNullOrEmpty(ref_produto)) return Json(phccontext.ObterProdutosArmazem(ref_produto).ToList().FirstOrDefault() ?? new Produto());

            return Json("");
        }

        //Obter todas as pecas baseadas num filtro e que estejam num armazem
        [HttpGet]
        public JsonResult Pecas(string filter, int armazem)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

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

            Utilizador u = context.ObterListaUtilizadores(false, false).Where(u => u.IdArmazem == id).DefaultIfEmpty().First();
            List<string> LstGuias = phccontext.ObterGuiasTransporte(u.IdArmazem);
            if (string.IsNullOrEmpty(gt)) gt = LstGuias.First();

            ViewData["Guias"] = new SelectList(LstGuias);
            ViewData["GT"] = gt;

            Armazem a = phccontext.ObterArmazem(id);
            a.LstMovimentos = phccontext.ObterPecasGuiaTransporte(gt, u).OrderBy(m => m.DataMovimento).ToList();
            return View(a);
        }

        //Gerar guia global
        [HttpPost]
        public JsonResult GuiaGlobal(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return Json(phccontext.GerarGuiaGlobal(id));
        }

        //Imprimir etiqueta normal
        [HttpGet]
        public virtual ActionResult Etiqueta(string id, int armazemid)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            var file = context.DesenharEtiquetaProduto(phccontext.ObterProduto(id, armazemid)).ToArray();
            var output = new MemoryStream();
            output.Write(file, 0, file.Length);
            output.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "Produto_" + id + ".pdf",
                Inline = true,
                Size = file.Length

            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            return new FileContentResult(output.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        //Imprimir etiqueta pequena
        [HttpGet]
        public ActionResult EtiquetaPequena(string id, int armazemid)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            var filePath = Path.GetTempFileName();
            context.DesenharEtiqueta80x25QR(phccontext.ObterProduto(id, armazemid)).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            return File(context.BitMapToMemoryStream(filePath, 810, 504), "application/pdf");
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

            var filePath = Path.GetTempFileName();
            context.DesenharEtiqueta40x25QR(phccontext.ObterProduto(id, armazemid)).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            return File(context.BitMapToMemoryStream(filePath, 810, 504), "application/pdf");
        }

    }
}