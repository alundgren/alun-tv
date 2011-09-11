using System;

namespace TvMvc3.Integration.CouchDb.User
{
    public class WatchListEpisode
    {
        public string EpisodeName { get; set; }
        public DateTimeOffset? AirDate { get; set; }
        public int SeasonNo { get; set; }
        public int InSeasonEpisodeNo { get; set; }
    }
}
