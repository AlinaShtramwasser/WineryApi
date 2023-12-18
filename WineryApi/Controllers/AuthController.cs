using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WineryApi.Models;
using WineryApi.Services;

/* For this file, User.cs, Login.cs, Register.cs used the below tutorial.   To see how to hook in Swagger (maybe for later) https://aka/ms/aspnetcore/swashbuckle
 * https://www.youtube.com/watch?v=semPMqxziTQ&t=971s
 */
namespace WineryApi.Controllers;

[Controller]
public class AuthController : Controller
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
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
            using var hmac = new HMACSHA512();
            user.PasswordSalt = hmac.Key;
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(model.Password));
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
        if (user == null) return BadRequest("Username or Password was invalid");

        var match = CheckPassword(model.Password, user);
        if (!match) return BadRequest("Username or Password was invalid");
        var token = GetToken(user);
        //_httpContextAccessor.HttpContext.Response.Cookies.Append("UserId", user.Id, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.None });
        return Ok(token);
    }

    public dynamic GetToken(User user)
    {
        //get the token
        var token = _userService.JwtGenerator(user.UserName);
        //https://stackoverflow.com/questions/72561109/how-to-set-cookie-in-the-browser-using-aspnet-core-6-web-api
        //https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-6.0
        //https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-6.0
        //https://developers.google.com/identity/protocols/oauth2
        //https://www.yogihosting.com/aspnet-core-identity-login-with-google/
        return new { token };
    }

    //private void SetJwt(dynamic token)
    //{
    //    //HttpContext.Response.Cookies.Append("X-Access-Token", token.token,
    //    //    new CookieOptions
    //    //    {
    //    //        Expires = DateTime.Now.AddDays(1),
    //    //        HttpOnly = true,
    //    //        Secure = true,
    //    //        IsEssential = true,
    //    //        SameSite = SameSiteMode.None
    //    //    });
    //    Response.Cookies.Append("X-Access-Token", token, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.None });
    //}

    /*at about min 29:16 of the How to Add Google Sign In With Angular Correctly - https://www.youtube.com/watch?v=G4BBNq1tgwE
    https://www.youtube.com/watch?v=semPMqxziTQ for the link to github etc*/
    [HttpPost("LoginWithGoogle")]
    public async Task<IActionResult> LoginWithGoogle([FromBody] string credential)
    {
        //need to be off the vpn when the app is started, on the network to get the payload, then come of to query mongo
        var settings = _userService.GetSettings();
        //on the network to get the payload
        var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);
        //off the network to query mongo
        var user = _userService.Get(payload.Email);
        //check emailverified also
        if (!payload.EmailVerified) return BadRequest();
        if (user != null) return Ok(GetToken(user));

        var newUser = new User
        {
            UserName = payload.Email,
            FirstName = payload.GivenName,
            LastName = payload.FamilyName,
            Email = payload.Email
        };
        _userService.Create(newUser);
        return Ok(GetToken(newUser));
    }

    private bool CheckPassword(string password, User user)
    {
        using var hmac = new HMACSHA512(user.PasswordSalt);
        var compute = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        var result = compute.SequenceEqual(user.PasswordHash);

        return result;
    }
}