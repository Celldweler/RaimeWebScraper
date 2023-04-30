using Microsoft.AspNetCore.Mvc;

namespace RaimeWebScraper.Api.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        public virtual IActionResult Get() => Ok("test controller is work!");
    }
}