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

            ViewData["Tecnicos"] = context.ObterListaUtilizadores(false).Where(u => u.TipoUtilizador == 1).ToList();
            ViewData["TipoEquipamento"] = phccontext.ObterTipoEquipamento();
            ViewData["TipoServico"] = phccontext.ObterTipoServico();
            ViewData["Estado"] = phccontext.ObterMarcacaoEstados();
            ViewData["Periodo"] = phccontext.ObterPeriodo();
            ViewData["Prioridade"] = phccontext.ObterPrioridade();
            ViewData["TipoPedido"] = phccontext.ObterTipoPedido();

            if (id != 0) return View(phccontext.ObterMarcacao(id));

            return View(new Marcacao());
        }


        [Authorize(Roles = "Admin, Escritorio")]
        [HttpPost]
        public ActionResult ValidarMarcacao(Marcacao m)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            return Content(phccontext.ValidarMarcacao(m));
        }

        [Authorize(Roles = "Admin, Escritorio")]
        [HttpPost]
        public ActionResult Adicionar(Marcacao m)
        {
            int IdMarcacao = 0;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (m.LstTecnicosSelect.Count() == 0)
            {
                ModelState.AddModelError("", "Tem de selecionar pelo menos um técnico!");
            }
            else
            {
                foreach (var item in m.LstTecnicosSelect)
                {
                    m.LstTecnicos.Add(context.ObterUtilizador(item));
                }
                m.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
                m.Tecnico = m.LstTecnicos.First();
                ModelState.Remove("Tecnico.Password");
            }


            if (ModelState.IsValid)
            {
                IdMarcacao = phccontext.CriarMarcacao(m);
                
                if (IdMarcacao>0) return RedirectToAction("Editar", "Pedidos", new { id=IdMarcacao});
            }

            ViewData["Tecnicos"] = context.ObterListaUtilizadores(false).Where(u => u.TipoUtilizador == 1).ToList();
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

            ViewData["Tecnicos"] = context.ObterListaUtilizadores(false).Where(u => u.TipoUtilizador == 1).ToList();
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

            if (m.LstTecnicosSelect.Count() == 0)
            {
                ModelState.AddModelError("", "Tem de selecionar pelo menos um técnico!");
            }
            else
            {
                foreach (var item in m.LstTecnicosSelect)
                {
                    m.LstTecnicos.Add(context.ObterUtilizador(item));
                }
                m.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
                m.Tecnico = m.LstTecnicos.First();
                ModelState.Remove("Tecnico.Password");
            }

            if (m.DatasAdicionais == null) ModelState.AddModelError("", "Tem de adicionar pelo menos uma data!");


            if (ModelState.IsValid)
            {
                phccontext.AtualizaMarcacao(m);
                return RedirectToAction("Editar", "Pedidos", new { id = id });
            }

            ViewData["Tecnicos"] = context.ObterListaUtilizadores(false).Where(u => u.TipoUtilizador == 1).ToList();
            ViewData["TipoEquipamento"] = phccontext.ObterTipoEquipamento();
            ViewData["TipoServico"] = phccontext.ObterTipoServico();
            ViewData["Estado"] = phccontext.ObterMarcacaoEstados();
            ViewData["Periodo"] = phccontext.ObterPeriodo();
            ViewData["Prioridade"] = phccontext.ObterPrioridade();
            ViewData["TipoPedido"] = phccontext.ObterTipoPedido();

            return View(m);
        }

        [HttpPost]
        public ActionResult AdicionarComentario(int id, string comentario, int fechar)
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

            phccontext.CriarComentarioMarcacao(c);

            if (fechar == 1) {
                m.JustificacaoFecho = comentario;
                m.EstadoMarcacaoDesc = "Finalizado";
                m.Utilizador = c.Utilizador;
                phccontext.AtualizaMarcacao(m);
            }

            return Content("Sucesso");
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

            if (!MailContext.EnviarEmailMarcacaoCliente(email, m)) return Content("Erro");

            return Content("Sucesso");
        }

        [HttpPost]
        public JsonResult ObterClientes(string prefix)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (prefix is null) prefix = "";

            return Json(phccontext.ObterClientes(prefix, true));
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
            res += "<div class=\"mb-3\"><label>Cliente</label><div class=\"input-group\"><input type=\"text\" class=\"form-control\" value='" + m.Cliente.NomeCliente + "' readonly><a class=\"btn btn-outline-warning\"  onclick=\"location.href = '/Clientes/Cliente?IdCliente=" + m.Cliente.IdCliente+"&IdLoja="+m.Cliente.IdLoja+"'\" type=\"button\"><i class=\"fas fa-eye float-left\" style=\"margin-top:5px\"></i></a></div></div>";
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
                    res += "<tr><td onclick=\"location.href = '/FolhasObra/Detalhes/" + item.IdFolhaObra + "'\"><span>" + item.DataServico.ToShortDateString() + "</span></td><td onclick=\"location.href = '/FolhasObra/Detalhes/" + item.IdFolhaObra + "'\"><span>" + item.ClienteServico.NomeCliente + "</span></td><td onclick=\"location.href = '/FolhasObra/Detalhes/" + item.IdFolhaObra + "'\"><span>" + item.EquipamentoServico.NumeroSerieEquipamento + "</span></td>";
                }

                res += "</tbody></table>";
            }
            return res;
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
            if (IdUtilizador == 0) return File("", "");

            var calendar = new Calendar();
            List<Marcacao> LstMarcacoes = phccontext.ObterMarcacoes(context.ObterUtilizador(IdUtilizador).IdPHC, DateTime.Now.AddDays(-30), DateTime.Now.AddDays(30));

            foreach (Marcacao m in LstMarcacoes)
            {
                if (d.ToShortDateString() != m.DataMarcacao.ToShortDateString()) d= m.DataMarcacao.Add(TimeSpan.FromHours(8));
                var e = new CalendarEvent
                {
                    Start = new CalDateTime(d),
                    End = new CalDateTime(d.AddMinutes(30)),
                    LastModified = new CalDateTime(DateTime.Now),
                    Uid =  m.IdMarcacao.ToString(),
                    Description = "### Estado do Pedido: " + m.EstadoMarcacaoDesc + " ###" + Environment.NewLine + Environment.NewLine + m.ResumoMarcacao,
                    Summary = (m.EstadoMarcacao == 4 ? "✔ " : m.EstadoMarcacao != 1 && m.EstadoMarcacao != 5 ? "⌛ " : m.DataMarcacao < DateTime.Now ? "❌ " : "") + m.Cliente.NomeCliente,
                    Url = new Uri("http://"+Request.Host+"/Pedidos/Pedido?id=" + m.IdMarcacao + "&IdTecnico=" + context.ObterUtilizador(IdUtilizador).IdPHC)
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

        public ActionResult CalendarioView(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            if (id != 0) id = context.ObterListaUtilizadores(false).Where(u => u.IdPHC == id).FirstOrDefault().Id;
            if (!User.IsInRole("Admin") && !User.IsInRole("Escritorio") || id != 0)
            {
                if (id == 0) id = int.Parse(this.User.Claims.First().Value);
                Utilizador u = context.ObterUtilizador(id);
                return View("CalendarioTecnico", u);
            }
            return View("CalendarioNew");
        }

        [Authorize(Roles = "Admin, Escritorio")]
        public JsonResult AlteracaoCalendario(int id, DateTime date, DateTime dateOriginal)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Marcacao m = phccontext.ObterMarcacao(id);
            m.DatasAdicionais = m.DatasAdicionais.Replace(dateOriginal.ToString("yyyy-MM-dd"), date.ToString("yyyy-MM-dd"));

            phccontext.AtualizaMarcacao(m);

            return Json("Ok");
        }

        public JsonResult ObterMarcacoes(DateTime start, DateTime end, int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            if (id > 0) return new JsonResult(context.ConverterMarcacoesEventos(phccontext.ObterMarcacoes(context.ObterUtilizador(id).IdPHC, start, end).ToList().OrderBy(m => m.DataMarcacao).ToList()).ToList());
            return new JsonResult(context.ConverterMarcacoesEventos(phccontext.ObterMarcacoes(start, end).OrderBy(m => m.Tecnico.Id).ToList().OrderBy(m => m.DataMarcacao).ToList()).ToList());
        }

        public ActionResult Print(string id)
        {
            if (id == null) return RedirectToAction("Index");

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            var filePath = Path.GetTempFileName();
            context.DesenharEtiquetaMarcacao(phccontext.ObterMarcacao(int.Parse(id))).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            return File(context.BitMapToMemoryStream(filePath), "application/pdf");
        }

        public ActionResult Index(string numMarcacao, string nomeCliente, string referencia, string tipoe, int idtecnico)
        {
           FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (!User.IsInRole("Admin") && !User.IsInRole("Escritorio"))
            {
                int IdTecnico = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).IdPHC;
                return RedirectToAction("ListaPedidos", new { IdTecnico, DataPedidos = DateTime.Now.ToShortDateString() });
            }

            List<Utilizador> LstUtilizadores = context.ObterListaTecnicos(false).ToList();
            LstUtilizadores.Insert(0, new Utilizador() { Id = 0, NomeCompleto = "Todos" });
            ViewBag.ListaTecnicos = LstUtilizadores;

            List<String> LstTipoEquipamento = phccontext.ObterTipoEquipamento().ToList();
            LstTipoEquipamento.Insert(0, "Todos");

            ViewBag.TipoEquipamento = LstTipoEquipamento.Select(l => new SelectListItem() { Value = l, Text = l });

            if (string.IsNullOrEmpty(nomeCliente)) { nomeCliente = ""; }
            if (string.IsNullOrEmpty(referencia)) { referencia = ""; }
            if (string.IsNullOrEmpty(tipoe)) { tipoe = ""; }
            if (string.IsNullOrEmpty(numMarcacao)) { numMarcacao = ""; }

            ViewData["numMarcacao"] = numMarcacao;
            ViewData["nomeCliente"] = nomeCliente;
            ViewData["referencia"] = referencia;
            ViewData["tipoe"] = tipoe;
            ViewData["idtecnico"] = idtecnico;

            if (string.IsNullOrEmpty(numMarcacao) && string.IsNullOrEmpty(nomeCliente) && string.IsNullOrEmpty(referencia) && string.IsNullOrEmpty(tipoe) && idtecnico == 0) return View(phccontext.ObterMarcacoes(DateTime.Now, DateTime.Now.AddDays(1)));

            return View(phccontext.ObterMarcacoes(int.Parse(numMarcacao != "" ? numMarcacao : "0"), nomeCliente, referencia, tipoe, idtecnico));
        }

        public ActionResult ListaPedidos(string IdTecnico, string DataPedidos)
        {
            if (DataPedidos == null || DataPedidos == string.Empty) DataPedidos = DateTime.Now.ToString("dd-MM-yyyy");

            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            List<Marcacao> ListaMarcacoes = phccontext.ObterMarcacoes(int.Parse(IdTecnico), DateTime.Parse(DataPedidos));
           
            ViewData["DataPedidos"] = DataPedidos;
            ViewData["IdTecnico"] = IdTecnico;

            return View(ListaMarcacoes);
        }

        public ActionResult ListaPedidosPendentes(string IdTecnico)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            List<Marcacao> ListaMarcacoes = phccontext.ObterMarcacoesPendentes(int.Parse(IdTecnico)).OrderBy(m => m.DataMarcacao).ToList();
 
            ViewData["DataPedidos"] = DateTime.Now.ToString("dd-MM-yyyy");
            ViewData["IdTecnico"] = IdTecnico;
   
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
            ViewData["Tecnicos"] = context.ObterListaUtilizadores(true).Where(u => u.TipoUtilizador != 3).ToList();

            return View(m);
        }
    }
}
