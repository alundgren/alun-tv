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

        public static WatchListEpisode Map(SourceEpisode source)
        {
            return new WatchListEpisode
            {
                AirDate = source.AirDate,
                EpisodeName = source.Name,
                InSeasonEpisodeNo = source.InSeasonEpisodeNo,
                SeasonNo = source.SeasonNo
            };
        }
    }
}
