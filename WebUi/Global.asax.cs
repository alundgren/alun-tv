using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AlunTv.Test;
using Autofac;
using Autofac.Integration.Mvc;
using MongoDB.Driver;
using NLog;
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
        public static Logger Logger = LogManager.GetCurrentClassLogger();
        
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
                new {controller = "Account", action = "LogOn", id = UrlParameter.Optional} // Parameter defaults
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
            var ravenCredentials = WebConfigurationManager.AppSettings["RavenCredentials"]??"";
            var ravenUrl = WebConfigurationManager.AppSettings["RavenUrl"];
            if (ravenCredentials == "None")
            {
                InitTestDb(ravenUrl);
            }
            else
            {
                InitProdDb(ravenUrl, ravenCredentials);
            }
            DocumentStore.Conventions.DefaultQueryingConsistency = ConsistencyOptions.QueryYourWrites;
            IndexCreation.CreateIndexes(typeof (SourceShowInfoCaches_ByName).Assembly, DocumentStore);
        }

        private static void InitProdDb(string ravenUrl, string ravenCredentials)
        {
            var c = ravenCredentials.Split(';');
            SetBypassSslCertificateValidation();
            DocumentStore = new DocumentStore
                                {
                                    Url = ravenUrl,
                                    Credentials = new NetworkCredential(c[0], c[1])
                                }.Initialize();
        }

        private static void InitTestDb(string ravenUrl)
        {
            DocumentStore = new DocumentStore
                                {
                                    Url = ravenUrl
                                }.Initialize();
            using (var session = DocumentStore.OpenSession())
            {
                var infoCount = session.Query<ShowInfoCache>().Count();
                if (infoCount == 0)
                {
                    var updater = new ShowUpdater(session, x => Logger.Info("Updated names"));
                    updater.UpdateShowNames();
                    session.SaveChanges();
                }
            }
        }

        public static void SetBypassSslCertificateValidation()
        {
            ServicePointManager.ServerCertificateValidationCallback
                += BypassSslCertificateValidation;
        }

        private static bool BypassSslCertificateValidation(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }
    }
}