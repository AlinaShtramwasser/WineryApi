using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WineryApi.Models;
using WineryApi.Services;

namespace WineryApi.Controllers
{
     [Authorize]
    //https://www.restapitutorial.com/lessons/httpmethods.html
    [Route("api/[controller]")]
    [ApiController]
    public class WineryController : ControllerBase
    {
        private readonly WineryService _wineryService;

        public WineryController(WineryService wineryService)
        {
            _wineryService = wineryService;
        }


        //returns status code 200 no matter what and a list
        [HttpGet]
        public IActionResult Get()
        {
            //var cookie = Request.Cookies["user"];
            var wineries = _wineryService.Get();
            Response.StatusCode = StatusCodes.Status200OK;
            return new JsonResult(wineries);
        }

        //return 404 not found if no id like that or invalid
        [HttpGet("{id:length(24)}", Name = "GetWinery")]
        public IActionResult Get(string id)
        {
            var winery = _wineryService.Get(id);
            if (winery == null) Response.StatusCode = StatusCodes.Status404NotFound;
            return new JsonResult(winery);
        }

        //Returning 201 if succeeded or 500 if not -> later see if I can return just the id -> header with link to /winery/{id} containing new ID.
        [HttpPost]
        public JsonResult Create(Winery winery)
        {
            if (winery == null)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return new JsonResult(null);
            }

            var result = _wineryService.Create(winery);
            Response.StatusCode = result ? StatusCodes.Status201Created : StatusCodes.Status500InternalServerError;
            return new JsonResult(winery);
        }

        [HttpPut("{id:length(24)}")]
        public JsonResult Update(string id, Winery winery)
        {
            var wineryIn = _wineryService.Get(id);

            if (wineryIn == null) Response.StatusCode = StatusCodes.Status404NotFound;
            var result = _wineryService.Update(id, winery);
            Response.StatusCode = result == false ? StatusCodes.Status500InternalServerError : StatusCodes.Status200OK;

            return new JsonResult(result);
        }


        [HttpPut("rating/{id:length(24)}/{ratingId:int}")]
        public JsonResult UpdateRating(string id, int ratingId)
        {
            var wineryIn = _wineryService.Get(id);

            if (wineryIn == null) Response.StatusCode = StatusCodes.Status404NotFound;
            var result = _wineryService.UpdateRating(id, ratingId);
            Response.StatusCode = result == false ? StatusCodes.Status500InternalServerError : StatusCodes.Status200OK;

            return new JsonResult(result);
        }


        [HttpDelete("{id:length(24)}")]
        public JsonResult Delete(string id)
        {
            var wineryIn = _wineryService.Get(id);

            if (wineryIn == null) Response.StatusCode = StatusCodes.Status404NotFound;

            var result = _wineryService.Remove(id);
            Response.StatusCode = result == false ? StatusCodes.Status404NotFound : StatusCodes.Status200OK;

            return new JsonResult(result);
        }
    }
}