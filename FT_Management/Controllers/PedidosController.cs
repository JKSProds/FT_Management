using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FT_Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FT_Management.Controllers
{
    public class PedidosController : Controller
    {
        // GET: Pedidos
        public ActionResult Index()
        {
            TrelloConector trello = HttpContext.RequestServices.GetService(typeof(TrelloConector)) as TrelloConector;


            return View(trello.ObterQuadros());
        }

        public ActionResult ListaPedidos(string idQuadro, string idlista, string GuiaTransporte)
        {
            TrelloConector trello = HttpContext.RequestServices.GetService(typeof(TrelloConector)) as TrelloConector;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            List<TrelloListas> trelloListas = trello.ObterListas(idQuadro);

            if (idlista == null || idlista == string.Empty)
            {
                foreach (var lista in trelloListas)
                {

                    if (lista.NomeLista.Contains(DateTime.Now.ToString("dd/MM/yyyy")))
                    {
                        idlista = lista.IdLista;
                    }
                }
                if (idlista == null && trelloListas.Count > 0)
                {
                    idlista = trelloListas.First().IdLista;

                }
            }

            string NomeTecnico = trello.ObterQuadros().Where(q => q.IdQuadro == idQuadro).First().NomeQuadro.Replace("Serviços ", "");
            List<Movimentos> LstGuias = context.ObterListaMovimentos(NomeTecnico);
            ViewData["LstGuias"] = LstGuias;
            if (GuiaTransporte == null)
            {
                ViewData["LstGuiasPecas"] = context.ObterListaMovimentos(NomeTecnico, LstGuias.Count > 0 ? LstGuias.Last().GuiaTransporte : "");
                ViewData["LstGuiasSelected"] = LstGuias.Count > 0 ? LstGuias.Last().GuiaTransporte : "";
            }
            else
            {
                ViewData["LstGuiasPecas"] = context.ObterListaMovimentos(NomeTecnico, GuiaTransporte);
                ViewData["LstGuiasSelected"] = GuiaTransporte;
            }

            trelloListas.Where(l => l.IdLista == idlista).FirstOrDefault().ListaCartoes = trello.ObterCartoes(idlista);

            ViewData["SelectedLista"] = idlista;

            return View(trelloListas);
        }
        public ActionResult Pedido(string idCartao)
        {
            if (idCartao == null) return RedirectToAction("Index");

            TrelloConector trello = HttpContext.RequestServices.GetService(typeof(TrelloConector)) as TrelloConector;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            TrelloCartoes cartao = trello.ObterCartao(idCartao);
            if (cartao.IdCartao == null) return RedirectToAction("Index");
            cartao.FolhasObra = context.ObterListaFolhasObraCartao(idCartao);
            cartao.Comentarios = trello.ObterComentarios(idCartao);

            return View(cartao);
        }
        public ActionResult ValidarPedido(string idcartao, string estado)
        {

            TrelloConector trello = HttpContext.RequestServices.GetService(typeof(TrelloConector)) as TrelloConector;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            TrelloCartoes cartao = trello.ObterCartao(idcartao);

            foreach (var folhaObra in context.ObterListaFolhasObraCartao(idcartao))
            {
                if (folhaObra.RelatorioServico != String.Empty && folhaObra.RelatorioServico != null) { trello.NovoComentario(folhaObra.IdCartao, folhaObra.RelatorioServico); }
                trello.NovoAnexo(folhaObra.IdCartao, context.PreencherFormularioFolhaObra(folhaObra).ToArray(), "FolhaObra_" + folhaObra.IdFolhaObra + ".pdf");
            }

            trello.NovaLabel(idcartao, estado == "1" ? "green" : estado == "2" ? "yellow" : "red");
            return RedirectToAction("ListaPedidos", new { idQuadro = cartao.IdQuadro, idlista = cartao.IdLista});
        }
        public ActionResult AdicionarComentario(string idcartao, string comentario)
        {
            TrelloConector trello = HttpContext.RequestServices.GetService(typeof(TrelloConector)) as TrelloConector;

            trello.NovoComentario(idcartao, comentario);
            TrelloComentarios Comentario = trello.ObterComentarios(idcartao).Where(c => c.Comentario.Replace(Environment.NewLine, "") == comentario).First();
            return Json(Comentario);
        }
    }
}
