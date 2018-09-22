using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.Identity;
using Bookkeeping.Data.Models;
using Microsoft.AspNet.Identity.Owin;
using System;
using Bookkeeping.Data.Repository.Ef;
using Bookkeeping.Data.Context;
using Bookkeeping.WebUi.App_Start;
using Bookkeeping.Common.Interfaces;

[assembly: OwinStartup(typeof(Bookkeeping.WebUi.Startup))]

namespace Bookkeeping.WebUi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.CreatePerOwinContext(() => new BkDbContext());
            app.CreatePerOwinContext<BkUserManager>(BkUserManager.Create);
            app.CreatePerOwinContext<BkRoleManager>(BkRoleManager.Create);

            //app.Use(async (context, next) => {
            //    using (AsyncScopedLifestyle.BeginScope(SimpleInjectorInitializer.Container))
            //    {
            //        await next();
            //    }
            //});

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                CookieName = SimpleInjectorInitializer.Container.GetInstance<ISettingsManager>().GetValue<string>("AuthCookieName"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<BkUserManager, BkUser, string>(
                        validateInterval: TimeSpan.FromSeconds(1),
                        regenerateIdentityCallback: (manager, user) => user.GenerateUserIdentityAsync(manager),
                        getUserIdCallback: (id) => id.GetUserId<string>())
                }
            });
        }
    }
}