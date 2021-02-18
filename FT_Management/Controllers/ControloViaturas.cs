using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FT_Management.Models;
using Microsoft.AspNetCore.Mvc;

namespace FT_Management.Controllers
{
    public class ControloViaturas : Controller
    {
        // GET: Viaturas
        public ActionResult Index()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            ViewData["Tecnicos"] = context.ObterListaUtilizadores().Where(u => u.TipoUtilizador != "3").ToList();

            return View(context.ObterViaturas());

        }

        [HttpPost]
        public ActionResult LevantarViatura(string data, string kms, string tecnico, string matricula)
        {

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            ControloViatura Viatura = (new ControloViatura(){
                DataInicio = DateTime.Parse(data),
                KmsViatura = kms,
                Nome_Tecnico = tecnico,
                MatriculaViatura = matricula
            });

            context.LevantamentoViatura(Viatura);

            return Content("1");

        }

        [HttpPost]
        public ActionResult DevolverViatura(string data, string kms, string tecnico, string matricula)
        {

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            ControloViatura Viatura = (new ControloViatura()
            {
                DataFim = DateTime.Parse(data),
                KmsFinais = kms,
                Nome_Tecnico = tecnico,
                MatriculaViatura = matricula
            });

            context.DevolverViatura(Viatura);

            return Content("1");

        }

    }
}
