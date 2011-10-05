using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlunTv.Test;
using Raven.Client;

namespace TvMvc3.Integration.CouchDb.User
{
    public class UserRepository
    {
        private readonly IDocumentSession _session;

        public UserRepository(IDocumentSession session)
        {
            _session = session;
        }

        public User GetUser(string userName)
        {
            return _session.Load<User>(User.IdFromUserName(userName));
        }
    }
}
