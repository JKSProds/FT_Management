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

namespace FT_Management.Controllers
{

    [Authorize(Roles = "Admin, Tech, Escritorio")]
    public class PedidosController : Controller
    {
        public JsonResult ObterMarcacoes(DateTime start, DateTime end)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            return new JsonResult(context.ConverterMarcacoesEventos(context.ObterListaMarcacoes(start, end)).ToList());

        }

        public virtual ActionResult ObterIcs ()
        {
            DateTime d = DateTime.Now;
            int h = 9;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            var calendar = new Calendar();
            List<Marcacao> LstMarcacoes = context.ObterListaMarcacoes(33, DateTime.Now.AddDays(-30), DateTime.Now.AddDays(30));
            foreach (Marcacao m in LstMarcacoes)
            {
                if (d.ToShortDateString() != m.DataMarcacao.ToShortDateString()) h = 9;
                d = m.DataMarcacao.Add(TimeSpan.FromHours(h));
                var e = new CalendarEvent
                {
                    Start = new CalDateTime(d),
                    End = new CalDateTime(d.AddMinutes(30)),
                    Uid = m.IdMarcacao.ToString(),
                    Description = m.ResumoMarcacao,
                    Summary = m.Cliente.NomeCliente
                };

                calendar.Events.Add(e);
                h += 1;
            }

            var serializer = new CalendarSerializer();

            var serializedCalendar = serializer.SerializeToString(calendar);

            var bytesCalendar = Encoding.ASCII.GetBytes(serializedCalendar);
            MemoryStream ms = new MemoryStream(bytesCalendar);

            ms.Write(bytesCalendar, 0, bytesCalendar.Length);
            ms.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "basic.ics",
                Inline = false,
                Size = bytesCalendar.Length,
                CreationDate = DateTime.Now,

            };
            Response.Headers.Add("Content-Disposition", cd.ToString());

            return File(ms, "text/calendar");
        }

        [Authorize(Roles = "Admin, Escritorio")]
        public ActionResult Calendario()
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            phccontext.AtualizarMarcacoes();

            return View();
        }
        public ActionResult CalendarioView()
        {
            return View();
        }

        
        // GET: Pedidos
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
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            phccontext.AtualizarMarcacoes();

            if (DataPedidos == null || DataPedidos == string.Empty) DataPedidos = DateTime.Now.ToString("dd-MM-yyyy");
            ViewData["DataPedidos"] = DataPedidos;

            List<Marcacao> ListaMarcacoes = context.ObterListaMarcacoes(int.Parse(IdTecnico), DateTime.Parse(DataPedidos), DateTime.Parse(DataPedidos));
            ViewData["IdTecnico"] = IdTecnico;
            return View(ListaMarcacoes);
        }
        public ActionResult Pedido(string idMarcacao, string IdTecnico)
        {
            if (idMarcacao == null) return RedirectToAction("Index");

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            phccontext.AtualizarMarcacoes();
            phccontext.AtualizarFolhasObra();

            Marcacao marcacao = context.ObterMarcacao(int.Parse(idMarcacao));
            marcacao.IdTecnico = int.Parse(IdTecnico);

            ViewData["PessoaContacto"] = marcacao.Cliente.PessoaContatoCliente;

            Utilizador user = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            ViewData["SelectedTecnico"] = user.NomeCompleto;
            ViewData["Tecnicos"] = context.ObterListaUtilizadores().Where(u => u.TipoUtilizador != 3).ToList();

            return View(marcacao);
        }
        public ActionResult ValidarPedido(string idcartao, string estado)
        {

            TrelloConector trello = HttpContext.RequestServices.GetService(typeof(TrelloConector)) as TrelloConector;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            TrelloCartoes cartao = trello.ObterCartao(idcartao);

            foreach (var folhaObra in context.ObterListaFolhasObraCartao(idcartao))
            {
                //if (folhaObra.RelatorioServico != String.Empty && folhaObra.RelatorioServico != null) { trello.NovoComentario(folhaObra.IdCartao, folhaObra.RelatorioServico); }
                TrelloAnexos Anexo = new TrelloAnexos
                {
                    Id = folhaObra.IdCartao,
                    Name = "FolhaObra_" + folhaObra.IdFolhaObra + ".pdf",
                    File = context.PreencherFormularioFolhaObra(folhaObra).ToArray(),
                };
                Anexo.dict.TryGetValue(Anexo.Name.Split('.').Last(), out string mimeType);
                Anexo.MimeType = mimeType;

                trello.NovoAnexo(Anexo);
            }

            trello.NovaLabel(idcartao, estado == "1" ? "green" : estado == "2" ? "yellow" : "red");
            return RedirectToAction("ListaPedidos", new { idQuadro = cartao.IdQuadro, idlista = cartao.IdLista});
        }

    }
}
