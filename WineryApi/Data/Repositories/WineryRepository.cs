using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Collections.Generic;
using WineryApi.Data.Interfaces;
using WineryApi.Models;

namespace WineryApi.Data.Repositories
{
    public class WineryRepository : IWineryRepository
    {
        private readonly IMongoCollection<Winery> _wineries;

        public WineryRepository(IConfiguration configuration)
        {
            MongoClient dbClient = new MongoClient(configuration.GetConnectionString("WineryAppConnection"));
            _wineries = dbClient.GetDatabase("winery").GetCollection<Winery>("wineries");
        }
        public bool Add(ref Winery winery)
        {
            try
            {
                _wineries.InsertOneAsync(winery).GetAwaiter().GetResult();
                return true;
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

        public bool Delete(string id)
        {
            var result = _wineries.DeleteOne(winery => winery.Id == id);
            return result.DeletedCount > 0;
        }

        public List<Winery> GetWineries()
        {
            return _wineries.Find(winery => true).ToList();
        }

        public Winery GetWineryById(string id)
        {
            return _wineries.Find(winery => winery.Id == id).FirstOrDefault();
        }

        public bool UpdateRating(string id, int rating)
        {
            var update = Builders<Winery>.Update.Set("Rating", rating);
            var result = _wineries.UpdateOne(winery => winery.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public bool UpdateWinery(string id, Winery wineryIn)
        {
            var result = _wineries.ReplaceOne(winery => winery.Id == id, wineryIn);
            return result.ModifiedCount > 0;
        }
    }
}
