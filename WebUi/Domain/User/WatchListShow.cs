using System;

namespace TvMvc3.Integration.CouchDb.User
{
    public class WatchListShow
    {
        public WatchListEpisode LastWatchedEpisode { get; set; }
        public WatchListEpisode FirstUnwatchedEpisode { get; set; }
        public string ShowName { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string SourceId { get; set; }
    }
}
