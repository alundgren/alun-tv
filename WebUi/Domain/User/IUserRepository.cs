namespace TvMvc3.Integration.CouchDb.User
{
    public interface IUserRepository
    {
        User GetByName(string userName);
        User CreateUser(string userName, string passwordHash, string passwordSalt);
        void UpdateUser(User user);
    }
}
