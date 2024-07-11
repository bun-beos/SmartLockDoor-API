using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartLockDoor.Controllers
{
    [Route("")]
    [ApiController]
    public class ServersController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok($"Server is running! Time {DateTime.UtcNow.ToLocalTime()}");
        }
    }
}
