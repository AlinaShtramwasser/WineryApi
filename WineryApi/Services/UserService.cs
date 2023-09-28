using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using WineryApi.Models;

namespace WineryApi.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IConfiguration _config;

        public UserService(IConfiguration configuration)
        {
            _config = configuration;
            MongoClient dbClient = new MongoClient(configuration.GetConnectionString("WineryAppConnection"));
            _users = dbClient.GetDatabase("winery").GetCollection<User>("users");
        }

        public GoogleJsonWebSignature.ValidationSettings GetSettings()
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { _config.GetSection("ApplicationSettings").GetValue<string>("GoogleClientId") }
            };
            return settings;
        }

        public string GetToken(string userName)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secret = _config.GetSection("ApplicationSettings").GetValue<string>("Secret");
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new [] {new Claim("id", userName)}),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encryptortoken = tokenHandler.WriteToken(token);
            return encryptortoken;
        }

        public dynamic JwtGenerator(string userName)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secret = _config.GetSection("ApplicationSettings").GetValue<string>("Secret");
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", userName) }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encrypterToken = tokenHandler.WriteToken(token);
            return new {token = encrypterToken, username = userName };
        }
      
        public List<User> Get() =>
            _users.Find(user => true).ToList();
        public User Get(string username) =>
            _users.Find<User>(user => user.UserName == username).FirstOrDefault();

        public bool Create(User user)
        {
            //https://stackoverflow.com/questions/30102651/mongodb-server-v-2-6-7-with-c-sharp-driver-2-0-how-to-get-the-result-from-ins
            try
            {
                _users.InsertOneAsync(user).GetAwaiter().GetResult();
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
    }
}
