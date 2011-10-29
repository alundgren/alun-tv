using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using AlunTv.Test;
using AlunTv.Test.Users.Updater;
using SignalR;
using SignalR.Hubs;
using WebUi.Domain.Events;


namespace WebUi.Controllers
{
    [Authorize]
    public class ShowController : BaseController
    {
        public ActionResult AddAsync(string sourceId, IPrincipal principal)
        {
            StartBackgroundSignallingTask(
                (session, signal) =>
                {
                    var u = new UserUpdater(session, signal);
                    u.AddShow(principal.Identity.Name, sourceId);
                });

            return Json("ok",
                        JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchAsync(string partialName)
        {
            return Json((new DbShowSource(DocumentSession))
                            .FindByName(partialName)
                            .OrderBy(r => r.Name)
                            .ToArray(), JsonRequestBehavior.AllowGet);
        }
    }
}
