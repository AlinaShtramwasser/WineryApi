using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using WineryApi.Models;

namespace WineryApi.Services
{
    //https://www.mongodb.com/docs/drivers/csharp/current/usage-examples/ -> documentation for c# mongo DB driver
    /*This is a POC - ideally I would want to have a single common list of wineries (taken from google based on a region name), these would have a rating which would be an average of all the users'
     * ratings.  The user would choose which of these they want for their list, or be able to add their own (which they can choose to show to others or not).   The user would be able to share whatever they
     * choose re: rating or notes with other users*/
    public class WineryForUserService
    {
        private readonly IMongoCollection<Winery> _wineries;
        private readonly IMongoCollection<User> _users;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WineryForUserService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            MongoClient dbClient = new MongoClient(configuration.GetConnectionString("WineryAppConnection"));
            _wineries = dbClient.GetDatabase("winery").GetCollection<Winery>("wineries");
            _users = dbClient.GetDatabase("winery").GetCollection<User>("users");
            _httpContextAccessor = httpContextAccessor;
        }
        public string GetCurrentUserId()
        {
           return _httpContextAccessor.HttpContext.User.FindFirst("id").Value;
        }

        public List<Winery> Get()
        {
            User currentUser = GetCurrentUser();
            if (currentUser != null && currentUser.Wineries != null)
            {
                List<string> wineryIds = currentUser.Wineries.ToList();
                List<Winery> allWineries = _wineries.Find(winery => true).ToList();
                List<Winery> usersWinries = new List<Winery>();
                foreach (var winery in allWineries)
                {
                    if (wineryIds.Contains(winery.Id))
                        usersWinries.Add(winery);
                }
                return usersWinries;
            }

            return new List<Winery>();
        }

        private User GetCurrentUser()
        {
            return _users.Find(users => users.UserName == GetCurrentUserId()).FirstOrDefault();
        }

        public Winery Get(string id) =>
            _wineries.Find<Winery>(winery => winery.Id == id).FirstOrDefault();

        public bool Create(Winery winery)
        {
            //https://stackoverflow.com/questions/30102651/mongodb-server-v-2-6-7-with-c-sharp-driver-2-0-how-to-get-the-result-from-ins
            try
            {
                _wineries.InsertOneAsync(winery).GetAwaiter().GetResult();
                var id = winery.Id;
                User currentUser = GetCurrentUser();
                //not strictly necessary since I'd initialized this when creating a user, but doing it just in case
                currentUser.Wineries ??= new List<string>();
                currentUser.Wineries.Add(winery.Id);
                var result = _users.ReplaceOne(user => user.Id == currentUser.Id, currentUser);
                return result.ModifiedCount > 0;
            }
            catch
            {
                return false;
            }
            //catch (MongoWriteException mwx)
            //{
            //    //https://stackoverflow.com/questions/63671280/is-there-a-complete-list-of-mongodb-error-codes
            //    //https://jira.mongodb.org/browse/DOCS-10757
            //    //WriteError.Code to get a particular exception -- common ones:
            //    //112 write conflict(when a transaction is failed)
            //    //11000 Duplicate Key(when a unique constraint index is violated)
            //    //211 or 11600 When mongo is down or i have a bad config
            //}
        }

        public bool Update(string id, Winery wineryIn)
        {
            try
            {
                var result = _wineries.ReplaceOne(winery => winery.Id == id, wineryIn);
                return result.ModifiedCount > 0;
            }
            catch
            {
                return false;
            }
        }


        public bool UpdateRating(string id, int rating)
        {
            try
            {
                var update = Builders<Winery>.Update.Set("Rating", rating);
                var result = _wineries.UpdateOne(winery => winery.Id == id, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                return false;
            }
        }

        //if I need to pass a winery - not doing this for now
        //public void Remove(Winery wineryIn) =>
        //    _wineries.DeleteOne(winery => winery.Id == wineryIn.Id);
        //for the POC this is just gonna remove wineries from the user's list and from the full list 
        public bool Remove(string id)
        {
            try
            {
                User currentUser = GetCurrentUser();
                currentUser.Wineries.Remove(id);
                var result1 = _users.ReplaceOne(user => user.Id == currentUser.Id, currentUser);
                var result2 = _wineries.DeleteOne(winery => winery.Id == id);
                return result2.DeletedCount > 0 && result1.ModifiedCount > 0; 
            }
            catch
            {
                return false;
            }
        }

    }
}
