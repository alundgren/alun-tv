using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TvMvc3.Integration.CouchDb.Source;

namespace WebUi.Infrastructure
{
    public class TvRageWrapper
    {
        private readonly ILogger _logger;

        public TvRageWrapper(ILogger logger)
        {
            _logger = logger;
        }

        public SourceShow GetShowFromTvRage(string sourceId)
        {
            try
            {
                return GetShowFromTvRageI(sourceId);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return null;
            }
        }

        private SourceShow GetShowFromTvRageI(string sourceId)
        {
            var showId = int.Parse(sourceId);
            var info = TvRage.showinfo(showId);
            if (info == null)
                return null;
            var eps = TvRage.episode_list(showId);

            //Map
            var s = new SourceShow();
            s.SourceId = sourceId;
            s.Name = info.showname;
            s.LastUpdate = DateTimeOffset.UtcNow;
            s.ExternalInfoUrl = info.showlink;
            s.EndDate = ToNullable(info.ended);
            s.Episodes = eps.SelectMany(se => se.episodes.Select(ep =>
            {
                var sep = new SourceEpisode();
                sep.AirDate = ToNullable(ep.airdate);
                sep.InSeasonEpisodeNo = ep.seasonnum;
                sep.Name = ep.title;
                sep.SeasonNo = se.no;
                return sep;
            })).ToList();

            return s;
        }

        public IEnumerable<SourceShowInfo> FindByName(string partialName)
        {
            try
            {
                return FindByNameI(partialName);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return new SourceShowInfo[] {};
            }
        }

        private IEnumerable<SourceShowInfo> FindByNameI(string partialName)
        {
            return TvRage
                .search(partialName)
                .Select(x =>
                    new SourceShowInfo
                    {
                        Name = x.name,
                        SourceId = x.showid.ToString()
                    }).ToList();
        }

        private static T? ToNullable<T>(Microsoft.FSharp.Core.FSharpOption<T> s) where T : struct
        {
            return Microsoft.FSharp.Core.FSharpOption<T>.get_IsNone(s)
                       ? new T?()
                       : s.Value;
        }
    }
}