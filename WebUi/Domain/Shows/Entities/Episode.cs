using System;

namespace AlunTv.Test
{
    public class Episode
    {
        public string Name { get; set; }
        public DateTimeOffset? AirDate { get; set; }
        public int SeasonNo { get; set; }
        public int InSeasonEpisodeNo { get; set; }
    }
}