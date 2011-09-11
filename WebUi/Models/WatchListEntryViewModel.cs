using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TvMvc3.Integration.CouchDb.User;

namespace WebUi.Models
{
    public class WatchListEntryViewModel
    {
        private readonly WatchListShow _show;
        private readonly WatchListEpisode _episode;

        public WatchListEntryViewModel(WatchListShow show, WatchListEpisode episode)
        {
            _show = show;
            _episode = episode;
        }

        public string SourceId
        {
            get { return _show.SourceId; }
        }

        public string FormattedDate 
        { 
            get 
            {
                if (!_episode.AirDate.HasValue)
                    return "Unknown";
                var ad = _episode.AirDate.Value.Date;
                var today = DateTime.Today;
                switch (Convert.ToInt32(Math.Round(ad.Subtract(today).TotalDays)))
                {
                    case -1:
                        return "Yesterday";
                    case 0:
                        return "Today";
                    case 1:
                        return "Tomorrow";
                    default:
                        return ad.ToShortDateString();
                }
            }
        }

        public string FormattedEpisodeNo
        {
            get
            {
                return String.Format("{0:00}x{1:00}", _episode.SeasonNo, _episode.InSeasonEpisodeNo);
            }
        }

        public string ShowName
        {
            get { return _show.ShowName; }
        }

        public string EpisodeName
        {
            get { return _episode.EpisodeName; }
        }
    }
}