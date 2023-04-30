using Microsoft.AspNetCore.Mvc;

namespace RaimeWebScraper.Api.Controllers
{
    [ApiController]
    [Route("api/tricks")]
    public class TricksController : ControllerBase
    {
        public IActionResult Get()
        {
            return Ok("dont have any trick");
        }
    }
}