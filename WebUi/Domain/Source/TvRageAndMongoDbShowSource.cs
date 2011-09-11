using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using WebUi.Infrastructure;

namespace TvMvc3.Integration.CouchDb.Source
{
    public class TvRageAndMongoDbShowSource : IShowSource
    {
        private readonly MongoDatabase _db;
        private readonly TvRageWrapper _tvRage;

        public TvRageAndMongoDbShowSource(MongoDatabase mongoDatabase, TvRageWrapper tvRage)
        {
            _db = mongoDatabase;
            _tvRage = tvRage;
        }

        public SourceShow GetById(string sourceId)
        {
            var shows = _db.GetCollection<SourceShow>("SourceShows");
            var query = Query.EQ("SourceId", sourceId);
            var show = shows.Find(query).FirstOrDefault();
            if (show == null || (!show.EndDate.HasValue && DateTimeOffset.UtcNow.Subtract(show.LastUpdate) > TimeSpan.FromDays(1)))
            {
                var extShow = _tvRage.GetShowFromTvRage(sourceId);
                if (extShow == null || extShow.Episodes == null || extShow.Episodes.Count() == 0)
                    return show; //If it returns null because of some temporary problem return the old one
                if (show != null)
                    shows.Remove(query);
                shows.Insert(extShow);
            }
            return show;
        }

        public IEnumerable<SourceShowInfo> FindByName(string partialName)
        {
            return _tvRage.FindByName(partialName);
        }

        public SourceEpisode GetFirstEpisodeAfter(string sourceId, int seasonNo, int inSeasonEpisodeNo)
        {
            var show = GetById(sourceId);
            if (show == null)
                return null;
            return show.Episodes
                .Where(x => (x.SeasonNo == seasonNo && x.InSeasonEpisodeNo > inSeasonEpisodeNo) || (x.SeasonNo > seasonNo))
                .OrderBy(x => x.SeasonNo)
                .ThenBy(x => x.InSeasonEpisodeNo)
                .FirstOrDefault();
        }
    }
}
