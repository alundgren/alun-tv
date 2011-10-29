using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUi.Models
{
    public class OptionsViewModel
    {
        public string SourceId { get; set; }
        public string ShowName { get; set; }
        public string CurrentEpisode { get; set; }
        public string RadioChoiceWatched { get; set; }
        public string RadioChoiceCustom { get; set; }
    }
}