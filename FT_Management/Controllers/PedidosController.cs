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

namespace FT_Management.Controllers
{

    [Authorize(Roles = "Admin, Tech, Escritorio")]
    public class PedidosController : Controller
    {
        [Authorize(Roles = "Admin, Escritorio")]
        public ActionResult Adicionar()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            ViewData["Tecnicos"] = context.ObterListaUtilizadores().Where(u => u.TipoUtilizador == 1).ToList();
            ViewData["TipoEquipamento"] = phccontext.ObterTipoEquipamento();
            ViewData["TipoServico"] = phccontext.ObterTipoServico();
            ViewData["Estado"] = phccontext.ObterMarcacaoEstados();
            ViewData["Periodo"] = phccontext.ObterPeriodo();
            ViewData["Prioridade"] = phccontext.ObterPrioridade();
            ViewData["TipoPedido"] = phccontext.ObterTipoPedido();

            return View();
        }

        [Authorize(Roles = "Admin, Escritorio")]
        [HttpPost]
        public ActionResult Adicionar(Marcacao m)
        {
            return RedirectToAction("Index", "Pedidos");
        }

        [HttpPost]
        public JsonResult ObterClientes(string prefix)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            if (prefix is null) prefix = "";

            return Json(phccontext.ObterClientes(prefix, true));
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
                    res += "<tr><td><span>" + item.NomeUtilizador + "</span></td><td><span>" + item.Descricao + "</span></td>";
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

        [HttpGet]
        public virtual ActionResult ObterIcs ()
        {
            DateTime d = DateTime.Now;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            var calendar = new Calendar();
            List<Marcacao> LstMarcacoes = phccontext.ObterMarcacoes(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).IdPHC, DateTime.Now.AddDays(-30), DateTime.Now.AddDays(30));

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
                    Url = new Uri("http://"+Request.Host+"/Pedidos/Pedido?idMarcacao=" + m.IdMarcacao + "&IdTecnico=" + context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).IdPHC)
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
                FileName = "basic.ics",
                Inline = false,
                Size = bytesCalendar.Length,
                CreationDate = DateTime.Now

            };
            Response.Headers.Add("Content-Disposition", cd.ToString());

            return File(ms, "text/calendar");
        }

        [Authorize(Roles = "Admin, Escritorio")]
        public ActionResult Calendario()
        {
            return View();
        }

        public JsonResult ObterMarcacoes(DateTime start, DateTime end)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return new JsonResult(context.ConverterMarcacoesEventos(phccontext.ObterMarcacoes(start, end)).ToList());
        }

        public ActionResult CalendarioView()
        {
            return View();
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

        public ActionResult Index(int IdTecnico)
        {
           FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            if (!User.IsInRole("Admin") && !User.IsInRole("Escritorio"))
            {
                IdTecnico = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).IdPHC;
                return RedirectToAction("ListaPedidos", new { IdTecnico = IdTecnico, DataPedidos = DateTime.Now.ToShortDateString() });
            }

            return View(context.ObterListaTecnicos());
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

        public ActionResult Pedido(string idMarcacao, string IdTecnico)
        {
            if (idMarcacao == null) return RedirectToAction("Index");

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            Marcacao m = phccontext.ObterMarcacao(int.Parse(idMarcacao));
            Utilizador user = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            ViewData["PessoaContacto"] = m.Cliente.PessoaContatoCliente;
            ViewData["SelectedTecnico"] = user.NomeCompleto;
            ViewData["Tecnicos"] = context.ObterListaUtilizadores().Where(u => u.TipoUtilizador != 3).ToList();

            return View(m);
        }
    }
}
