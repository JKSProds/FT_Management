using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FT_Management.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        public JsonResult ObterMarcacoes(DateTime start, DateTime end)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            return new JsonResult(context.ConverterMarcacoesEventos(context.ObterListaMarcacoes(start, end)).ToList());

        }

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
        public ActionResult Index(int Tipo)
        {
           FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador user = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            if (!user.Admin) return RedirectToAction("ListaPedidos", new { IdTecnico = user.Id });

            if (Tipo == 1) return View(context.ObterListaTecnicos());

            return View(context.ObterListaComerciais());
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
