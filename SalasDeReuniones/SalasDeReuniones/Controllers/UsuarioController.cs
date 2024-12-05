using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SalasDeReuniones.Controllers
{
    public class UsuarioController : Controller
    {
        private ProyectoPAEntities context = new ProyectoPAEntities();

        // GET: Usuario
        public ActionResult Index()
        {
            var usuarios = context.usuarios.ToList();
            return View(usuarios);
        }

        //GET Create
        [HttpGet]
        public ActionResult Crear()
        {
            ViewBag.Roles = new SelectList(context.roles, "IdRol", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(usuario newUser)
        {
            if (ModelState.IsValid)
            {
                if (context.usuarios.Any(x => x.Usuario1 == newUser.Usuario1))
                {
                    ModelState.AddModelError("Usuario1", "Este nombre de usuario ya existe");
                    ViewBag.Roles = new SelectList(context.roles, "IdRol", "Nombre");
                    return View(newUser);
                }

                //Asigna automaticamente el rol Usuario
                var rolUsuario = context.roles.FirstOrDefault(r => r.Nombre == "Usuario");
                if (rolUsuario != null)
                {
                    newUser.role = rolUsuario;  //Asigna el rol Usuario
                    newUser.IdRol = rolUsuario.IdRol;
                }

                // A la contraseña en el usuario
                var hasher = new PasswordHasher<usuario>();
                newUser.SetPassword(newUser.contrasena); // Establece la contraseña hasheada



                // Guarda el nuevo usuario en la base de datos
                context.usuarios.Add(newUser);
                context.SaveChanges();

                return RedirectToAction("Login", "Usuario");
            }

            ViewBag.Roles = new SelectList(context.roles, "IdRol", "Nombre");
            return View(newUser);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(String Usuario1, String contrasena)
        {
            //Verifica si el usuario existe en la base de datos
            var usuario = context.usuarios.FirstOrDefault(x => x.Usuario1 == Usuario1);

            if (usuario != null)
            {
                var hasher = new PasswordHasher<usuario>();
                //Valida la contraseña usando el hasher
                if (hasher.VerifyHashedPassword(usuario, usuario.contrasena, contrasena) == PasswordVerificationResult.Success)
                {
                    FormsAuthentication.SetAuthCookie(usuario.Usuario1, false);

                    Session["UsuarioId"] = usuario.IdUsuario;
                    Session["UsuarioNombre"] = usuario.Nombre;
                    Session["UsuarioRol"] = usuario.role.Nombre;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("Contraseña", "La contraseña es incorrecta");
                }
            }
            else
            {
                ModelState.AddModelError("Usaurio1", "El nombre no existe");

            }
            return View();


        }

        private string GenerarToken(string email)
        {
            var Token = "Token";
            var tiempoActual = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var datos = $"{email}-{tiempoActual}-{Token}";  // Incluye la información en texto claro

            // Codificar la cadena en base64
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(datos));
        }

        public ActionResult SendToken(string email)
        {
            System.Diagnostics.Debug.WriteLine("Comenzando el proceso de envío de correo...");

            // Verificar si el usuario existe
            var usuario = context.usuarios.FirstOrDefault(x => x.Correo == email);
            if (usuario != null)
            {
                System.Diagnostics.Debug.WriteLine($"Usuario encontrado: Nombre = {usuario.Nombre}, Correo = {usuario.Correo}");

                // Generar token
                var token = GenerarToken(email);
                System.Diagnostics.Debug.WriteLine($"Token generado para el correo {email}: {token}");

                // Crear enlace para restablecer contraseña
                var enlace = $"http://localhost:49575/Usuario/Restablecer?token={HttpUtility.UrlEncode(token)}";
                System.Diagnostics.Debug.WriteLine($"Enlace generado: {enlace}");

                try
                {
                    // Configurar el correo
                    System.Diagnostics.Debug.WriteLine("Configurando el mensaje de correo...");
                    var correo = new MailMessage
                    {
                        From = new MailAddress("equiros30766@ufide.ac.cr", "Salas El Mejor"),
                        Subject = "Recuperación de contraseña",
                        Body = $"<p>Has solicitado restablecer tu contraseña. Haz clic en el siguiente enlace:</p><a href='{enlace}'>Restablecer contraseña</a>",
                        IsBodyHtml = true
                    };
                    correo.To.Add(email);
                    System.Diagnostics.Debug.WriteLine($"Correo configurado correctamente. Destinatario: {email}");

                    // Configurar el cliente SMTP
                    System.Diagnostics.Debug.WriteLine("Configurando el cliente SMTP...");
                    var smtpClient = new SmtpClient("smtp-mail.outlook.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("equiros30766@ufide.ac.cr", "Memo2022+"),
                        EnableSsl = true,
                        Timeout = 5000
                    };
                    System.Diagnostics.Debug.WriteLine("Cliente SMTP configurado correctamente.");

                    // Enviar el correo
                    System.Diagnostics.Debug.WriteLine("Enviando el correo...");
                    smtpClient.Send(correo);
                    System.Diagnostics.Debug.WriteLine("Correo enviado exitosamente a: " + email);

                    TempData["Mensaje"] = "Se ha enviado un código a su correo con el enlace para restablecer su contraseña";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    // Manejo de excepciones
                    System.Diagnostics.Debug.WriteLine($"Error al enviar el correo: {ex.Message}");
                    TempData["Error"] = $"Error al enviar el correo: {ex.Message}";
                    return View();
                }
            }
            else
            {
                // Usuario no encontrado
                System.Diagnostics.Debug.WriteLine($"No se encontró el usuario con el correo: {email}");
                ModelState.AddModelError("Email", "El correo electrónico no está registrado.");
                return View();
            }
        }




        public ActionResult Recuperar()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Restablecer(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "El token es inválido.";
                return RedirectToAction("Login");
            }

            try
            {
                // Decodificar el token recibido
                var decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var partes = decodedToken.Split('-');

                if (partes.Length != 3 || partes[2] != "Token")
                {
                    TempData["Error"] = "El token no es válido.";
                    return RedirectToAction("Login");
                }

                // Asegurarse de que el token no haya expirado, por ejemplo, si ha pasado más de 1 hora.
                DateTime tokenTime = DateTime.ParseExact(partes[1], "yyyyMMddHHmmss", null);
                if (DateTime.UtcNow - tokenTime > TimeSpan.FromHours(1))
                {
                    TempData["Error"] = "El token ha expirado.";
                    return RedirectToAction("Login");
                }

                // Si el token es válido, pasar el email a la vista para que el usuario pueda ingresar la nueva contraseña.
                ViewBag.Email = partes[0];
                return View();
            }
            catch
            {
                TempData["Error"] = "El token no es válido.";
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public ActionResult Restablecer(string token, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Las contraseñas no coinciden.";
                return View();
            }

            try
            {
                // Decodificar el token recibido
                var decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var partes = decodedToken.Split('-');

                if (partes.Length != 3 || partes[2] != "Token")
                {
                    TempData["Error"] = "El token no es válido.";
                    return View();
                }

                // Obtener el correo asociado con el token
                var email = partes[0];
                var usuario = context.usuarios.FirstOrDefault(x => x.Correo == email);

                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return View();
                }

                // Actualizar la contraseña del usuario
                var hasher = new PasswordHasher<usuario>();
                usuario.contrasena = hasher.HashPassword(usuario, newPassword);
                context.SaveChanges();

                TempData["Mensaje"] = "La contraseña se ha restablecido exitosamente.";
                return RedirectToAction("Login");
            }
            catch
            {
                TempData["Error"] = "Ocurrió un error al restablecer la contraseña.";
                return View();
            }
        }






        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session["UsuarioId"] = null;
            Session["UsuarioNombre"] = null;
            return RedirectToAction("Login", "Usuario");
        }



    }
}