using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Comercial, Escritorio")]
    public class VisitasController : Controller
    {
        // GET: VisitasController
        public ActionResult Index()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            if (User.IsInRole("Admin"))
            {

                return View(context.ObterListaComerciais());
            }
            else
            {
                return RedirectToAction("ListaVisitas");
            }
        }

        [HttpPost]
        public JsonResult ObterClientes(string prefix)
        {
            if (prefix is null) prefix = "";
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return Json(context.ObterListaClientes(prefix, false));
        }

        public ActionResult ListaVisitas(int IdComercial, string DataVisitas)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            if (!User.IsInRole("Admin")) IdComercial = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).IdPHC;

            if (DataVisitas == null || DataVisitas == string.Empty) DataVisitas = DateTime.Now.ToString("dd-MM-yyyy");
            ViewData["DataVisitas"] = DataVisitas;

            List<Visita> ListaVisitas = context.ObterListaVisitas(IdComercial, DateTime.Parse(DataVisitas), DateTime.Parse(DataVisitas));
            ViewData["IdComercial"] = IdComercial;
            return View(ListaVisitas);
        }
        public ActionResult Adicionar()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            ViewData["Comerciais"] = context.ObterListaUtilizadores().Where(u => u.TipoUtilizador == 2).ToList();
            return View();
        }
        [HttpPost]
        public ActionResult Adicionar(int IdCliente, int IdLoja, DateTime txtData, int txtComercial, string Obs)
        {

            if (IdCliente == 0 && IdLoja == 0 && txtComercial == 0) return View();

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                Visita visita = new Visita()
                {
                    IdVisita = 0,
                    DataVisita = txtData,
                    Cliente = new Cliente()
                    {
                        IdCliente = IdCliente,
                        IdLoja = IdLoja
                    },
                    IdComercial = txtComercial,
                    ResumoVisita = Obs,
                    ObsVisita = "",
                    EstadoVisita = "Agendada"
                };
                List<Visita> lstVisitas = new List<Visita>();
                lstVisitas.Add(visita);

                context.CriarVisitas(lstVisitas);
                context.AdicionarLog(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).NomeUtilizador, "Foi adicionada uma visita nova ao cliente: " + IdCliente, 5);

            return RedirectToAction("Index", "Visitas");

        }

        [HttpGet]
        public ActionResult Apagar(int idVisita, string ReturnUrl)
        {

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            context.ApagarVisita(idVisita);

            return Redirect(ReturnUrl);
        }
    }
}
