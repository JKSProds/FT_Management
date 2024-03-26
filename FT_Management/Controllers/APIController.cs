
namespace FT_Management.Controllers
{

    [Authorize(Roles = "Admin")]
    public class APIController : Controller
    {
        //Obter lista de marcacoes
        public JsonResult Pedidos(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;

            return Json(phccontext.ObterMarcacoes(id, DateTime.Now));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Mail(string id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            if (string.IsNullOrEmpty(id)) return StatusCode(500);
            
            Notificacao n = phccontext.ObterEmail(id);

            return MailContext.EnviarEmailManual(n.UtilizadorDestino.EmailUtilizador,n.Assunto,n.Mensagem,n.Cc) ? (phccontext.FecharEmail(id)[0] != "0" ? StatusCode(200) : StatusCode(500)) : StatusCode(500);
        }


        //Obter dados para graficos
        [HttpGet("API/Graficos/Assistencias")]
        public IActionResult GraficoAssistencias(DateTime dInicio, DateTime dFim, int c, string cl)
        {
            // Retrieve data from your data source
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            
            8
            var data = new {
                labels = d.Keys,
                values = d.Values
            };
            return Ok(data);
        }

        //Obter dados para graficos
        [HttpGet("API/Dados/Assistencias")]
        public IActionResult DadosAssistencias(DateTime dInicio, DateTime dFim, int c, string cl, string vl)
        {
            // Retrieve data from your data source
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            
            var data = phccontext.ObterFolhasObraEstatisticas(phccontext.ObterCliente(c,0), cl, vl, dInicio, dFim);

            return Ok(data);
        }
    }
}
