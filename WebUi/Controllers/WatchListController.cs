using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using AlunTv.Test;
using AlunTv.Test.Users.Updater;
using TvMvc3.Integration.CouchDb.User;
using WebUi.Controllers;
using WebUi.Infrastructure;
using WebUi.Models;

namespace TvMvc3.Controllers
{
    [Authorize]
    public class WatchListController  : BaseController
    {
        public ActionResult IndexAsync(IPrincipal principal)
        {
            return Json(GetWatchList(principal), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Options(string sourceId, IPrincipal principal)
        {
            var u = GetUser(principal.Identity.Name);
            var show = u.WatchList.Shows.Single(x => x.SourceId == sourceId);
            var fu = show.FirstUnwatchedEpisode;
            return View(new OptionsViewModel
                            {
                                SourceId = show.SourceId,
                                ShowName = show.ShowName,
                                CurrentEpisode =
                                    fu == null 
                                    ? ""
                                    : string.Format("{0:00}x{1:00}", fu.SeasonNo, fu.InSeasonEpisodeNo)
                            });
        }

        [HttpPost]
        public ActionResult Options(OptionsViewModel model, IPrincipal principal)
        {
            return View();
        }

        private bool UpdateWatched(string sourceId, IPrincipal principal)
        {
            return (new UserUpdater(DocumentSession, _ => { })).SetEpisodeWatched(principal.Identity.Name, sourceId);       
        }

        private bool UpdateWatchedSeason(string sourceId, IPrincipal principal)
        {
            return (new UserUpdater(DocumentSession, _ => { })).SetSeasonWatched(principal.Identity.Name, sourceId);
        }

        private bool UpdateLastWatchedTo(string sourceId, int seasonNo, int episodeNo, IPrincipal principal)
        {
            return (new UserUpdater(DocumentSession, _ => { })).SetLastWatchedTo(principal.Identity.Name, sourceId, seasonNo, episodeNo);
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

        public ActionResult WatchedToAsync(string sourceId, int seasonNo, int episodeno, IPrincipal principal)
        {
            return Json(UpdateLastWatchedTo(sourceId, seasonNo, episodeno, principal) ? "ok" : "fail", JsonRequestBehavior.AllowGet);
        }

        private User GetUser(string userName)
        {
            var ur = new UserRepository(DocumentSession);
            return ur.GetUser(userName);
        }

        private WatchListViewModel GetWatchList(IPrincipal principal)
        {
            var u = GetUser(principal.Identity.Name);
            if (u == null)
                return new WatchListViewModel
                {
                    Future = new List<WatchListEntryViewModel>(),
                    Available = new List<WatchListEntryViewModel>()
                };
            var wl = u.WatchList;
            if (wl == null || wl.Shows == null || wl.Shows.Length == 0)
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
