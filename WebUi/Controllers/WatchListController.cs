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
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var u = new UserUpdater(DocumentSession, _ => { });
            if (model.RadioChoiceWatched == "choice-episode")
            {
                u.SetEpisodeWatched(principal.Identity.Name, model.SourceId);
            }
            if (model.RadioChoiceWatched == "choice-season")
            {
                u.SetSeasonWatched(principal.Identity.Name, model.SourceId);
            }
            if (model.RadioChoiceWatched == "choice-custom")
            {
                var foo = model.RadioChoiceCustom.ToLower().Split('x');
                u.SetLastWatchedTo(principal.Identity.Name, model.SourceId, int.Parse(foo[0]), int.Parse(foo[1]));
            }
            return View("Index");
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
