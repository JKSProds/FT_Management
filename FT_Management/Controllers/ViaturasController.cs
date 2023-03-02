using FT_Management.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Custom;
using System.Linq;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio")]
    public class ViaturasController : Controller
    {
        //Obtem todas as viaturas numa pagina web
        [HttpGet]
        public ActionResult Index()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return View(context.ObterViaturas());
        }

        //Obtem o mapa com as viaturas
        [HttpGet]
        public ActionResult Mapa()
        {
            return View();
        }

        //Obtem uma viatura em especifico com viagens num dia em especifico
        [HttpGet]
        public ActionResult Viatura(string id, string Data)
        {
            if (Data == null || Data == string.Empty) Data = DateTime.Now.ToString("dd-MM-yyyy");

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            ViewData["Data"] = Data;
            ViewData["Matricula"] = id;

            List<Viagem> LstViagens = context.ObterViagens(id, Data);

            return View(LstViagens);
        }

        //Obtem todas as viaturas em formato json
        [HttpGet]
        [Authorize(Roles = "Admin, Escritorio")]
        public List<Viatura> Viaturas(string API_KEY)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return context.ObterViaturas();
        }

        //Atualiza o buzzer de uma viatura em especifico
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<ActionResult> Buzzer(string id, bool buzzer)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Viatura v = context.ObterViatura(id);

            if (v.Buzzer != buzzer)
            {
                v.Buzzer = buzzer;

                var TARGETURL = ConfigurationManager.AppSetting["CarTrack:URL"] + "/set_terminals_configuration_1_toggle_list?buzzer_toggle=UNCHANGED&immobilisation_toggle=UNCHANGED&did_toggle=" + (v.Buzzer ? "ON" : "OFF") + "&terminal_identifiers=" + v.Matricula;

                HttpClientHandler handler = new HttpClientHandler()
                {
                    UseProxy = false
                };


                // ... Use HttpClient.            
                HttpClient client = new HttpClient(handler);

                var byteArray = Encoding.ASCII.GetBytes(ConfigurationManager.AppSetting["CarTrack:Auth"]);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                try
                {
                    HttpResponseMessage response = await client.GetAsync(TARGETURL);
                    HttpContent content = response.Content;

                    string result = await content.ReadAsStringAsync();

                    // ... Display the result.
                    if (response.IsSuccessStatusCode)
                    {
                        context.AtualizarBuzzer(v);
                        context.AdicionarLog(int.Parse(this.User.Claims.First().Value.ToString()), (buzzer ? "Ativado" : "Desativado") + " buzzer da viatura com a matricula " + id + "!", 0);
                        return Content("1");
                    }

                }
                catch (Exception ex)
                {

                    Console.WriteLine("Ocorreu um erro ao tentar atualizar o buzzer da viatura. " + ex.Message);
                }
            }
            else
            {
                return Content("1");
            }
            return Content("0");
        }




    }

}
