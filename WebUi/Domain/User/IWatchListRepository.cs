namespace TvMvc3.Integration.CouchDb.User
{
    /// <summary>
    /// Note: This manages updating the watchlist on fetch. 
    /// A better alternative would be to do async updates in a background process on the server
    /// and just fetching this from the db.
    /// </summary>
    public interface IWatchListRepository
    {
        WatchList GetByUserName(string userName);
    }
}
