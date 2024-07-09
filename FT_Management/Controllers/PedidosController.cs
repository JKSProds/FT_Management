using Ical;
using Ical.Net.DataTypes;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;

namespace FT_Management.Controllers
{

    [Authorize(Roles = "Admin, Tech, Escritorio")]
    public class PedidosController : Controller
    {
        //Obter lista de marcacoes
        public ActionResult Index(string numMarcacao, string nomeCliente, string referencia, string tipoe, int idtecnico, string estado)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (!User.IsInRole("Admin") && !User.IsInRole("Escritorio"))
            {
                int IdTecnico = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).IdPHC;
                return RedirectToAction("Pedidos", new { IdTecnico, DataPedidos = DateTime.Now.ToShortDateString() });
            }

            List<Utilizador> LstUtilizadores = context.ObterListaTecnicos(false, false).ToList();
            LstUtilizadores.Insert(0, new Utilizador() { Id = 0, NomeCompleto = "Todos" });
            ViewBag.ListaTecnicos = LstUtilizadores;

            List<String> LstTipoEquipamento = phccontext.ObterTipoEquipamento().ToList();
            LstTipoEquipamento.Insert(0, "Todos");

            ViewBag.TipoEquipamento = LstTipoEquipamento.Select(l => new SelectListItem() { Value = l, Text = l });

            List<String> LstEstados = phccontext.ObterMarcacaoEstados().Select(e => e.EstadoMarcacaoDesc).ToList();
            LstEstados.Insert(0, "Todos");

            ViewBag.Estados = LstEstados.Select(l => new SelectListItem() { Value = l, Text = l });


            if (string.IsNullOrEmpty(nomeCliente)) { nomeCliente = ""; }
            if (string.IsNullOrEmpty(referencia)) { referencia = ""; }
            if (string.IsNullOrEmpty(tipoe)) { tipoe = ""; }
            if (string.IsNullOrEmpty(numMarcacao)) { numMarcacao = ""; }
            if (string.IsNullOrEmpty(estado)) { estado = ""; }

            ViewData["numMarcacao"] = numMarcacao;
            ViewData["nomeCliente"] = nomeCliente;
            ViewData["referencia"] = referencia;
            ViewData["tipoe"] = tipoe;
            ViewData["idtecnico"] = idtecnico;
            ViewData["estado"] = estado;

            if (string.IsNullOrEmpty(numMarcacao) && string.IsNullOrEmpty(nomeCliente) && string.IsNullOrEmpty(referencia) && string.IsNullOrEmpty(tipoe) && idtecnico == 0 && string.IsNullOrEmpty(estado)) return View(phccontext.ObterMarcacoes(DateTime.Now, DateTime.Now.AddDays(1)));
            
            return View(phccontext.ObterMarcacoes(int.Parse(numMarcacao != "" ? numMarcacao : "0"), nomeCliente, referencia, tipoe, idtecnico, estado));
        }

        //Obter Pedidos de um tecnico num dia especifico
        [HttpGet]
        public ActionResult Pedidos(int id, string DataPedidos)
        {
            if (DataPedidos == null || DataPedidos == string.Empty) DataPedidos = DateTime.Now.ToString("dd-MM-yyyy");
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterListaTecnicos(true, false).Where(u => u.IdPHC == id).DefaultIfEmpty(new Utilizador()).First();
            if (id == 0) u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            List<Marcacao> ListaMarcacoes = phccontext.ObterMarcacoes(u.IdPHC, DateTime.Parse(DataPedidos));

            ViewData["DataPedidos"] = DataPedidos;
            ViewData["IdTecnico"] = u.IdPHC;
            ViewData["IdArmazem"] = u.IdArmazem;
            ViewData["Piquete"] = context.VerificarPiquete(DateTime.Parse(DataPedidos), u);

            return View(ListaMarcacoes.OrderBy(m => m.EstadoMarcacao).OrderBy(m => m.Cliente.ClienteStamp));
        }

        //Obter ICS do calendario
        [HttpGet]
        public virtual ActionResult Calendario(string ApiKey)
        {
            DateTime d = DateTime.Now;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            int IdUtilizador = int.Parse(this.User.Claims.First().Value);
            if (IdUtilizador == 0) return Forbid();

            var calendar = new Calendar();
            List<Marcacao> LstMarcacoes = phccontext.ObterMarcacoes(context.ObterUtilizador(IdUtilizador).IdPHC, DateTime.Now.AddDays(-30), DateTime.Now.AddDays(30)).OrderBy(d => d.DataMarcacao).ToList();

            foreach (Marcacao m in LstMarcacoes)
            {
                if (d.ToShortDateString() != m.DataMarcacao.ToShortDateString()) d = m.DataMarcacao.Add(TimeSpan.FromHours(8));
                var e = new CalendarEvent
                {
                    Start = new CalDateTime(d),
                    End = new CalDateTime(d.AddMinutes(30)),
                    LastModified = new CalDateTime(DateTime.Now),
                    Uid = m.IdMarcacao.ToString(),
                    Description = "### Estado do Pedido: " + m.EstadoMarcacaoDesc + " ###" + Environment.NewLine + Environment.NewLine + m.ResumoMarcacao,
                    Summary = m.EmojiEstado + m.Cliente.NomeCliente,
                    Url = new Uri(m.GetUrl),
                    Location = m.Cliente.MoradaCliente
                };
                calendar.Events.Add(e);
                d = d.AddMinutes(30);
            }

            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(calendar);
            var bytesCalendar = new UTF8Encoding(false).GetBytes(serializedCalendar);

            MemoryStream ms = new MemoryStream(bytesCalendar);

            ms.Write(bytesCalendar, 0, bytesCalendar.Length);
            ms.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "Servicos.ics",
                Inline = false,
                Size = bytesCalendar.Length,
                CreationDate = DateTime.Now

            };
            Response.Headers.Append("Content-Disposition", cd.ToString());

            return File(ms, "text/calendar");
        }

        //Obter ICS do calendario
        [HttpGet]
        public virtual ActionResult CalendarioInstalacoes(string ApiKey)
        {
            DateTime d = DateTime.Now;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            int IdUtilizador = int.Parse(this.User.Claims.First().Value);
            if (IdUtilizador == 0) return Forbid();

            var calendar = new Calendar();
            List<Marcacao> LstMarcacoes = phccontext.ObterMarcacoesInstalacao(DateTime.Now.AddDays(-7), DateTime.Now.AddDays(365)).OrderBy(d => d.DataMarcacao).ToList();

            foreach (Marcacao m in LstMarcacoes)
            {
                if (d.ToShortDateString() != m.DataMarcacao.ToShortDateString()) d = m.DataMarcacao.Add(TimeSpan.FromHours(8));
                var e = new CalendarEvent
                {
                    Start = new CalDateTime(d),
                    End = new CalDateTime(d.AddMinutes(30)),
                    LastModified = new CalDateTime(DateTime.Now),
                    Uid = m.IdMarcacao.ToString(),
                    Description = "### " + m.IdMarcacao + " - " + m.Referencia + " - " + m.EstadoMarcacaoDesc + " ###" + Environment.NewLine + Environment.NewLine + m.ResumoMarcacao,
                    Summary = m.EmojiEstado + m.Cliente.NomeCliente,
                    Url = new Uri(m.GetUrl),
                    Location = m.Cliente.MoradaCliente
                };
                calendar.Events.Add(e);
                d = d.AddMinutes(30);
            }

            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(calendar);
            var bytesCalendar = new UTF8Encoding(false).GetBytes(serializedCalendar);

            MemoryStream ms = new MemoryStream(bytesCalendar);

            ms.Write(bytesCalendar, 0, bytesCalendar.Length);
            ms.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "Instalacoes.ics",
                Inline = false,
                Size = bytesCalendar.Length,
                CreationDate = DateTime.Now

            };
            Response.Headers.Append("Content-Disposition", cd.ToString());

            return File(ms, "text/calendar");
        }

        //Obter calendario de agendamentos
        public ActionResult Agendamento(int id, int zona, int tipo)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            if (id != 0) id = context.ObterListaUtilizadores(false, false).Where(u => u.IdPHC == id).FirstOrDefault().Id;

            ViewData["zona"] = zona;
            ViewData["tipo"] = tipo;

            List<Zona> LstZonas = context.ObterZonas(false);
            LstZonas.Insert(0, new Zona() { Id = 0, Valor = "Todos" });
            ViewBag.Zonas = LstZonas.Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Valor });

            List<TipoTecnico> LstTipoTecnicos = context.ObterTipoTecnicos();
            LstTipoTecnicos.Insert(0, new TipoTecnico() { Id = 0, Valor = "Todos" });
            ViewBag.TipoTecnico = LstTipoTecnicos.Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Valor });

            if (!User.IsInRole("Admin") && !User.IsInRole("Escritorio") || id != 0)
            {
                if (id == 0) id = int.Parse(this.User.Claims.First().Value);
                Utilizador u = context.ObterUtilizador(id);
                return View("Calendario", new List<Utilizador>() { u });
            }

            List<Utilizador> LstTecnicos = context.ObterListaTecnicos(true, true);

            if (zona > 0) LstTecnicos = LstTecnicos.Where(t => t.Zona == zona).ToList();
            if (tipo > 0) LstTecnicos = LstTecnicos.Where(t => t.TipoTecnico == tipo).ToList();

            return View("Calendario", LstTecnicos);
        }

        //Obter pedidos pendentes de um tecnico em especifico
        [HttpGet]
        public ActionResult Pendentes(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterListaTecnicos(true, false).Where(u => u.IdPHC == id).DefaultIfEmpty(new Utilizador()).First();
            List<Marcacao> ListaMarcacoes = phccontext.ObterMarcacoesPendentes(id).OrderBy(m => m.DataMarcacao).ToList();

            ViewData["DataPedidos"] = DateTime.Now.ToString("dd-MM-yyyy");
            ViewData["IdTecnico"] = u.IdPHC;
            ViewData["IdArmazem"] = u.IdArmazem;
            ViewData["Piquete"] = false;

            return View("Pedidos", ListaMarcacoes);
        }

        //Obter pedido em especifica
        [HttpGet]
        public ActionResult Pedido(int id)
        {
            if (id == 0) return RedirectToAction("Index");

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Marcacao m = phccontext.ObterMarcacao(id);
            Utilizador user = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            ViewData["PessoaContacto"] = m.Cliente.PessoaContatoCliente;
            ViewData["SelectedTecnico"] = user.NomeCompleto;
            ViewData["Tecnicos"] = context.ObterListaTecnicos(false, false);

            return View(m);
        }

        //Obter JSON do objeto da marcacao
        [HttpGet]
        public Marcacao Marcacao(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Marcacao m = phccontext.ObterMarcacao(id);

            if (m.EstadoMarcacaoDesc == "Criado") m.EstadoMarcacaoDesc = "Agendado";
            return m;
        }

        //Obter JSON do objeto das marcacaos
        public JsonResult Marcacoes(DateTime start, DateTime end, int id)
        {

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            List<CalendarioEvent> LstEventos = context.ConverterMarcacoesEventos(phccontext.ObterMarcacoes(context.ObterUtilizador(id).IdPHC, start, end.AddDays(-1)).ToList().OrderBy(m => m.DataMarcacao).ToList()).ToList();
            LstEventos.AddRange(context.ConverterFeriasEventos(context.ObterListaFerias(start, end.AddDays(-1), id), new List<Feriado>()));
            LstEventos.AddRange(context.ConverterFeriadosEventos(context.ObterListaFeriados(start.Year.ToString())));
            LstEventos.AddRange(context.ConverterPiquetesEventos(context.ObterPiquetes(start, end,context.ObterUtilizador(id))));

            if (id > 0) return new JsonResult(LstEventos);

            List<Marcacao> LstMarcacoesCriadas = phccontext.ObterMarcacoesCriadas();
            if (LstMarcacoesCriadas.Count > 0)
            {
                List<Marcacao> LstMarcacoesFiltro = new List<Marcacao>();
                int nPerDay = LstMarcacoesCriadas.Count() / 5 == 0 ? 1 : LstMarcacoesCriadas.Count() / 5;
                for (int i = 0; i < 6; i++)
                {
                    LstMarcacoesFiltro.AddRange(LstMarcacoesCriadas.Skip(i * nPerDay).Take(nPerDay).Select(c => { c.DataMarcacao = DateTime.Now.AddDays(i - (int)DateTime.Now.DayOfWeek + 1); return c; }).ToList());
                }

                return new JsonResult(context.ConverterMarcacoesEventos(LstMarcacoesFiltro).ToList());
            }
            return new JsonResult("");
        }

        //Adicionar um Pedido
        [Authorize(Roles = "Admin, Escritorio")]
        [HttpGet]
        public ActionResult Adicionar(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            ViewData["Tecnicos"] = context.ObterListaTecnicos(false, false);
            ViewData["TipoEquipamento"] = phccontext.ObterTipoEquipamento();
            ViewData["TipoServico"] = phccontext.ObterTipoServico();
            ViewData["Estado"] = phccontext.ObterMarcacaoEstados();
            ViewData["Periodo"] = phccontext.ObterPeriodo();
            ViewData["Prioridade"] = phccontext.ObterPrioridade();
            ViewData["Exclusoes"] = phccontext.ObterExclusoes(1);
            ViewData["TipoPedido"] = phccontext.ObterTipoPedido();
            ViewBag.Formularios = phccontext.ObterFormularios().Select(l => new SelectListItem() { Value = l.Key, Text = l.Key });

            if (id != 0) return View(phccontext.ObterMarcacao(id));

            return View(new Marcacao());
        }

        //Obter view da Edição
        [Authorize(Roles = "Admin, Escritorio")]
        [HttpGet]
        public ActionResult Editar(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            ViewData["Tecnicos"] = context.ObterListaTecnicos(false, false);
            ViewData["TipoEquipamento"] = phccontext.ObterTipoEquipamento();
            ViewData["TipoServico"] = phccontext.ObterTipoServico();
            ViewData["Estado"] = phccontext.ObterMarcacaoEstados();
            ViewData["Periodo"] = phccontext.ObterPeriodo();
            ViewData["Prioridade"] = phccontext.ObterPrioridade();
            ViewData["Exclusoes"] = phccontext.ObterExclusoes(1);
            ViewData["TipoPedido"] = phccontext.ObterTipoPedido();
            ViewBag.Formularios = phccontext.ObterFormularios().Select(l => new SelectListItem() { Value = l.Key, Text = l.Key });
            Marcacao m = phccontext.ObterMarcacao(id);
            return View(m);
        }

        //Criar ou editar Marcacao
        [Authorize(Roles = "Admin, Escritorio")]
        [HttpPost]
        public ActionResult Marcacao(Marcacao m)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            ModelState.Remove("DataMarcacao");
            if (m.LstTecnicosSelect.Count() > 0)
            {
                foreach (var item in m.LstTecnicosSelect)
                {
                    m.LstTecnicos.Add(context.ObterUtilizador(item));
                }

                m.Tecnico = m.LstTecnicos.First();
                ModelState.Remove("Tecnico.Password");
            }
            else
            {
                m.LstTecnicos = new List<Utilizador>() { new Utilizador() };
                m.Tecnico = new Utilizador();
                m.EstadoMarcacaoDesc = "Criado";
            }

            m.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(m.MarcacaoStamp))
                {
                    m.Cliente = phccontext.ObterClienteSimples(m.Cliente.IdCliente, m.Cliente.IdLoja);
                    m.IdMarcacao = phccontext.CriarMarcacao(m);
                }
                else
                {
                    phccontext.AtualizaMarcacao(m);
                }

                if (m.IdMarcacao > 0) return RedirectToAction("Editar", "Pedidos", new { id = m.IdMarcacao });
            }

            ViewData["Tecnicos"] = context.ObterListaTecnicos(false, false);
            ViewData["TipoEquipamento"] = phccontext.ObterTipoEquipamento();
            ViewData["TipoServico"] = phccontext.ObterTipoServico();
            ViewData["Estado"] = phccontext.ObterMarcacaoEstados();
            ViewData["Periodo"] = phccontext.ObterPeriodo();
            ViewData["Prioridade"] = phccontext.ObterPrioridade();
            ViewData["Exclusoes"] = phccontext.ObterExclusoes(1);
            ViewData["TipoPedido"] = phccontext.ObterTipoPedido();
            ViewBag.Formularios = phccontext.ObterFormularios().Select(l => new SelectListItem() { Value = l.Key, Text = l.Key });

            ModelState.AddModelError("", "Ocorreu um erro ao modificar a marcação! Por favor tente novamente.");
            if (m.IdMarcacao == 0) return View("Adicionar", m);
            return View("Editar", m);
        }

        //Agendar servico alterado no calendario
        [HttpPost]
        [Authorize(Roles = "Admin, Escritorio")]
        public JsonResult Agendar(int id, string dateOriginal, string date, int idTecnicoOriginal, int idTecnico)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Marcacao m = phccontext.ObterMarcacao(id);
            m.DatasAdicionais = m.DatasAdicionais.Replace(DateTime.Parse(dateOriginal).ToString("yyyy-MM-dd"), DateTime.Parse(date).ToString("yyyy-MM-dd"));

            Utilizador u = context.ObterUtilizador(idTecnico);
            if (idTecnicoOriginal == 0)
            {
                m.Tecnico = u;
                m.LstTecnicos = new List<Utilizador>() { u };
                m.EstadoMarcacaoDesc = "Agendado";
                m.DatasAdicionais = m.DatasAdicionais.Replace(m.DataMarcacao.ToString("yyyy-MM-dd"), DateTime.Parse(date).ToString("yyyy-MM-dd"));
                m.DataMarcacao = DateTime.Parse(date);

            }
            else if (idTecnico == 0)
            {
                m.EstadoMarcacaoDesc = "Criado";
                m.LstTecnicos = new List<Utilizador>() { new Utilizador() };
                m.Tecnico = new Utilizador();
            }
            else
            {
                if (m.Tecnico.Id == idTecnicoOriginal) m.Tecnico = u;
                if (m.LstTecnicos.Where(u => u.Id == idTecnicoOriginal).Count() > 0) m.LstTecnicos[m.LstTecnicos.FindIndex(u => u.Id == idTecnicoOriginal)] = u;
                m.EstadoMarcacaoDesc = "Reagendado";
            }
            

            m.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            phccontext.AtualizaMarcacao(m);

            return Json("Ok");
        }

        //Alterar alguns valores da marcacao
        [HttpPost]
        [Authorize(Roles = "Admin, Escritorio")]
        public JsonResult Alterar(int id, string EstadoMarcacaoDesc, int Tecnico, string DataMarcacao)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Marcacao m = phccontext.ObterMarcacao(id);

            m.EstadoMarcacaoDesc = EstadoMarcacaoDesc;
            m.Tecnico = context.ObterUtilizador(Tecnico);
            m.LstTecnicos = new List<Utilizador>() { m.Tecnico };
            m.DatasAdicionais = DataMarcacao;
            m.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            phccontext.AtualizaMarcacao(m);

            return Json("Ok");
        }

        //Iniciar deslocação
        [HttpPost]
        public ActionResult Iniciar(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Marcacao m = phccontext.ObterMarcacao(id);

            return phccontext.AtualizarMarcacaoEmCurso(m) ? StatusCode(200) : StatusCode(500);
        }

        //Validar Marcacao
        [Authorize(Roles = "Admin, Escritorio")]
        [HttpPost]
        public JsonResult ValidarMarcacao(Marcacao m)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            m.DataMarcacao = m.DatasAdicionaisDistintas.First();
            return Json(phccontext.ValidarMarcacao(m));
        }

        //Adicionar Comentario
        [HttpPost]
        public JsonResult Comentario(int id, string comentario, int fechar, int encaminhar, int reagendar)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Marcacao m = phccontext.ObterMarcacao(id);

            Comentario c = new Comentario()
            {
                Descricao = comentario,
                Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)),
                Marcacao = m,
                DataComentario = DateTime.Now
            };

            bool res = phccontext.CriarComentarioMarcacao(c);
            FolhaObra fo = new FolhaObra() { RelatorioServico = c.Descricao, ReferenciaServico = m.Referencia, Utilizador = c.Utilizador };
            if (fechar == 1)
            {
                m.JustificacaoFecho = comentario;
                m.EstadoMarcacaoDesc = "Finalizado";
                m.Utilizador = c.Utilizador;
                if (m.Cliente.IdCliente == 878 || m.Cliente.IdCliente == 297) MailContext.EnviarEmailMarcacaoPD(fo, m, 1);
                if (m.Cliente.IdCliente == 561 || m.Cliente.IdCliente == 1568) MailContext.EnviarEmailMarcacaoSONAE(fo, m, 1);
            }
            else if (encaminhar == 1)
            {
                m.JustificacaoFecho = comentario;
                m.EstadoMarcacaoDesc = "Finalizado";
                m.Utilizador = c.Utilizador;
                if (m.Cliente.IdCliente == 878 || m.Cliente.IdCliente == 297) MailContext.EnviarEmailMarcacaoPD(fo, m, 2);
                if (m.Cliente.IdCliente == 561 || m.Cliente.IdCliente == 1568) MailContext.EnviarEmailMarcacaoSONAE(fo, m, 2);
            }
            else if (reagendar == 1)
            {
                m.EstadoMarcacaoDesc = "Reagendar";
                m.Utilizador = c.Utilizador;
                if (m.Cliente.IdCliente == 878 || m.Cliente.IdCliente == 297) {
                    MailContext.EnviarEmailMarcacaoPD(fo, m, 3);
                }
                if (m.Cliente.IdCliente == 1568 || m.Cliente.IdCliente == 561)
                {
                    MailContext.EnviarEmailMarcacaoSONAE(fo, m, 3);
                }
            }

            phccontext.AtualizaMarcacao(m);
            return Json(new { json = res });
        }

        //Obter Anexo
        [HttpGet]
        public ActionResult Anexo(string id)
        {
            if (id != null)
            {
                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

                MarcacaoAnexo a = phccontext.ObterAnexo(id);
                string CaminhoFicheiro = FicheirosContext.FormatLinuxServer(a.NomeFicheiro);
                if (!System.IO.File.Exists(CaminhoFicheiro)) return Forbid();

                if (new FileExtensionContentTypeProvider().TryGetContentType(CaminhoFicheiro, out var mimeType))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(CaminhoFicheiro);

                    Response.Headers.Append("Content-Disposition", "inline;filename=" + a.ObterNomeFicheiro());
                    //Send the File to Download.
                    return new FileContentResult(bytes, mimeType);

                }
            }
            return Forbid();
        }

        //Adicionar Anexo da Marcacao
        [HttpPost]
        public JsonResult Anexo(int id, int tipo, IFormFile file)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            if (file.Length > 0)
            {
                MarcacaoAnexo a = new MarcacaoAnexo()
                {
                    MarcacaoStamp = phccontext.ObterMarcacao(id).MarcacaoStamp,
                    IdMarcacao = id,
                    AnexoMarcacao = true,
                    NomeUtilizador = this.User.ObterNomeCompleto(),
                    AnexoInstalacao = tipo > 0,
                    TipoDocumento = tipo == 1 ? "GT" : tipo == 2 ? "AF" : tipo == 3 ? "AR" : ""
                };
                    
                a.NomeFicheiro = a.ObterNomeUnico() + (file.FileName.Contains(".") ? "." + file.FileName.Split(".").Last() : "");

                string res = phccontext.CriarAnexoMarcacao(a);
                if (res.Length == 0) return Json("-1");
                if (!FicheirosContext.CriarAnexoMarcacao(phccontext.ObterAnexo(res), file))
                {
                    Anexo(res, true);
                    return Json("-1");
                }
            }
            return Json("0");
        }

        //Apagar Anexo da Marcacao
        [HttpDelete]
        public ActionResult Anexo(string id, bool apagar)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (id != null)
            {
                MarcacaoAnexo a = phccontext.ObterAnexo(id);
                phccontext.ApagarAnexoMarcacao(a);
                FicheirosContext.ApagarAnexoMarcacao(a);
                return Content("1");

            }
            return Content("0");
        }

        //Obter etiqueta da marcacao
        [HttpGet]
        public ActionResult Etiqueta(string id)
        {
            if (id == null) return RedirectToAction("Index");

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return File(context.MemoryStreamToPDF(context.DesenharEtiquetaMarcacao(phccontext.ObterMarcacao(int.Parse(id))), 801, 504), "application/pdf");
        }

        //Obter percentagem de marcacoes concluido
        [HttpGet]
        public List<int> Percentagem(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            return phccontext.ObterPercentagemMarcacoes(id);
        }

        //Enviar email do pedido para tecnico ou cliente
        [Authorize(Roles = "Admin, Escritorio")]
        [HttpPost]
        public ActionResult Email(int id, string email)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Marcacao m = phccontext.ObterMarcacao(id);

            if (string.IsNullOrEmpty(email))
            {
                foreach (var item in m.LstTecnicos)
                {
                    if (!MailContext.EnviarEmailMarcacaoTecnico(item.EmailUtilizador, m, item.NomeCompleto)) return Content("Erro");
                }
            }
            else
            {
                var calendar = new Calendar();

                var e = new CalendarEvent
                {
                    Start = new CalDateTime(m.DataMarcacao),
                    End = new CalDateTime(m.DataMarcacao),
                    IsAllDay = true,
                    LastModified = new CalDateTime(DateTime.Now),
                    Uid = m.IdMarcacao.ToString() + "_Cliente",
                    Description = "Foi agendada uma assistência técnica. Para mais informações contacte: +351 229 479 670 (" + m.Utilizador.NomeCompleto + ")",
                    Location = m.Cliente.MoradaCliente,
                    Contacts = new List<string>() { "+351229479670" },
                    //Name = "Assistência Técnica | Food-Tech",
                    Summary = "Food-Tech | Marc. Nº" + m.IdMarcacao + " - Assistência Técnica",
                };

                calendar.Events.Add(e);
                var serializer = new CalendarSerializer();
                var serializedCalendar = serializer.SerializeToString(calendar);
                var bytesCalendar = new UTF8Encoding(false).GetBytes(serializedCalendar);

                if (!MailContext.EnviarEmailMarcacaoCliente(email, m, new System.Net.Mail.Attachment((new MemoryStream(bytesCalendar)), m.IdMarcacao + ".ics"))) return Content("Erro");

            }

            return Content("Sucesso");
        }

        //Obtem as direções para as marcacoes de um tecnico num determinado dia
        [HttpGet]
        public IActionResult Direcaos(int id, DateTime DataPedidos)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            List<Marcacao> ListaMarcacoes = phccontext.ObterMarcacoes(id, DataPedidos);
            string url = "https://www.google.com/maps/dir";

            foreach (var item in ListaMarcacoes.Where(m => m.EstadoMarcacaoDesc == "Agendado" || m.EstadoMarcacaoDesc == "Reagendado").GroupBy(c => c.Cliente).Select(X => X.First()))
            {
                url += "/" + item.Cliente.ObterMoradaDirecoes().Replace("/", " ");
            }

            return Redirect(new Uri(url).AbsoluteUri);
        }

        //Assinar Guias
        [HttpPost]
        public ActionResult AssinarGuias(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            MarcacaoAnexo a = phccontext.ObterAnexo(id);
            Marcacao m = phccontext.ObterMarcacao(a.MarcacaoStamp);

            if (m.LstFolhasObra.Count() == 0) return StatusCode(500);

            FolhaObra fo = phccontext.ObterFolhaObraSimples(m.LstFolhasObra.Last().StampFO);

            m.LstAnexos = new List<MarcacaoAnexo> { a };
            fo.Marcacao = m;

            return StatusCode(phccontext.AtualizarAnexosAssinatura(fo) ? 200 : 500);
        }
    }
}
