using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using AlunTv.Test.Users.Updater;
using Raven.Client;
using Raven.Client.Document;
using SignalR;
using SignalR.Hubs;
using WebUi.Domain.Events;

namespace WebUi.Controllers
{
    public class BaseController : Controller
    {
        protected IDocumentSession DocumentSession;

        protected void StartBackgroundSignallingTask(Action<IDocumentSession, Action<string>> task)
        {
            ThreadPool.QueueUserWorkItem(__ =>
            {
                //Need a new session to avoid a race condition with the controller
                using (var session = MvcApplication.DocumentStore.OpenSession())
                {
                    session.Advanced.UseOptimisticConcurrency = true;
                    Action<string> signal = s => Hub.GetClients<EventHub>().watchListChanged();
                    task(session, signal);
                    session.SaveChanges();
                }
            });
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            DocumentSession = MvcApplication.DocumentStore.OpenSession();
            DocumentSession.Advanced.UseOptimisticConcurrency = true;
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            using (DocumentSession)
            {
                if (filterContext.Exception == null)
                {
                    DocumentSession.SaveChanges();
                }
            }
        }
    }
}
