using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FT_Management.Models;
using Ical;
using Ical.Net.DataTypes;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;

namespace FT_Management.Controllers
{

    [Authorize(Roles = "Admin, Tech, Escritorio")]
    public class PedidosController : Controller
    {
        [Authorize(Roles = "Admin, Escritorio")]
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
            ViewData["TipoPedido"] = phccontext.ObterTipoPedido();

            if (id != 0) return View(phccontext.ObterMarcacao(id));

            return View(new Marcacao());
        }
        public ActionResult Iniciar(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Marcacao m = phccontext.ObterMarcacao(id);
            m.EstadoMarcacaoDesc = "Em Curso";
            m.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            phccontext.AtualizaMarcacao(m);

            return Content("1");
        }

        [Authorize(Roles = "Admin, Escritorio")]
        [HttpPost]
        public JsonResult ValidarMarcacao(Marcacao m)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            return Json(phccontext.ValidarMarcacao(m));
        }

        public List<int> ObterPercentagemTecnico(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            return phccontext.ObterPercentagemMarcacoes(id);
        }

        [Authorize(Roles = "Admin, Escritorio")]
        [HttpPost]
        public ActionResult Adicionar(Marcacao m)
        {
            int IdMarcacao = 0;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

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
            }

            m.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            if (ModelState.IsValid)
            {
                m.Cliente = phccontext.ObterClienteSimples(m.Cliente.IdCliente, m.Cliente.IdLoja);
                IdMarcacao = phccontext.CriarMarcacao(m);

                if (IdMarcacao > 0) return RedirectToAction("Editar", "Pedidos", new { id = IdMarcacao });
            }

            ViewData["Tecnicos"] = context.ObterListaTecnicos(false, false);
            ViewData["TipoEquipamento"] = phccontext.ObterTipoEquipamento();
            ViewData["TipoServico"] = phccontext.ObterTipoServico();
            ViewData["Estado"] = phccontext.ObterMarcacaoEstados();
            ViewData["Periodo"] = phccontext.ObterPeriodo();
            ViewData["Prioridade"] = phccontext.ObterPrioridade();
            ViewData["TipoPedido"] = phccontext.ObterTipoPedido();

            ModelState.AddModelError("", "Ocorreu um erro ao adicionar a marcação! Por favor tente novamente.");

            return View(m);
        }


        [Authorize(Roles = "Admin, Escritorio")]
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
            ViewData["TipoPedido"] = phccontext.ObterTipoPedido();
            Marcacao m = phccontext.ObterMarcacao(id);
            return View(m);
        }

        [Authorize(Roles = "Admin, Escritorio")]
        [HttpPost]
        public ActionResult Editar(int id, Marcacao m)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

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
            }
            m.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            if (m.DatasAdicionais == null) ModelState.AddModelError("", "Tem de adicionar pelo menos uma data!");


            if (ModelState.IsValid)
            {
                phccontext.AtualizaMarcacao(m);
                return RedirectToAction("Editar", "Pedidos", new { id = id });
            }

            ViewData["Tecnicos"] = context.ObterListaTecnicos(false, false);
            ViewData["TipoEquipamento"] = phccontext.ObterTipoEquipamento();
            ViewData["TipoServico"] = phccontext.ObterTipoServico();
            ViewData["Estado"] = phccontext.ObterMarcacaoEstados();
            ViewData["Periodo"] = phccontext.ObterPeriodo();
            ViewData["Prioridade"] = phccontext.ObterPrioridade();
            ViewData["TipoPedido"] = phccontext.ObterTipoPedido();

            return View(m);
        }

        [HttpPost]
        public JsonResult AdicionarComentario(int id, string comentario, int fechar, int encaminhar, int reagendar)
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
                if (m.Cliente.IdCliente == 878) MailContext.EnviarEmailMarcacaoPD(fo, m, 1);
                if (m.Cliente.IdCliente == 561) MailContext.EnviarEmailMarcacaoSONAE(fo, m, 1);
            }
            else if (encaminhar == 1)
            {
                m.JustificacaoFecho = comentario;
                m.EstadoMarcacaoDesc = "Finalizado";
                m.Utilizador = c.Utilizador;
                if (m.Cliente.IdCliente == 878) MailContext.EnviarEmailMarcacaoPD(fo, m, 2);
                if (m.Cliente.IdCliente == 561) MailContext.EnviarEmailMarcacaoSONAE(fo, m, 2);
            }
            else if (reagendar == 1)
            {
                m.EstadoMarcacaoDesc = "Reagendado";
                m.Utilizador = c.Utilizador;
            }

            phccontext.AtualizaMarcacao(m);
            return Json(new { json = res });
        }

        [HttpPost]
        public JsonResult AdicionarAnexo(int id, IFormFile file)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            //foreach (IFormFile file in files)
            //{
            if (file.Length > 0)
            {
                Anexo a = new Anexo()
                {
                    MarcacaoStamp = phccontext.ObterMarcacao(id).MarcacaoStamp,
                    IdMarcacao = id,
                    AnexoMarcacao = true,
                    NomeUtilizador = this.User.ObterNomeCompleto()
                };
                a.NomeFicheiro = a.ObterNomeUnico() + (file.FileName.Contains(".") ? "." + file.FileName.Split(".").Last() : "");

                string res = phccontext.CriarAnexoMarcacao(a);
                if (res.Length == 0) return Json("-1");
                if (!FicheirosContext.CriarAnexoMarcacao(phccontext.ObterAnexo(res), file))
                {
                    ApagarAnexo(res);
                    return Json("-1");
                }

            }
            //}

            return Json("0");
        }

        public ActionResult DownloadAnexo(string id)
        {
            if (id != null)
            {
                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

                Anexo a = phccontext.ObterAnexo(id);
                string CaminhoFicheiro = FicheirosContext.FormatLinuxServer(a.NomeFicheiro);
                if (!System.IO.File.Exists(CaminhoFicheiro)) return Forbid();

                if (new FileExtensionContentTypeProvider().TryGetContentType(CaminhoFicheiro, out var mimeType))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(CaminhoFicheiro);

                    Response.Headers.Add("Content-Disposition", "inline;filename=" + a.ObterNomeFicheiro());
                    //Send the File to Download.
                    return new FileContentResult(bytes, mimeType);

                }
            }
            return Forbid();
        }
        public ActionResult ApagarAnexo(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (id != null)
            {
                Anexo a = phccontext.ObterAnexo(id);
                phccontext.ApagarAnexoMarcacao(a);
                FicheirosContext.ApagarAnexoMarcacao(a);
                return RedirectToAction("Pedido", "Pedidos", new { id = phccontext.ObterMarcacao(a.IdMarcacao).IdMarcacao });

            }
            return RedirectToAction("Pedido");
        }

        [Authorize(Roles = "Admin, Escritorio")]
        [HttpPost]
        public ActionResult EmailPedidoTecnico(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Marcacao m = phccontext.ObterMarcacao(id);

            foreach (var item in m.LstTecnicos)
            {
                if (!MailContext.EnviarEmailMarcacaoTecnico(item.EmailUtilizador, m, item.NomeCompleto)) return Content("Erro");
            }

            return Content("Sucesso");
        }

        [Authorize(Roles = "Admin, Escritorio")]
        [HttpPost]
        public ActionResult EmailPedidoCliente(int id, string email)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Marcacao m = phccontext.ObterMarcacao(id);

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

            return Content("Sucesso");
        }

        public IActionResult ObterDirecaosDia(int id, DateTime DataPedidos)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            List<Marcacao> ListaMarcacoes = phccontext.ObterMarcacoes(id, DataPedidos);
            string url = "https://www.google.com/maps/dir";

            foreach (var item in ListaMarcacoes.GroupBy(c => c.Cliente).Select(X => X.First()))
            {
                url += "/" + item.Cliente.ObterMoradaDirecoes().Replace("/", " ");
            }

            return Redirect(new Uri(url).AbsoluteUri);
        }

        [HttpPost]
        public JsonResult ObterResponsavel(string IdCliente, string IdLoja, string TipoEquipamento)
        {
            if (string.IsNullOrEmpty(IdCliente)) return Json("");
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return Json(phccontext.ObterResponsavelCliente(int.Parse(IdCliente), int.Parse(IdLoja), TipoEquipamento));
        }

        [HttpPost]
        public string ObterPedido(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Marcacao m = phccontext.ObterMarcacao(id);

            string res = "";
            res += "<div class=\"mb-3\"><label>Cliente</label><div class=\"input-group\"><input type=\"text\" class=\"form-control\" value='" + m.Cliente.NomeCliente + "' readonly><a class=\"btn btn-outline-warning\"  onclick=\"location.href = '/Clientes/Cliente?IdCliente=" + m.Cliente.IdCliente + "&IdLoja=" + m.Cliente.IdLoja + "'\" type=\"button\"><i class=\"fas fa-eye float-left\" style=\"margin-top:5px\"></i></a></div></div>";
            res += "<div class=\"mb-3\"><label>Detalhes</label><textarea type=\"text\" class=\"form-control\" rows=\"12\" readonly>" + m.ResumoMarcacao + "</textarea></div>";

            if (m.LstComentarios.Count() > 0)
            {
                res += "<table class=\"table table-hover\"><thead><tr><th>Utilizador</th><th>Comentário</th><tbody>";

                foreach (var item in m.LstComentarios)
                {
                    res += "<tr><td><span>" + item.Utilizador.NomeCompleto + "</span></td><td><span>" + item.Descricao + "</span></td>";
                }

                res += "</tbody></table>";
            }

            if (m.LstFolhasObra.Count() > 0)
            {
                res += "<table class=\"table table-hover\"><thead><tr><th>Data</th><th>Cliente</th><th>Nº Série</th><tbody>";

                foreach (var item in m.LstFolhasObra)
                {
                    res += "<tr><td onclick=\"location.href = '/FolhasObra/FolhaObra/" + item.IdFolhaObra + "'\"><span>" + item.DataServico.ToShortDateString() + "</span></td><td onclick=\"location.href = '/FolhasObra/FolhaObra/" + item.IdFolhaObra + "'\"><span>" + item.ClienteServico.NomeCliente + "</span></td><td onclick=\"location.href = '/FolhasObra/FolhaObra/" + item.IdFolhaObra + "'\"><span>" + item.EquipamentoServico.NumeroSerieEquipamento + "</span></td>";
                }

                res += "</tbody></table>";
            }
            return res;
        }

        [HttpGet]
        public Marcacao ObterMarcacao(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Marcacao m = phccontext.ObterMarcacao(id);

            if (m.EstadoMarcacaoDesc == "Criado") m.EstadoMarcacaoDesc = "Agendado";
            return m;
        }

        [AllowAnonymous]
        [HttpGet]
        public virtual ActionResult Calendario(string ApiKey)
        {
            DateTime d = DateTime.Now;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(ApiKey);
            if (String.IsNullOrEmpty(ApiKey) && User.Identity.IsAuthenticated) IdUtilizador = int.Parse(this.User.Claims.First().Value);
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
            Response.Headers.Add("Content-Disposition", cd.ToString());

            return File(ms, "text/calendar");
        }

        public ActionResult Agendamento(int id, int zona, int tipo)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            if (id != 0) id = context.ObterListaUtilizadores(false, false).Where(u => u.IdPHC == id).FirstOrDefault().Id;

            ViewData["zona"] = zona;
            ViewData["tipo"] = tipo;

            List<Zona> LstZonas = context.ObterZonas();
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

            List<Utilizador> LstTecnicos = context.ObterListaTecnicos(false, true);

            if (zona > 0) LstTecnicos = LstTecnicos.Where(t => t.Zona == zona).ToList();
            if (tipo > 0) LstTecnicos = LstTecnicos.Where(t => t.TipoTecnico == tipo).ToList();

            return View("Calendario", LstTecnicos);
        }

        [Authorize(Roles = "Admin, Escritorio")]
        public JsonResult AlteracaoCalendarioTecnico(int id, string dateOriginal, string date, int idTecnicoOriginal, int idTecnico)
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
            }

            m.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            phccontext.AtualizaMarcacao(m);

            return Json("Ok");
        }
        [Authorize(Roles = "Admin, Escritorio")]
        public JsonResult AlteracaoRapida(int id, string EstadoMarcacaoDesc, int Tecnico, string DataMarcacao)
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

        public JsonResult ObterMarcacoes(DateTime start, DateTime end, int id)
        {

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            List<CalendarioEvent> LstEventos = context.ConverterMarcacoesEventos(phccontext.ObterMarcacoes(context.ObterUtilizador(id).IdPHC, start, end.AddDays(-1)).ToList().OrderBy(m => m.DataMarcacao).ToList()).ToList();
            LstEventos.AddRange(context.ConverterFeriasEventos(context.ObterListaFerias(start, end, id), new List<Feriado>()));

            if (id > 0) return new JsonResult(LstEventos);

            List<Marcacao> LstMarcacoesCriadas = phccontext.ObterMarcacoesCriadas();
            if (LstMarcacoesCriadas.Count > 0)
            {
                List<Marcacao> LstMarcacoesFiltro = new List<Marcacao>();
                int nPerDay = LstMarcacoesCriadas.Count() / 6 == 0 ? 1 : LstMarcacoesCriadas.Count() / 6;
                for (int i = 0; i < 7; i++)
                {
                    LstMarcacoesFiltro.AddRange(LstMarcacoesCriadas.Skip(i * nPerDay).Take(nPerDay).Select(c => { c.DataMarcacao = DateTime.Now.AddDays(i - (int)DateTime.Now.DayOfWeek + 1); return c; }).ToList());
                }

                return new JsonResult(context.ConverterMarcacoesEventos(LstMarcacoesFiltro).ToList());
            }
            return new JsonResult("");
        }

        public ActionResult Print(string id)
        {
            if (id == null) return RedirectToAction("Index");

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            var filePath = Path.GetTempFileName();
            context.DesenharEtiquetaMarcacao(phccontext.ObterMarcacao(int.Parse(id))).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            return File(context.BitMapToMemoryStream(filePath, 810, 504), "application/pdf");
        }

        public ActionResult Index(string numMarcacao, string nomeCliente, string referencia, string tipoe, int idtecnico, string estado)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (!User.IsInRole("Admin") && !User.IsInRole("Escritorio"))
            {
                int IdTecnico = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).IdPHC;
                return RedirectToAction("ListaPedidos", new { IdTecnico, DataPedidos = DateTime.Now.ToShortDateString() });
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

        public ActionResult ListaPedidos(string IdTecnico, string DataPedidos)
        {
            if (DataPedidos == null || DataPedidos == string.Empty) DataPedidos = DateTime.Now.ToString("dd-MM-yyyy");
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterListaTecnicos(true, false).Where(u => u.IdPHC == int.Parse(IdTecnico)).DefaultIfEmpty(new Utilizador()).First();
            List<Marcacao> ListaMarcacoes = phccontext.ObterMarcacoes(u.IdPHC, DateTime.Parse(DataPedidos));

            ViewData["DataPedidos"] = DataPedidos;
            ViewData["IdTecnico"] = u.IdPHC;
            ViewData["IdArmazem"] = u.IdArmazem;

            return View(ListaMarcacoes);
        }

        public ActionResult Lista()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            return RedirectToAction("ListaPedidos", new { IdTecnico = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).IdPHC });
        }

        public ActionResult ListaPedidosPendentes(string IdTecnico)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador u = context.ObterListaTecnicos(true, false).Where(u => u.IdPHC == int.Parse(IdTecnico)).DefaultIfEmpty(new Utilizador()).First();
            List<Marcacao> ListaMarcacoes = phccontext.ObterMarcacoesPendentes(int.Parse(IdTecnico)).OrderBy(m => m.DataMarcacao).ToList();

            ViewData["DataPedidos"] = DateTime.Now.ToString("dd-MM-yyyy");
            ViewData["IdTecnico"] = u.IdPHC;
            ViewData["IdArmazem"] = u.IdArmazem;

            return View("ListaPedidos", ListaMarcacoes);
        }


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

    }
}
