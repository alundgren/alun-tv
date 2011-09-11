using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using TvMvc3.Integration.CouchDb.Source;
using TvMvc3.Integration.CouchDb.User;
using WebUi.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using WebUi.Infrastructure;

namespace WebUi.Controllers
{
    [Authorize]
    public class ShowController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IShowSource _showSource;

        public ShowController(IUserRepository userRepository, IShowSource showSource)
        {
            _userRepository = userRepository;
            _showSource = showSource;
        }

        public RedirectToRouteResult Add(string externalId, IPrincipal principal)
        {
            var user = _userRepository.GetByName(principal.Identity.Name);

            if (user.WatchList == null)
                user.WatchList = new WatchList();
            if (user.WatchList.Shows == null)
                user.WatchList.Shows = new List<WatchListShow>();

            //We already have this show
            if(user.WatchList.Shows.Any(x => x.SourceId.Equals(externalId)))
                return RedirectToAction("Index", "WatchList");

            var sourceShow = _showSource.GetById(externalId);

            if(sourceShow == null)
                return RedirectToAction("Index", "WatchList"); //TODO: Include some sort of error message

            var wl = new WatchListShow
                         {
                             SourceId = sourceShow.SourceId,
                             EndDate = sourceShow.EndDate,
                             ShowName = sourceShow.Name,
                             LastWatchedEpisode = null,
                             FirstUnwatchedEpisode = WatchListRepository.Map(sourceShow.FirstEpisode)
                         };
            user.WatchList.Shows.Add(wl);

            _userRepository.UpdateUser(user);

             return RedirectToAction("Index", "WatchList");
        }

        [HttpPost]
        public ViewResult Search(string searchFor)
        {
            return View(_showSource
                .FindByName(searchFor)
                .OrderBy(r => r.Name).ToList());
        }
    }
}
