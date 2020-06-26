using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FT_Management.Controllers
{
    public class FolhasObraController : Controller
    {
        // GET: FolhasObraController
        public ActionResult Index()
        {
            return View();
        }

        // GET: FolhasObraController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: FolhasObraController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FolhasObraController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: FolhasObraController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: FolhasObraController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: FolhasObraController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: FolhasObraController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
