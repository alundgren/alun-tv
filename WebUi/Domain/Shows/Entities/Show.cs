using System;
using System.Linq;

namespace AlunTv.Test
{
    public class Show : ShowInfo
    {
        public static string IdFromSourceId(string sourceId)
        {
            return String.Format("Shows/{0}", sourceId);
        }

        public override string SourceId
        {
            get { return Id.Substring("Shows/".Length); }
        }

        public Episode[] Episodes { get; set; }
        public DateTimeOffset LastUpdate { get; set; }
        public string ExternalInfoUrl { get; set; }

        public Episode FirstEpisode
        {
            get
            {
                return Episodes
                    .OrderBy(x => x.SeasonNo)
                    .ThenBy(x => x.InSeasonEpisodeNo)
                    .FirstOrDefault();
            }
        }

        public Episode GetFirstEpisodeAfter(int seasonNo, int inSeasonEpisodeNo)
        {
            return Episodes
                .Where(x => (x.SeasonNo == seasonNo && x.InSeasonEpisodeNo > inSeasonEpisodeNo) || (x.SeasonNo > seasonNo))
                .OrderBy(x => x.SeasonNo)
                .ThenBy(x => x.InSeasonEpisodeNo)
                .FirstOrDefault();
        }
    }
}