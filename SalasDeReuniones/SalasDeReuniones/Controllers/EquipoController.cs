using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace SalasDeReuniones.Controllers
{
    [Authorize]
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
        [HttpGet]
        public ActionResult Editar(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            var equipo = context.equipoes.Find(id);
            if (equipo == null)
                return HttpNotFound();

            return View(equipo);
        }

        [HttpPost]
        public ActionResult Editar(equipo equipoes)
        {
            if (ModelState.IsValid)
            {


                context.Entry(equipoes).State = EntityState.Modified;
                context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(equipoes);
        }

        [HttpGet]
        public ActionResult Eliminar(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            var equipos = context.equipoes.SingleOrDefault(l => l.Id_Equipo == id);
            if (equipos == null)
                return HttpNotFound();
            return View(equipos);

        }
        [HttpPost, ActionName("Eliminar")]
        public ActionResult ConfirmarEliminar(int id)
        {
            var equipos = context.equipoes.Find(id);
            context.equipoes.Remove(equipos);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}