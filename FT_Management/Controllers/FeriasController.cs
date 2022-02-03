using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FT_Management.Models;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FT_Management.Controllers
{
    [Authorize]
    public class FeriasController : Controller
    {

        public ActionResult Index()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            if (!User.IsInRole("Admin") && !User.IsInRole("Escritorio"))
            {
                return RedirectToAction("Detalhes", new { IdUtilizador = int.Parse(this.User.Claims.First().Value)});
            }

            return View(context.ObterListaUtilizadores());
        }

        [AllowAnonymous]
        [HttpGet]
        public virtual ActionResult Calendar()
        {
            var calendar = new Calendar();
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            List<Ferias> LstFerias = context.ObterListaFerias(DateTime.Parse(DateTime.Now.Year + "-01-01"), DateTime.Parse(DateTime.Now.Year + "-12-31"));
            foreach (var f in LstFerias)
            {
                
                var e = new CalendarEvent
                {
                    Start = new CalDateTime(f.DataInicio),
                    End = new CalDateTime(f.DataFim),
                    IsAllDay = true,
                    Uid = f.Id.ToString(),
                    Description = "Validado por: " + f.ValidadoPorNome,
                    Summary = "Férias - " + context.ObterTecnico(f.Id).NomeCompleto,
                };
                calendar.Events.Add(e);
            }
            var serializer = new CalendarSerializer();

            var serializedCalendar = serializer.SerializeToString(calendar);
            var bytesCalendar = new UTF8Encoding(false).GetBytes(serializedCalendar);

            MemoryStream ms = new MemoryStream(bytesCalendar);

            ms.Write(bytesCalendar, 0, bytesCalendar.Length);
            ms.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "Ferias.ics",
                Inline = false,
                Size = bytesCalendar.Length,
                CreationDate = DateTime.Now

            };
            Response.Headers.Add("Content-Disposition", cd.ToString());

            return File(ms, "text/calendar");
        }

        public JsonResult ObterFeriasCalendario(DateTime start, DateTime end)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            return new JsonResult(context.ConverterFeriasEventos(context.ObterListaFerias(start, end), context.ObterListaFeriados(start.Year.ToString())).ToList());

        }

        public ActionResult Calendario()
        {

            return View();
        }

        public ActionResult Detalhes(int IdUtilizador)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            ViewData["Ano"] = context.ObterAnoAtivo();
            if (!User.IsInRole("Admin") && !User.IsInRole("Escritorio"))
            {
                if (int.Parse(this.User.Claims.First().Value) != IdUtilizador)
                {
                    return RedirectToAction("Detalhes", new { IdUtilizador = int.Parse(this.User.Claims.First().Value) });
                }
            }

            ViewData["IdUtilizador"] = IdUtilizador;
            return View(context.ObterListaFeriasUtilizador(IdUtilizador, ViewData["Ano"].ToString()));
        }

        public ActionResult Validar(string id, string obs)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            List<Ferias> LstFerias = new List<Ferias>();
            Ferias ferias = context.ObterFerias(int.Parse(id));
            ferias.Validado = true;
            ferias.Obs = obs;
            ferias.ValidadoPor = int.Parse(this.User.Claims.First().Value);
            LstFerias.Add(ferias);

            context.CriarFerias(LstFerias);

            return RedirectToAction("Detalhes", new { IdUtilizador = ferias.IdUtilizador });
        }

        public ActionResult Apagar(Ferias ferias)
        {

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            context.ApagarFerias(ferias.Id);

            return RedirectToAction("Detalhes", new { IdUtilizador = ferias.IdUtilizador });
        }

        public void AlterarDiasFerias(string ano, string idutilizador, string dias)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            context.CriarFeriasUtilizador(int.Parse(idutilizador), ano, int.Parse(dias));
        }

        public void AdicionarFerias(string datainicio, string datafim, int idutilizador) {
            DateTime dataInicio = DateTime.Parse(datainicio);
            DateTime dataFim = DateTime.Parse(datafim);
            DateTime dataAtual = DateTime.Parse(datainicio);
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            List<Ferias> LstFerias = new List<Ferias>();
            List<Feriado> LstFeriados = context.ObterListaFeriados(DateTime.Parse(datainicio).Year.ToString());

            bool weekend = false;

            do 
                {

                if (weekend)
                {
                    
                    weekend = (dataAtual.DayOfWeek == DayOfWeek.Saturday || dataAtual.DayOfWeek == DayOfWeek.Sunday);
                    if (!weekend) dataInicio = dataAtual;
                }
                if (LstFeriados.Where(d => d.DataFeriado == dataAtual).Any())
                {
                    if (dataInicio == dataAtual) { dataInicio = dataAtual.AddDays(1); }
                    else {
                        LstFerias.Add(
                         new Ferias
                         {
                             IdUtilizador = idutilizador,
                             DataInicio = dataInicio,
                             DataFim = dataAtual.AddDays(-1),
                             ValidadoPor = 0
                         });
                        dataInicio = dataAtual.AddDays(1);
                    }
                }
                if (dataAtual == dataFim && weekend) break;
                    if (dataAtual.DayOfWeek == DayOfWeek.Friday || dataAtual == dataFim)
                    {
                        LstFerias.Add(
                            new Ferias
                            {
                                IdUtilizador = idutilizador,
                                DataInicio = dataInicio,
                                DataFim = dataAtual,
                                ValidadoPor = 0
                            });
                        weekend = true;
                    }
                   
                    dataAtual = dataAtual.AddDays(1);
                } while (dataAtual <= dataFim);

            context.CriarFerias(LstFerias);
        }
    }
}
