using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using WineryApi.Models;

namespace WineryApi.Services
{
    public class WineryService
    {
        private readonly IMongoCollection<Winery> _wineries;
        private readonly IMongoCollection<User> _users;

        public WineryService(IConfiguration configuration)
        {
            MongoClient dbClient = new MongoClient(configuration.GetConnectionString("WineryAppConnection"));
            _wineries = dbClient.GetDatabase("winery").GetCollection<Winery>("wineries");
            _users = dbClient.GetDatabase("winery").GetCollection<User>("users");
        }

        public List<Winery> Get() =>
            _wineries.Find(winery => true).ToList();

        public Winery Get(string id) =>
            _wineries.Find<Winery>(winery => winery.Id == id).FirstOrDefault();

        public bool Create(Winery winery)
        {
            //https://stackoverflow.com/questions/30102651/mongodb-server-v-2-6-7-with-c-sharp-driver-2-0-how-to-get-the-result-from-ins
            try
            {
                _wineries.InsertOneAsync(winery).GetAwaiter().GetResult();
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

            return true;
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

        public bool Remove(string id)
        {
            try
            {
                var result = _wineries.DeleteOne(winery => winery.Id == id);
                return result.DeletedCount > 0;
            }
            catch
            {
                return false;
            }
        }
           
    }
}
