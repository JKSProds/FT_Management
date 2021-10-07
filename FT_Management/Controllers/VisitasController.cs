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
using WebDav;

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

            if (!User.IsInRole("Admin") && !User.IsInRole("Escritorio")) IdComercial = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).IdPHC;

            if (DataVisitas == null || DataVisitas == string.Empty) DataVisitas = DateTime.Now.ToString("dd-MM-yyyy");
            ViewData["DataVisitas"] = DataVisitas;

            List<Visita> ListaVisitas = context.ObterListaVisitas(IdComercial, DateTime.Parse(DataVisitas), DateTime.Parse(DataVisitas));
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
                    EstadoVisita = "Agendada"
                };
                List<Visita> lstVisitas = new List<Visita>();
                lstVisitas.Add(visita);

                context.CriarVisitas(lstVisitas);
                context.AdicionarLog(context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).NomeUtilizador, "Foi adicionada uma visita nova ao cliente: " + IdCliente, 5);

            return RedirectToAction("Index", "Visitas");

        }

        [Authorize(Roles = "Admin, Escritorio")]
        public ActionResult Editar(int idVisita)
        {
            ViewData["ReturnUrl"] = Request.Query["ReturnUrl"];
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

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
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            if (!User.IsInRole("Admin") && !User.IsInRole("Escritorio")) IdComercial = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value)).IdPHC;
            ViewData["ReturnUrl"] = Request.Query["ReturnUrl"];

            Visita visita = context.ObterVisita(idVisita);

            if (visita.IdComercial == IdComercial || User.IsInRole("Admin")) return View(visita);

            return Redirect(Request.Query["ReturnUrl"]);
        }
        [HttpPost]
        public ActionResult Visita(Visita visita)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

           

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

            EnviarNextCloud(files, "https://food-tech.cloud/remote.php/dav/files/jmonteiro/Dep. Comercial/", "Anexos", visita.Cliente.NomeCliente);

            return Redirect(Request.Query["ReturnUrl"]);
        }

        [HttpPost]
        public ActionResult AdicionarProposta(List<IFormFile> files, int IdVisita, string ReturnUrl, string data, string estado, string valor)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Visita visita = context.ObterVisita(IdVisita);

            EnviarNextCloud(files, "https://food-tech.cloud/remote.php/dav/files/jmonteiro/Dep. Comercial/", "Propostas", visita.Cliente.NomeCliente);

            return RedirectToAction("Editar", new { idVisita = IdVisita });
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
                        Credentials = new NetworkCredential("", "")
                    };
                    var client = new WebDavClient(clientParams);

                    await client.Mkcol(Folder + "/" + Path);

                    clientParams.BaseAddress = new Uri(clientParams.BaseAddress + Folder + "/" + Path + "/");
                    client = new WebDavClient(clientParams);

                    await client.PutFile(formFile.FileName, ms); // upload a resource
                }
            }
        }

    }
}
