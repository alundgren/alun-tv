using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using MongoDB.Driver;
using TvMvc3.Integration.CouchDb;
using TvMvc3.Integration.CouchDb.Source;
using TvMvc3.Integration.CouchDb.User;
using WebUi.Models;
using WebUi.Infrastructure;
using System.Diagnostics;
using System.Web.Configuration;

namespace WebUi
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private void RegisterAutoFac()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof (MvcApplication).Assembly);

            var appHbMongoUrl = WebConfigurationManager.AppSettings["MONGOHQ_URL"];
            var db = MongoDatabase.Create(appHbMongoUrl ?? "mongodb://localhost/alun-tv?safe=true");
            Application["MongoDbAlunTv"] = db;
            MongoDbInit.MapClasses();

            builder
                .Register(x => db)
                .SingleInstance();

            builder
                .RegisterType<MongoDbUserRepository>()
                .As<IUserRepository>()
                .SingleInstance();

            builder
                .RegisterType<TvRageAndMongoDbShowSource>()
                .As<IShowSource>()
                .SingleInstance();

            builder
                .RegisterType<WatchListRepository>()
                .As<IWatchListRepository>()
                .SingleInstance();

            builder
                .RegisterType<DiagnosticsTraceLogger>()
                .As<ILogger>();

            builder
                .RegisterType<TvRageWrapper>()
                .SingleInstance();

            //End module
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("elmah.axd");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new {controller = "WatchList", action = "Index", id = UrlParameter.Optional} // Parameter defaults
                );
        }

        protected void Application_Start()
        {
            ModelBinders.Binders[typeof (IPrincipal)] = new PrincipalModelBinder();
            RegisterAutoFac();
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }
    }
}