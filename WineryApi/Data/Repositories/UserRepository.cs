using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Collections.Generic;
using WineryApi.Data.Interfaces;
using WineryApi.Models;

namespace WineryApi.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;
        public UserRepository(IConfiguration configuration)
        {
            MongoClient dbClient = new MongoClient(configuration.GetConnectionString("WineryAppConnection"));
            _users = dbClient.GetDatabase("winery").GetCollection<User>("users");
        }

        public bool Add(User user)
        {
            try
            {
                _users.InsertOneAsync(user).GetAwaiter().GetResult();
            }
            catch
            {
                return false;
            }
            //can use for debugging
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

        public List<User> Get()
        {
            return _users.Find(user => true).ToList();
        }

        public User GetUserById(string id)
        {
            return _users.Find(users => users.UserName == id).FirstOrDefault();
        }

        public User GetUserByName(string username)
        {
            return _users.Find(user => user.UserName == username).FirstOrDefault();
        }

        public bool Update(User currentUser)
        {
            try
            {
                var result = _users.ReplaceOne(user => user.Id == currentUser.Id, currentUser);
                return result.ModifiedCount > 0;
            }
            catch
            {
                return false;
            }
            //can use for debugging
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
    }
}
