using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client;
using WebUi.Domain.Shows.Entities;

namespace AlunTv.Test
{
    public class ShowUpdater
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IDocumentSession _session;
        private readonly Action<string> _eventSink;

        public ShowUpdater(IDocumentSession session, Action<string> eventSink)
        {
            _session = session;
            _eventSink = eventSink;
        }

        public void UpdateShows()
        {
            var lastFreshDate = DateTimeOffset.UtcNow.AddDays(-1);
            var showsNeedingUpdate = _session
                .Query<Show>()
                .Where(x => !x.HasEnded && x.LastUpdate < lastFreshDate)
                .ToList();
            var source = new EpGuideShowSource();
            var updatedShows = new ConcurrentBag<Tuple<Show, Show>>();
            Parallel.ForEach(
                showsNeedingUpdate,
                x => updatedShows.Add(Tuple.Create(x, source.FetchShowFromEpGuide(x.SourceId, x))));

            foreach (var i in updatedShows.Where(x => x.Item1 != null))
            {
                var originalShow = i.Item1;
                var updatedShow = i.Item2;
                _session.Advanced.Evict(originalShow);
                _session.Store(updatedShow);
            }
            _eventSink("watchlist");
        }

        //This set is so small that we don't really care about dates and such. Just update all of them.
        public void UpdateShowNames()
        {
            var lastUpdateDateCount = _session
                .Query<NameSourceUpdate>()
                .Select(x => x.Date)
                .Count();
            if (lastUpdateDateCount > 0)
            {
                var lastUpdateDate = _session
                    .Query<NameSourceUpdate>()
                    .OrderByDescending(x => x.Date)
                    .First();
                if (lastUpdateDate.Date > DateTimeOffset.UtcNow.AddDays(-7))
                {
                    Logger.Info("Skipping UpdateShowNames. Last update was: {0}", lastUpdateDate);
                    return;
                }
            }

            Logger.Info("UpdateShowNames running");
            var source = new EpGuideShowSource();
            var caches = source.FetchShowNamesFromEpGuides();
            foreach (var cache in caches)
            {
                _session.Store(cache);
            }

            _session.Store(new NameSourceUpdate { Date = DateTimeOffset.UtcNow });
        }

        public void SeedShow(string sourceId)
        {
            Logger.Info(string.Format("Seeding show: {0}", sourceId));
            var cache = _session
                .Load<ShowInfoCache>(ShowInfoCache.IdFromSourceId(sourceId));
            if (cache == null)
            {
                Logger.Warn(string.Format("Show {0} could not be seeded because it's not in the name cache", sourceId));
                return;
            }
            var source = new EpGuideShowSource();
            var show = source.FetchShowFromEpGuide(sourceId, cache);
            _session.Store(show);
            _eventSink("watchlist");
        }
    }
}