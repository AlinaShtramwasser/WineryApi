using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using WineryApi.Data.Interfaces;
using WineryApi.Models;

namespace WineryApi.Services
{
    //https://www.mongodb.com/docs/drivers/csharp/current/usage-examples/ -> documentation for c# mongo DB driver
    /*This is a POC - ideally I would want to have a single common list of wineries (taken from google based on a region name), these would have a rating which would be an average of all the users'
     * ratings.  The user would choose which of these they want for their list, or be able to add their own (which they can choose to show to others or not).   The user would be able to share whatever they
     * choose re: rating or notes with other users*/
    public class WineryForUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        private readonly IWineryRepository _wineryRepository;
        public WineryForUserService(IConfiguration configuration, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
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
                List<Winery> allWineries = _wineryRepository.GetWineries();
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
            return _userRepository.GetUserById(GetCurrentUserId());
        }

        public Winery Get(string id) => _wineryRepository.GetWineryById(id);

        public bool Create(Winery winery)
        {
            //https://stackoverflow.com/questions/30102651/mongodb-server-v-2-6-7-with-c-sharp-driver-2-0-how-to-get-the-result-from-ins
            try
            {
                var success = _wineryRepository.Add(ref winery);
                if (!success)
                    return false;

                var id = winery.Id;
                User currentUser = GetCurrentUser();
                currentUser.Wineries ??= new List<string>();
                currentUser.Wineries.Add(winery.Id);
                return _userRepository.Update(currentUser);
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
                return _wineryRepository.UpdateRating(id, rating);
            }
            catch
            {
                return false;
            }
        }

        public bool Update(string id, Winery wineryIn)
        {
            try
            {
                return _wineryRepository.UpdateWinery(id, wineryIn);
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
                var updateUserWineryResult = _userRepository.Update(currentUser);
                var updateWineriesResult = _wineryRepository.Delete(id);
                return updateUserWineryResult && updateWineriesResult;
            }
            catch
            {
                return false;
            }
        }

    }
}
