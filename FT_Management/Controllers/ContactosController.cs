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

namespace FT_Management.Controllers
{
    [Authorize(Roles = "Admin, Comercial, Escritorio")]
    public class ContactosController : Controller
    {
        public IActionResult Index()
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            return View(context.ObterListaContactos());
        }

        [HttpPost]
        public string ObterContacto(int id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

            Contacto contacto = context.ObterContacto(id);
            string res= "<div class=\"mb-3\"><label>Nome da Empresa</label><input type=\"text\" class=\"form-control\" value='"+contacto.NomeContacto+ "' readonly></div>";
            res += "<div class=\"mb-3\"><label>Contacto</label><input type=\"text\" class=\"form-control\" value='" + contacto.PessoaContacto + "' readonly></div>";
            res += "<div class=\"mb-3\"><label>Email</label><div class=\"input-group\"><input type=\"text\" class=\"form-control\" value='" + contacto.EmailContacto + "' readonly><a class=\"btn btn-outline-secondary "+(contacto.EmailContacto.Length == 0 ? "disabled" : "")+" \" href=\"mailto:" +contacto.EmailContacto+ "\" type=\"button\"><i class=\"fas fa-envelope float-left\" style=\"margin-top:5px\"></i></a></div></div>";
            res += "<div class=\"mb-3\"><label>Telemóvel</label><div class=\"input-group\"><input type=\"text\" class=\"form-control\" value='" + contacto.TelefoneContacto + "' readonly><a class=\"btn btn-outline-secondary " + (contacto.TelefoneContacto.Length < 9 ? "disabled" : "") + " \" href=\"tel:" + contacto.TelefoneContacto + "\" type=\"button\"><i class=\"fas fa-phone-alt float-left\" style=\"margin-top:5px\"></i></a></div></div>";
            res += "<div class=\"mb-3\"><label>Morada</label><div class=\"input-group\"><input type=\"text\" class=\"form-control\" value='" + contacto.MoradaContacto + "' readonly><a class=\"btn btn-outline-secondary " + (contacto.MoradaContacto.Length == 0 ? "disabled" : "") + " \" href=\"https://maps.google.com/?daddr=" + contacto.MoradaContacto + "\" type=\"button\"><i class=\"fas fa-location-arrow float-left\" style=\"margin-top:5px\"></i></a></div></div>";
            res += "<div class=\"mb-3\"><label>NIF</label><input type=\"text\" class=\"form-control\" value='" + contacto.NIFContacto + "' readonly></div>";
            res += "<div class=\"mb-3\"><label>Data de Contacto</label><input type=\"text\" class=\"form-control\" value='" + contacto.DataContacto.ToShortDateString() + "' readonly></div>";
            res += "<div class=\"mb-3\"><label>Tipo de Contacto</label><input type=\"text\" class=\"form-control\" value='" + contacto.TipoContacto + "' readonly></div>";
            res += "<div class=\"mb-3\"><label>Observações</label><textarea type=\"text\" class=\"form-control\" rows=\"6\" readonly>" + contacto.Obs + "</textarea></div>";

            return res;
        }

        public IActionResult Novo()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Novo(string txtNomeEmpresa, string txtNomeCliente, string txtContacto, string txtEmail, string txtNIF, string txtMorada, string txtObs, string txtTipoContacto)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            
            Contacto contacto = new Contacto {
                NomeContacto = txtNomeEmpresa ?? "",
                PessoaContacto = txtNomeCliente ?? "",
                TelefoneContacto = txtContacto ?? "",
                EmailContacto = txtEmail ?? "",
                NIFContacto = txtNIF ?? "",
                MoradaContacto = txtMorada ?? "",
                Obs = txtObs ?? "",
                TipoContacto = txtTipoContacto,
                URL = "https://food-tech.cloud/index.php/apps/files/?dir=/FT_Management/Contactos/[" + txtNomeEmpresa + "] " + txtNomeCliente,
                DataContacto = DateTime.Now
            };  

            context.CriarContactos(new List<Contacto> { contacto });
            return RedirectToAction("Index");
        }

        public ActionResult Apagar(int Id)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;
            context.ApagarContacto(Id);

            return RedirectToAction("Index");
        }
            [HttpPost]
        public JsonResult AdicionarAnexo(List<IFormFile> files, string NomeEmpresa, string NomeCliente)
        {
            FT_ManagementContext context = HttpContext.RequestServices.GetService(typeof(FT_ManagementContext)) as FT_ManagementContext;

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
