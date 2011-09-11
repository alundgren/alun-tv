using System;
using System.Collections.Generic;
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
        private readonly IUserRepository _userRepository;
        private readonly IShowSource _showSource;

        public WatchListController(IWatchListRepository watchListRepository, IUserRepository userRepository, IShowSource showSource)
        {
            _watchListRepository = watchListRepository;
            _userRepository = userRepository;
            _showSource = showSource;
        }

        public ActionResult Index(IPrincipal principal)
        {
            var wl = _watchListRepository.GetByUserName(principal.Identity.Name);
            if (wl == null || wl.Shows == null || wl.Shows.Count == 0)
                return View(new List<WatchListEntryViewModel>());

            //TODO: Allow filtering out episodes too far in the future
            return View(
                wl
                .Shows
                .Where(x => x.FirstUnwatchedEpisode != null)
                .OrderBy(x => x.ShowName)
                .Select(x => new WatchListEntryViewModel(x, x.FirstUnwatchedEpisode)));
        }

        [HttpPost]
        public RedirectToRouteResult Watched(string sourceId, IPrincipal principal)
        {
            var result = RedirectToAction("Index");
            var user = _userRepository.GetByName(principal.Identity.Name);
            if (user == null || user.WatchList == null || user.WatchList.Shows == null)
                return result;

            var show = user
                .WatchList
                .Shows
                .Single(x => x.SourceId.Equals(sourceId));

            var current = show.FirstUnwatchedEpisode;
            if (current == null)
                return result;
            
            show.LastWatchedEpisode = current;
            show.FirstUnwatchedEpisode = 
                WatchListRepository.Map(
                    _showSource
                    .GetFirstEpisodeAfter(sourceId, current.SeasonNo, current.InSeasonEpisodeNo));

            _userRepository.UpdateUser(user);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public RedirectToRouteResult WatchedSeason(string sourceId, IPrincipal principal)
        {
            var result = RedirectToAction("Index");
            var user = _userRepository.GetByName(principal.Identity.Name);
            if (user == null || user.WatchList == null || user.WatchList.Shows == null)
                return result;

            var show = user
                .WatchList
                .Shows
                .Single(x => x.SourceId.Equals(sourceId));

            var sourceShow = _showSource.GetById(sourceId);
            if (sourceShow == null)
                return result;

            var currentSeasonNo = show.FirstUnwatchedEpisode.SeasonNo;
            var lastEpisodeOfSeason =
                sourceShow
                    .Episodes
                    .Where(x => x.SeasonNo == currentSeasonNo)
                    .OrderByDescending(x => x.InSeasonEpisodeNo)
                    .FirstOrDefault();
            if (lastEpisodeOfSeason == null)
                return result;
            var firstEpisodeOfNextSeason = 
                _showSource
                .GetFirstEpisodeAfter(sourceId, lastEpisodeOfSeason.SeasonNo, lastEpisodeOfSeason.InSeasonEpisodeNo);

            show.FirstUnwatchedEpisode = WatchListRepository.Map(firstEpisodeOfNextSeason);
            show.LastWatchedEpisode = WatchListRepository.Map(lastEpisodeOfSeason);

            _userRepository.UpdateUser(user);

            return result;
        }
    }
}
