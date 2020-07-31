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
        // GET: Pedidos
        public ActionResult Index()
        {
            TrelloConector trello = HttpContext.RequestServices.GetService(typeof(TrelloConector)) as TrelloConector;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Utilizador user = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            if (user.IdCartaoTrello != null && user.IdCartaoTrello != string.Empty) return RedirectToAction("ListaPedidos", new { idQuadro = user.IdCartaoTrello });
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

            Utilizador Tecnico = context.ObterUtilizadorNome(trello.ObterQuadros().Where(q => q.IdQuadro == idQuadro).First().NomeQuadro.Replace("Serviços ", ""));
               
            List<Movimentos> LstGuias = context.ObterListaMovimentos(Tecnico.Id);
            ViewData["LstGuias"] = LstGuias;
            if (GuiaTransporte == null)
            {
                ViewData["LstGuiasPecas"] = context.ObterListaMovimentos(Tecnico.Id, LstGuias.Count > 0 ? LstGuias.Last().GuiaTransporte : "");
                ViewData["LstGuiasSelected"] = LstGuias.Count > 0 ? LstGuias.Last().GuiaTransporte : "";
            }
            else
            {
                ViewData["LstGuiasPecas"] = context.ObterListaMovimentos(Tecnico.Id, GuiaTransporte);
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
            ViewData["PessoaContacto"] = context.ObterClienteNome(cartao.NomeCartao).PessoaContatoCliente;

            Utilizador user = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            ViewData["SelectedTecnico"] = user.NomeCompleto;
            ViewData["Tecnicos"] = context.ObterListaUtilizadores().Where(u => u.TipoUtilizador != "3").ToList();



            if (cartao.IdCartao == null) return RedirectToAction("Index");
            cartao.FolhasObra = context.ObterListaFolhasObraCartao(idCartao);

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
        public ActionResult AdicionarComentario(string idcartao, string comentario)
        {
            TrelloConector trello = HttpContext.RequestServices.GetService(typeof(TrelloConector)) as TrelloConector;

            trello.NovoComentario(idcartao, comentario);
            TrelloComentarios Comentario = trello.ObterComentarios(idcartao).Where(c => c.Comentario.Replace(Environment.NewLine, "") == comentario).First();
            return Json(Comentario);
        }
        [HttpPost("AdicionarAnexo")]
        public  ActionResult AdicionarAnexo(List<IFormFile> files, string idcartao)
        {
            TrelloConector trello = HttpContext.RequestServices.GetService(typeof(TrelloConector)) as TrelloConector;

            foreach (var formFile in files)
            {
                if (formFile.Length > 0 )
                {
                    using var ms = new MemoryStream();
                    formFile.CopyTo(ms);
                    var fileBytes = ms.ToArray();

                    TrelloAnexos Anexo = new TrelloAnexos
                    {
                        Id = idcartao,
                        Name = formFile.FileName,
                        File = fileBytes,
                    };

                    if (Anexo.Name.Split('.').Count() > 1)
                    {
                        Anexo.dict.TryGetValue("." + Anexo.Name.Split('.').Last().ToString(), out string mimeType);
                        Anexo.MimeType = mimeType;
                    }
                    else
                    {
                        Anexo.MimeType = "application/pdf";
                    }

                    trello.NovoAnexo(Anexo);
                }
            }

            return RedirectToAction("Pedido", new { idCartao = idcartao });
        }

        public virtual ActionResult DescarregarAnexo(string id, string idcartao)
        {
            TrelloConector trello = HttpContext.RequestServices.GetService(typeof(TrelloConector)) as TrelloConector;

            TrelloAnexos Anexo = trello.ObterAnexo(id, idcartao);
            var file = Anexo.File.ToArray();
            var output = new MemoryStream();
            output.Write(file, 0, file.Length);
            output.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = Uri.EscapeDataString(Anexo.Name),
                Inline = false,
                Size = file.Length,
                CreationDate = DateTime.Now,

            };

            Response.Headers.Add("Content-Disposition", cd.ToString());
            return File(output, Anexo.MimeType);
        }
        public virtual ActionResult AssinarDocumento(string cartaoid, string idanexo, string nometecnico, string nomecliente, string tipodocumento, string manualentregue)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            TrelloConector trello = HttpContext.RequestServices.GetService(typeof(TrelloConector)) as TrelloConector;
            TrelloAnexos Anexo = trello.ObterAnexo(idanexo, cartaoid);

            Anexo.File = context.AssinarDocumento(nomecliente, nometecnico, tipodocumento, manualentregue == "true", Anexo.File).ToArray();
            Anexo.Id = cartaoid;
            Anexo.Name = Anexo.Name.Contains("Assinada_") ? Anexo.Name : "Assinada_" + Anexo.Name;
            Anexo.Date = DateTime.Parse(DateTime.Now.ToString("dd-MM-yyyy HH:mm"));

            trello.NovoAnexo(Anexo);
            return Json(Anexo);
        }


    }
}
