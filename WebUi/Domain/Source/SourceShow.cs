using System;
using System.Collections.Generic;
using System.Linq;

namespace TvMvc3.Integration.CouchDb.Source
{
    public class SourceShow : SourceShowInfo
    {
        /// <summary>
        /// Not used for anything. This is just mongodb leaking out. 
        /// </summary>
        public Guid Id { get; set; }

        public IEnumerable<SourceEpisode> Episodes { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public DateTimeOffset LastUpdate { get; set; }
        public string ExternalInfoUrl { get; set; }

        public SourceEpisode FirstEpisode
        {
            get
            {
                return Episodes
                    .OrderBy(x => x.SeasonNo)
                    .ThenBy(x => x.InSeasonEpisodeNo)
                    .FirstOrDefault();
            }
        }
    }
}
