using FT_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Custom;
using WebDav;
using Microsoft.AspNetCore.StaticFiles;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Comercial, Escritorio")]
    public class VisitasController : Controller
    {
        [Authorize(Roles = "Admin, Escritorio")]
        public JsonResult ObterVisitas(DateTime start, DateTime end)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            return new JsonResult(context.ConverterVisitasEventos(context.ObterListaVisitas(start, end)).ToList());

        }

        public JsonResult ObterVisitasComercial(DateTime start, DateTime end)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            return new JsonResult(context.ConverterVisitasEventos(context.ObterListaVisitas(int.Parse(this.User.Claims.First().Value), start, end)).ToList());

        }

        [Authorize(Roles = "Admin, Escritorio")]
        public ActionResult Calendario()
        {
            return View();
        }

        public ActionResult CalendarioComercial()
        {
            return View();
        }

        [Authorize(Roles = "Admin, Escritorio, Comercial")]
        public JsonResult AlteracaoCalendario(DateTime data, int id)
        {

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            List<Visita> LstVisita = new List<Visita>();
            LstVisita.Add(context.ObterVisita(id));
            LstVisita.Last().DataVisita = data;

            context.CriarVisitas(LstVisita);
            return Json("ok");
        }

        public ActionResult Index()
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Escritorio")) return RedirectToAction("ListaVisitas");

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

   \        return View(context.ObterListaComerciais());
        }

        [HttpPost]
        public JsonResult ObterClientes(string prefix)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            if (prefix is null) prefix = "";

            return Json(context.ObterListaClientes(prefix, false));
        }

        public ActionResult ListaVisitas(int IdComercial, string DataVisitas)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Escritorio")) IdComercial = int.Parse(this.User.Claims.First().Value);
            if (DataVisitas == null || DataVisitas == string.Empty) DataVisitas = DateTime.Now.ToString("dd-MM-yyyy");

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            List<Visita> ListaVisitas = context.ObterListaVisitas(IdComercial, DateTime.Parse(DataVisitas), DateTime.Parse(DataVisitas));

            ViewData["DataVisitas"] = DataVisitas;
            ViewData["IdComercial"] = IdComercial;

            return View(ListaVisitas);
        }

        [Authorize(Roles = "Admin, Escritorio")]
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
                EstadoVisita = "Agendado",
                Contacto = new Contacto() { IdContacto = 0}
            };

            List<Visita> lstVisitas = new List<Visita>();
            lstVisitas.Add(visita);

            context.CriarVisitas(lstVisitas);

            return RedirectToAction("Index", "Visitas");

        }

        [Authorize(Roles = "Admin, Escritorio")]
        public ActionResult Editar(int idVisita)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
           
            ViewData["ReturnUrl"] = Request.Query["ReturnUrl"];
            ViewData["Comerciais"] = context.ObterListaUtilizadores().Where(u => u.TipoUtilizador == 2).ToList();

            return View(context.ObterVisita(idVisita));
        }

        [Authorize(Roles = "Admin, Escritorio")]
        [HttpPost]
        public ActionResult Editar(Visita visita, string ReturnUrl)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            List<Visita> lstVisitas = new List<Visita>();

            lstVisitas.Add(visita);
            context.CriarVisitas(lstVisitas);

            return Redirect(ReturnUrl);
        }

        public ActionResult Visita(int idVisita, int IdComercial)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Escritorio")) IdComercial = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).Id;

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Visita visita = context.ObterVisita(idVisita);

            if (visita.IdComercial == IdComercial || User.IsInRole("Admin")) return View(visita);

            ViewData["ReturnUrl"] = Request.Query["ReturnUrl"];

            return Redirect(Request.Query["ReturnUrl"]);
        }

        [HttpPost]
        public ActionResult Visita(Visita visita)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            List<Visita> LstVisitas = new List<Visita> { visita };

            context.CriarVisitas(LstVisitas);

            return Redirect(Request.Query["ReturnUrl"]);
        }

        [Authorize(Roles = "Admin, Escritorio")]
        [HttpGet]
        public ActionResult Apagar(int idVisita, string ReturnUrl)
        {

            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            context.ApagarVisita(idVisita);

            return Redirect(ReturnUrl);
        }

        [HttpPost]
        public ActionResult AdicionarAnexo(List<IFormFile> files, int IdVisita, string ReturnUrl)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Visita visita = context.ObterVisita(IdVisita);

            EnviarNextCloud(files, ConfigurationManager.AppSetting["NextCloud:URL"], "Anexos", visita.Cliente.NomeCliente);

            return Redirect(Request.Query["ReturnUrl"]);
        }

        [HttpPost]
        public ActionResult AdicionarProposta(List<IFormFile> files, int IdVisita, string ReturnUrl, string data, string estado, string valor)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Visita visita = context.ObterVisita(IdVisita);
            List<Proposta> LstPropostas = new List<Proposta>();

            if (files.Count > 0)
            {

                LstPropostas.Add(new Proposta()
                {
                    Comercial = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)),
                    Visita = context.ObterVisita(IdVisita),
                    DataProposta = DateTime.Parse(data),
                    EstadoProposta = estado,
                    ValorProposta = valor,
                    UrlAnexo = ConfigurationManager.AppSetting["NextCloud:URL"] + visita.Cliente.NomeCliente + "/Propostas/" + files[0].FileName

                });

                context.CriarPropostas(LstPropostas);
                EnviarNextCloud(files, ConfigurationManager.AppSetting["NextCloud:URL"], "Propostas", visita.Cliente.NomeCliente);
            }

            return Redirect(Request.Query["ReturnUrl"]);
        }

        public async void EnviarNextCloud(List<IFormFile> files, string Url, string Path, string Folder)
        {
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await formFile.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    var clientParams = new WebDavClientParams
                    {
                        BaseAddress = new Uri(Url),
                        Credentials = new NetworkCredential(ConfigurationManager.AppSetting["NextCloud:User"], ConfigurationManager.AppSetting["NextCloud:Password"])
                    };
                    var client = new WebDavClient(clientParams);

                    await client.Mkcol(Folder + "/");
                    await client.Mkcol(Folder + "/" + Path + "/");

                    clientParams.BaseAddress = new Uri(clientParams.BaseAddress + Folder + "/" + Path + "/");
                    client = new WebDavClient(clientParams);

                    await client.PutFile(formFile.FileName, ms); // upload a resource

                }
            }
        }

        public async Task<ActionResult> ObterFicheiroNextCloud(string Url)
        {
            var clientParams = new WebDavClientParams
            {
                BaseAddress = new Uri(Url),
                Credentials = new NetworkCredential(ConfigurationManager.AppSetting["NextCloud:User"], ConfigurationManager.AppSetting["NextCloud:Password"])
            };

            var client = new WebDavClient(clientParams);

            using (var response = await client.GetRawFile(Url))
            using (var reader = new StreamReader(response.Stream))
            {
                var bytes = default(byte[]);
                using (var memstream = new MemoryStream())
                {
                    reader.BaseStream.CopyTo(memstream);
                    bytes = memstream.ToArray();
                }

                new FileExtensionContentTypeProvider().TryGetContentType(Url.Split('/').Last(), out string contentType);
                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = Url.Split('/').Last(),
                    Inline = false,
                    CreationDate = DateTime.Now,

                };
                Response.Headers.Add("Content-Disposition", cd.ToString());

                return File(bytes, contentType);
            }
        }
    }
}
