using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Custom;
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
        //Obter todos os utilizadores ou redirecionar para as ferias do utilziador em especifico
        [HttpGet]
        public ActionResult Index()
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Utilizador", new { id = int.Parse(this.User.Claims.First().Value) });
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return View(context.ObterListaUtilizadores(true, false));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public virtual ActionResult Ferias()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            string id = context.ObterAnoAtivo();

            var file = context.GerarMapaFerias(id);
            var output = new MemoryStream();

            output.Write(file, 0, file.Length);
            output.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "MapaFerias_" + id + ".xlsx",
                Inline = false,
                Size = file.Length,
                CreationDate = DateTime.Now,

            };
            Response.Headers.Add("Content-Disposition", cd.ToString());

            return File(output, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        //Criar ferias com base no utilizador e nada data, saltando feriados e fins de semanas 
        [HttpPost]
        public ContentResult Ferias(string datainicio, string datafim, int idutilizador)
        {
            if (idutilizador != int.Parse(this.User.Claims.First().Value.ToString()) && !this.User.IsInRole("Admin")) return Content("0");

            DateTime dataInicio = DateTime.Parse(datainicio);
            DateTime dataFim = DateTime.Parse(datafim);
            DateTime dataAtual = DateTime.Parse(datainicio);
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            List<Ferias> LstFerias = new List<Ferias>();
            List<Ferias> LstFeriasExistentes = context.ObterListaFerias(idutilizador);
            List<Feriado> LstFeriados = context.ObterListaFeriados(DateTime.Parse(datainicio).Year.ToString());

            bool weekend = false;

            do
            {
                if (weekend) { dataInicio = dataAtual; }

                weekend = (dataAtual.DayOfWeek == DayOfWeek.Saturday || dataAtual.DayOfWeek == DayOfWeek.Sunday);

                if (!weekend)
                {
                    if (dataAtual == dataFim && weekend) break;
                    if (LstFeriados.Where(d => d.DataFeriado == dataAtual).Any() || LstFeriasExistentes.Where(d => d.DataInicio >= dataAtual && d.DataFim <= dataAtual).Any())
                    {
                        weekend = (dataAtual.DayOfWeek == DayOfWeek.Friday || dataAtual == dataFim);

                        if (dataInicio == dataAtual) { dataInicio = dataAtual.AddDays(1); }
                        else
                        {
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
                    else if (dataAtual.DayOfWeek == DayOfWeek.Friday || dataAtual == dataFim)
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
                }
                dataAtual = dataAtual.AddDays(1);
            } while (dataAtual <= dataFim);

            context.CriarFerias(LstFerias);

            return Content("1");
        }

        //Apagar um registo de ferias
        [Authorize(Roles = "Admin, Tech, Escritorio")]
        [HttpDelete]
        public ContentResult Ferias(string id, string obs)
        {
            if (obs == null) obs = "";

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Ferias ferias = context.ObterFerias(int.Parse(id));

            if (ferias.IdUtilizador == int.Parse(this.User.Claims.First().Value.ToString()) || this.User.IsInRole("Admin")) context.ApagarFerias(int.Parse(id));

            if (ferias.IdUtilizador != int.Parse(this.User.Claims.First().Value.ToString())) MailContext.EnviarEmailFeriasNaoAprovadas(context.ObterUtilizador(ferias.IdUtilizador), ferias);

            return Content("1");
        }

        //Obter ferias do utilizador em especifico
        [HttpGet]
        public ActionResult Utilizador(int id)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Escritorio"))
            {
                if (int.Parse(this.User.Claims.First().Value) != id)
                {
                    return RedirectToAction("Utilizador", new { id = int.Parse(this.User.Claims.First().Value) });
                }
            }

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            ViewData["Ano"] = context.ObterAnoAtivo();
            ViewData["IdUtilizador"] = id;

            return View(context.ObterListaFeriasUtilizador(id));
        }

        //Validar ferias de um utilizador em especifico
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Validar(string id, string obs)
        {
            if (obs == null) obs = "";

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            List<Ferias> LstFerias = new List<Ferias>();

            Ferias ferias = context.ObterFerias(int.Parse(id));
            ferias.Validado = true;
            ferias.Obs = obs;
            ferias.Utilizador = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value));
            ferias.ValidadoPor = ferias.Utilizador.Id;
            ferias.ValidadoPorNome = ferias.Utilizador.NomeCompleto;
            LstFerias.Add(ferias);
            context.CriarFerias(LstFerias);

            MailContext.EnviarEmailFeriasAprovadas(context.ObterUtilizador(ferias.IdUtilizador), ferias);

            return RedirectToAction("Utilizador", new { IdUtilizador = ferias.IdUtilizador });
        }

        //Alterar dias de férias de um utilizador em especifico
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public void Dias(string ano, string idutilizador, string dias)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            context.CriarFeriasUtilizador(int.Parse(idutilizador), ano, int.Parse(dias));
        }

        //Obter o calendario
        [HttpGet]
        public ActionResult CalendarioView()
        {
            return View("Calendario");
        }

        //Obter o calendario ICS
        [AllowAnonymous]
        [HttpGet]
        public virtual ActionResult Calendario(string ApiKey)
        {
            var calendar = new Calendar();
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            int IdUtilizador = context.ObterIdUtilizadorApiKey(ApiKey);
            if (IdUtilizador == 0) return Forbid();

            List<Ferias> LstFerias = context.ObterListaFeriasValidadas();

            foreach (var f in LstFerias)
            {
                var e = new CalendarEvent
                {
                    Start = new CalDateTime(f.DataInicio),
                    End = new CalDateTime(f.DataFim.AddDays(1)),
                    IsAllDay = true,
                    Uid = f.Id.ToString(),
                    Description = "Validado por: " + f.ValidadoPorNome + "\r\nObservações: " + f.Obs,
                    Summary = f.Emoji + context.ObterUtilizador(f.IdUtilizador).NomeCompleto,
                    LastModified = new CalDateTime(DateTime.Now)
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

        //Obter eventos de ferias para calendario
        [HttpGet]
        public JsonResult EventosFerias(DateTime start, DateTime end)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return new JsonResult(context.ConverterFeriasEventos(context.ObterListaFerias(start, end), context.ObterListaFeriados(start.Year.ToString())).ToList());
        }



    }
}
