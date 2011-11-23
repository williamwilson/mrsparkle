using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using log4net;

namespace sparkle
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MrSparkleApplication : System.Web.HttpApplication
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MrSparkleApplication));

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            Log.Debug("Registering global filters");
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            Log.Debug("Registering routes");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "MessagesByTag",
                "tag/{tag}",
                new { controller = "Message", action = "MessagesByTag", tag = "" }
            );

            routes.MapRoute(
                "MessageByRoomAndDate",
                "room/{room}/{year}/{month}/{day}",
                new { controller = "Message", 
                    action = "MessagesByRoomAndDate", 
                    room = "", 
                    year = DateTime.Now.Year, 
                    month = DateTime.Now.Month, 
                    day = DateTime.Now.Day
                }
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();
            Log.Info("Starting MrSparkle");

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}