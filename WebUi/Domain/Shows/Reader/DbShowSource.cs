using System.Collections.Generic;
using System.Linq;
using Raven.Client;

namespace AlunTv.Test
{
    public class DbShowSource
    {
        private readonly IDocumentSession _session;

        public DbShowSource(IDocumentSession session)
        {
            _session = session;
        }

        public Show GetById(string sourceId)
        {
            var id = Show.IdFromSourceId(sourceId);
            return _session.Load<Show>(id);
        }

        public IEnumerable<ShowInfo> FindByName(string partialName)
        {
            return _session
                .Query<ShowInfoCache>(SourceShowInfoCaches_ByName.TheIndexName)
                .Where(x => x.Name.Contains(partialName))
                .ToArray();
        }
    }
}