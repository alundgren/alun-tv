using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AlunTv.Test.Users.Updater;
using TvMvc3.Integration.CouchDb.User;
using WebUi.Infrastructure;
using WebUi.Models;
using System.Web.Security;

namespace WebUi.Controllers
{
    //TODO: Refactor this class to use a membership provider backed by mongodb
    //TODO: Add support for changing passwords
    public class AccountController : BaseController
    {
        public ActionResult LogOn()
        {
            var model = new LogonViewModel();
            if (MvcApplication.IsLocalDebug)
            {
                model.UserName = "alun";
                model.Password = "123456789012";
            }
            return View(model);
        }

        private User GetUser(string userName)
        {
            return
                DocumentSession.Load<User>(TvMvc3.Integration.CouchDb.User.User.IdFromUserName(userName));
        }

        [HttpPost]
        public ActionResult LogOn(LogonViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = GetUser(model.UserName);
                if (user == null)
                {
                    ModelState.AddModelError("", "Incorrect username or password");
                }
                else
                {
                    var hashOfEnteredPw = Passwords.Hash(model.Password, user.PasswordSalt);
                    if (hashOfEnteredPw.Equals(user.PasswordHash))
                    {
                        FormsAuthentication.SetAuthCookie(model.UserName, true);
                        return RedirectToAction("Index", "WatchList");
                    }
                    ModelState.AddModelError("", "Incorrect username or password");
                }
            }

            return View();
        }

        //TODO: Should be a post
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("LogOn");
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(LogonViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isNameTaken = GetUser(model.UserName) != null;
                if (isNameTaken)
                {
                    ModelState.AddModelError("", "UserName is already in use");
                }
                else
                {
                    string salt;
                    var hash = Passwords.Hash(model.Password, out salt);
                    var updater = new UserUpdater(DocumentSession, _ => { });
                    var user = updater.CreateUser(model.UserName, hash, salt);
                    FormsAuthentication.SetAuthCookie(user.Name, true);
                    return RedirectToAction("Index", "WatchList");
                }

            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }
    }
}
