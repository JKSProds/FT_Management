namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio")]
    public class PickingController : Controller
    {
        private readonly ILogger<PickingController> _logger;

        public PickingController(ILogger<PickingController> logger)
        {
            _logger = logger;
        }

        //Obter todas as encomendas com base em alguns filtros
        [HttpGet]
        public IActionResult Index(int IdEncomenda, int Tipo, string NomeCliente)
        {
            ViewData["IdEncomenda"] = (IdEncomenda == 0 ? "" : IdEncomenda.ToString());
            ViewData["NomeCliente"] = (string.IsNullOrEmpty(NomeCliente) ? "" : NomeCliente);
            ViewData["Tipo"] = Tipo;

            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter todas as encomendas com base num filtro: Id - {3}, Tipo - {4}, Nome - {5}.", u.NomeCompleto, u.Id, IdEncomenda, Tipo, NomeCliente);

            List<Encomenda> LstEncomendas = phccontext.ObterEncomendas().Where(e => e.ExisteEncomenda(Models.Encomenda.Tipo.TODAS) && e.NItems > 0).ToList();

            if (Tipo > 0) LstEncomendas = LstEncomendas.Where(e => e.NumDossier == Tipo).ToList();
            if (!string.IsNullOrEmpty(NomeCliente)) LstEncomendas = LstEncomendas.Where(e => e.NomeCliente.ToUpper().Contains(NomeCliente.ToUpper())).ToList();
            if (IdEncomenda > 0) LstEncomendas = LstEncomendas.Where(e => e.Id.ToString().Contains(IdEncomenda.ToString())).ToList();

            return View(LstEncomendas);
        }

        //Obter todas os fornecedores
        [HttpGet]
        public IActionResult Fornecedores(string filtro)
        {
            ViewData["filtro"] = (string.IsNullOrEmpty(filtro) ? "" : filtro);

            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter todas os fornecedores com base num filtro: {3}.", u.NomeCompleto, u.Id, filtro);

            return View(phccontext.ObterFornecedoresEncomendas(true, filtro));
        }

        //Obter um fornecedor e as suas encomendas
        [HttpGet]
        public IActionResult Fornecedor(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Fornecedor f = phccontext.ObterFornecedor(id);

            _logger.LogDebug("Utilizador {1} [{2}] a obter um fornecedor em especifico e as suas encomendas: {3}.", u.NomeCompleto, u.Id, id);

            return View(phccontext.ObterFornecedor(id));
        }

        //Obter todos os tecnicos
        [HttpGet]
        public IActionResult Tecnicos()
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter todas os tecnicos.", u.NomeCompleto, u.Id);
            List<Utilizador> Tecnicos = context.ObterListaTecnicos(true, false);

            Parallel.ForEach(Tecnicos, t =>
            {
                (t.Linhas ??= new List<Linha_Dossier>()).AddRange(phccontext.ObterLinhasDossierAbertas(36, t).Concat(phccontext.ObterLinhasDossierAbertas(96, t)));
            });

            return View(Tecnicos.Where(t => t.Linhas.Count() > 0));
        }

        //Obter um tecnico e as suas peças pedidas
        [HttpGet]
        public IActionResult Tecnico(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            if (id==0) return StatusCode(500);

            Utilizador t = context.ObterUtilizador(id);
            t.Linhas = phccontext.ObterLinhasDossierAbertas(36, t);
            t.Linhas.AddRange(phccontext.ObterLinhasDossierAbertas(96, t));

            _logger.LogDebug("Utilizador {1} [{2}] a obter um tecnico em especifico e as suas linhas: {3}.", u.NomeCompleto, u.Id, id);

            return View(t);
        }

        //Criar uma transferencia em viagem
        [HttpPost]
        public IActionResult Tecnico(int id, string linhas)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Utilizador t = context.ObterUtilizador(id);

            if (id==0 || string.IsNullOrEmpty(linhas)) return StatusCode(500);

            _logger.LogDebug("Utilizador {1} [{2}] a criar uma transferencia em viagem do utilizador nº {3}: {4}", u.NomeCompleto, u.Id, id, linhas);

             //Dossier d = phccontext.ObterDossier(res[2]);
             Cliente c = new Cliente() {IdCliente=1, IdLoja=0};
            Dossier d = new Dossier() {Serie=98, Cliente=c, StampMoradaDescarga="SUBIC Alverca", EditadoPor=u.NomeCompleto, Tecnico=t, DataDossier=DateTime.Now};
            
            List<Linha_Dossier> LstLinhas = linhas.Split(';')
            .Select(l => l.Split('|'))
            .Where(p => p.Length == 4)
            .Select(p => new Linha_Dossier() { Stamp_Linha = p[0], Referencia=p[1], Designacao =p[2], Quantidade = int.Parse(p[3]), Stamp_Dossier = d.StampDossier, Armazem_Origem=3, Armazem_Destino = t.IdArmazem })
            .ToList();

            d.Linhas = LstLinhas;
            
            if (d.Linhas == null || d.Linhas.Count == 0) return StatusCode(500);

            List<string> res = phccontext.CriarTransferenciaViagem(u, d);

            MailContext.EnviarEmailTransferenciaViagem(u, d, res[1], new Attachment(context.MemoryStreamToPDF(context.DesenharEtiquetaViagem(d, res[1]), 801, 504), "Viagem_" + d.Tecnico.IdArmazem + ".pdf"));

            return (res[0] == "-1") ? StatusCode(500) : StatusCode(200);
        }


        //Obter uma encomenda em especifico
        [HttpGet]
        public JsonResult Encomenda(string stamp)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            Encomenda e = phccontext.ObterEncomenda(stamp);
            e.LinhasEncomenda = e.LinhasEncomendaPorFornecer.Where(l => l.DataEnvio.Year > 1900 && !l.Fornecido || e.Total).ToList();

            _logger.LogDebug("Utilizador {1} [{2}] a obter uma encomenda em especifico: Id - {3}, Stamp Encomenda - {4}, Picking Stamp - {5}.", u.NomeCompleto, u.Id, e.Id, e.BO_STAMP, e.PI_STAMP);

            return new JsonResult(e);
        }

        //Obter um picking
        [HttpGet]
        public IActionResult Picking(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            ViewBag.Armazens = phccontext.ObterArmazensFixos().Select(l => new SelectListItem() { Value = l.ArmazemStamp, Text = l.ArmazemNome, Selected = l.ArmazemId == 3 });

            Encomenda e = phccontext.ObterEncomenda(id);
            string pi_stamp = e.PI_STAMP;

            if (string.IsNullOrEmpty(pi_stamp))
            {
                pi_stamp = phccontext.CriarPicking(id, u.Iniciais);
                context.AdicionarLog(u.Id, "Criado um picking novo com sucesso! - Encomenda Nº " + e.Id + ", " + e.NomeCliente + " pelo utilizador " + u.NomeCompleto, 6);

            }
            Picking p = phccontext.ObterPicking(pi_stamp);

            if (p.IdPicking == 0)
            {
                pi_stamp = phccontext.CriarPicking(id, u.Iniciais);
                context.AdicionarLog(u.Id, "Criado um picking novo com sucesso! - Encomenda Nº " + e.Id + ", " + e.NomeCliente + " pelo utilizador " + u.NomeCompleto, 6);

                p = phccontext.ObterPicking(pi_stamp);
            }

            _logger.LogDebug("Utilizador {1} [{2}] a obter um picking em especifico: Id - {3}, Stamp Encomenda - {4}, Picking Stamp - {5}.", u.NomeCompleto, u.Id, p.IdPicking, e.BO_STAMP, p.Picking_Stamp);

            return View(p);
        }

        //Obter varias ordens de rececao
        [HttpGet]
        public IActionResult OrdensRececao(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            ViewBag.Armazens = phccontext.ObterArmazensFixos().Select(l => new SelectListItem() { Value = l.ArmazemStamp, Text = l.ArmazemNome, Selected = l.ArmazemId == 3 });

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            _logger.LogDebug("Utilizador {1} [{2}] a obter ordens de rececao associadas a um fornecedor: {3}.", u.NomeCompleto, u.Id, id);

            return Json(phccontext.ObterFornecedor(id).OrdensRececao);
        }


        //Obter uma ordem de rececao
        [HttpGet]
        public IActionResult OrdemRececao(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            ViewBag.Armazens = phccontext.ObterArmazensFixos().Select(l => new SelectListItem() { Value = l.ArmazemStamp, Text = l.ArmazemNome, Selected = l.ArmazemId == 3 });

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            _logger.LogDebug("Utilizador {1} [{2}] a obter uma ordem de rececao em especifico: {3}.", u.NomeCompleto, u.Id, id);

            return View("Picking", phccontext.ObterPicking(id));
        }

        //Criar uma ordem de rececao
        [HttpPost]
        public IActionResult OrdemRececao(string id, string linhas)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            _logger.LogDebug("Utilizador {1} [{2}] a criar uma ordem de rececao em especifico: {3}.", u.NomeCompleto, u.Id, id);

            List<string> res = phccontext.CriarOrdemRececao(id, linhas, u);

            return Json(res[2]);
        }

        //Fechar um picking
        [HttpDelete]
        public ActionResult Picking(string id, string obs, string armazem)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Picking p = phccontext.ObterPicking(id);

            _logger.LogDebug("Utilizador {1} [{2}] a fechar um picking em especifico: Id - {3}, Stamp Encomenda - {4}, Picking Stamp - {5}.", u.NomeCompleto, u.Id, p.IdPicking, p.Encomenda.BO_STAMP, p.Picking_Stamp);

            p.EditadoPor = u.Iniciais;
            p.Obs = (string.IsNullOrEmpty(obs) ? "" : (obs + "\r\n\r\n")) + "<b>" + (p.Serie == 10 ? phccontext.ValidarOrdemRececao(p) : phccontext.ValidarPicking(p)) + "</b>";
            p.ArmazemDestino = phccontext.ObterArmazem(armazem);

            phccontext.FecharPicking(p);
            context.AdicionarLog(u.Id, "Foi fechado um picking com sucesso! - Picking Nº " + p.IdPicking + ", " + p.NomeCliente + " pelo utilizador " + u.NomeCompleto, 6);

            MailContext.EnviarEmailFechoPicking(u, p, new Attachment(context.MemoryStreamToPDF(context.DesenharEtiquetaPicking(p), 801, 504), "Picking_" + p.IdPicking + ".pdf"));

            return Content("Ok");
        }

        //Validar o picking
        [HttpGet]
        public ActionResult Validar(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Picking p = phccontext.ObterPicking(id);

            _logger.LogDebug("Utilizador {1} [{2}] a validar um picking em especifico: Id - {3}, Stamp Encomenda - {4}, Picking Stamp - {5}.", u.NomeCompleto, u.Id, p.IdPicking, p.Encomenda.BO_STAMP, p.Picking_Stamp);

            return Content(p.Serie == 10 ? phccontext.ValidarOrdemRececao(p) : phccontext.ValidarPicking(p));
        }

        //Atualizar uma linha
        [HttpPut]
        public JsonResult Linha(string stamp, Double qtd, string serie, string bomastamp, string armazem)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a atualizar uma linha de um picking em especifico: Stamp - {3}, Qtd - {4}, Serie - {5}, BOMA STAMP - {6}, Armazem STAMP - {7}.", u.NomeCompleto, u.Id, stamp, qtd, serie, bomastamp, armazem);

            Linha_Picking linha_picking = new Linha_Picking()
            {
                Picking_Linha_Stamp = stamp,
                Qtd_Linha = qtd,
                Lista_Ref = new List<Ref_Linha_Picking>(),
                EditadoPor = u.NomeCompleto
            };
            if (serie != null || bomastamp != null)
            {
                linha_picking.Serie = true;
                linha_picking.Lista_Ref.Add(new Ref_Linha_Picking()
                {
                    BOMA_STAMP = bomastamp == null ? "" : bomastamp,
                    NumSerie = serie == null ? "" : serie
                });
            }

            return new JsonResult(phccontext.AtualizarLinhaPicking(linha_picking, phccontext.ObterArmazem(armazem)));
        }


    }
}
