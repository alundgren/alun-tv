using System;

namespace TvMvc3.Integration.CouchDb.User
{
    public class User
    {
        /// <summary>
        /// Not used for anything. This is just mongodb leaking out. 
        /// </summary>
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public WatchList WatchList { get; set; }
    }
}
