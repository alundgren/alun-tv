using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using TvMvc3.Integration.CouchDb.Source;
using TvMvc3.Integration.CouchDb.User;
using WebUi.Infrastructure;
using WebUi.Models;

namespace TvMvc3.Controllers
{
    [Authorize]
    public class WatchListController : Controller
    {
        private readonly IWatchListRepository _watchListRepository;

        public WatchListController(IWatchListRepository watchListRepository)
        {
            _watchListRepository = watchListRepository;
        }

        public ActionResult IndexAsync(IPrincipal principal)
        {
            return Json(GetWatchList(principal), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            return View();
        }

        private bool UpdateWatched(string sourceId, IPrincipal principal)
        {
            return _watchListRepository.SetEpisodeWatched(principal.Identity.Name, sourceId);       
        }

        private bool UpdateWatchedSeason(string sourceId, IPrincipal principal)
        {
            return _watchListRepository.SetSeasonWatched(principal.Identity.Name, sourceId);
        }

        public ActionResult WatchedAsync(string sourceId, IPrincipal principal)
        {
            return Json(UpdateWatched(sourceId, principal) ? "ok" : "fail", JsonRequestBehavior.AllowGet);
        }

        public ActionResult WatchedSeasonAsync(string sourceId, IPrincipal principal)
        {
            return  Json(UpdateWatchedSeason(sourceId, principal) ? "ok" : "fail", 
                JsonRequestBehavior.AllowGet);
        }

        private WatchListViewModel GetWatchList(IPrincipal principal)
        {
            var wl = _watchListRepository.GetByUserName(principal.Identity.Name);
            if (wl == null || wl.Shows == null || wl.Shows.Count == 0)
                return new WatchListViewModel
                {
                    Future = new List<WatchListEntryViewModel>(),
                    Available = new List<WatchListEntryViewModel>()
                };

            //TODO: Allow skipping filtering out episodes too far in the future
            var episodes = wl
                .Shows
                .Where(x =>
                       x.FirstUnwatchedEpisode != null
                       && x.FirstUnwatchedEpisode.AirDate.HasValue
                       && x.FirstUnwatchedEpisode.AirDate.Value < DateTimeOffset.UtcNow.AddDays(14))
                .OrderBy(x => x.FirstUnwatchedEpisode.AirDate.Value)
                .Select(x => new WatchListEntryViewModel(x, x.FirstUnwatchedEpisode))
                .ToList();

            var availableEpisodes =
                episodes
                    .Where(x => x.IsAvailable)
                    .ToList();

            var futureEpisodes =
                episodes
                    .Where(x => !x.IsAvailable)
                    .ToList();
            return new WatchListViewModel
            {
                Future = futureEpisodes,
                Available = availableEpisodes
            };
        }
    }
}
