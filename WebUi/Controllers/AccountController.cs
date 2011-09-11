﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TvMvc3.Integration.CouchDb.User;
using WebUi.Models;
using System.Web.Security;

namespace WebUi.Controllers
{
    //TODO: Refactor this class to use a membership provider backed by mongodb
    //TODO: Add support for changing passwords
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AccountController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogonViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = _userRepository.GetByName(model.UserName);
                if (user == null)
                {
                    ModelState.AddModelError("", "Incorrect username or password");
                }
                else
                {
                    var hashOfEnteredPw = Hash(model.Password, user.PasswordSalt);
                    if (hashOfEnteredPw.Equals(user.PasswordHash))
                    {
                        FormsAuthentication.SetAuthCookie(model.UserName, true);
                        return Redirect(returnUrl ?? Url.Action("Index", "WatchList"));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Incorrect username or password");
                    }
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
                var isNameTake = _userRepository.GetByName(model.UserName) != null;
                if (isNameTake)
                {
                    ModelState.AddModelError("", "UserName is already in use");
                }
                else
                {
                    string salt;
                    var hash = Hash(model.Password, out salt);
                    var user = _userRepository.CreateUser(model.UserName, hash, salt);
                    FormsAuthentication.SetAuthCookie(user.Name, true);
                    return RedirectToAction("Index", "WatchList");
                }

            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private const int SaltLength = 16;
        private const int HashRoundsCount = 5000;

        public string Hash(string value, string salt)
        {
            var i = salt.IndexOf(';');
            var rounds = int.Parse(salt.Substring(0, i));
            var actualSalt = FromHex(salt.Substring(i + 1));
            var valueBytes = Encoding.UTF8.GetBytes(value);
            using (var b = new Rfc2898DeriveBytes(valueBytes, actualSalt, rounds))
            {
                return ToHex(b.GetBytes(24));
            }
        }

        public string Hash(string value, out string salt)
        {
            var saltLocal = ToHex(CreateRandomBytes(SaltLength));
            salt = string.Format("{0};{1}", HashRoundsCount, saltLocal);
            return Hash(value, salt);
        }

        private static byte[] FromHex(string hex)
        {
            if (hex == null)
                throw new ArgumentNullException("hex");
            if (hex.Length % 2 != 0)
                throw new FormatException("Should be a hex string");

            var s = hex.ToLowerInvariant().Replace("-", "");
            var n = s.Length;
            var bytes = new byte[n / 2];
            for (var i = 0; i < n; i += 2)
                bytes[i / 2] = Convert.ToByte(s.Substring(i, 2), 16);
            return bytes;
        }

        private static string ToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        public static byte[] CreateRandomBytes(int byteCount)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[byteCount];
                rng.GetBytes(bytes); //Note that this method is threadsafe
                return bytes;
            }
        }
    }
}
