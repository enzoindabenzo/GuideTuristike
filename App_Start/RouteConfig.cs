using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;


namespace DK1
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Default route
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            // Guida Turistike route
            routes.MapRoute(
                name: "GuidaTuristike",
                url: "{language}/Home/GuidaTuristike",
                defaults: new { controller = "Home", action = "GuidaTuristike", language = UrlParameter.Optional }
            );

            // Calculator route
            routes.MapRoute(
                name: "Calculate",
                url: "{language}/Rruga/Calculate",
                defaults: new { controller = "Rruga", action = "Calculate", language = UrlParameter.Optional }
            );
        }
    }
}