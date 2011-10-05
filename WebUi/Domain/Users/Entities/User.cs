using System;

namespace TvMvc3.Integration.CouchDb.User
{
    public class User
    {
        public string Id { get; set; }
        public string Name
        {
            get { return Id.Substring("Users/".Length); }
        }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public WatchList WatchList { get; set; }

        public static string IdFromUserName(string userName)
        {
            return string.Format("Users/{0}", userName);
        }
    }
}
