namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio, Tech")]
    public class FolhasObraController : Controller
    {
        //Obter todas as folhas de obra com base numa data
        [HttpGet]
        public ActionResult Index(string DataFolhasObra)
        {
            if (DataFolhasObra == null || DataFolhasObra == string.Empty) DataFolhasObra = DateTime.Now.ToString("dd-MM-yyyy");

            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            ViewData["DataFolhasObra"] = DataFolhasObra;

            return View(phccontext.ObterFolhasObra(DateTime.Parse(DataFolhasObra)));
        }

        //Obter view para adicionar folhas de obra
        [HttpGet]
        [Authorize(Roles = "Admin, Tech")]
        public ActionResult Adicionar(string id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Marcacao m = phccontext.ObterMarcacao(id);
            List<FolhaObra> LstFolhasObra = phccontext.ObterFolhasObra(DateTime.Now, m.Cliente);

            if (m.EstadoMarcacaoDesc == "Finalizado" || m.EstadoMarcacaoDesc == "Cancelado") return Forbid();

            FolhaObra fo = new FolhaObra().PreencherDadosMarcacao(m);
            fo.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            fo.PreencherViagem(context.ObterViagens(fo.Utilizador.Viatura.Matricula, DateTime.Now.ToShortDateString()).Where(v => v.Fim_Viagem.Year > 1).DefaultIfEmpty(new Viagem() { Fim_Viagem = fo.IntervencaosServico.First().HoraInicio, Distancia_Viagem = "0" }).Last());
            if (LstFolhasObra.Count() > 0)
            {
                fo.RubricaCliente = LstFolhasObra.First().RubricaCliente.Replace("}", "").Replace("{", "");
                fo.ConferidoPor = LstFolhasObra.First().ConferidoPor;
            }

            ViewBag.EstadoFolhaObra = phccontext.ObterEstadoFolhaObra().Select(l => new SelectListItem() { Value = l.Key.ToString(), Text = l.Value });
            ViewData["TipoFolhaObra"] = phccontext.ObterTipoFolhaObra();
            return View(fo);
        }

        //Obter uma folha de obra em especifico
        [HttpGet]
        public ActionResult FolhaObra(int Id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FolhaObra fo = phccontext.ObterFolhaObra(Id);

            Utilizador user = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && fo.IntervencaosServico.Where(i => i.IdTecnico == context.ObterUtilizador(int.Parse(this.User.Claims.First().Value.ToString())).IdPHC).Count() == 0) return Redirect("~/Home/AcessoNegado");

            ViewData["SelectedTecnico"] = user.NomeCompleto;
            ViewData["Tecnicos"] = context.ObterListaTecnicos(false, false);

            return View(fo);
        }

        //Criar uma folha de obra
        [HttpPost]
        [Authorize(Roles = "Admin, Tech")]
        public ActionResult FolhaObra(FolhaObra fo)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            if (!User.IsInRole("Admin")) fo = fo.PreencherDadosMarcacao(phccontext.ObterMarcacao(fo.IdMarcacao));
            fo.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));


            if (ModelState.IsValid)
            {
                fo.ClienteServico = phccontext.ObterClienteSimples(fo.ClienteServico.IdCliente, fo.ClienteServico.IdLoja);
                fo.EquipamentoServico = phccontext.ObterEquipamentoSimples(fo.EquipamentoServico.EquipamentoStamp);
                fo.Marcacao = phccontext.ObterMarcacao(fo.IdMarcacao);
                fo.ValidarIntervencoes();
                fo.ValidarPecas(phccontext.ObterProdutosArmazem(fo.Utilizador.IdArmazem));
                fo.ValidarTipoFolhaObra();

                if (fo.EquipamentoServico.Cliente.ClienteStamp != fo.ClienteServico.ClienteStamp) phccontext.AtualizarClienteEquipamento(fo.ClienteServico, fo.EquipamentoServico, fo.Utilizador);

                Marcacao m = fo.Marcacao;
                m.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
                int Estado = fo.EstadoFolhaObra;
                if (fo.FecharMarcacao && fo.EstadoFolhaObra == 1) m.EstadoMarcacaoDesc = "Finalizado";
                if (fo.EstadoFolhaObra == 2) m.EstadoMarcacaoDesc = "Pedido Peças";
                if (fo.EstadoFolhaObra == 3) m.EstadoMarcacaoDesc = "Pedido Orçamento";
                if (fo.EstadoFolhaObra == 4)
                {
                    m.EstadoMarcacaoDesc = "Reagendado";
                    fo.EstadoFolhaObra = 2;
                }

                List<string> res = phccontext.CriarFolhaObra(fo);
                if (int.Parse(res[0]) > 0)
                {
                    fo.StampFO = res[2].ToString();
                    fo.IdFolhaObra = int.Parse(res[1]);

                    phccontext.AtualizaMarcacao(m);
                    phccontext.FecharFolhaObra(fo);
                    phccontext.CriarAnexosFolhaObra(fo);
                    fo = phccontext.ObterFolhaObra(fo.IdFolhaObra);

                    if (Estado == 2) return RedirectToAction("Pedido", "Dossiers", new { id = fo.StampFO, serie = 96, ReturnUrl = "/Pedidos/ListaPedidos?IdTecnico=" + fo.Utilizador.IdPHC });
                    if (Estado == 3) return RedirectToAction("Pedido", "Dossiers", new { id = fo.StampFO, serie = 97, ReturnUrl = "/Pedidos/ListaPedidos?IdTecnico=" + fo.Utilizador.IdPHC });

                    return RedirectToAction("ListaPedidos", "Pedidos", new { IdTecnico = fo.Utilizador.IdPHC });
                }

                ModelState.AddModelError("", res[1]);
            }

            ModelState.AddModelError("", string.Join("|", ModelState.Where(e => e.Value.Errors.Count() > 0).Select(e => e.Value.Errors.First().ErrorMessage)));
            ViewBag.EstadoFolhaObra = phccontext.ObterEstadoFolhaObra().Select(l => new SelectListItem() { Value = l.Key.ToString(), Text = l.Value });
            ViewData["TipoFolhaObra"] = phccontext.ObterTipoFolhaObra();

            return View(fo);
        }

        //Validar alguns paramentros da folha de obra antes de a criar
        [HttpPost]
        public ActionResult Validar(FolhaObra fo)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            fo.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            fo.ValidarIntervencoes();
            fo.ValidarPecas(phccontext.ObterProdutosArmazem(fo.Utilizador.IdArmazem));
            fo.ClienteServico = phccontext.ObterClienteSimples(fo.ClienteServico.IdCliente, fo.ClienteServico.IdLoja);
            fo.EquipamentoServico = phccontext.ObterEquipamentoSimples(fo.EquipamentoServico.EquipamentoStamp);
            fo.Marcacao = phccontext.ObterMarcacao(fo.IdMarcacao);

            return Content(phccontext.ValidarFolhaObra(fo));
        }

        //Obter o email do cliente associado á folha de obra
        [HttpGet]
        public JsonResult Email(int id)
        {
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return Json(phccontext.ObterFolhaObra(id).ClienteServico.EmailCliente);
        }

        //Enviar o email da folha de obra
        [HttpPost]
        public ActionResult Email(int id, string emailDestino)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FolhaObra fo = phccontext.ObterFolhaObra(id);

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && fo.IntervencaosServico.Where(i => i.IdTecnico == context.ObterUtilizador(int.Parse(this.User.Claims.First().Value.ToString())).IdPHC).Count() == 0) return Redirect("~/Home/AcessoNegado");

            if (MailContext.EnviarEmailFolhaObra(emailDestino, fo, new Attachment((new MemoryStream(context.PreencherFormularioFolhaObra(fo).ToArray())), "FO" + id + ".pdf", System.Net.Mime.MediaTypeNames.Application.Pdf))) return Content("Sucesso");

            return Content("Erro");

        }

        //Imprimir Etiqueta 80x60
        [HttpGet]
        public ActionResult Etiqueta(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            FolhaObra fo = phccontext.ObterFolhaObra(int.Parse(id));

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && fo.IntervencaosServico.Where(i => i.IdTecnico == context.ObterUtilizador(int.Parse(this.User.Claims.First().Value.ToString())).IdPHC).Count() == 0) return Redirect("~/Home/AcessoNegado");

            var filePath = Path.GetTempFileName();
            context.DesenharEtiquetaFolhaObra(fo).Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            return File(context.BitMapToMemoryStream(filePath, 810, 504), "application/pdf");
        }

        //Imprimir Documento A4
        [HttpGet]
        public virtual ActionResult Documento(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FolhaObra fo = phccontext.ObterFolhaObra(id);

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && fo.IntervencaosServico.Where(i => i.IdTecnico == context.ObterUtilizador(int.Parse(this.User.Claims.First().Value.ToString())).IdPHC).Count() == 0) return Redirect("~/Home/AcessoNegado");

            var file = context.PreencherFormularioFolhaObra(fo).ToArray();
            var output = new MemoryStream();
            output.Write(file, 0, file.Length);
            output.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "FolhaObra_" + id + ".pdf",
                Inline = true,
                Size = file.Length,
                CreationDate = DateTime.Now,

            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            //Send the File to Download.
            return new FileContentResult(output.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        //Imprimir ticket 
        [HttpGet]
        public virtual ActionResult Ticket(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            FolhaObra fo = phccontext.ObterFolhaObra(id);

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && fo.IntervencaosServico.Where(i => i.IdTecnico == context.ObterUtilizador(int.Parse(this.User.Claims.First().Value.ToString())).IdPHC).Count() == 0) return Redirect("~/Home/AcessoNegado");

            var filePath = Path.GetTempFileName();
            Bitmap bm = context.DesenharFolhaObraSimples(fo);
            bm.Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "TicketFO_" + id + ".pdf",
                Inline = false,
                CreationDate = DateTime.Now
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            return new FileContentResult(context.BitMapToMemoryStream(filePath, bm.Width, bm.Height).ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        //Criar codigo da folha de obra
        [HttpPost]
        public ActionResult CriarCodigo(string id, string obs)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Codigo c = new Codigo()
            {
                Stamp = id,
                Estado = 0,
                ValidadeCodigo = DateTime.Now.AddMinutes(10),
                utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)),
                Obs = obs
            };
            context.CriarCodigo(c);
            foreach (var u in context.ObterListaUtilizadores(false, false).Where(u => u.Admin))
            {
                ChatContext.EnviarNotificacaoCodigo(c, u);
            }
            return Content("OK");
        }

        //Validar o codigo
        [HttpPost]
        public ActionResult ValidarCodigo(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            return Content(context.ValidarCodigo(id).ToString());
        }










    }
}
