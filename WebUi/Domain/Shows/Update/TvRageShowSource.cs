using System;
using System.Linq;

namespace AlunTv.Test
{
    public class TvRageShowSource
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public virtual Show GetById(string sourceId)
        {
            try
            {
                return GetShowFromTvRageI(sourceId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        private Show GetShowFromTvRageI(string sourceId)
        {
            var showId = int.Parse(sourceId);
            var info = TvRage.showinfo(showId);
            if (info == null)
                return null;
            var eps = TvRage.episode_list(showId);

            //Map
            var s = new Show();
            s.Id = ShowInfoCache.IdFromSourceId(sourceId);
            s.Name = info.showname;
            s.LastUpdate = DateTimeOffset.UtcNow;
            s.ExternalInfoUrl = info.showlink;
            s.HasEnded = ToNullable(info.ended).HasValue;
            s.Episodes = eps.SelectMany(se => se.episodes.Select(ep =>
                                                                     {
                                                                         var sep = new Episode();
                                                                         sep.AirDate = ToNullable<DateTime>(ep.airdate);
                                                                         sep.InSeasonEpisodeNo = ep.seasonnum;
                                                                         sep.Name = ep.title;
                                                                         sep.SeasonNo = se.no;
                                                                         return sep;
                                                                     })).ToArray();

            return s;
        }

        private static T? ToNullable<T>(Microsoft.FSharp.Core.FSharpOption<T> s) where T : struct
        {
            return Microsoft.FSharp.Core.FSharpOption<T>.get_IsNone(s)
                       ? new T?()
                       : s.Value;
        }
    }
}