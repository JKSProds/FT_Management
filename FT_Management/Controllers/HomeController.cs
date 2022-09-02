using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FT_Management.Models;
using System.Text.Json;
using System.Dynamic;
using Microsoft.AspNetCore.Authorization;

namespace FT_Management.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AcessoNegado()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [AllowAnonymous]
        [HttpPost]
        public string Notificacoes(string deviceId, string action)
        {
            string res = "";
            Console.WriteLine("POST NOTIFICACOES");

            if (deviceId == "1" && action=="SEND")
            {
                FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
                Console.WriteLine("Leitura de Notificacoes da BD com Sucesso");

                foreach (var item in context.ObterNotificacoesPendentes())
                {
                    dynamic obj = new ExpandoObject();
                    obj.message = item.Mensagem;
                    obj.number = item.Destino;
                    obj.messageId = item.ID;

                    res += Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                }
            }
            return res;
        }
    }
}
