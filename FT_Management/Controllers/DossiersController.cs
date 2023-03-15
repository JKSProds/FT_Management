namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio, Tech")]
    public class DossiersController : Controller
    {
        private readonly ILogger<DossiersController> _logger;

        public DossiersController(ILogger<DossiersController> logger)
        {
            _logger = logger;
        }

        //Obter todos os dossiers de uma data especifica
        [HttpGet]
        [Authorize(Roles = "Admin, Escritorio")]
        public ActionResult Index(string Data, string Filtro, int Serie, string Ecra)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            List<KeyValuePair<int, string>> LstSeries = new List<KeyValuePair<int, string>>();

            if (Data == null || Data == string.Empty) Data = DateTime.Now.ToString("dd-MM-yyyy");
            if (string.IsNullOrEmpty(Filtro)) Filtro = "";
            if (string.IsNullOrEmpty(Ecra)) Ecra = "BO";
            ViewData["Data"] = Data;
            ViewData["Filtro"] = Filtro;
            ViewData["Serie"] = Serie;
            ViewData["Ecra"] = Ecra;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            _logger.LogDebug("Utilizador {1} [{2}] a obter todos os dossiers da seguinte data: {3}", u.NomeCompleto, u.Id, Data);

            if (Ecra == "BO") LstSeries = phccontext.ObterSeriesDossiers();
            if (Ecra == "FT") LstSeries = phccontext.ObterSeriesFaturacao();

            LstSeries.Insert(0, new KeyValuePair<int, string>(0, "Todos"));
            ViewBag.Series = LstSeries.Select(l => new SelectListItem() { Value = l.Key.ToString(), Text = l.Value, Selected = l.Key == Serie });

            return View(Ecra == "BO" ? phccontext.ObterDossiers(DateTime.Parse(Data), Filtro, Serie) : (Ecra == "FT" ? phccontext.ObterDossiersFaturacao(DateTime.Parse(Data), Filtro, Serie) : phccontext.ObterDossiersCompras(DateTime.Parse(Data), Filtro, Serie)));
        }

        //Obter um dossier em especifico
        [HttpGet]
        public ActionResult Dossier(string id, string ecra, string ReturnUrl)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            if (string.IsNullOrEmpty(ecra)) ecra = "BO";

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Dossier d = new Dossier();
            if (ecra == "BO") d = phccontext.ObterDossier(id);
            if (ecra == "FT") d = phccontext.ObterDossierFaturacao(id);
            if (ecra == "FO") d = phccontext.ObterDossierCompras(id);
            if (d == new Dossier()) return Forbid();

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && u.Id != d.Tecnico.Id) return Forbid();

            _logger.LogDebug("Utilizador {1} [{2}] a obter um dossier em especifico: Id - {3}, Cliente - {4}, Serie - {5}", u.NomeCompleto, u.Id, d.IdDossier, d.Cliente.NomeCliente, d.NomeDossier);

            ViewData["ReturnUrl"] = ReturnUrl;

            if (d.Ecra == "BO")
            {
                if (d.Serie == 96 || d.Serie == 97) return View("Pedido", d);
                if (d.Serie == 36) return View("Transferencia", d);
            }
            return View("Dossier", d);
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

            _logger.LogDebug("Utilizador {1} [{2}] a criar um dossier novo: Id FO - {3}, Id Marcacao - {4}, Cliente - {5}, Serie - {6}", u.NomeCompleto, u.Id, fo.IdFolhaObra, fo.IdMarcacao, fo.ClienteServico.NomeCliente, serie);

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
                _logger.LogDebug("Utilizador {1} [{2}] a criar um pedido de transferencia novo: Id Tecnico - {3}, Serie - {4}", u.NomeCompleto, u.Id, d.Tecnico.Id, d.Serie);

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
                _logger.LogDebug("Utilizador {1} [{2}] a criar uma linha nova ao dossier: Id - {3}, Serie - {4}, Ref - {5}, Qtd - {6}", u.NomeCompleto, u.Id, d.Tecnico.Id, d.NomeDossier, l.Referencia, l.Quantidade);
            }

            return Json(int.Parse(res[0].ToString()) > 0 ? phccontext.ObterLinhaDossier(res[3].ToString()) : new Linha_Dossier());
        }

        //Apagar uma linha do dossier
        [HttpDelete]
        public JsonResult Linha(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Linha_Dossier l = phccontext.ObterLinhaDossier(id);
            Dossier d = phccontext.ObterDossier(l.Stamp_Dossier);

            if (d.Fechado) return Json("");

            _logger.LogDebug("Utilizador {1} [{2}] a apagar uma linha do dossier: Id - {3}, Serie - {4}, Ref - {5}, Qtd - {6}", u.NomeCompleto, u.Id, d.Tecnico.Id, d.NomeDossier, l.Referencia, l.Quantidade);

            return Json(phccontext.ApagarLinhaDossier(l.Stamp_Dossier, l.Stamp_Linha));
        }

        //Obter um anexo
        [HttpGet]
        public virtual ActionResult Anexo(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Anexo a = phccontext.ObterAnexoDossier(id);
            if (new FileExtensionContentTypeProvider().TryGetContentType(a.LocalizacaoFicheiro, out var mimeType))
            {
                byte[] file = FicheirosContext.ObterFicheiro(a.LocalizacaoFicheiro);
                if (file == null) return Forbid();

                return File(file, mimeType);
            }
            return Content("");
        }

        //Adiciona um anexo
        [HttpPost]
        public IActionResult Anexo(string id, string ecra, string resumo, string serie, IFormFile file)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            List<string> res = new List<string>() { "-1", "Erro", "", "" };
            string extensao = (file.FileName.Split(".").Count() > 0 ? "." + file.FileName.Split(".").Last() : "");

            if (string.IsNullOrEmpty(id))
            {
                string nome = u.Iniciais + "_" + DateTime.Now.Ticks + extensao;
                _logger.LogDebug("Utilizador {1} [{2}] a anexar um ficheiro num dossier: Serie - {4}, NomeFicheiro - {5}", u.NomeCompleto, u.Id, "Assis. Tecnica", nome);
                return Content(FicheirosContext.CriarFicheiroTemporario(nome, file));
            }

            Dossier d = new Dossier();
            if (ecra == "BO") d = phccontext.ObterDossier(id);
            if (ecra == "FT") d = phccontext.ObterDossierFaturacao(id);

            Anexo a = new Anexo()
            {
                Ecra = d.Ecra,
                Serie = d.Serie,
                Stamp_Origem = d.StampDossier,
                Resumo = string.IsNullOrEmpty(resumo) ? d.NomeDossier + " (" + d.IdDossier + ") - " + u.NomeCompleto : resumo,
                Nome = d.Iniciais + "_" + d.IdDossier + "_" + d.Cliente.NomeCliente.Trim() + "_" + DateTime.Now.Ticks + extensao,
                Utilizador = u
            };

            res = phccontext.CriarAnexo(a, file);

            return Ok();
        }
    }
}
