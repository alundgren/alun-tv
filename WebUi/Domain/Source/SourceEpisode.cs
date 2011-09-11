using System;

namespace TvMvc3.Integration.CouchDb.Source
{
    public class SourceEpisode
    {
        public string Name { get; set; }
        public DateTimeOffset? AirDate { get; set; }
        public int SeasonNo { get; set; }
        public int InSeasonEpisodeNo { get; set; }
    }
}
