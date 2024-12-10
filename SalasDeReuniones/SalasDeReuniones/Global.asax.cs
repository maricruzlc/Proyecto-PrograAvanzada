using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace SalasDeReuniones
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }

        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ApplicationCookie",
                LoginPath = new PathString("/Account/Login")
            });
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            // Verifica si hay una cookie de autenticación
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                // Desencripta el ticket
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket != null)
                {
                    // Extrae el rol desde UserData
                    var roles = new[] { authTicket.UserData }; // Rol almacenado en UserData
                    var identity = new GenericIdentity(authTicket.Name);
                    var principal = new GenericPrincipal(identity, roles);

                    HttpContext.Current.User = principal;
                }
            }
        }



    }

}

