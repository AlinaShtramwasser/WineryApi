using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

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
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(
                    a => a.AddAuthentication(x =>
                        {
                            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        }
                    ).AddCookie(x =>
                        x.Cookie.Name = "token"
                    ).AddJwtBearer(x =>
                        {
                            x.RequireHttpsMetadata = false;
                            x.SaveToken = true;
                            x.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuerSigningKey = true,
                                //if this doesn't work just hardcode
                                IssuerSigningKey = new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        "This is my example of a custom secret key used for authentication")),
                                ValidateIssuer = false,
                                ValidateAudience = false
                            };
                            x.Events = new JwtBearerEvents
                            {
                                OnMessageReceived = context =>
                                {
                                    context.Token = context.Request.Cookies["X-Access-Token"];
                                    return Task.CompletedTask;
                                }
                            };
                        }
                    ));
                webBuilder.UseStartup<Startup>();
            });
    }
}