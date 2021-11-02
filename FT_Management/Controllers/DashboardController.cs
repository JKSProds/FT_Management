using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;

namespace FT_Management.Controllers
{
    public class PedidosDiariosViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Marcacao> model)
        {
            return View(model.Where(m => m.DataMarcacao == DateTime.Now.Date));
        }
    }
    public class PedidoOrcamentoViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Marcacao> model)
        {
            return View(model.Where(m => m.EstadoMarcacao == "Pedido Orçamento"));
        }
    }
    public class PedidoPecasViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Marcacao> model)
        {
            return View(model.Where(m => m.EstadoMarcacao == "Pedido Peças"));
        }
    }
    public class AguardarClienteViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Marcacao> model)
        {
            return View(model.Where(m => m.EstadoMarcacao == "Enc. a Fornecedor" || m.EstadoMarcacao == "Orçamentado" || m.EstadoMarcacao == "Enc. de Cliente"));
        }
    }
    public class OficinaViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Marcacao> model)
        {
            return View(model.Where(m => m.EstadoMarcacao == "Em oficina" || m.EstadoMarcacao == "Em receção" || m.Oficina == 1));
        }
    }
    [Authorize(Roles = "Admin, Escritorio")]
    public class DashboardController : Controller
    {

        public IActionResult Index()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;


            return View(context.ObterListaMarcacoes(DateTime.Parse("01/01/1900 00:00:00"), DateTime.Parse("01/01/2100 00:00:00")));
        }

        public int ObterNPedidosPendentes(List<Marcacao> LstMarcacoes)
        {
            return LstMarcacoes.Where(e => e.EstadoMarcacao != "Finalizado").Count();
        }

        public int ObterNPedidosConcluidos(DateTime data, List<Marcacao> LstMarcacoes) 
        {
            return LstMarcacoes.Where(d => d.DataMarcacao == data).Where(e => e.EstadoMarcacao == "Finalizado").Count();
        }
        public int ObterNPedidosAgendados(List<Marcacao> LstMarcacoes)
        {
            return LstMarcacoes.Where(e => e.EstadoMarcacao == "Agendado" || e.EstadoMarcacao == "Criado").Count();
        }
        public int ObterNPedidosPecas(List<Marcacao> LstMarcacoes)
        {
            return LstMarcacoes.Where(e => e.EstadoMarcacao == "Pedido Peças").Count();
        }
        public int ObterNPedidosOrcamentos(List<Marcacao> LstMarcacoes)
        {
            return LstMarcacoes.Where(e => e.EstadoMarcacao == "Pedido Orçamento").Count();
        }
        public int ObterNPedidosOficinaPesagem(List<Marcacao> LstMarcacoes)
        {
            return LstMarcacoes.Where(e => e.EstadoMarcacao != "Finalizado" && e.TipoEquipamento == "Pesagem" && e.Oficina == 1).Count();
        }
        public int ObterNPedidosOficinaMecanica(List<Marcacao> LstMarcacoes)
        {
            return LstMarcacoes.Where(e => e.EstadoMarcacao != "Finalizado" && e.TipoEquipamento != "Pesagem" && e.Oficina == 1).Count();
        }
    }
}
