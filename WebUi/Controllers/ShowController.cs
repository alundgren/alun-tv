using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using TvMvc3.Integration.CouchDb.Source;
using TvMvc3.Integration.CouchDb.User;
using WebUi.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using WebUi.Infrastructure;

namespace WebUi.Controllers
{
    [Authorize]
    public class ShowController : Controller
    {
        private readonly IShowSource _showSource;
        private readonly IWatchListRepository _watchListRepository;

        public ShowController(IShowSource showSource, IWatchListRepository watchListRepository)
        {
            _showSource = showSource;
            _watchListRepository = watchListRepository;
        }

        public RedirectToRouteResult Add(string externalId, IPrincipal principal)
        {
            _watchListRepository.AddShow(principal.Identity.Name, externalId);
            return RedirectToAction("Index", "WatchList");
        }

        public ActionResult AddAsync(string sourceId, IPrincipal principal)
        {
            return Json(_watchListRepository.AddShow(principal.Identity.Name, sourceId) ? "ok" : "fail",
                        JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ViewResult Search(string partialName)
        {
            return View(_showSource
                .FindByName(partialName)
                .OrderBy(r => r.Name).ToList());
        }

        public ActionResult SearchAsync(string partialName)
        {
            return Json(_showSource
                            .FindByName(partialName)
                            .OrderBy(r => r.Name), JsonRequestBehavior.AllowGet);
        }
    }
}
