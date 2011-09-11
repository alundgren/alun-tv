using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace TvMvc3.Integration.CouchDb.User
{
    public class MongoDbUserRepository : IUserRepository
    {
        private readonly MongoDatabase _db;

        public MongoDbUserRepository(MongoDatabase db)
        {
            _db = db;
        }

        public User GetByName(string userName)
        {
            var query = Query.EQ("Name", userName);
            var users = _db.GetCollection<User>("Users");
            return users.Find(query).FirstOrDefault();
        }

        public User CreateUser(string userName, string passwordHash, string passwordSalt)
        {
            if(GetByName(userName) != null)
                throw new ArgumentException("UserName is taken");

            var users = _db.GetCollection<User>("Users");
            var user = new User
                           {
                               Name = userName,
                               PasswordHash = passwordHash,
                               PasswordSalt = passwordSalt
                           };
            users.Insert(user);
            return user;
        }

        public void UpdateUser(User user)
        {
            var users = _db.GetCollection<User>("Users");
            var query = Query.EQ("Name", user.Name);
            users.Remove(query);
            users.Insert(user);
        }
    }
}
