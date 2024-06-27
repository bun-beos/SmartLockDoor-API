using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartLockDoor.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MqttController : ControllerBase
    {
        private readonly MQTTService _mqttService;
        public MqttController(MQTTService mqttService)
        {
            _mqttService = mqttService;
        }

        [HttpPost]
        [Route("Publish")]
        //[Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<IActionResult> PublishMessageAsync(MqttPublishRequest request)
        {
            var result = await _mqttService.PublishMessageAsync(request.Topic, request.Payload);
            return Ok(result);
        }
    }
}
