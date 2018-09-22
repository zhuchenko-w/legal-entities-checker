using Bookkeeping.Common.Interfaces;
using Bookkeeping.WebUi.App_Start;
using System.Web.Mvc;
using System.Web.Routing;

namespace Bookkeeping.WebUi
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "ErrorDefault",
                url: "Error",
                defaults:
                    new
                    {
                        controller = "Error",
                        action = "Index"
                    }
            );

            routes.MapRoute(
                name: "ErrorNotFound",
                url: "NotFound",
                defaults:
                    new
                    {
                        controller = "Error",
                        action = "NotFound"
                    }
            );

            routes.MapRoute(
                name: "ErrorForbidden",
                url: "Forbidden",
                defaults:
                    new
                    {
                        controller = "Error",
                        action = "Forbidden"
                    }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new
                    {
                        controller = SimpleInjectorInitializer.Container.GetInstance<ISettingsManager>().GetValue("DefaultRouteControllerName", "Manager"),
                        action = "Tasks",
                        id = UrlParameter.Optional
                    }
            );
        }
    }
}
