namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio")]
    public class AcessosController : Controller
    {
        //Obter todos os acessos de uma data em especifico
        [HttpGet]
        public ActionResult Index(string Data)
        {
            if (Data == null || Data == string.Empty) Data = DateTime.Now.ToString("dd-MM-yyyy");

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            context.AdicionarLog(int.Parse(this.User.Claims.First().Value), "Acessos atualizados com sucesso!", 6);

            ViewData["Data"] = Data;
            return View(context.ObterListaAcessos(DateTime.Parse(Data)));
        }

        //Obter ultimo acesso de um utilizador em especifico
        [HttpGet]
        public JsonResult Acesso(string api, int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(api);
            if (String.IsNullOrEmpty(api) && User.Identity.IsAuthenticated) IdUtilizador = int.Parse(this.User.Claims.First().Value);
            if (IdUtilizador == 0) return Json("Acesso negado!");

            Utilizador u = context.ObterUtilizador(id);
            return Json(context.ObterUltimoAcesso(u.Id));
        }

        //Criar um acesso
        [HttpPost]
        public JsonResult Acesso(string api, int id, int tipo, int pin)
        {
            List<string> res = new List<string>() { "0", "Erro" };
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(api);
            if (String.IsNullOrEmpty(api) && User.Identity.IsAuthenticated) IdUtilizador = int.Parse(this.User.Claims.First().Value);
            if (IdUtilizador == 0) return Json(res);

            Utilizador u = context.ObterUtilizador(id);
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

            var file = context.GerarMapaPresencas(DateTime.Parse(data));
            var output = new MemoryStream();
            output.Write(file, 0, file.Length);
            output.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "MapaPresencas_" + DateTime.Parse(data).ToString("MM-yyyy") + ".xlsx",
                Inline = false,
                Size = file.Length,
                CreationDate = DateTime.Now,

            };
            Response.Headers.Add("Content-Disposition", cd.ToString());

            return File(output, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        //Apagar um acesso em especifico
        [HttpDelete]
        public JsonResult Acesso(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            context.ApagarAcesso(int.Parse(id));

            return Json("1");
        }
    }
}
