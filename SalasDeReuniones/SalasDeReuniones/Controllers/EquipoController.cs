using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SalasDeReuniones.Controllers
{
    public class EquipoController : Controller
    {
        private ProyectoPAEntities context = new ProyectoPAEntities();
        // GET: Equipo
        public ActionResult Index()
        {
            var Equipos = context.equipoes.ToList();
            return View(Equipos);
        }

        [HttpGet]
        public ActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Crear(equipo equipo)
        {
            if (ModelState.IsValid)
            {
                context.equipoes.Add(equipo);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            
            return View(equipo);
        }
    }
}