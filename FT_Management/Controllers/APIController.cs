
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
    }
}
