using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using FT_Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Custom;
using Microsoft.AspNetCore.Authorization;

namespace FT_Management.Controllers
{
    [Authorize]
    public class FolhasObraController : Controller
    {
        // GET: FolhasObraController
        public ActionResult Index(string DataFolhasObra)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            if (context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).TipoUtilizador == "2") return RedirectToAction("Index", "Pedidos");
            if (DataFolhasObra == null || DataFolhasObra == string.Empty) DataFolhasObra = DateTime.Now.ToString("dd-MM-yyyy");
            ViewData["DataFolhasObra"] = DataFolhasObra;
            return View(context.ObterListaFolhasObra(DateTime.Parse(DataFolhasObra).ToString("yyyy-MM-dd")));
        }

        

        // GET: FolhasObraController/Create
        public ActionResult Adicionar(string idCartao)
        {
            TrelloConector trello = HttpContext.RequestServices.GetService(typeof(TrelloConector)) as TrelloConector;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            if (idCartao == null) idCartao = "";
            ViewData["IdCartao"] = idCartao;
            TrelloCartoes cartao = trello.ObterCartao(idCartao);

            string nSerie = TrelloConector.GetBetween(cartao.DescricaoCartao.ToUpper(), "SERIAL NUMBER:", Environment.NewLine).Trim();
            if (nSerie == "") { nSerie = TrelloConector.GetBetween(cartao.DescricaoCartao.ToUpper(), "N/S:", Environment.NewLine).Trim(); }
            if (nSerie == "") { nSerie = TrelloConector.GetBetween(cartao.DescricaoCartao.ToUpper(), "S/N:", Environment.NewLine).Trim(); }
 
            string ticketNumero = TrelloConector.GetBetween(cartao.DescricaoCartao.ToUpper(), "TICKET#", "\\]").Replace(@"\", "");
            if (ticketNumero == "") { ticketNumero = TrelloConector.GetBetween(cartao.DescricaoCartao.ToUpper(), "INC", " "); }
            if (ticketNumero == "") { ticketNumero = TrelloConector.GetBetween(cartao.DescricaoCartao.ToUpper(), "TICKET#", Environment.NewLine).Replace(@"\", ""); }
            if (ticketNumero == "") { ticketNumero = TrelloConector.GetBetween(cartao.DescricaoCartao.ToUpper(), "OT VINCULADA N°", "PROCEDENTE").Replace(@"\", "").Trim(); }

            FolhaObra folha = new FolhaObra
            {
                ReferenciaServico = ticketNumero,
                ClienteServico = context.ObterClienteNome(cartao.NomeCartao.Trim()),
                EquipamentoServico = context.ObterEquipamentoNS(nSerie),
                PecasServico = new List<Produto>(),
                IntervencaosServico = new List<Intervencao>(),
                IdCartao = idCartao
            };

            folha.ConferidoPor = folha.ClienteServico.PessoaContatoCliente;

            return View(folha);
        }



        // POST: FolhasObraController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Adicionar(FolhaObra folhaObra, string IdCartao)
        {
            if (folhaObra.EquipamentoServico.NumeroSerieEquipamento == null || folhaObra.ClienteServico.NomeCliente == null) return View(folhaObra);
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            folhaObra.DataServico = DateTime.Parse(DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day);
            folhaObra.IdCartao = IdCartao ?? "";
            if (folhaObra.ClienteServico.PessoaContatoCliente != string.Empty && folhaObra.ClienteServico.PessoaContatoCliente != null) folhaObra.ConferidoPor = folhaObra.ClienteServico.PessoaContatoCliente;
            folhaObra.ClienteServico.PessoaContatoCliente = folhaObra.ConferidoPor;
            folhaObra.GuiaTransporteAtual = "GT" + DateTime.Now.Year + "BO91/";
            return RedirectToAction("Editar", new { id = context.NovaFolhaObra(folhaObra) });

        }

        [HttpPost]
        public ActionResult AdicionarIntervencao(string data, string horainicio, string horafim, string tecnico, string idfolhaobra)
        {
 
                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                Intervencao intervencao = new Intervencao()
                {
                    IdFolhaObra = int.Parse(idfolhaobra),
                    NomeTecnico = tecnico,
                    DataServiço = DateTime.Parse(data),
                    HoraInicio = DateTime.Parse(horainicio),
                    HoraFim = DateTime.Parse(horafim),
                    IdTecnico = int.Parse(this.User.Claims.First().Value)
                };

                return Content(context.NovaIntervencao(intervencao).ToString());

        }

        [HttpPost]
        public ActionResult AdicionarPeca(string referencia, string designacao, string quantidade, string idfolhaobra)
        {
                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                Produto produto = new Produto()
                {
                    Ref_Produto = referencia,
                    Designacao_Produto = designacao,
                    Stock_Fisico = int.Parse(quantidade)
                };
                context.NovaPecaIntervencao(produto, idfolhaobra);

                return Content(referencia);
        }

        public JsonResult ObterDesignacaoProduto (string RefProduto)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return Json(new { result = context.ObterProduto(RefProduto).Designacao_Produto });
        }

        public JsonResult ObterEquipamento(string NumeroSerieEquipamento)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return Json(new { json = context.ObterEquipamentoNS(NumeroSerieEquipamento) });
        }
        public JsonResult ObterHistorico(string NumeroSerieEquipamento)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return Json(new { json = context.ObterHistorico(NumeroSerieEquipamento) });
        }
        public JsonResult ObterCliente(string NomeCliente)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return Json(new { json = context.ObterListaClientes(NomeCliente, false).FirstOrDefault() }) ;
        }
        public JsonResult ObterEmailClienteFolhaObra(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return Json(context.ObterFolhaObra(id).ClienteServico.EmailCliente);
        }

        public JsonResult AtualizarEmailClienteFolhaObra(int id, string novoemail)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Cliente cliente = context.ObterFolhaObra(id).ClienteServico;
            cliente.EmailCliente = novoemail;
            context.NovoCliente(cliente);
            return Json("");
        }
        public virtual ActionResult PrintFolhaObra(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
           

            var file = context.PreencherFormularioFolhaObra(context.ObterFolhaObra(id)).ToArray();
            var output = new MemoryStream();
            output.Write(file, 0, file.Length);
            output.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "FolhaObra_"+ id +".pdf",
                Inline = false,
                Size = file.Length,
                CreationDate = DateTime.Now,
                
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            return File(output, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }
        // GET: FolhasObraController/Edit/5
        public ActionResult Editar(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            TrelloConector trello = HttpContext.RequestServices.GetService(typeof(TrelloConector)) as TrelloConector;
            string IdQuadro = trello.ObterCartao(context.ObterFolhaObra(id).IdCartao).IdQuadro;
            Utilizador user = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            ViewData["SelectedTecnico"] = user.NomeCompleto;
            ViewData["Tecnicos"] = context.ObterListaUtilizadores().Where(u => u.TipoUtilizador != "3").ToList();

            FolhaObra folhaObra = context.ObterFolhaObra(id);
            if (user.TipoUtilizador == "2" && !(folhaObra.IntervencaosServico.Where(t => t.IdTecnico == user.Id).Count() > 0 || folhaObra.IntervencaosServico.Count == 0)) return RedirectToAction("Index", "Pedidos");

            if (folhaObra.ConferidoPor == string.Empty || folhaObra.ConferidoPor == null) folhaObra.ConferidoPor = folhaObra.ClienteServico.PessoaContatoCliente;

            return View(folhaObra);
        }

        // POST: FolhasObraController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(int id, FolhaObra folhaObra)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            folhaObra.IdFolhaObra = id;
            folhaObra.ClienteServico.PessoaContatoCliente = folhaObra.ConferidoPor;
            context.NovaFolhaObra(folhaObra);
            return RedirectToAction("Pedido", "Pedidos", new { idCartao = folhaObra.IdCartao});

        }

        public ActionResult EmailFolhaObra(int id, string emailDestino)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            try
            {
                Console.WriteLine("Sending email");
                SmtpClient mySmtpClient = new SmtpClient(ConfigurationManager.AppSetting["Email:ClienteSMTP"])
                {
                    UseDefaultCredentials = false
                };

                System.Net.NetworkCredential basicAuthenticationInfo = new
                   System.Net.NetworkCredential(ConfigurationManager.AppSetting["Email:EmailOrigem"], ConfigurationManager.AppSetting["Email:SenhaEmailOrigem"]);
                mySmtpClient.Credentials = basicAuthenticationInfo;

                // add from,to mailaddresses
                MailAddress from = new MailAddress(ConfigurationManager.AppSetting["Email:EmailOrigem"], ConfigurationManager.AppSetting["Email:NomeOrigem"]);
                MailAddress to = new MailAddress(emailDestino);
                MailMessage myMail = new MailMessage(from, to)
                {
                    Subject = "Folha de Obra - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    SubjectEncoding = System.Text.Encoding.UTF8
                };

                if (int.Parse(DateTime.Now.ToString("dd")) < 13) {
                    myMail.Body = "Bom Dia, ";
                } else
                {
                    myMail.Body = "Boa Tarde, ";
                }

                // set body-message and encoding
                myMail.Body += "<br><br>Segue em anexo a folha de obra de acordo com o serviço realizado.<br><br><b>Atenção este é um email automático, por favor não responda a este email!</b><br><br><p style='margin:0cm;margin-bottom:.0001pt;font-size:15px;font-family:'Calibri',sans-serif;'><strong><span style='font-size:11px;font-family:'Rubik-Regular',serif;color:#0069A5;'><a href='http://www.food-tech.pt/'><span style='color:#0563C1;'>www.food-tech.pt</span></a></span></strong></p><p style='margin:0cm;margin-bottom:.0001pt;font-size:15px;font-family:'Calibri',sans-serif;'>&nbsp;</p><p style='margin:0cm;margin-bottom:.0001pt;font-size:15px;font-family:'Calibri',sans-serif;'><img width='250' src='data:image/png;base64,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' alt='image'></p><p style='margin:0cm;margin-bottom:.0001pt;font-size:15px;font-family:'Calibri',sans-serif;'><strong><span style='font-size:11px;font-family:'Rubik-Regular',serif;color:#0069A5;'>Subic, Lda</span></strong><strong><span style='font-size:11px;font-family:'Cambria',serif;color:#0069A5;'>&nbsp;</span></strong></p><p style='margin:0cm;margin-bottom:.0001pt;font-size:15px;font-family:'Calibri',sans-serif;'><em><span style='font-size:11px;font-family:'Rubik-Regular',serif;color:#0069A5;'>Grupo Miranda &amp; Serra, S.A.</span></em></p><p style='margin:0cm;margin-bottom:.0001pt;font-size:15px;font-family:'Calibri',sans-serif;'><span style='font-size:11px;font-family:'Rubik-Regular',serif;color:black;'><a href='x-apple-data-detectors%3A//2/1' style='color:var(--linkColor);'>Rua Eng. Sabino Marques, 144</a>, Zona Industrial da Maia Sector II,</span><span style='font-size:11px;font-family:'Cambria',serif;color:black;'>&nbsp;</span></p><p style='margin:0cm;margin-bottom:.0001pt;font-size:15px;font-family:'Calibri',sans-serif;'><span style='font-size:11px;font-family:'Rubik-Regular',serif;color:black;'><a href='tel:4470-605' style='color:var(--linkColor);'>4470-605</a> Maia, Portugal &bull; Tel. <a href='tel:(+351)%20229%20479%20670' style='color:var(--linkColor);'>(+351) 229 479</a></span><span style='font-size:11px;font-family:'Cambria',serif;color:black;'>&nbsp;</span><span style='font-size:11px;font-family:'Rubik-Regular',serif;color:black;'><a href='tel:(+351)%20229%20479%20670' style='color:var(--linkColor);'>670</a></span></p>";
                myMail.BodyEncoding = System.Text.Encoding.UTF8;
                // text or html
                myMail.IsBodyHtml = true;

                var pdf = new MemoryStream(context.PreencherFormularioFolhaObra(context.ObterFolhaObra(id)).ToArray());
                Attachment att = new Attachment(pdf, "FolhaObra_" + id, System.Net.Mime.MediaTypeNames.Application.Pdf);
                myMail.Attachments.Add(att);

                mySmtpClient.Send(myMail);
 
            }

            catch (Exception)
            {
                return Content("Erro");
            }

            return Content("Sucesso");

        }

        // POST: FolhasObraController/Delete/5
        [HttpPost]
        public ActionResult ApagarFolhaObra(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            foreach (Intervencao intervencao in context.ObterFolhaObra(id).IntervencaosServico)
            {
                context.ApagarIntervencao(intervencao.IdIntervencao);
            }

            foreach (Produto peca in context.ObterFolhaObra(id).PecasServico)
            {
                context.ApagarPecaFolhaObra(peca.Ref_Produto, id);
            }

            context.ApagarFolhaObra(id);
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public ActionResult ApagarIntervencao(int id)
        {
                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                context.ApagarIntervencao(id);
                return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public ActionResult ApagarPeca(string Ref, string Id)
        {

                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                context.ApagarPecaFolhaObra(Ref, int.Parse(Id));
                return RedirectToAction(nameof(Index));

        }
    }
}
