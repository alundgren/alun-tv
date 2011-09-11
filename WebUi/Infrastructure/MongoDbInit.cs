using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization;
using TvMvc3.Integration.CouchDb.Source;

namespace TvMvc3.Integration.CouchDb
{
    public static class MongoDbInit
    {
        public static void MapClasses()
        {
            BsonClassMap.RegisterClassMap<SourceShow>(cm =>
            {
                cm.AutoMap();
                //cm.UnmapProperty(c => c.SomeProperty);
            });            
        }
    }
}
