using Bookkeeping.Common.Interfaces;
using Bookkeeping.Common.Log;
using Bookkeeping.WebUi.App_Start;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Bookkeeping.WebUi
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            SimpleInjectorInitializer.Initialize();
            TinyMapperInitializer.Initialize();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();
            if (exception != null)
            {
                (SimpleInjectorInitializer.Container?.GetInstance<ILogger>() ?? new Logger()).Error("An unexpected error occured", exception);
            }
        }
    }
}

