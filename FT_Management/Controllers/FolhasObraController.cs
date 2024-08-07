﻿using System.Drawing;
using FT_Management.Models;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio, Tech")]
    public class FolhasObraController : Controller
    {
        private readonly ILogger<FolhasObraController> _logger;

        public FolhasObraController(ILogger<FolhasObraController> logger)
        {
            _logger = logger;
        }

        //Obter todas as folhas de obra com base numa data
        [HttpGet]
        public ActionResult Index(string DataFolhasObra)
        {
            if (DataFolhasObra == null || DataFolhasObra == string.Empty) DataFolhasObra = DateTime.Now.ToString("dd-MM-yyyy");

            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a todas das folhas de obra com a seguinte data: {3}.", u.NomeCompleto, u.Id, DataFolhasObra);

            ViewData["DataFolhasObra"] = DataFolhasObra;

            return View(phccontext.ObterFolhasObra(DateTime.Parse(DataFolhasObra)));
        }

        //Obter view para adicionar folhas de obra
        [HttpGet]
        [Authorize(Roles = "Admin, Tech")]
        public ActionResult Adicionar(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Marcacao m = phccontext.ObterMarcacao(id);

            if (!string.IsNullOrEmpty(m.Formulario) && !m.FormularioSubmetido) return RedirectToAction(phccontext.ObterFormularios().Where(f => f.Key == m.Formulario).Select(l => l.Value).First(), "Formulario", new { id = id });

            _logger.LogDebug("Utilizador {1} [{2}] a adicionar uma folha de obra nova do cliente: {3}.", u.NomeCompleto, u.Id, m.Cliente.NomeCliente);

            List<FolhaObra> LstFolhasObra = phccontext.ObterFolhasObra(DateTime.Now, m.Cliente);

            if (m.EstadoMarcacaoDesc == "Finalizado" || m.EstadoMarcacaoDesc == "Cancelado") return Forbid();

            FolhaObra fo = new FolhaObra().PreencherDadosMarcacao(m);
            fo.Utilizador = u;
            fo.Marcacao = m;

            fo.PreencherViagem(context.ObterViagens(fo.Utilizador.Viatura.Matricula, DateTime.Now.ToShortDateString()).Where(v => v.Fim_Viagem.Year > 1).DefaultIfEmpty(new Viagem() { Fim_Viagem = DateTime.Parse(fo.IntervencaosServico.First().HoraInicio.ToString()), Distancia_Viagem = "0" }).Last());
            if (LstFolhasObra.Count() > 0)
            {
                fo.RubricaCliente = LstFolhasObra.First().RubricaCliente.Replace("}", "").Replace("{", "");
                fo.ConferidoPor = LstFolhasObra.First().ConferidoPor;
            }

            ViewBag.EstadoFolhaObra = phccontext.ObterEstadoFolhaObra().Select(l => new SelectListItem() { Value = l.Key.ToString(), Text = l.Value });
            ViewData["TipoFolhaObra"] = phccontext.ObterTipoFolhaObra();
            ViewBag.MotivosGarantia = phccontext.ObterMotivosAvariaGarantia().Select(l => new SelectListItem() { Value = l, Text = l });
            ViewBag.MotivosNaoGarantia = phccontext.ObterMotivosAvariaNaoGarantia().Select(l => new SelectListItem() { Value = l, Text = l });
            ViewData["Exclusoes"] = phccontext.ObterExclusoes(0);

            var categoriasResolucao = context.ObterCategoriasResolucao().ToList();
            ViewBag.CategoriaResolucao = categoriasResolucao;

            ViewBag.CategoriaResolucao1 = categoriasResolucao.Distinct()
                .Select(l => new SelectListItem() { Value = l.Id_1.ToString(), Text = l.Categoria1 }).DistinctBy(l => l.Value)
                .ToList();

            ViewBag.CategoriaResolucao2 = categoriasResolucao
                .GroupBy(l => l.Id_1)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(l => new SelectListItem() { Value = l.Id_2.ToString(), Text = l.Categoria2 }).DistinctBy(l => l.Value).ToList()
                );

            ViewBag.CategoriaResolucao3 = categoriasResolucao
                .GroupBy(l => l.Id_1 + "_" + l.Id_2)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(l => new SelectListItem() { Value = l.Id_3.ToString(), Text = l.Categoria3 }).DistinctBy(l => l.Value).ToList()
                );

            return View(fo);
        }

        //Obter uma folha de obra em especifico
        [HttpGet]
        public ActionResult FolhaObra(int Id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FolhaObra fo = phccontext.ObterFolhaObra(Id);

            if (fo.StampFO == null) return Forbid();
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter uma folha de obra em especifico: Id - {3}, Cliente - {4}, Equipamento - {5}, Tecnico - {6}.", u.NomeCompleto, u.Id, fo.IdFolhaObra, fo.ClienteServico.NomeCliente, fo.EquipamentoServico.NumeroSerieEquipamento, fo.Utilizador.NomeCompleto);

            ViewData["SelectedTecnico"] = u.NomeCompleto;
            ViewData["Tecnicos"] = context.ObterListaTecnicos(false, false);

            return View(fo);
        }

        //Criar uma folha de obra
        [HttpPost]
        [Authorize(Roles = "Admin, Tech")]
        public ActionResult FolhaObra(FolhaObra fo)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            fo.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            fo.Marcacao = phccontext.ObterMarcacao(fo.IdMarcacao);
            fo.ValidarIntervencoes();
            fo.CatResolucao = fo.CategoriaResolucao || fo.CatResolucao.Id_3 > 0 ? context.ObterCategoriaResolucao(fo.CatResolucao) : fo.CatResolucao;
            if (fo.Contrato) fo.JustExtraContrato = "";

            ModelState.Remove("Utilizador.Password");
            ModelState.Remove("Marcacao.ResumoMarcacao");
            ModelState.Remove("Marcacao.PrioridadeMarcacao");
            ModelState.Remove("Marcacao.Referencia");
            ModelState.Remove("DataServico");
            if (!fo.CategoriaResolucao) ModelState.Remove("CatResolucao.Id_3");

            if (ModelState.IsValid)
            {   
                fo.EquipamentoServico = phccontext.ObterEquipamentoSimples(fo.EquipamentoServico.EquipamentoStamp);
                fo.ClienteServico = phccontext.ObterClienteSimples(fo.ClienteServico.IdCliente, fo.ClienteServico.IdLoja);
                fo.ValidarPecas(phccontext.ObterProdutosArmazem(fo.Utilizador.IdArmazem));
                fo.ValidarTipoFolhaObra();
                fo.SituacoesPendentes += "\r\n" + string.Join(" | ", fo.PecasServico.Where(p => p.Garantia != fo.EquipamentoServico.Garantia).Select(x => x.Ref_Produto + " - " + "[" + x.MotivoGarantia + "] " + x.ObsGarantia));

                if (fo.EquipamentoServico.Cliente.ClienteStamp != fo.ClienteServico.ClienteStamp) phccontext.AtualizarClienteEquipamento(fo.ClienteServico, fo.EquipamentoServico, fo.Utilizador);
                if (fo.ClienteServico.Contrato && !fo.EquipamentoServico.Contrato && fo.Contrato) phccontext.AssociarEquipamentoContrato(fo.ClienteServico, fo.EquipamentoServico, fo.Utilizador);

                Marcacao m = fo.Marcacao;
                m.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
                m.Oficina = fo.RecolhaOficina || (fo.Oficina && !fo.Guia);
                m.Remoto = (fo.TipoFolhaObra == "Remoto");

                _logger.LogDebug("Utilizador {1} [{2}] a criar uma nova folha de obra: Id Marcacao - {3}, Cliente - {4}, Equipamento - {5}, Tecnico - {6}, N. Int - {7}, N. Pecas {8}, Estado - {9}.", fo.Utilizador.NomeCompleto, fo.Utilizador.Id, m.IdMarcacao, fo.ClienteServico.NomeCliente, fo.EquipamentoServico.NumeroSerieEquipamento, fo.Utilizador.NomeCompleto, fo.IntervencaosServico.Count(), fo.PecasServico.Count(), fo.EstadoFolhaObra);

                int Estado = fo.EstadoFolhaObra;
                if (fo.FecharMarcacao && fo.EstadoFolhaObra == 1)
                {
                    m.EstadoMarcacaoDesc = "Finalizado";
                }
                if (fo.EstadoFolhaObra == 2) m.EstadoMarcacaoDesc = "Pedido Peças";
                if (fo.EstadoFolhaObra == 3) m.EstadoMarcacaoDesc = "Pedido Orçamento";
                if (fo.EstadoFolhaObra == 4) m.EstadoMarcacaoDesc = "Reagendar";

                List<Produto> LstPecasSemStock = phccontext.ValidarStockPecas(fo);
                List<string> res = phccontext.CriarFolhaObra(fo);
                if (int.Parse(res[0]) > 0)
                {
                    fo.IdAT = res[1].ToString();
                    fo.StampFO = res[2].ToString();
                    fo.IdFolhaObra = int.Parse(res[3]);
                    fo.RubricaCliente = !string.IsNullOrEmpty(fo.RubricaCliente) ? fo.RubricaCliente.Split(",").Last() : "";

                    phccontext.AtualizaMarcacao(m);
                    phccontext.FecharFolhaObra(fo);
                    phccontext.CriarAnexosFolhaObra(fo);
                    if (fo.FecharMarcacao && fo.Instalação && fo.Marcacao.GuiasInstalacao) phccontext.AtualizarAnexosAssinatura(fo);
                    if (fo.FecharMarcacao && fo.Instalação && fo.Marcacao.GuiasInstalacao && fo.EnviarEmailGuias) MailContext.EnviarEmailGuias(fo.EmailCliente, fo.Marcacao.LstAnexos.Where(a => a.AnexoInstalacao).ToList());
                    if (fo.PecasServico.Where(p => p.Garantia).Count() > 0) phccontext.CriarRMAFLinhas(phccontext.CriarRMAF(fo)[2], fo);
                    fo = phccontext.ObterFolhaObra(fo.IdFolhaObra);

                    if (LstPecasSemStock.Count() > 0) MailContext.EnviarEmailFolhaObraPecasSemStock("cperes@food-tech.pt", fo, LstPecasSemStock);
                    if (!fo.IntervencaosServico.Any(d => d.DataServiço.Date == DateTime.Now.Date)) MailContext.EnviarEmailFolhaObraAtrasada("faturacao@food-tech.pt", fo);

                    if (Estado == 2) return RedirectToAction("Pedido", "Dossiers", new { id = fo.StampFO, serie = 96, ReturnUrl = "/Pedidos/Pedidos/" + fo.Utilizador.IdPHC });
                    if (Estado == 3) return RedirectToAction("Pedido", "Dossiers", new { id = fo.StampFO, serie = 97, ReturnUrl = "/Pedidos/Pedidos/" + fo.Utilizador.IdPHC });

                    return RedirectToAction("Pedidos", "Pedidos", new { IdTecnico = fo.Utilizador.IdPHC });
                }

                ModelState.AddModelError("", res[1]);
            }

            ModelState.AddModelError("", string.Join("\r\n", ModelState.Where(e => e.Value.Errors.Count() > 0).Select(e => e.Value.Errors.First().ErrorMessage)));
            ViewBag.EstadoFolhaObra = phccontext.ObterEstadoFolhaObra().Select(l => new SelectListItem() { Value = l.Key.ToString(), Text = l.Value });
            ViewBag.MotivosGarantia = phccontext.ObterMotivosAvariaGarantia().Select(l => new SelectListItem() { Value = l, Text = l });
            ViewBag.MotivosNaoGarantia = phccontext.ObterMotivosAvariaNaoGarantia().Select(l => new SelectListItem() { Value = l, Text = l });
            ViewData["Exclusoes"] = phccontext.ObterExclusoes(0);
            ViewData["TipoFolhaObra"] = phccontext.ObterTipoFolhaObra();

            return View("Adicionar", fo);
        }

        //Validar alguns paramentros da folha de obra antes de a criar
        [HttpPost]
        public ActionResult Validar(FolhaObra fo)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            ModelState.Remove("Utilizador.Password");
            ModelState.Remove("Marcacao.ResumoMarcacao");
            ModelState.Remove("Marcacao.PrioridadeMarcacao");
            ModelState.Remove("Marcacao.Referencia");
            ModelState.Remove("DataServico");
            if (!fo.CategoriaResolucao) ModelState.Remove("CatResolucao.Id_3");

            fo.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            fo.ValidarIntervencoes();
            fo.ValidarPecas(phccontext.ObterProdutosArmazem(fo.Utilizador.IdArmazem));
            fo.ClienteServico = phccontext.ObterClienteSimples(fo.ClienteServico.IdCliente, fo.ClienteServico.IdLoja);
            fo.EquipamentoServico = phccontext.ObterEquipamentoSimples(fo.EquipamentoServico.EquipamentoStamp);
            fo.Marcacao = phccontext.ObterMarcacao(fo.IdMarcacao);

            if (fo.EmGarantia != fo.EquipamentoServico.Garantia && string.IsNullOrEmpty(fo.SituacoesPendentes)) ModelState.AddModelError("SituacoesPendentes", "Necessario identificar o motivo para alteração de garantia!");
            if ((fo.EstadoFolhaObra == 4) && string.IsNullOrEmpty(fo.SituacoesPendentes)) ModelState.AddModelError("SituacoesPendentes", "Estado da folha de obra pendente. Necessita de justificar!");
            if (fo.IntervencaosServico.Where(i => i.HoraInicio > i.HoraFim).Count() > 0) ModelState.AddModelError("ListaIntervencoes", "Existe pelo menos uma intervenção em que a hora de inicio é maior que a hora de fim.");
            if (fo.Marcacao.Contrato != fo.Contrato && string.IsNullOrEmpty(fo.JustExtraContrato)) ModelState.AddModelError("JustExtraContrato", "É obrigatório escolher pelo menos um dos motivos para estar fora do contrato!");
            if (fo.ClienteServico.IdCliente == 3269 && fo.FicheirosAnexo.Split(";").Count() < 2) ModelState.AddModelError("", "É obrigatorio inserir pelo menos 2 anexos para o Cliente LIDL!");
            if ((fo.ClienteServico.IdCliente == 5828 || fo.ClienteServico.IdCliente == 5829) && fo.FicheirosAnexo.Split(";").Count() < 1) ModelState.AddModelError("", "É obrigatorio inserir pelo menos 1 anexo para o Cliente ALDI!");
            if (fo.TipoFolhaObra == "Instalação" && fo.FicheirosAnexo.Split(";").Count() < 3 && fo.FecharMarcacao && !fo.Marcacao.GuiasInstalacao) ModelState.AddModelError("", "É obrigatorio inserir pelo menos 3 anexos ao finalizar uma instalação!");
            if (fo.PecasServico.Where(p => p.Ref_Produto == "SRV.156").Count() > fo.FicheirosAnexo.Split(";").Count() - 1) ModelState.AddModelError("", "É obrigatorio inserir pelo menos 1 anexo para cada referencia SRV!");


            String[] res = { string.Join("\r\n", ModelState.Where(e => e.Value.Errors.Count() > 0).Select(e => e.Value.Errors.First().ErrorMessage)), phccontext.ValidarFolhaObra(fo) };

            _logger.LogDebug("Utilizador {1} [{2}] a validar uma nova folha de obra: Id Marcacao - {3}, Cliente - {4}, Equipamento - {5}, Tecnico - {6}, N. Int - {7}, N. Pecas {8}, Estado - {9}.", fo.Utilizador.NomeCompleto, fo.Utilizador.Id, fo.Marcacao.IdMarcacao, fo.ClienteServico.NomeCliente, fo.EquipamentoServico.NumeroSerieEquipamento, fo.Utilizador.NomeCompleto, fo.IntervencaosServico.Count(), fo.PecasServico.Count(), fo.EstadoFolhaObra);

            return Json(res);
        }

        //Obter o email do cliente associado á folha de obra
        [HttpGet]
        public JsonResult Email(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            FolhaObra fo = phccontext.ObterFolhaObra(id);

            _logger.LogDebug("Utilizador {1} [{2}] a obter o email associado a um cliente: Cliente - {3}, Email - {4}.", u.NomeCompleto, u.Id, fo.ClienteServico.NomeCliente, fo.ClienteServico.EmailCliente);

            return Json(fo.ClienteServico.EmailCliente);
        }

        //Enviar o email da folha de obra
        [HttpPost]
        public ActionResult Email(int id, string emailDestino)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FolhaObra fo = phccontext.ObterFolhaObra(id);
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && fo.IntervencaosServico.Where(i => i.IdTecnico == u.IdPHC).Count() == 0) return Redirect("~/Home/AcessoNegado");

            _logger.LogDebug("Utilizador {1} [{2}] a enviar o email com uma folha de obra a um cliente: Cliente - {3}, Email - {4}, Id FO - {5}.", u.NomeCompleto, u.Id, fo.ClienteServico.NomeCliente, fo.ClienteServico.EmailCliente, fo.IdFolhaObra);

            if (MailContext.EnviarEmailFolhaObra(emailDestino, fo, new Attachment((new MemoryStream(context.PreencherFormularioFolhaObra(fo).ToArray())), "FO" + id + ".pdf", System.Net.Mime.MediaTypeNames.Application.Pdf))) return Content("Sucesso");

            return Content("Erro");

        }

        //Imprimir Etiqueta 80x60
        [HttpGet]
        public ActionResult Etiqueta(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            FolhaObra fo = phccontext.ObterFolhaObra(int.Parse(id));

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && fo.IntervencaosServico.Where(i => i.IdTecnico == u.IdPHC).Count() == 0) return Redirect("~/Home/AcessoNegado");

            _logger.LogDebug("Utilizador {1} [{2}] a imprimir uma etiqueta de uma folha de obra: Cliente - {3}, Id FO - {4}.", u.NomeCompleto, u.Id, fo.ClienteServico.NomeCliente, fo.IdFolhaObra);

            return File(context.MemoryStreamToPDF(context.DesenharEtiquetaFolhaObra(fo), 801, 504), "application/pdf");
        }

        //Imprimir Documento A4
        [HttpGet]
        public virtual ActionResult Documento(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FolhaObra fo = phccontext.ObterFolhaObra(id);
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && fo.IntervencaosServico.Where(i => i.IdTecnico == u.IdPHC).Count() == 0) return Redirect("~/Home/AcessoNegado");

            _logger.LogDebug("Utilizador {1} [{2}] a imprimir o documento A4 de uma folha de obra: Cliente - {3}, Id FO - {4}.", u.NomeCompleto, u.Id, fo.ClienteServico.NomeCliente, fo.IdFolhaObra);

            var file = context.PreencherFormularioFolhaObra(fo).ToArray();
            var output = new MemoryStream();
            output.Write(file, 0, file.Length);
            output.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "FolhaObra_" + id + ".pdf",
                Inline = true,
                Size = file.Length,
                CreationDate = DateTime.Now,

            };
            Response.Headers.Append("Content-Disposition", cd.ToString());
            //Send the File to Download.
            return new FileContentResult(output.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        //Imprimir ticket 
        [HttpGet]
        public virtual ActionResult Ticket(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            FolhaObra fo = phccontext.ObterFolhaObra(int.Parse(id));

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && fo.IntervencaosServico.Where(i => i.IdTecnico == u.IdPHC).Count() == 0) return Redirect("~/Home/AcessoNegado");

            _logger.LogDebug("Utilizador {1} [{2}] a imprimir um ticket de uma folha de obra: Cliente - {3}, Id FO - {4}.", u.NomeCompleto, u.Id, fo.ClienteServico.NomeCliente, fo.IdFolhaObra);

            return File(context.MemoryStreamToPDF(context.DesenharTicketFO(fo), 1024, (840 + fo.PecasServico.Where(p => !p.Ref_Produto.Contains("SRV")).Count() * 140) * 2), "application/pdf");  }

        //Criar codigo da folha de obra
        [HttpPost]
        public ActionResult CriarCodigo(string id, string obs)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Marcacao m = phccontext.ObterMarcacao(id);

            Codigo c = new Codigo()
            {
                Stamp = DateTime.Now.Ticks.ToString(),
                Estado = 0,
                ValidadeCodigo = DateTime.Now.AddMinutes(10),
                utilizador = u,
                Obs = obs,
                ObsInternas = "Cliente: " + m.Cliente.NomeCliente + " - Marcação Nº: " + m.IdMarcacao
            };

            _logger.LogDebug("Utilizador {1} [{2}] a criar um codigo para validar uma folha de obra: Codigo - {3}, Validade - {4}.", u.NomeCompleto, u.Id, c.Stamp, c.ValidadeCodigo.ToShortTimeString());

            context.CriarCodigo(c);
            foreach (var utilizador in context.ObterListaUtilizadores(false, false).Where(u => u.Admin))
            {
                ChatContext.EnviarNotificacaoCodigo(c, utilizador);
            }
            return Content(c.Stamp);
        }

        //Validar o codigo
        [HttpPost]
        public ActionResult ValidarCodigo(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            _logger.LogDebug("Utilizador {1} [{2}] a tentar validar o codigo de uma folha de obra: Codigo - {3}.", u.NomeCompleto, u.Id, id);
            Codigo c = context.ObterCodigo(id);
            if (string.IsNullOrEmpty(c.Stamp)) return StatusCode(500);

            return Json(c.Estado + "|" + c.ValidadoPor.NomeCompleto);
        }

    }
}
