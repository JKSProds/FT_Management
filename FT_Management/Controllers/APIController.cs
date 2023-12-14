
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
    }
}
