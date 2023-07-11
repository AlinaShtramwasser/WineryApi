using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using WineryApi.Models;
using WineryApi.Services;
/* For this file, User.cs, Login.cs, Register.cs used the below tutorial.   To see how to hook in Swagger (maybe for later) https://aka/ms/aspnetcore/swashbuckle
 * https://www.youtube.com/watch?v=semPMqxziTQ&t=971s
 */
namespace WineryApi.Controllers
{
    public class AuthController : Controller
    {
        public static List<User> UserList = new List<User>();
        private readonly UserService _userService;

        public AuthController(UserService userService)
        {
            _userService= userService;
        }
        [HttpPost("Register")]
        public IActionResult Register([FromBody] Register model)
        {
            var user = new User
            {
                UserName = model.UserName
            };
            if (model.Password == model.ConfirmPassword)
            {
                using HMACSHA512 hmac = new HMACSHA512();
                user.PasswordSalt = hmac.Key;
                user.PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(model.Password));
            }
            else
            {
                return BadRequest("Passwords don't match");
            }
            _userService.Create(user);
            return Ok(user);
        }
        [HttpPost("Login")]
        public IActionResult Login([FromBody] Login model)
        {
            var user = _userService.Get(model.UserName);
            if (user == null)
            {
                return BadRequest("Username or Password was invalid");
            }

            var match = CheckPassword(model.Password, user);
            if (!match)
            {
                return BadRequest("Username or Password was invalid");
            }

            var token = _userService.GetToken(model.UserName);
            return Ok(new {token = token, username = user.UserName});

        }

        private bool CheckPassword(string password, User user)
        {
            bool result;
            using (HMACSHA512 hmac = new HMACSHA512(user.PasswordSalt))
            {
                var compute = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                result = compute.SequenceEqual(user.PasswordHash);
            }
            return result;
        }
    }
}
