using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using CsvHelper;

namespace AlunTv.Test
{
    public class EpGuideShowSource
    {
        public ShowInfoCache[] FetchShowNamesFromEpGuides()
        {
            var request = WebRequest.Create("http://epguides.com/common/allshows.txt");
            var response = request.GetResponse();
            using (var r = new StreamReader(response.GetResponseStream()))
            {
                //title,directory,tvrage,start date,end date,number of episodes,run time,network,country
                var parser = new CsvReader(r);
                var ids = new HashSet<string>(); //The datasource is a bit crap so we need to remove their dupes
                var result = new List<ShowInfoCache>();
                while (parser.Read())
                {
                    DateTimeOffset d;
                    var hasEnded = false;
                    if(DateTimeOffset.TryParseExact(
                        parser.GetField("end date"), 
                        "MMM yyyy", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out d))
                    {
                        hasEnded = true;
                    }
                    var id = ShowInfoCache.IdFromSourceId(parser.GetField("tvrage"));
                    if (!ids.Contains(id))
                        result.Add(new ShowInfoCache
                                       {
                                           Id = id,
                                           Name = parser.GetField("title"),
                                           HasEnded = hasEnded
                                       });
                    ids.Add(id);
                }
                return result.ToArray();
            }
        }

        public Show FetchShowFromEpGuide(string sourceId, ShowInfo cacheItem)
        {
            var request = WebRequest.Create(String.Format("http://epguides.com/common/exportToCSV.asp?rage={0}", sourceId));
            var response = request.GetResponse();
            using (var r = new StreamReader(response.GetResponseStream()))
            {

                var htmlCrapPaddedContent = r.ReadToEnd();
                var pattern = new Regex(@".*<pre>(.*)</pre>.*", RegexOptions.Singleline);
                var m = pattern.Match(htmlCrapPaddedContent);
                if (!m.Success)
                {
                    return null;
                }
                var rr = new StringReader(m.Groups[1].Value.Trim());
                var parser = new CsvReader(rr);
               
                var result = new List<Episode>();
                while (parser.Read())
                {
                    //number,season,episode,production code,airdate,title,special?
                    if (parser.GetField("special?") == "y")
                        continue;

                    result.Add(new Episode
                                   {
                                       Name = parser.GetField("title"),
                                       AirDate = ParseDate(parser.GetField("airdate")),
                                       InSeasonEpisodeNo = Int32.Parse(parser.GetField("episode")),
                                       SeasonNo = Int32.Parse(parser.GetField("season"))
                                   });
                }
                return new Show
                           {
                               HasEnded = cacheItem.HasEnded,
                               Episodes = result.ToArray(),
                               ExternalInfoUrl = String.Format("http://www.tvrage.com/shows/id-{0}", sourceId),
                               Id = Show.IdFromSourceId(sourceId),
                               LastUpdate = DateTimeOffset.UtcNow,
                               Name = cacheItem.Name
                           };
            }
        }

        private static DateTimeOffset? ParseDate(string d)
        {
            DateTimeOffset s;
            if (DateTimeOffset.TryParseExact(
                d,
                "dd/MMM/yy",
                CultureInfo.GetCultureInfo("en-US"),
                DateTimeStyles.None, out s))
            {
                return s;
            }
            return null;
        }
    }
}