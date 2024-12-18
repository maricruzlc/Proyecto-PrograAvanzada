using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity.SqlServer;
using System.Data.Entity;
using System.Diagnostics;

namespace SalasDeReuniones.Controllers
{
    [Authorize]
    public class EstadisticasController : Controller
    {
        private ProyectoPAEntities context = new ProyectoPAEntities();

        public ActionResult Dashboard()
        {
            Debug.WriteLine("Usuario en Dashboards: " + User.Identity.Name);
            Debug.WriteLine("Es administrador en Dashboards: " + User.IsInRole("Administrador"));

            var idUsuario = Session["UsuarioId"] as int?;
            if (!idUsuario.HasValue)
            {
                return RedirectToAction("Login", "Usuario");
            }

            //  si el usuario tiene el rol de "Administrador"
            if (!User.IsInRole("Administrador"))
            {
                // Si no es administrador, redirige a login
                return RedirectToAction("Login", "Usuario");
            }

            // Verifica que idUsuario tiene un valor correcto
            Debug.WriteLine($"Usuario ID: {idUsuario.Value}");

            var ocupacionPorSala = context.salas
                .Select(s => new
                {
                    Sala = s.nombre,
                    MinutosReservados = context.reservas
                        .Where(r => r.IdSala == s.IdSala)
                        .Sum(r => (int?)SqlFunctions.DateDiff("MINUTE", r.hora_inicio, r.hora_fin)) ?? 0,
                    MinutosDisponibles = SqlFunctions.DateDiff("MINUTE", s.hora_inicio_dispo, s.hora_fin_dispo) ?? 0
                })
                .ToList()
                .Select(s => new
                {
                    Sala = s.Sala,
                    PorcentajeOcupacion = s.MinutosDisponibles > 0
                        ? (double)s.MinutosReservados / s.MinutosDisponibles * 100
                        : 0,
                    MinutosReservados = s.MinutosReservados
                })
                .ToList();

            var totalReservasPorSala = context.salas
                .Select(s => new
                {
                    Sala = s.nombre,
                    TotalReservas = context.reservas.Count(r => r.IdSala == s.IdSala)
                })
                .ToList();

            var promedioDuracionReservas = context.salas
                .Select(s => new
                {
                    Sala = s.nombre,
                    PromedioDuracion = context.reservas
                        .Where(r => r.IdSala == s.IdSala)
                        .Average(r => (double?)SqlFunctions.DateDiff("MINUTE", r.hora_inicio, r.hora_fin)) ?? 0
                })
                .ToList();

            ViewData["OcupacionPorSala"] = ocupacionPorSala;
            ViewData["TotalReservasPorSala"] = totalReservasPorSala;
            ViewData["PromedioDuracionReservas"] = promedioDuracionReservas;

            return View();
        }

    }
}
