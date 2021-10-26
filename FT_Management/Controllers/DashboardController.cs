using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;

namespace FT_Management.Controllers
{

    [Authorize(Roles = "Admin, Escritorio")]
    public class DashboardController : Controller
    {

        public IActionResult Index()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            List<Marcacao> LstMarcacoes = context.ObterListaMarcacoes(DateTime.Now.AddDays(-365), DateTime.Now.AddDays(365));

            ViewData["NPedidosConcluidos"] = ObterNPedidosConcluidos(DateTime.Today, LstMarcacoes);
            ViewData["NPedidosAgendados"] = ObterNPedidosAgendados(LstMarcacoes);

            ViewData["NPedidosPecas"] = ObterNPedidosPecas(LstMarcacoes);
            ViewData["NPedidosOrcamentos"] = ObterNPedidosOrcamentos(LstMarcacoes);

            ViewData["NPedidosOficinaPesagem"] = ObterNPedidosOficinaPesagem(LstMarcacoes);
            ViewData["NPedidosOficinaMecanica"] = ObterNPedidosOficinaMecanica(LstMarcacoes);

            return View();
        }

        public JsonResult ObterDadosGraficoPie()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            List<Marcacao> LstMarcacoes = context.ObterListaMarcacoes(DateTime.Now.AddDays(-365), DateTime.Now.AddDays(365));

            int[] lst = new int[] { ObterNPedidosPendentes(LstMarcacoes), ObterNPedidosPecas(LstMarcacoes), ObterNPedidosOrcamentos(LstMarcacoes), ObterNPedidosOficinaPesagem(LstMarcacoes) + ObterNPedidosOficinaMecanica(LstMarcacoes) };


            return new JsonResult(lst.ToList());
        }

        public JsonResult ObterDadosGraficoBar()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            List<Marcacao> LstMarcacoes = context.ObterListaMarcacoes(DateTime.Now.AddDays(-6), DateTime.Now.AddDays(0));

            int[] lst = new int[] { 10, 7, 17, 0, 0, 8, 13};

            DateTime data = DateTime.Today.AddDays(-6);

            for (int i = 0; i < 7; i++)
            {
                lst[i] = ObterNPedidosConcluidos(data.AddDays(i), LstMarcacoes);
            }

            return new JsonResult(lst.ToList());
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
