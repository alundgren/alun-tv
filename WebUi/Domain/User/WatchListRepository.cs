using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TvMvc3.Integration.CouchDb.Source;

namespace TvMvc3.Integration.CouchDb.User
{
    public class WatchListRepository : IWatchListRepository
    {
        private readonly IUserRepository _userRepository;
        private readonly IShowSource _showSource;

        public WatchListRepository(IUserRepository userRepository, IShowSource showSource)
        {
            _userRepository = userRepository;
            _showSource = showSource;
        }

        public WatchList GetByUserName(string userName)
        {
            var user = _userRepository.GetByName(userName);
            if (user == null || user.WatchList == null)
                return null;

            //The lastwatched != null filter is based on the assumption that we can never add a show without episodes
            //and so this will always hold.
            var showsThatCouldHaveNewEps =
                user
                    .WatchList
                    .Shows
                    .Where(x => !x.EndDate.HasValue && x.LastWatchedEpisode != null &&  x.FirstUnwatchedEpisode == null)
                    .ToList();
            var changed = false;
            showsThatCouldHaveNewEps.ForEach(show =>
                {
                    var lastWatched = show.LastWatchedEpisode;
                    var newEp = _showSource.GetFirstEpisodeAfter(
                        show.SourceId, 
                        lastWatched.SeasonNo, 
                        lastWatched.InSeasonEpisodeNo);
                    if (newEp == null)
                        return;
                    changed = true;
                    show.FirstUnwatchedEpisode = Map(newEp);
                });
            if(changed)
                _userRepository.UpdateUser(user);

            return user.WatchList;
        }

        private static WatchListEpisode Map(SourceEpisode source)
        {
            if (source == null)
                return null;
            return new WatchListEpisode
            {
                AirDate = source.AirDate,
                EpisodeName = source.Name,
                InSeasonEpisodeNo = source.InSeasonEpisodeNo,
                SeasonNo = source.SeasonNo
            };
        }

        public bool SetSeasonWatched(string userName, string sourceId)
        {
            var user = _userRepository.GetByName(userName);
            if (user == null || user.WatchList == null || user.WatchList.Shows == null)
                return false;

            var show = user
                .WatchList
                .Shows
                .Single(x => x.SourceId.Equals(sourceId));

            var sourceShow = _showSource.GetById(sourceId);
            if (sourceShow == null)
                return false;

            var currentSeasonNo = show.FirstUnwatchedEpisode.SeasonNo;
            var lastEpisodeOfSeason =
                sourceShow
                    .Episodes
                    .Where(x => x.SeasonNo == currentSeasonNo)
                    .OrderByDescending(x => x.InSeasonEpisodeNo)
                    .FirstOrDefault();
            if (lastEpisodeOfSeason == null)
                return false;
            var firstEpisodeOfNextSeason =
                _showSource
                .GetFirstEpisodeAfter(sourceId, lastEpisodeOfSeason.SeasonNo, lastEpisodeOfSeason.InSeasonEpisodeNo);

            show.FirstUnwatchedEpisode = Map(firstEpisodeOfNextSeason);
            show.LastWatchedEpisode = Map(lastEpisodeOfSeason);

            _userRepository.UpdateUser(user);
            return true;
        }

        public bool SetEpisodeWatched(string userName, string sourceId)
        {
            var user = _userRepository.GetByName(userName);
            if (user == null || user.WatchList == null || user.WatchList.Shows == null)
                return false;

            var show = user
                .WatchList
                .Shows
                .Single(x => x.SourceId.Equals(sourceId));

            var current = show.FirstUnwatchedEpisode;
            if (current == null)
                return false;

            show.LastWatchedEpisode = current;
            show.FirstUnwatchedEpisode =
                Map(
                    _showSource
                    .GetFirstEpisodeAfter(sourceId, current.SeasonNo, current.InSeasonEpisodeNo));

            _userRepository.UpdateUser(user);
            return true; 
        }

        public bool AddShow(string userName, string sourceId)
        {
            var user = _userRepository.GetByName(userName);

            if (user.WatchList == null)
                user.WatchList = new WatchList();
            if (user.WatchList.Shows == null)
                user.WatchList.Shows = new List<WatchListShow>();

            //We already have this show
            if (user.WatchList.Shows.Any(x => x.SourceId.Equals(sourceId)))
                return true;

            var sourceShow = _showSource.GetById(sourceId);

            if (sourceShow == null)
                return false;

            var wl = new WatchListShow
            {
                SourceId = sourceShow.SourceId,
                EndDate = sourceShow.EndDate,
                ShowName = sourceShow.Name,
                LastWatchedEpisode = null,
                FirstUnwatchedEpisode = WatchListRepository.Map(sourceShow.FirstEpisode)
            };
            user.WatchList.Shows.Add(wl);

            _userRepository.UpdateUser(user);

            return true;
        }
    }
}
