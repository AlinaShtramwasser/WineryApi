using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using WineryApi.Models;

namespace WineryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WineryController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public WineryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet]
        public JsonResult Get()
        {
            MongoClient dbClient = new MongoClient(_configuration.GetConnectionString("WineryAppConnection"));
            var dbList = dbClient.GetDatabase("winery").GetCollection<Winery>("wineries").AsQueryable();
            return new JsonResult(dbList);
        }
    }
}
