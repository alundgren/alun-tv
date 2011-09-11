using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TvMvc3.Integration.CouchDb.Source;

namespace WebUi.Domain.Source
{
    //The api seems very slow of late. Using this to try and speed it up a bit
    public class SearchCacheItem
    {
        /// <summary>
        /// Not used for anything. This is just mongodb leaking out. 
        /// </summary>
        public Guid Id { get; set; }

        public DateTimeOffset CreationDate { get; set; }
        public string PartialName { get; set; }
        public List<SourceShowInfo> Hits { get; set; }
    }
}