using FT_Management.Models;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio, Tech")]
    public class FormularioController : Controller
    {
        private readonly ILogger<FormularioController> _logger;

        public FormularioController(ILogger<FormularioController> logger)
        {
            _logger = logger;
        }

        //Abrir formulario de certificacao de detetor de metais
        [HttpGet]
        public ActionResult CertificaDetetorMetais(string id)
        {
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a obter formulario de certificação do detetor de metais: Marcacao ID: {3}", u.NomeCompleto, u.Id, id);
           
            return View(phccontext.ObterMarcacao(id));
        }
        [HttpPost]
        public ActionResult CertificaDetetorMetais(string email, string nome, string equipamento, string marcacao)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(equipamento) || string.IsNullOrEmpty(marcacao)) return StatusCode(500);

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));

            _logger.LogDebug("Utilizador {1} [{2}] a guardar formulario de certificação do detetor de metais: Equipamento Stamp: {3}", u.NomeCompleto, u.Id, equipamento);

            phccontext.CertificacaoDetetorMetais(email, nome, phccontext.ObterEquipamentoSimples(equipamento), phccontext.ObterMarcacao(marcacao), u);

            return RedirectToAction("Adicionar", "FolhasObra", new {id = marcacao });
        }

        //Abrir inventario tecnico
        [HttpGet]
        public ActionResult InventarioTecnico(string id)
        {
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            return RedirectToAction("Tecnico", "Inventario", 0);
        }

        //Abrir inventario loja
        [HttpGet]
        public ActionResult InventarioLoja(string id)
        {
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Marcacao m = phccontext.ObterMarcacaoSimples(id);

            _logger.LogDebug("Utilizador {1} [{2}] a obter formulario de inventario de loja: Cliente: {3}, Loja: {4}", u.NomeCompleto, u.Id, m.Cliente.IdCliente, m.Cliente.IdLoja);
            return View(phccontext.ObterCliente(m.Cliente.IdCliente, m.Cliente.IdLoja));
        }

        //Guardar inventario loja
        [HttpPost]
        public ActionResult InventarioLoja(string id, string inventario)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(inventario)) return StatusCode(500);

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            PHCContext phccontext = HttpContext.RequestServices.GetService(typeof(PHCContext)) as PHCContext;
            Utilizador u = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            Marcacao m = phccontext.ObterMarcacaoSimples(id);
            Cliente c = phccontext.ObterClienteSimples(m.Cliente.IdCliente, m.Cliente.IdLoja);
            List<Equipamento> Equipamentos = new List<Equipamento>();

            foreach (var e in inventario.Split(";"))
            {
                if (!string.IsNullOrEmpty(e))
                {
                    Equipamentos.Add(phccontext.ObterEquipamento(e.Split("|")[0]));
                    Equipamentos.Last().TipoEquipamento = e.Split("|")[1];

                    phccontext.AtualizarClienteEquipamento(c, Equipamentos.Last(), u);
                    phccontext.AssociarEquipamentoContrato(c, Equipamentos.Last(), u);
                }
            }

            MailContext.EnviarEmailInventarioLoja(u, Equipamentos, phccontext.ObterCliente(m.Cliente.IdCliente, m.Cliente.IdLoja));

            _logger.LogDebug("Utilizador {1} [{2}] a guardar formulario de inventario de loja: Cliente: {3}, Loja: {4}", u.NomeCompleto, u.Id, c.IdCliente, c.IdLoja);
            return RedirectToAction("Adicionar", "FolhasObra", new { id = id });
        }
    }
}
