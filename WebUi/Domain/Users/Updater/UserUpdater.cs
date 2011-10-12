using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using TvMvc3.Integration.CouchDb.User;

namespace AlunTv.Test.Users.Updater
{
    public class UserUpdater
    {
        private readonly IDocumentSession _session;
        private readonly Action<string> _eventSink;

        public UserUpdater(IDocumentSession session, Action<string> eventSink)
        {
            _session = session;
            _eventSink = eventSink;
        }

        public bool SetLastWatchedTo(string userName, string sourceId, int seasonNo, int episodeNo)
        {
            var user = _session.Load<User>(User.IdFromUserName(userName));

            if (user == null || user.WatchList == null || user.WatchList.Shows == null)
                return false;

            var show = user
                .WatchList
                .Shows
                .Single(x => x.SourceId.Equals(sourceId));

            var source = new DbShowSource(_session);
            var sourceShow = source.GetById(sourceId);
            if (sourceShow == null)
                return false;

            var lastWatchedEpisode = sourceShow
                .Episodes
                .Where(x => x.SeasonNo == seasonNo && x.InSeasonEpisodeNo == episodeNo)
                .SingleOrDefault();
            if (lastWatchedEpisode == null)
                return false;

            var firstUnwatched = sourceShow.GetFirstEpisodeAfter(seasonNo, episodeNo);

            show.FirstUnwatchedEpisode = Map(firstUnwatched);
            show.LastWatchedEpisode = Map(lastWatchedEpisode);

            _session.Store(user);

            return true;           
        }

        public bool SetSeasonWatched(string userName, string sourceId)
        {
            var user = _session.Load<User>(User.IdFromUserName(userName));

            if (user == null || user.WatchList == null || user.WatchList.Shows == null)
                return false;

            var show = user
                .WatchList
                .Shows
                .Single(x => x.SourceId.Equals(sourceId));

            var source = new DbShowSource(_session);
            var sourceShow = source.GetById(sourceId);
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
                sourceShow
                .GetFirstEpisodeAfter(lastEpisodeOfSeason.SeasonNo, lastEpisodeOfSeason.InSeasonEpisodeNo);

            show.FirstUnwatchedEpisode = Map(firstEpisodeOfNextSeason);
            show.LastWatchedEpisode = Map(lastEpisodeOfSeason);

            _session.Store(user);

            return true;
        }

        public bool SetEpisodeWatched(string userName, string sourceId)
        {
            var user = _session.Load<User>(User.IdFromUserName(userName));

            if (user == null || user.WatchList == null || user.WatchList.Shows == null)
                return false;

            var show = user
                .WatchList
                .Shows
                .Single(x => x.SourceId.Equals(sourceId));

            var current = show.FirstUnwatchedEpisode;
            if (current == null)
                return false;

            var source = new DbShowSource(_session);
            var sourceShow = source.GetById(sourceId);
            if (sourceShow == null)
                return false;

            show.LastWatchedEpisode = current;
            show.FirstUnwatchedEpisode =
                Map(
                    sourceShow
                    .GetFirstEpisodeAfter(current.SeasonNo, current.InSeasonEpisodeNo));

            _session.Store(user);

            return true;
        }
        
        //TODO: Event based and asynch
        public bool AddShow(string userName, string sourceId)
        {
            var user = _session.Load<User>(User.IdFromUserName(userName));

            if (user == null || user.WatchList == null || user.WatchList.Shows == null)
                return false;

            if (user.WatchList == null)
                user.WatchList = new WatchList();

            var shows = user.WatchList.Shows == null
                            ? new List<WatchListShow>()
                            : new List<WatchListShow>(user.WatchList.Shows);

            //We already have this show
            if (shows.Any(x => x.SourceId.Equals(sourceId)))
                return true;

            var source = new DbShowSource(_session);
            var sourceShow = source.GetById(sourceId);

            if (sourceShow == null)
            {
                //Try seeding it, this should be asynch later with reporting back
                var updater = new ShowUpdater(_session, _eventSink);
                updater.SeedShow(sourceId);
                sourceShow = source.GetById(sourceId);
                if (sourceShow == null)
                    return false;
            }

            var wl = new WatchListShow
            {
                SourceId = sourceShow.SourceId,
                ShowName = sourceShow.Name,
                LastWatchedEpisode = null,
                FirstUnwatchedEpisode = Map(sourceShow.FirstEpisode)
            };
            shows.Add(wl);
            user.WatchList.Shows = shows.ToArray();

            _session.Store(user);
            _eventSink("watchlist"); //TODO: Scope to this user
            return true;
        }

        public User CreateUser(string userName, string passwordHash, string passwordSalt)
        {
            var u = _session.Load<User>(User.IdFromUserName(userName));
            if (u != null)
                throw new ArgumentException("UserName is taken");

            u = new User
            {
                Id = User.IdFromUserName(userName),
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                WatchList = new WatchList { Shows = new WatchListShow[]{}}
            };

            _session.Store(u);

            return u;
        }

        private static WatchListEpisode Map(Episode source)
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
    }
}
