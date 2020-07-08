using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FT_Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FT_Management.Controllers
{
    public class PedidosController : Controller
    {
        // GET: Pedidos
        public ActionResult Index()
        {
            TrelloConector trello = new TrelloConector();

            return View(trello.ObterQuadros());
        }

        public ActionResult ListaPedidos(string idQuadro, string idlista)
        {
            TrelloConector trello = new TrelloConector();

            List<TrelloListas> trelloListas = trello.ObterListas(idQuadro);

            if (idlista == null)
            {
                foreach (var lista in trelloListas)
                {

                    if (lista.NomeLista.Contains(DateTime.Now.ToString("dd/MM/yyyy")))
                    {
                        lista.ListaCartoes = trello.ObterCartoes(lista.IdLista);
                        idlista = lista.IdLista;
                    }
                }
            }
            else
            {
                trelloListas.Where(l => l.IdLista == idlista).FirstOrDefault().ListaCartoes = trello.ObterCartoes(idlista);
            }

            ViewData["SelectedLista"] = idlista;

            return View(trelloListas);
        }

        public ActionResult Pedido(string idCartao)
        {
            if (idCartao == null) return RedirectToAction("Index");

            TrelloConector trello = new TrelloConector();
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            TrelloCartoes cartao = trello.ObterCartao(idCartao);

            if (cartao.IdCartao == null) return RedirectToAction("Index");
            cartao.FolhasObra = context.ObterListaFolhasObraCartao(idCartao);

            return View(cartao);
        }
    }
}
