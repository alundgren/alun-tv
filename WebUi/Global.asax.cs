using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AlunTv.Test;
using Autofac;
using Autofac.Integration.Mvc;
using MongoDB.Driver;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using SignalR.Routing;
using WebUi.Domain.Events;
using WebUi.Models;
using WebUi.Infrastructure;
using System.Diagnostics;
using System.Web.Configuration;

namespace WebUi
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static IDocumentStore DocumentStore;
        
        private void RegisterAutoFac()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof (MvcApplication).Assembly);

            builder
                .RegisterType<DiagnosticsTraceLogger>()
                .As<ILogger>();
            
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

            RouteTable.Routes.MapConnection<EventConnection>("event", "event/{*operation}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new {controller = "WatchList", action = "Index", id = UrlParameter.Optional} // Parameter defaults
                );
        }

        protected void Application_Start()
        {
            InitRavenDb();

            ModelBinders.Binders[typeof (IPrincipal)] = new PrincipalModelBinder();
            RegisterAutoFac();
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        private static void InitRavenDb()
        {
            DocumentStore = new DocumentStore
                                {
                                    Url = "http://localhost:8081"
                                }.Initialize();
            DocumentStore.Conventions.DefaultQueryingConsistency = ConsistencyOptions.QueryYourWrites;
            IndexCreation.CreateIndexes(typeof (SourceShowInfoCaches_ByName).Assembly, DocumentStore);
        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }
    }
}