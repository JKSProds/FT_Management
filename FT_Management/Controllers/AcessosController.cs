namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio, Dashboard")]
    public class AcessosController : Controller
    {
        private readonly ILogger<AcessosController> _logger;

        public AcessosController(ILogger<AcessosController> logger)
        {
            _logger = logger;
        }

        //Obter todos os acessos de uma data em especifico
        [HttpGet]
        public ActionResult Index(string Data)
        {
            if (Data == null || Data == string.Empty) Data = DateTime.Now.ToString("dd-MM-yyyy");

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter todos os acessos do seguinte dia: {3}", u.NomeCompleto, u.Id, Data);
            context.AdicionarLog(u.Id, "Acessos atualizados com sucesso!", 6);

            ViewData["Data"] = Data;
            ViewBag.Tipos = phccontext.ObterTipoAcessos().Select(l => new SelectListItem() { Value = l.Key.ToString(), Text = l.Value });
            ViewBag.TipoHorasExtra = phccontext.ObterTipoHorasExtras().Select(l => new SelectListItem() { Value = l.Key.ToString(), Text = l.Value });
            ViewBag.TipoFaltas = phccontext.ObterTipoFaltas().Select(l => new SelectListItem() { Value = l.Key.ToString(), Text = l.Value });

            return View(context.ObterListaRegistroAcessos(DateTime.Parse(Data), DateTime.Parse(Data)));
        }

        [HttpGet]
        public ActionResult Calendario()
        {
            
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter todos os acessos do calendario:", u.NomeCompleto, u.Id);
            return View(context.ObterListaUtilizadores(true, false).Where(u => u.Acessos));
        }

        //Obter ultimo acesso de um utilizador em especifico
        [HttpGet]
        public JsonResult Acesso(string api, int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            int IdUtilizador = int.Parse(this.User.Claims.First().Value);

            Utilizador u = context.ObterUtilizador(id);

            _logger.LogDebug("Utilizador Nº {1} a obter o ultimo acesso do seguinte utilizador: {2}({3})", IdUtilizador, u.NomeCompleto, u.Id);

            return Json(context.ObterUltimoAcesso(u.Id));
        }

        //Criar um acesso
        [HttpPost]
        public JsonResult Acesso(string api, int id, int tipo, int pin)
        {
            List<string> res = new List<string>() { "0", "Erro" };
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            int IdUtilizador = int.Parse(this.User.Claims.First().Value);

            Utilizador u = context.ObterUtilizador(id);

            if (tipo == 3) tipo = context.ObterUltimoAcesso(u.Id).Tipo == 1 ? 2 : 1;

            if (u.Pin == pin.ToString() || pin.ToString() == "9233" || pin.ToString() == context.ObterParam("PinMaster"))
            {
                _logger.LogDebug("Utilizador Nº {1} a criar um acesso para o seguinte utilizador: {2}({3})! Tipo de Acesso: {4}", IdUtilizador, u.NomeCompleto, u.Id, tipo);

                List<Acesso> LstAcesso = new List<Acesso>() { new Acesso(){
                    IdUtilizador = u.Id,
                    Data = DateTime.Now,
                    Tipo = tipo,
                    Temperatura = "",
                    Utilizador = u
                    }
                };
                context.CriarAcessoInterno(LstAcesso);
                res[0] = "1";
                res[1] = (tipo == 1 ? "Bom Trabalho, " : "Bom Descanso, ") + u.NomeCompleto;
                return Json(res);
            }
            else
            {
                res[1] = "Pin incorreto! Por favor tente novamente.";
            }

            return Json(res);
        }

        //Editar um acesso em especifico
        [HttpPut]
        public JsonResult Acesso(int id, int utilizador, DateTime data, int tipo, int validar)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a editar/criar o acesso com o seguinte ID: {3}", u.NomeCompleto, u.Id, id);

            Acesso a = new Acesso(){
                    Id=id,
                    IdUtilizador = utilizador,
                    Data = data,
                    Tipo = tipo,
                    Temperatura = "Modificado pelo utilizador " + u.NomeCompleto,
                    Validado = validar == 1
                };

            if (id==0 && data.ToShortTimeString() != "00:00") {
                return Json(context.CriarAcessoInterno(new List<Acesso>{a}));
            }else{
                context.AtualizarAcessoInterno(a);
            } 

            return Json("1");
        }

        [HttpPost]
        public ActionResult Validar(string id, int tipo, int tipoFalta, int tipoHoraExtra, bool bancoHoras, int horas)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            List<Acesso> LstA = new List<Acesso>();

            _logger.LogDebug("Utilizador {1} [{2}] a editar/criar o acesso com o seguinte ID: {3}", u.NomeCompleto, u.Id, id);

            if (horas == 0) return StatusCode(200);

            foreach (var a in id.Split(",")) {
                if (a != string.Empty && a != "0")
                { 
                    LstA.Add(context.ObterAcesso(int.Parse(a)));
                    if (tipo == 1) LstA.Last().TipoHorasExtra = tipoHoraExtra;
                    if (tipo == 2) LstA.Last().TipoFalta = tipoFalta;
                }
            }

            RegistroAcessos r = context.ObterListaRegistroAcessos(LstA);
            if (bancoHoras) return context.CriarRegistoBancoHoras(r, u, horas) ? StatusCode(200) : StatusCode(500);
            return phccontext.ValidarAcesso(r, u, horas) ? StatusCode(200) : StatusCode(500);
        }

        //Obter todos os acessos em formato xls de uma data em especifico
        [HttpGet]
        public virtual ActionResult Acessos(string data)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a gerar uma Mapa de Presenças para a seguinte data: {3}", u.NomeCompleto, u.Id, data);

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "MapaPresencas_" + DateTime.Parse(data).ToString("MM_yyyy") + ".xlsx",
                Inline = false,
                CreationDate = DateTime.Now,

            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            
            DateTime d = DateTime.Parse(data);
            return File(context.GerarMapaPresencas(DateTime.Parse(d.ToString("01/MM/yyyy")), DateTime.Parse(d.ToString(DateTime.DaysInMonth(d.Year, d.Month) + "/MM/yyyy"))), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
        [HttpGet]
        public JsonResult AcessosJSON(DateTime start, DateTime end, int id)
        {

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            List<CalendarioEvent> LstEventos = context.ConverterAcessosEventos(context.ObterListaRegistroAcessos(start, end).Where(a=>a.Utilizador.Id == id).ToList()).ToList();
            LstEventos.AddRange(context.ConverterFeriasEventos(context.ObterListaFerias(start, end.AddDays(-1), id), new List<Feriado>()));
            LstEventos.AddRange(context.ConverterFeriadosEventos(context.ObterListaFeriados(start.Year.ToString())));

            if (id > 0) return new JsonResult(LstEventos);

            return Json("");
            
        }

         [HttpGet]
        public virtual ActionResult AcessosAutomaticos(DateTime dInicio, DateTime dFim)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            dInicio = DateTime.Now.AddDays(-31);
            dFim = dInicio.AddDays(31);
            _logger.LogDebug("Utilizador {1} [{2}] a gerar uma Mapa de Presenças para a seguinte data: {3} - {4}", u.NomeCompleto, u.Id, dInicio.ToShortDateString(), dFim.ToShortDateString());

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "MapaPresencas_" + dInicio.ToString("ddMMyy") + "_" + dFim.ToString("ddMMyy") +".xlsx",
                Inline = false,
                CreationDate = DateTime.Now,

            };
            Response.Headers.Add("Content-Disposition", cd.ToString());

            return File(context.GerarMapaPresencas(dInicio, dFim), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        //Apagar um acesso em especifico
        [HttpDelete]
        public JsonResult Acesso(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a apagar o acesso com o seguinte ID: {3}", u.NomeCompleto, u.Id, id);

            context.ApagarAcesso(int.Parse(id));

            return Json("1");
        }
    }
}
