[assembly: WebActivator.PostApplicationStartMethod(typeof(Bookkeeping.WebUi.App_Start.SimpleInjectorInitializer), "Initialize")]

namespace Bookkeeping.WebUi.App_Start
{
    using System.Reflection;
    using System.Web.Mvc;
    using SimpleInjector;
    using SimpleInjector.Integration.Web.Mvc;
    using Bookkeeping.Common.Interfaces;
    using Bookkeeping.Common.Log;
    using Bookkeeping.Common.Settings;
    using Bookkeeping.Data.Repository.Interfaces;
    using Bookkeeping.Data.Repository.Ef;
    using SimpleInjector.Integration.Web;
    using Bookkeeping.BusinessLogic.Interfaces;
    using Bookkeeping.BusinessLogic;

    public static class SimpleInjectorInitializer
    {
        public static Container Container;

        public static void Initialize()
        {
            Container = new Container();
            Container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();

            InitializeContainer(Container);

            Container.RegisterMvcControllers(Assembly.GetExecutingAssembly());

            Container.Verify();
            
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(Container));
        }
     
        private static void InitializeContainer(Container container)
        {
            container.Register<ILogger, Logger>(Lifestyle.Singleton);
            container.Register<IT1000Logic, T1000Logic>(Lifestyle.Singleton);
            container.Register<ISettingsManager, SettingsManager>(Lifestyle.Singleton);
            container.Register<ITaskRepository, TaskRepository>(Lifestyle.Singleton);
            container.Register<IResolutionRepository, ResolutionRepository>(Lifestyle.Singleton);
            container.Register<IAgentRepository, AgentRepository>(Lifestyle.Singleton);
            container.Register<IT1000Repository, T1000Repository>(Lifestyle.Singleton);

            //container.Register<IdentityDbContext<BkUser>>(() => new BkDbContext("BkDbContext"), Lifestyle.Scoped);
            //container.Register<IUserStore<BkUser>>(() => new UserStore<BkUser>(), Lifestyle.Scoped);
            //container.Register(() => new RoleStore<BkRole>(), Lifestyle.Scoped);
            //container.Register<UserManager<BkUser>, BkUserManager>(Lifestyle.Scoped);
            //container.Register<RoleManager<BkRole>, BkRoleManager>(Lifestyle.Scoped);
        }
    }
}