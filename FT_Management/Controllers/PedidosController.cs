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
        public JsonResult ObterMarcacoes(DateTime start, DateTime end)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            return new JsonResult(context.ConverterMarcacoesEventos(context.ObterListaMarcacoes(start, end)).ToList());

        }

        [HttpPost]
        public string ObterPedido(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Marcacao marcacao = context.ObterMarcacao(id);
            string res = "";
            res += "<div class=\"mb-3\"><label>Cliente</label><div class=\"input-group\"><input type=\"text\" class=\"form-control\" value='" + marcacao.Cliente.NomeCliente + "' readonly><a class=\"btn btn-outline-warning\"  onclick=\"location.href = '/Clientes/Cliente?IdCliente=" + marcacao.Cliente.IdCliente+"&IdLoja="+marcacao.Cliente.IdLoja+"'\" type=\"button\"><i class=\"fas fa-eye float-left\" style=\"margin-top:5px\"></i></a></div></div>";
            res += "<div class=\"mb-3\"><label>Detalhes</label><textarea type=\"text\" class=\"form-control\" rows=\"12\" readonly>" + marcacao.ResumoMarcacao + "</textarea></div>";

            if (marcacao.LstFolhasObra.Count() > 0)
            {
                res += "<table class=\"table table-hover\"><thead><tr><th>Data</th><th>Cliente</th><th>Nº Série</th><tbody>";

                foreach (var item in marcacao.LstFolhasObra)
                {
                    res += "<tr><td onclick=\"location.href = '/FolhasObra/Editar/" + item.IdFolhaObra + "'\"><span>" + item.DataServico.ToShortDateString() + "</span></td><td onclick=\"location.href = '/FolhasObra/Editar/" + item.IdFolhaObra + "'\"><span>" + item.ClienteServico.NomeCliente + "</span></td><td onclick=\"location.href = '/FolhasObra/Editar/" + item.IdFolhaObra + "'\"><span>" + item.EquipamentoServico.NumeroSerieEquipamento + "</span></td>";
                }

                res += "</tbody></table>";
            }
            //res += "<div class=\"mb-3\"><label>Cargo</label><input type=\"text\" class=\"form-control\" value='" + contacto.CargoPessoaContacto + "' readonly></div>";
            //res += "<div class=\"mb-3\"><label>Email</label><div class=\"input-group\"><input type=\"text\" class=\"form-control\" value='" + contacto.EmailContacto + "' readonly><a class=\"btn btn-outline-secondary " + (contacto.EmailContacto.Length == 0 ? "disabled" : "") + " \" href=\"mailto:" + contacto.EmailContacto + "\" type=\"button\"><i class=\"fas fa-envelope float-left\" style=\"margin-top:5px\"></i></a></div></div>";
            //res += "<div class=\"mb-3\"><label>Telemóvel</label><div class=\"input-group\"><input type=\"text\" class=\"form-control\" value='" + contacto.TelefoneContacto + "' readonly><a class=\"btn btn-outline-secondary " + (contacto.TelefoneContacto.Length < 9 ? "disabled" : "") + " \" href=\"tel:" + contacto.TelefoneContacto + "\" type=\"button\"><i class=\"fas fa-phone-alt float-left\" style=\"margin-top:5px\"></i></a></div></div>";
            //res += "<div class=\"mb-3\"><label>Morada</label><div class=\"input-group\"><input type=\"text\" class=\"form-control\" value='" + contacto.MoradaContacto + "' readonly><a class=\"btn btn-outline-secondary " + (contacto.MoradaContacto.Length == 0 ? "disabled" : "") + " \" href=\"https://maps.google.com/?daddr=" + contacto.MoradaContacto + "\" type=\"button\"><i class=\"fas fa-location-arrow float-left\" style=\"margin-top:5px\"></i></a></div></div>";
            //res += "<div class=\"mb-3\"><label>NIF</label><input type=\"text\" class=\"form-control\" value='" + contacto.NIFContacto + "' readonly></div>";
            //res += "<div class=\"mb-3\"><label>Data de Contacto</label><input type=\"text\" class=\"form-control\" value='" + contacto.DataContacto.ToShortDateString() + "' readonly></div>";
            //res += "<div class=\"mb-3\"><label>Tipo de Contacto</label><input type=\"text\" class=\"form-control\" value='" + contacto.TipoContacto + "' readonly></div>";
            //res += "<div class=\"mb-3\"><label>Área de Negócio</label><input type=\"text\" class=\"form-control\" value='" + contacto.AreaNegocio + "' readonly></div>";
            //res += "<div class=\"mb-3\"><label>Criado por</label><input type=\"text\" class=\"form-control\" value='" + contacto.Utilizador.NomeCompleto + "' readonly></div>";
            //if (this.User.IsInRole("Admin")) res += "<div class=\"mb-3\"><label>Comercial Associado</label><div class=\"input-group\"><input type=\"text\" class=\"form-control\" value='" + contacto.Comercial.NomeCompleto + "' readonly><a class=\"btn btn-outline-primary\" onclick=AssociarComercial() type=\"button\"><i class=\"fas fa-list float-left\" style=\"margin-top:5px\"></i></a></div></div>";

            return res;
        }



        [HttpGet]
        public virtual ActionResult ObterIcs ()
        {
            DateTime d = DateTime.Now;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            phccontext.AtualizarMarcacoes();

            var calendar = new Calendar();
            List<Marcacao> LstMarcacoes = context.ObterListaMarcacoes(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).IdPHC, DateTime.Now.AddDays(-30), DateTime.Now.AddDays(30)).OrderBy(m => m.DataMarcacao).ToList();
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
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            phccontext.AtualizarMarcacoes();

            return View();
        }
        public ActionResult CalendarioView()
        {
            return View();
        }


        public MemoryStream BitMapToMemoryStream(string filePath)
        {
            var ms = new MemoryStream();

            PdfDocument doc = new PdfDocument();
            PdfPage page = new PdfPage
            {
                Width = 810,
                Height = 504
            };

            XImage img = XImage.FromFile(filePath);
            img.Interpolate = false;

            doc.Pages.Add(page);

            XGraphics xgr = XGraphics.FromPdfPage(doc.Pages[0]);
            XRect box = new XRect(0, 0, 810, 504);
            xgr.DrawImage(img, box);

            doc.Save(ms, false);

            System.IO.File.Delete(filePath);

            return ms;

        }


        public ActionResult Print(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            var filePath = Path.GetTempFileName();
            context.DesenharEtiquetaMarcacao(context.ObterMarcacao(int.Parse(id))).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            context.AdicionarLog(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).NomeUtilizador, "Impressa etiqueta normal da marcação: " + id, 2);
            //return File(outputStream, "image/bmp");
            return File(BitMapToMemoryStream(filePath), "application/pdf");
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
        public ActionResult ListaPedidosPendentes(string IdTecnico)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            phccontext.AtualizarMarcacoes();

            ViewData["DataPedidos"] = DateTime.Now.ToString("dd-MM-yyyy");

            List<Marcacao> ListaMarcacoes = context.ObterListaMarcacoesPendentes(int.Parse(IdTecnico));
            ViewData["IdTecnico"] = IdTecnico;
            return View("ListaPedidos", ListaMarcacoes);
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
