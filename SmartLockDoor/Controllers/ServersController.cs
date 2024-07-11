using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartLockDoor.Controllers
{
    [Route("")]
    [ApiController]
    public class ServersController : ControllerBase
    {
        private readonly TimeService _timeService;
        public ServersController(TimeService timeService)
        {
            _timeService = timeService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok($"Server is running! Time {_timeService.GetLocalTime()}");
        }
    }
}
