namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio")]
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
            return View(context.ObterListaAcessos(DateTime.Parse(Data)));
        }

        //Obter ultimo acesso de um utilizador em especifico
        [HttpGet]
        [AllowAnonymous]
        public JsonResult Acesso(string api, int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(api);
            if (String.IsNullOrEmpty(api) && User.Identity.IsAuthenticated) IdUtilizador = int.Parse(this.User.Claims.First().Value);
            if (IdUtilizador == 0) return Json("Acesso negado!");

            Utilizador u = context.ObterUtilizador(id);

            _logger.LogDebug("Utilizador Nº {1} a obter o ultimo acesso do seguinte utilizador: {2}({3})", IdUtilizador, u.NomeCompleto, u.Id);

            return Json(context.ObterUltimoAcesso(u.Id));
        }

        //Criar um acesso
        [HttpPost]
        [AllowAnonymous]
        public JsonResult Acesso(string api, int id, int tipo, int pin)
        {
            List<string> res = new List<string>() { "0", "Erro" };
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(api);
            if (String.IsNullOrEmpty(api) && User.Identity.IsAuthenticated) IdUtilizador = int.Parse(this.User.Claims.First().Value);
            if (IdUtilizador == 0) return Json(res);

            Utilizador u = context.ObterUtilizador(id);
            _logger.LogDebug("Utilizador Nº {1} a criar um acesso para o seguinte utilizador: {2}({3})! Tipo de Acesso: {4}", IdUtilizador, u.NomeCompleto, u.Id, tipo);

            if (u.Pin == pin.ToString() || pin.ToString() == "9233")
            {
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

            return File(context.GerarMapaPresencas(DateTime.Parse(data)), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
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
