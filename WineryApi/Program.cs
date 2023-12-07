using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        //for swagger -> https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<WineryForUserService>();
        builder.Services.AddSingleton<UserService>();
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        //authentication - when debugging on net can use: https://jwt.io/ to see whats in the token
        builder.Services.AddAuthentication(x =>
         {
             x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
             x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            //go to http://localhost:12895/swagger/index.html or whatever port
        }

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
