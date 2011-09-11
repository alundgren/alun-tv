using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUi.Models
{
    public class WatchListViewModel
    {
        public IEnumerable<WatchListEntryViewModel> Available  { get; set; }
        public IEnumerable<WatchListEntryViewModel> Future { get; set; }
    }
}