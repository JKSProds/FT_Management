namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio")]
    public class InventarioController : Controller
    {
        //Obter todos os armazens
        [HttpGet]
        public ActionResult Index()
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return View(phccontext.ObterArmazensFixos());
        }

        //Obter todos os dossiers associados ao um armazem
        [HttpGet]
        public JsonResult Dossiers(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(phccontext.ObterInventarios(id));
        }

        //Obter as linhas de um dossier filtrando pela referencia
        [HttpGet]
        public ActionResult Dossier(string id, string referencia)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (string.IsNullOrEmpty(referencia)) referencia = "";
            ViewData["Referencia"] = referencia;

            Picking p = phccontext.ObterInventario(id);
            p.Linhas = p.Linhas.Where(l => l.Ref_linha.Contains(referencia)).ToList();

            return View(p);
        }

        //Criar um dossier de inventario se o mesmo nao existir
        [HttpPost]
        public JsonResult Dossier(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            List<string> res = phccontext.CriarInventario(id, this.User.ObterNomeCompleto());
            res[2] = (!string.IsNullOrEmpty(res[2])) ? "/Inventario/Dossier/" + res[2] : "";

            return new JsonResult(res);
        }

        //Fechar o dossier para previnir alterações futuras
        [HttpDelete]
        public JsonResult Dossier(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(phccontext.FecharInventario(new Picking() { Picking_Stamp = id, EditadoPor = this.User.ObterNomeCompleto() }));
        }

        //Obter linha baseado no stamp
        [HttpGet]
        public JsonResult Linha(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(phccontext.ObterLinhaInventario(id));
        }

        //Criar linha nova
        [HttpPost]
        public JsonResult Linha(string stamp, string ref_produto, double qtd)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Linha_Picking l = new Linha_Picking()
            {
                Picking_Linha_Stamp = stamp,
                Ref_linha = ref_produto,
                Qtd_Linha = qtd,
                EditadoPor = this.User.ObterNomeCompleto()
            };

            return new JsonResult(phccontext.CriarLinhaInventario(l));
        }

        //Apagar linha
        [HttpDelete]
        public JsonResult Linha(string stamp, string stamp_linha)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(phccontext.ApagarLinhaInventario(new Ref_Linha_Picking() { BOMA_STAMP = stamp, Picking_Linha_Stamp = stamp_linha }));
        }

        //Obter todos os numeros de serie associados a uma linha
        [HttpGet]
        public JsonResult Serie(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(phccontext.ObterSerieLinhaInventario(id).OrderBy(s => s.CriadoA).ToList());
        }

        //Criar numeros de serie associados a uma linha
        [HttpPost]
        public JsonResult Serie(string stamp, string stamp_linha, string serie)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

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

            return new JsonResult(phccontext.ApagarLinhaSerieInventario(stamp, new Ref_Linha_Picking() { BOMA_STAMP = stamp_boma, Picking_Linha_Stamp = stamp_linha, CriadoPor = this.User.ObterNomeCompleto() }));
        }
    }

}
