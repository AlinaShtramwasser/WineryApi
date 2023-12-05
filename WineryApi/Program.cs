using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using WineryApi.Services;

namespace WineryApi;
/*A .NET or .NET Core web application runs inside a host that handles application startup, web server configuration, etc.
 * The host encapsulates resources such as logging, configuration, dependency injection (DI),
 * and any IHostedService implementations. A host is created, configured, and executed using the code written in the Program class.
 *https://www.infoworld.com/article/3646098/demystifying-the-program-and-startup-classes-in-aspnet-core.html
 */

public class Program
{
    public static void Main(string[] args)
    {
        //creates the host
        var builder = WebApplication.CreateBuilder(args);
         var configuration = builder.Configuration;
         builder.Services.AddControllers();
        builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
                 options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
             .AddNewtonsoftJson(options =>
                 options.SerializerSettings.ContractResolver = new DefaultContractResolver());
         builder.Services.AddSingleton<WineryService>();
         builder.Services.AddSingleton<UserService>();

        //authentication - when debugging on net can use: https://jwt.io/ to see whats in the token
        builder.Services.AddAuthentication(x =>
         {
             x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
             x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
             //}).AddCookie(x =>
             //{
             //    x.Cookie.Name = "token";
             //
         }).AddJwtBearer(x =>
         {
             x.RequireHttpsMetadata = false;
             x.SaveToken = true;
             x.TokenValidationParameters = new TokenValidationParameters
             {
                 ValidateIssuerSigningKey = true,
                 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["ApplicationSettings:Secret"])),
                 ValidateIssuer = false,
                 ValidateAudience = false
             };
             //x.Events = new JwtBearerEvents
             //{
             //    OnMessageReceived = context =>
             //    {
             //        context.Token = context.Request.Cookies["X-Access-Token"];
             //        return Task.CompletedTask;
             //    }
             //};

         });


        //authorization
        builder.Services.AddAuthorization();
        builder.Services.AddHttpClient();
        builder.Services.AddCors(opt =>
        {
            opt.AddPolicy(name: "CorsPolicy", builder =>
            {
                builder.WithOrigins("http://localhost:12895", "http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        var app = builder.Build();
        app.UseCors("CorsPolicy");
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        app.Run();
    }
}
/*
 *  public string GetCookieString(string key, HttpRequest request)
        {
            var cookie = request.Cookies.Get("SYNCBASE_" + key);
            return cookie != null ? cookie.Value : string.Empty;
        }

        public void PutCookieString(string key, string value, HttpResponse response, DateTime expireDate)
        {
            response.Cookies.Add(new HttpCookie("SYNCBASE_" + key, value)
            {
                Expires = expireDate
            });
        }
   context.Response.Cookies.Clear();
 */