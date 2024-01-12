using System.Collections.Generic;
using WineryApi.Models;

namespace WineryApi.Data.Interfaces
{
    public interface IUserRepository
    {
        public bool Add(User user);
        public List<User> Get();
        public User GetUserById(string id);
        public User GetUserByName(string username);
        public bool Update(User currentUser);
    }
}
