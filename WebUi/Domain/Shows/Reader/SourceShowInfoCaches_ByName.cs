using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace AlunTv.Test
{
    public class SourceShowInfoCaches_ByName : AbstractIndexCreationTask<ShowInfoCache>
    {
        public SourceShowInfoCaches_ByName()
        {
            Map = infos => from i in infos
                           select new { i.Name };
            Index(x => x.Name, FieldIndexing.Analyzed);
        }
        public const string TheIndexName = "SourceShowInfoCaches/ByName";
    }
}