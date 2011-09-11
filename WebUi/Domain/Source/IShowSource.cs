using System;
using System.Collections.Generic;

namespace TvMvc3.Integration.CouchDb.Source
{
    public interface IShowSource
    {
        SourceShow GetById(string sourceId);
        IEnumerable<SourceShowInfo> FindByName(string partialName);

        /// <summary>
        /// Gets the first episode after the given season x episode.
        /// NOTE: That this method returns null does not necessarily mean that it will keep doing so unless the show has ended.
        /// </summary>
        /// <param name="sourceId">The source id.</param>
        /// <param name="seasonNo">The season no.</param>
        /// <param name="inSeasonEpisodeNo">The in season episode no.</param>
        /// <returns>The episode or null if none is available.</returns>
        SourceEpisode GetFirstEpisodeAfter(string sourceId, int seasonNo, int inSeasonEpisodeNo);
    }
}
