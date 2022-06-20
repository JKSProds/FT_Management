using Microsoft.AspNetCore.Mvc;
using FT_Management.Models;
using Custom;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using WebDav;
using System.Net;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Comercial, Escritorio")]
    public class ContactosController : Controller
    {
        public IActionResult Index(int? page, string filter, string area, int idcomercial)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            
            List<Utilizador> LstUtilizadores = context.ObterListaComerciais().ToList();
            ViewBag.Comerciais = LstUtilizadores;
                LstUtilizadores.Insert(0, new Utilizador() { Id= 0, NomeCompleto="Todos"});
            ViewBag.ListaComerciais = LstUtilizadores;

            int pageSize = 50;
            var pageNumber = page ?? 1;

            List<String> LstAreasNegocio = context.ObterListaAreasNegocio().ToList();
            LstAreasNegocio.Insert(0, "Todos");

            ViewBag.AreasNegocio = LstAreasNegocio.Select(l => new SelectListItem() { Value = l, Text = l });

            if (filter == null) { filter = ""; }
            if (area == null) { area = ""; }

            ViewData["filter"] = filter;
            ViewData["area"] = area;
            ViewData["idcomercial"] = idcomercial;


            if (idcomercial > 0) return View(context.ObterListaContactos(filter).Where(c => c.AreaNegocio.Contains(area)).Where(u => u.Comercial.Id == idcomercial).ToPagedList(pageNumber, pageSize));
            return View(context.ObterListaContactos(filter).Where(c => c.AreaNegocio.Contains(area)).ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public string ObterContacto(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Contacto contacto = context.ObterContacto(id);
            string res= "";
            res += "<div class=\"mb-3\"><label>Nome da Empresa</label><div class=\"field has-addons\"><input type=\"text\" class=\"input\" value='" + contacto.NomeContacto + "' readonly><a class=\"button is-outline is-warning\" onclick=MostrarCliente() type=\"button\"><i class=\"fas fa-eye float-left\"></i></a></div></div>";
            res += "<div class=\"mb-3\"><label>Contacto</label><input type=\"text\" class=\"input\" value='" + contacto.PessoaContacto + "' readonly></div>";
            res += "<div class=\"mb-3\"><label>Cargo</label><input type=\"text\" class=\"input\" value='" + contacto.CargoPessoaContacto + "' readonly></div>";
            res += "<div class=\"mb-3\"><label>Email</label><div class=\"field has-addons\"><input type=\"text\" class=\"input\" value='" + contacto.EmailContacto + "' readonly><a class=\"button is-primary "+(contacto.EmailContacto.Length == 0 ? "disabled" : "")+" \" href=\"mailto:" +contacto.EmailContacto+ "\" type=\"button\"><i class=\"fas fa-envelope float-left\"></i></a></div></div>";
            res += "<div class=\"mb-3\"><label>Telemóvel</label><div class=\"field has-addons\"><input type=\"text\" class=\"input\" value='" + contacto.TelefoneContacto + "' readonly><a class=\"button is-info " + (contacto.TelefoneContacto.Length < 9 ? "disabled" : "") + " \" href=\"tel:" + contacto.TelefoneContacto + "\" type=\"button\"><i class=\"fas fa-phone float-left\"></i></a></div></div>";
            res += "<div class=\"mb-3\"><label>Morada</label><div class=\"field has-addons\"><input type=\"text\" class=\"input\" value='" + contacto.MoradaContacto + "' readonly><a class=\"button is-primary is-outlined " + (contacto.MoradaContacto.Length == 0 ? "disabled" : "") + " \" href=\"https://maps.google.com/?daddr=" + contacto.MoradaContacto + "\" type=\"button\"><i class=\"fas fa-location-arrow float-left\"></i></a></div></div>";
            res += "<div class=\"mb-3\"><label>NIF</label><input type=\"text\" class=\"input\" value='" + contacto.NIFContacto + "' readonly></div>";
            res += "<div class=\"mb-3\"><label>Data de Contacto</label><input type=\"text\" class=\"input\" value='" + contacto.DataContacto.ToShortDateString() + "' readonly></div>";
            res += "<div class=\"mb-3\"><label>Tipo de Contacto</label><input type=\"text\" class=\"input\" value='" + contacto.TipoContacto + "' readonly></div>";
            res += "<div class=\"mb-3\"><label>Área de Negócio</label><input type=\"text\" class=\"input\" value='" + contacto.AreaNegocio + "' readonly></div>";
            res += "<div class=\"mb-3\"><label>Criado por</label><input type=\"text\" class=\"input\" value='" + contacto.Utilizador.NomeCompleto + "' readonly></div>";
            if (this.User.IsInRole("Admin")) res += "<div class=\"mb-3\"><label>Comercial Associado</label><div class=\"field has-addons\"><input type=\"text\" class=\"input\" value='" + contacto.Comercial.NomeCompleto + "' readonly><a class=\"button is-primary is-outline\" onclick=AssociarComercial() type=\"button\"><i class=\"fas fa-list float-left\"></i></a></div></div>";
            res += "<div class=\"mb-3\"><label>Observações</label><textarea type=\"text\" class=\"textarea\" rows=\"6\" readonly>" + contacto.Obs + "</textarea></div>";

            return res;
        }

        public IActionResult Adicionar()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            ViewBag.AreasNegocio = context.ObterListaAreasNegocio().ToList().Select(l => new SelectListItem() { Value = l, Text = l });

            return View();
        }

        [Authorize(Roles = "Admin, Escritorio, Comercial")]
        public IActionResult Editar(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Contacto contacto = context.ObterContacto(id);
            ViewBag.AreasNegocio = context.ObterListaAreasNegocio().ToList().Select(l => new SelectListItem() { Value = l, Text = l });

            if (!this.User.IsInRole("Admin") && !this.User.IsInRole("Escritorio") && contacto.IdComercial != int.Parse(this.User.Claims.First().Value.ToString())) return Redirect("~/Home/AcessoNegado");

            return View(contacto);
        }

        [Authorize(Roles = "Admin, Escritorio, Comercial")]
        [HttpPost]
        public IActionResult Editar(Contacto contacto)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            contacto.NIFContacto = contacto.NIFContacto is null ? "" : contacto.NIFContacto;
            contacto.EmailContacto = contacto.EmailContacto is null ? "" : contacto.EmailContacto;
            contacto.MoradaContacto = contacto.MoradaContacto is null ? "" : contacto.MoradaContacto;
            contacto.Obs = contacto.Obs is null ? "" : contacto.Obs;
            contacto.CargoPessoaContacto = contacto.CargoPessoaContacto is null ? "" : contacto.CargoPessoaContacto;

            context.CriarContactos(new List<Contacto> { contacto });

            return RedirectToAction("Index");
        }

        public string AdicionarObservacao(int idcontacto, string obs, int lembrete, string datalembrete)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Utilizador c = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value.ToString()));

            HistoricoContacto hC = new HistoricoContacto() {
                IdContacto = idcontacto,
                Data = DateTime.Now,
                IdComercial = c,
                Obs = obs
            };

            if (lembrete == 1)
            {
                Visita v = new Visita()
                {
                    DataVisita = DateTime.Parse(datalembrete),
                    Cliente = new Cliente() { IdCliente=0, IdLoja =0},
                    Contacto = new Contacto() { IdContacto = idcontacto},
                    IdComercial = c.Id,
                    ResumoVisita = "Lembrete criado:\r\n" + obs,
                    EstadoVisita = "Agendado"
                };
                context.CriarVisitas(new List<Visita> { v });
            }

            context.CriarHistoricoContacto(hC);
            

            return "OK";
        }

        [HttpPost]
        public ActionResult Adicionar(Contacto c)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            if (context.ExisteNIFDuplicadoContacto(c.NIFContacto)) ModelState.AddModelError("NIFContacto", "NIF Duplicado");
            if (ModelState.IsValid)
            {
                c.IdContacto = context.ObterUltimoID("dat_contactos", "Id") + 1;
                c.CheckNull();
                c.DataContacto = DateTime.Now;
                c.IdUtilizador = int.Parse(this.User.Claims.First().Value.ToString());
                c.IdComercial = 24; //Id do Artur Carneiro
                c.NIFContacto.Replace(" ", "");
                c.ValidadoPorAdmin = false;
                c.URL = "https://food-tech.cloud/index.php/apps/files/?dir=/Dep.%20Comercial/Contactos/[" + c.NomeContacto + "] " + c.PessoaContacto;

                c.NIFContacto = c.NIFContacto is null ? "" : c.NIFContacto;
                c.EmailContacto = c.EmailContacto is null ? "" : c.EmailContacto;
                c.MoradaContacto = c.MoradaContacto is null ? "" : c.MoradaContacto;
                c.Obs = c.Obs is null ? "" : c.Obs;
                c.CargoPessoaContacto = c.CargoPessoaContacto is null ? "" : c.CargoPessoaContacto;

                context.CriarContactos(new List<Contacto> { c });

                context.CriarHistoricoContacto(new HistoricoContacto()
                {
                    IdContacto = c.IdContacto,
                    Data = DateTime.Now,
                    IdComercial = context.ObterUtilizador(int.Parse(this.User.Claims.First().Value.ToString())),
                    Obs = "Criação do Contacto"
                });

                return RedirectToAction("Index");
            }
            ViewBag.AreasNegocio = context.ObterListaAreasNegocio().ToList().Select(l => new SelectListItem() { Value = l, Text = l });

            return View(c);
        }

        [Authorize(Roles = "Admin, Escritorio")]
        [HttpPost]
        public ActionResult AssociarComercial(string idcontacto, string idcomercial)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Contacto c = context.ObterContacto(int.Parse(idcontacto));
            c.IdComercial = int.Parse(idcomercial);
            c.ValidadoPorAdmin = true;

            context.CriarContactos(new List<Contacto> { c });
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin, Escritorio")]
        public ActionResult Apagar(int Id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            context.ApagarContacto(Id);

            return RedirectToAction("Index");
        }

        public string MostrarCliente(int Id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            Cliente c = context.ObterClienteContribuinte(context.ObterContacto(Id).NIFContacto);

            if (c.IdCliente == 0) throw new Exception();

            return "/Clientes/Cliente?IdCliente=" + c.IdCliente + "&IdLoja=" + c.IdLoja;
        }

        [HttpPost]
        public JsonResult AdicionarAnexo(List<IFormFile> files, string NomeEmpresa, string NomeCliente)
        {
            if (String.IsNullOrEmpty(NomeEmpresa) || String.IsNullOrEmpty(NomeCliente)) return Json("nok");
            EnviarNextCloud(files, ConfigurationManager.AppSetting["NextCloud:URL"], "[" + NomeEmpresa + "] " + NomeCliente, "Contactos");

            return Json("ok");
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

    }
}
