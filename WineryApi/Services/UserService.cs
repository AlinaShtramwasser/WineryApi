using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WineryApi.Data.Interfaces;
using WineryApi.Models;

namespace WineryApi.Services
{
    public class UserService
    {
        private readonly IConfiguration _config;
        private readonly IUserRepository _userRepository;

        public UserService(IConfiguration configuration, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _config = configuration;
        }

        public GoogleJsonWebSignature.ValidationSettings GetSettings()
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { _config.GetSection("ApplicationSettings").GetValue<string>("GoogleClientId") }
            };
            return settings;
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
            var encryptedToken = tokenHandler.WriteToken(token);
            return new { token = encryptedToken };
        }

        public List<User> Get() => _userRepository.Get();

        public User Get(string username) => _userRepository.GetUserByName(username);

        public bool Create(User user)
        {
            //https://stackoverflow.com/questions/30102651/mongodb-server-v-2-6-7-with-c-sharp-driver-2-0-how-to-get-the-result-from-ins
            try
            {
                user.Wineries = new List<string>();
                _userRepository.Add(user);
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
