using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SalasDeReuniones.Controllers
{
    [Authorize]
    public class SalaController : Controller
    {
        private ProyectoPAEntities context = new ProyectoPAEntities();
        // GET: Sala
        public ActionResult Index()
        {
            var Salas = context.salas.ToList();
            
            return View(Salas);
        }
       
        [HttpGet]
        public ActionResult Crear()
        {
            
            ViewBag.EquipoId = new SelectList(context.equipoes, "Id_Equipo", "nombre_equipo");
            return View();
        }

        [HttpPost]
        public ActionResult Crear(sala sala, List<int> EquipoId)
        {
            if (ModelState.IsValid)
            {
                context.salas.Add(sala);
                if (EquipoId != null)
                {
                    foreach (var equipoId in EquipoId)
                    {
                        context.equipo_salas.Add(new equipo_salas { IdSala = sala.IdSala, Id_Equipo = equipoId });
                    }
                }
                
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.EquipoId = new SelectList(context.equipoes, "Id_Equipo", "nombre_equipo");
            return View(sala);
        }

        [HttpGet]
        public ActionResult Editar(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            var sala = context.salas.Find(id);
            if (sala == null)
                return HttpNotFound();
            ViewBag.EquipoId = new SelectList(context.equipoes, "Id_Equipo", "nombre_equipo");
            return View(sala);
        }

        [HttpPost]
        public ActionResult Editar(sala sala,List<int> EquipoId)
        {
            if (ModelState.IsValid)
            {
                context.Entry(sala).State = EntityState.Modified;
                int salaId = sala.IdSala;
                var equipo_salas = context.equipo_salas.ToList();
                if (EquipoId != null)
                {
                    foreach (var equipo in context.equipo_salas)
                    {
                        if (equipo.IdSala == salaId)
                        {
                            context.equipo_salas.Remove(equipo);

                        }
                    }
                    foreach (var equipoId in EquipoId)
                    {
                        context.equipo_salas.Add(new equipo_salas { IdSala = salaId, Id_Equipo = equipoId });
                    }
                }
                else
                {
                    foreach (var equipo in context.equipo_salas)
                    {
                        if (equipo.IdSala == salaId)
                        {
                            context.equipo_salas.Remove(equipo);

                        }
                    }
                }
                
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.EquipoId = new SelectList(context.equipoes, "Id_Equipo", "nombre_equipo");
            return View(sala);
        }

        [HttpGet]
        public ActionResult Eliminar(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            var sala = context.salas.SingleOrDefault(l => l.IdSala == id);
            if (sala == null)
                return HttpNotFound();
            return View(sala);
        }

        [HttpPost, ActionName("Eliminar")]
        public ActionResult ConfirmarEliminar(int? id)
        {
            var sala = context.salas.Find(id);
            context.salas.Remove(sala);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}