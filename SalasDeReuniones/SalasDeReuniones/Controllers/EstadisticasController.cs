using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SalasDeReuniones.Controllers
{
    public class EstadisticasController : Controller
    {
        private ProyectoPAEntities context = new ProyectoPAEntities();
        [Authorize]
        public ActionResult Dashboard()
        {
            var ocupacionPorSala = context.salas
                .Select(s => new
                {
                    Sala = s.nombre,
                    MinutosReservados = context.reservas
                        .Where(r => r.IdSala == s.IdSala)
                        .Sum(r => (int?)System.Data.Entity.SqlServer.SqlFunctions.DateDiff("MINUTE", r.hora_inicio, r.hora_fin)) ?? 0,
                    MinutosDisponibles = System.Data.Entity.SqlServer.SqlFunctions.DateDiff("MINUTE", s.hora_inicio_dispo, s.hora_fin_dispo) ?? 0
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
                       .Average(r => (double?)System.Data.Entity.SqlServer.SqlFunctions.DateDiff("MINUTE", r.hora_inicio, r.hora_fin)) ?? 0
               })
               .ToList();



            ViewData["OcupacionPorSala"] = ocupacionPorSala;

            ViewData["TotalReservasPorSala"] = totalReservasPorSala;


            ViewData["PromedioDuracionReservas"] = promedioDuracionReservas;

            return View();
        }


    }
}
