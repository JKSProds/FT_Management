namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio, Tech")]
    public class DossiersController : Controller
    {
        //Obter todos os dossiers de uma data especifica
        [HttpGet]
        [Authorize(Roles = "Admin, Escritorio")]
        public ActionResult Index(string Data)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (Data == null || Data == string.Empty) Data = DateTime.Now.ToString("dd-MM-yyyy");
            ViewData["Data"] = Data;

            return View(phccontext.ObterDossiers(DateTime.Parse(Data)));
        }

        //Obter um dossier em especifico
        [HttpGet]
        public ActionResult Dossier(string id, string ReturnUrl)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Dossier d = phccontext.ObterDossier(id);

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && u.Id != d.Tecnico.Id) return Forbid();

            ViewData["ReturnUrl"] = ReturnUrl;

            if (d.Serie == 96 || d.Serie == 97) return View("Pedido", d);
            return View("Transferencia", d);
        }

        //Criar um pedido de dossier
        [HttpGet]
        public ActionResult Pedido(string id, int serie, string ReturnUrl)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            FolhaObra fo = phccontext.ObterFolhaObra(id);
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Dossier d = new Dossier()
            {
                Serie = serie,
                FolhaObra = fo,
                Marcacao = phccontext.ObterMarcacao(fo.IdMarcacao),
                EditadoPor = u.Iniciais,
                Tecnico = u
            };

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && u.Id != fo.Utilizador.Id) return Forbid();

            d.StampDossier = phccontext.CriarDossier(d)[2].ToString();
            d = phccontext.ObterDossier(d.StampDossier);
            MailContext.EnviarEmailPedidoTransferencia(u, d);

            //Criação de linhas por defeito
            phccontext.CriarLinhaDossier(new Linha_Dossier() { Stamp_Dossier = d.StampDossier, Designacao = "Pedido de Assistência Técnica N.º " + fo.IdFolhaObra, CriadoPor = d.EditadoPor });
            phccontext.CriarLinhaDossier(new Linha_Dossier() { Stamp_Dossier = d.StampDossier, Designacao = "Reparação de " + fo.EquipamentoServico.TipoEquipamento, CriadoPor = d.EditadoPor });
            phccontext.CriarLinhaDossier(new Linha_Dossier() { Stamp_Dossier = d.StampDossier, Designacao = fo.EquipamentoServico.MarcaEquipamento + " " + fo.EquipamentoServico.ModeloEquipamento + " N/S: " + fo.EquipamentoServico.NumeroSerieEquipamento, CriadoPor = d.EditadoPor });

            return RedirectToAction("Dossier", new { id = d.StampDossier, ReturnUrl = ReturnUrl });
        }

        //Criar um documento de transferencia
        [HttpGet]
        public ActionResult Transferencia(string id, int armazem, int load)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Utilizador t = context.ObterListaUtilizadores(false, false).Where(u => u.IdArmazem == armazem).DefaultIfEmpty().First();
            Dossier d = phccontext.ObterDossierAberto(u).DefaultIfEmpty(new Dossier()).Last();

            if (d.StampDossier == null)
            {
                d = new Dossier()
                {
                    Serie = 36,
                    Marcacao = new Marcacao(),
                    FolhaObra = new FolhaObra(),
                    EditadoPor = u.Iniciais,
                    Tecnico = t
                };
                d.StampDossier = phccontext.CriarDossier(d)[2].ToString();
                d = phccontext.ObterDossier(d.StampDossier);
                if (string.IsNullOrEmpty(d.StampDossier)) return Forbid();
                MailContext.EnviarEmailPedidoTransferencia(u, d);
            }
            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && u.Id != d.Tecnico.Id) return Forbid();
            if (load == 1)
            {
                List<Linha_Dossier> Linhas = new List<Linha_Dossier>();
                foreach (Movimentos m in phccontext.ObterPecasGuiaTransporte(id.Replace("|", "/"), t))
                {
                    if (Linhas.Where(l => l.Referencia == m.RefProduto).Count() == 0)
                    {
                        Linhas.Add(new Linha_Dossier() { Stamp_Dossier = d.StampDossier, Referencia = m.RefProduto, Designacao = m.Designacao, Quantidade = m.Quantidade, CriadoPor = u.Iniciais });
                    }
                    else
                    {
                        Linhas.Where(l => l.Referencia == m.RefProduto).First().Quantidade += m.Quantidade;
                    }
                }

                foreach (Linha_Dossier l in Linhas)
                {
                    phccontext.CriarLinhaDossier(l);
                }

            }
            return RedirectToAction("Dossier", new { id = d.StampDossier, ReturnUrl = "/Produtos/Armazem/" + armazem });
        }

        //Adicionar uma linha ao dossier
        [HttpPost]
        public JsonResult Linha(string id, string referencia, string design, double qtd)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            List<string> res = new List<string>();

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Dossier d = phccontext.ObterDossier(id);
            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && u.Id != d.Tecnico.Id) return Json("");
            if (d.Fechado) return Json("");

            for (int i = 0; i < design.Length; i += 60)
            {
                string s = i + 60 > design.Length ? design.Substring(i, design.Length - i) : design.Substring(i, 60);
                Linha_Dossier l = new Linha_Dossier()
                {
                    Stamp_Dossier = id,
                    Referencia = string.IsNullOrEmpty(referencia) ? "" : referencia,
                    Designacao = s,
                    Quantidade = qtd,
                    CriadoPor = u.Iniciais
                };
                res = phccontext.CriarLinhaDossier(l);
            }

            return Json(int.Parse(res[0].ToString()) > 0 ? phccontext.ObterLinhaDossier(res[3].ToString()) : new Linha_Dossier());
        }

        //Apagar uma linha do dossier
        [HttpDelete]
        public JsonResult Linha(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Linha_Dossier l = phccontext.ObterLinhaDossier(id);
            Dossier d = phccontext.ObterDossier(l.Stamp_Dossier);

            if (d.Fechado) return Json("");

            return Json(phccontext.ApagarLinhaDossier(l.Stamp_Dossier, l.Stamp_Linha));
        }

        //Adiciona um anexo
        [HttpPost]
        public JsonResult Anexo(string id, string ecra, string serie, string resumo, IFormFile file)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            if (string.IsNullOrEmpty(id))
            {
                return Json(FicheirosContext.CriarFicheiroTemporario(u.Iniciais + "_" + DateTime.Now.Ticks + (file.FileName.Split(".").Count() > 0 ? "." + file.FileName.Split(".").Last() : ""), file));
            }

            return Json("");
        }
    }
}
