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

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Escritorio")]
    public class ViaturasController : Controller
    {

        public ActionResult Index()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;


            return View(context.ObterViaturas());
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AtualizarBuzzer(string id, bool buzzer)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            var TARGETURL = ConfigurationManager.AppSetting["CarTrack:URL"] + "/set_terminals_configuration_1_toggle_list?buzzer_toggle=UNCHANGED&immobilisation_toggle=UNCHANGED&did_toggle=" + (buzzer ? "ON" : "OFF") + "&terminal_identifiers=" + id;

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
                    return Content("1");
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

            return Content("0");
        }

        public ActionResult Viagens(string id, string Data)
        {
            if (Data == null || Data == string.Empty) Data = DateTime.Now.ToString("dd-MM-yyyy");

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            ViewData["Data"] = Data;
            ViewData["Matricula"] = id;

            return View(context.ObterViagens(id, Data));
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Escritorio")]
        public List<Viatura> ObterViaturas()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return context.ObterViaturas();
        }
    }

}
