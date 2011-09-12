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

            //TODO: Allow skipping filtering out episodes too far in the future
            var episodes = wl
                .Shows
                .Where(x => 
                    x.FirstUnwatchedEpisode != null 
                    && x.FirstUnwatchedEpisode.AirDate.HasValue 
                    && x.FirstUnwatchedEpisode.AirDate.Value < DateTimeOffset.UtcNow.AddDays(14))
                .OrderBy(x => x.FirstUnwatchedEpisode.AirDate.Value);

            Func<WatchListShow, bool> isAvailable = 
                x => 
                    x.FirstUnwatchedEpisode.AirDate.HasValue 
                    && x.FirstUnwatchedEpisode.AirDate.Value.Date <= DateTimeOffset.UtcNow.Date;

            var availableEpisodes =
                episodes
                    .Where(isAvailable)
                    .Select(x => new WatchListEntryViewModel(x, x.FirstUnwatchedEpisode))
                    .ToList();

            var futureEpisodes =
                episodes
                    .Where(x => !isAvailable(x))
                    .Select(x => new WatchListEntryViewModel(x, x.FirstUnwatchedEpisode))
                    .ToList();
            return View(new WatchListViewModel {Future = futureEpisodes, Available = availableEpisodes});
        }

        public ActionResult ShowDialog(string sourceId, IPrincipal principal)
        {
            var watchList = _watchListRepository.GetByUserName(principal.Identity.Name);
            if(watchList == null || watchList.Shows == null)
                return RedirectToAction("Index");

            var show =
                watchList
                    .Shows
                    .Where(x => x.SourceId.Equals(sourceId))
                    .SingleOrDefault();

            if (show == null)
                return RedirectToAction("Index");

            //TODO: add link to wl so we dont have to do a second fetch here
            var sourceShow = _showSource.GetById(sourceId);

            if (sourceShow == null)
                return RedirectToAction("Index");

            return View(new ShowDialogViewModel
            {
                ShowLink = sourceShow.ExternalInfoUrl,
                ShowName = show.ShowName,
                SourceId = show.SourceId
            });
        }
        
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
