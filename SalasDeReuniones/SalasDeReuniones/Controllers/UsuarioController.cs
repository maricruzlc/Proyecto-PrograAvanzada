using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SalasDeReuniones.Controllers
{
    public class UsuarioController : Controller
    {
        private ProyectoPAEntities context = new ProyectoPAEntities();
        // GET: Usuario
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Crear(usuario usuario)
        {
            if (ModelState.IsValid)
            {
                usuario.IdRol = 2;
                usuario.FechaCreacion = DateTime.Now;
                context.usuarios.Add(usuario);
                context.SaveChanges();
                return RedirectToAction("Index","Home");
            }

            return View(usuario);
        }
    }
}