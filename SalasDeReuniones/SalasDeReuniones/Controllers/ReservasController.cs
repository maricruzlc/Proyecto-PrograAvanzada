using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace SalasDeReuniones.Controllers
{
    public class ReservasController : Controller
    {
        private ProyectoPAEntities context = new ProyectoPAEntities();



        [Authorize]
        public ActionResult Index(bool? soloFuturas = false, bool? soloActuales = false)
        {
            Debug.WriteLine("Usuario en Index: " + User.Identity.Name);
            Debug.WriteLine("Es administrador en IndexReservas: " + User.IsInRole("Administrador"));
            var idUsuario = Session["UsuarioId"] as int?;
            if (!idUsuario.HasValue)
            {
                return RedirectToAction("Login", "Usuario");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Usuario ID: {idUsuario.Value}");
            }

            var reservas = context.reservas.AsQueryable();

            // Si es administrador, mostrar todas las reservas
            if (User.IsInRole("Administrador"))
            {
                reservas = context.reservas; // No hay filtro por usuario
            }
            else
            {
                // Para usuarios normales, se filtra por IdUsuario
                reservas = reservas.Where(r => r.IdUsuario == idUsuario.Value);
            }

            // Filtrar reservas futuras o pasadas
            if (soloFuturas.HasValue && soloFuturas.Value)
            {
               reservas = reservas.Where(r => r.fecha.HasValue && r.fecha.Value > DateTime.Today);
            }
            else
            {
                reservas = reservas.Where(r => r.fecha.HasValue && r.fecha.Value <= DateTime.Today);
            }

      






            // Cargar datos de las reservas junto con los usuarios
            var reservasConUsuarios = reservas.Include(r => r.usuario).ToList();

            return View(reservasConUsuarios);
        }


        public ActionResult Crear()
        {
            return View();
        }

        [HttpGet]
        public JsonResult HorasDisponibles(int idSala, DateTime fecha)
        {
            //Busca la sala por IdSala en SQL
            var sala = context.salas.FirstOrDefault(s => s.IdSala == idSala);

            if (sala?.hora_inicio_dispo.HasValue == true && sala?.hora_fin_dispo.HasValue == true)
            {
                //Acá se alamcenan las horas disponibles de la sala
                var horasDisponibles = new List<string>(); // Lista para horas disponibles
                DateTime horaInicioSala = DateTime.Today.Add(sala.hora_inicio_dispo.Value);
                DateTime horaFinSala = DateTime.Today.Add(sala.hora_fin_dispo.Value);


                //Genera la lista de horas con un intervalo de 1 hora
                for (var hora = horaInicioSala; hora < horaFinSala; hora = hora.Add(TimeSpan.FromHours(1)))
                {
                    horasDisponibles.Add(hora.ToString(@"HH\:mm"));
                }

                //Acá se obtienen las reservas existentes para el día seleccionado
                var reservas = context.reservas
                    .Where(r => r.IdSala == idSala && r.fecha == fecha.Date)
                    .Select(r => new { r.hora_inicio, r.hora_fin })
                    .ToList();

                //Esto Filtra y elimina las horas ocupadas por las reservas
                foreach (var reserva in reservas)
                {
                    if (reserva.hora_inicio.HasValue && reserva.hora_fin.HasValue)
                    {
                        // Convertir las horas de las reservas a formato HH:mm
                        var horaInicioStr = reserva.hora_inicio.Value.ToString(@"hh\:mm");
                        var horaFinStr = reserva.hora_fin.Value.ToString(@"hh\:mm");

                        // Usar TimeSpan.ParseExact para analizar correctamente las horas.
                        TimeSpan horaInicioTS = TimeSpan.ParseExact(horaInicioStr, @"hh\:mm", null);
                        TimeSpan horaFinTS = TimeSpan.ParseExact(horaFinStr, @"hh\:mm", null);

                        horasDisponibles.RemoveAll(h =>
                            TimeSpan.ParseExact(h, @"hh\:mm", null) >= horaInicioTS
                            && TimeSpan.ParseExact(h, @"hh\:mm", null) < horaFinTS);
                    }
                }

                //Asegura de que las horas de inicio y fin estén dentro del rango de la sala
                horasDisponibles = horasDisponibles.Where(hora =>
                    DateTime.ParseExact(hora, "HH:mm", null) >= horaInicioSala && DateTime.ParseExact(hora, "HH:mm", null) <= horaFinSala)
                    .ToList();

                return Json(new { success = true, data = horasDisponibles }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "La sala no tiene horas configuradas correctamente." }, JsonRequestBehavior.AllowGet);
            }
        }





        [HttpPost]
        public ActionResult CrearReserva(int idSala, DateTime fecha, string horaInicio, string horaFin, string detalle)
        {

            try
            {
                // Convierte las horas de strings a TimeSpan
                TimeSpan nuevaHoraInicio = TimeSpan.Parse(horaInicio);
                TimeSpan nuevaHoraFin = TimeSpan.Parse(horaFin);

                if (nuevaHoraInicio >= nuevaHoraFin)
                {
                    return Json(new { success = false, message = "La hora de inicio debe ser menor que la hora de fin." });
                }

                // Verifica que no choquen los horarios en la base de datos
                var conflicto = context.reservas
                    .Where(r => r.IdSala == idSala && r.fecha == fecha.Date)
                    .Any(r => nuevaHoraInicio < r.hora_fin && nuevaHoraFin > r.hora_inicio);

                //Obtiene la sala para verificar su rango de horas
                var sala = context.salas.FirstOrDefault(s => s.IdSala == idSala);

                if (sala == null)
                {
                    return Json(new { success = false, message = "La sala seleccionada no existe." });
                }

                //Verifica que las horas de la reserva esten dentro del rango de la sala
                if (nuevaHoraInicio < sala.hora_inicio_dispo || nuevaHoraFin > sala.hora_fin_dispo)
                {
                    return Json(new { success = false, message = $"El horario debe estar dentro del rango de la sala: {sala.hora_inicio_dispo} - {sala.hora_fin_dispo}." });
                }
                if (conflicto)
                {
                    return Json(new { success = false, message = "El horario solicitado ya está reservado." });
                }

                var idUsuario = Session["UsuarioId"] as int?;

                if (!idUsuario.HasValue)
                {
                    return Json(new { success = false, message = "El usuario no está logueado." });
                }

                //Crea una nueva reserva
                var nuevaReserva = new reserva
                {
                    IdSala = idSala,
                    fecha = fecha.Date,
                    hora_inicio = nuevaHoraInicio,
                    hora_fin = nuevaHoraFin,
                    detalle = detalle,
                    IdUsuario = idUsuario.Value 

                };
                //Acá se guarda la reserva
                context.reservas.Add(nuevaReserva);
                context.SaveChanges();

                return Json(new { success = true, message = "Reserva creada exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al crear la reserva: " + ex.Message });
            }
        }



        [HttpGet]
        public JsonResult ObtenerSalas(DateTime fecha)
        {
            try
            {
                var salasDisponibles = context.salas
                    .Where(s => s.hora_inicio_dispo.HasValue && s.hora_fin_dispo.HasValue)
                    .ToList()
                    .Where(sala =>
                    {
                        DateTime horaInicioSala = DateTime.Today.Add(sala.hora_inicio_dispo.Value);
                        DateTime horaFinSala = DateTime.Today.Add(sala.hora_fin_dispo.Value);

                        var horasDisponibles = new List<DateTime>();
                        for (var hora = horaInicioSala; hora < horaFinSala; hora = hora.Add(TimeSpan.FromHours(1)))
                        {
                            horasDisponibles.Add(hora);
                        }

                        var reservas = context.reservas
                            .Where(r => r.IdSala == sala.IdSala && r.fecha == fecha.Date)
                            .Select(r => new { r.hora_inicio, r.hora_fin })
                            .ToList();

                        foreach (var reserva in reservas)
                        {
                            if (reserva.hora_inicio.HasValue && reserva.hora_fin.HasValue)
                            {
                                TimeSpan inicioReserva = reserva.hora_inicio.Value;
                                TimeSpan finReserva = reserva.hora_fin.Value;

                                horasDisponibles.RemoveAll(h =>
                                    h.TimeOfDay >= inicioReserva && h.TimeOfDay < finReserva);
                            }
                        }

                        return horasDisponibles.Any();
                    })
                    .Select(s => new { s.IdSala, Nombre = s.nombre })
                    .ToList();

                return Json(salasDisponibles, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //Log del error
                System.Diagnostics.Debug.WriteLine("Error en ObtenerSalas: " + ex.Message);

                //Retorna un mensaje de error
                Response.StatusCode = 500;
                return Json(new { error = "Ocurrió un error al procesar la solicitud." }, JsonRequestBehavior.AllowGet);
            }
        }



        [Authorize]
        public ActionResult Editar(int id)
        {
            var idUsuario = Session["UsuarioId"] as int?;
            if (!idUsuario.HasValue)
            {
                return RedirectToAction("Login", "Usuario");
            }

            //Se obtiene la reserva
            var reserva = context.reservas.Include(r => r.usuario).Include(r => r.sala)
                                          .FirstOrDefault(r => r.IdReserva == id);

            //Se Verifica que la reserva existe y el usuario tiene permisos para editarla
            if (reserva == null || (reserva.IdUsuario != idUsuario && !User.IsInRole("Administrador")))
            {
                return HttpNotFound();
            }

            //Obtiene la lista de usuarios 
            var usuarios = context.usuarios.ToList();
            ViewBag.Usuarios = new SelectList(usuarios, "IdUsuario", "Nombre");

            //Obtiene la lista de salas
            var salas = context.salas.ToList(); 
            ViewBag.Salas = new SelectList(salas, "IdSala", "Nombre");
            return View(reserva);
        }



        [HttpPost]
        [Authorize]
        public ActionResult Editar(int id, reserva reservaModificada)
        {
     
            var idUsuario = Session["UsuarioId"] as int?;
            if (!idUsuario.HasValue)
            {
                return RedirectToAction("Login", "Usuario");
            }

            //Recupera la reserva 
            var reserva = context.reservas.FirstOrDefault(r => r.IdReserva == id);
            if (reserva == null)
            {
                return HttpNotFound();
            }

            //Verifica los permisos del usuario
            if (reserva.IdUsuario != idUsuario && !User.IsInRole("Administrador"))
            {
                // Si el usuario no tiene permisos para editar esta reserva
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            //Si el modelo es válido, se actualiza la reserva
            if (ModelState.IsValid)
            {
                reserva.IdUsuario = reservaModificada.IdUsuario;
                reserva.fecha = reservaModificada.fecha;
                reserva.hora_inicio = reservaModificada.hora_inicio;
                reserva.hora_fin = reservaModificada.hora_fin;
                reserva.IdSala = reservaModificada.IdSala;
                reserva.detalle = reservaModificada.detalle;

                context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(reserva);
        }

        // Acción para eliminar una reserva
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id)
        {
            // Busca la reserva 
            var reserva = context.reservas.FirstOrDefault(r => r.IdReserva == id);

            if (reserva == null)
            {
               // Sino se encuentra la reserva, muestra un mensaje de error
                return HttpNotFound();
            }

            //Eliminar la reserva
            context.reservas.Remove(reserva);
            context.SaveChanges(); 

      
            return RedirectToAction("Index"); 
        }
    }
}


