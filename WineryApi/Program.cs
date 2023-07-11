using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace WineryApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(
                                a => a.AddAuthentication(x =>
                                {
                                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                                }
                            ).AddJwtBearer(x =>
                            {
                                x.RequireHttpsMetadata = false;
                                x.SaveToken = true;
                                x.TokenValidationParameters = new TokenValidationParameters
                                {
                                ValidateIssuerSigningKey = true,
                                //if this doesn't work just hardcode
                                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("This is my example of a custom secret key used for authentication")),
                                    ValidateIssuer = false,
                                    ValidateAudience = false
                                };
                            }
                            ));
                    webBuilder.UseStartup<Startup>();
                });
    }
}
