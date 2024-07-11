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
        [Route("Publish/Single")]
        //[Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<IActionResult> PublishMessageAsync(MqttPublishRequest request)
        {
            var result = await _mqttService.PublishMessageAsync(request);
            if (result != null)
            {
                return Ok(result);
            }
            else throw new Exception("Mqtt is not connected");
        }

        [HttpPost]
        [Route("Publish/Multiple")]
        //[Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<IActionResult> PublishManyMessageAsync(List<MqttPublishRequest> mqttPublishRequests)
        {
            var result = await _mqttService.PublishManyAsync(mqttPublishRequests);
            if (result)
            {
                return Ok(result);
            }
            else throw new Exception("Mqtt is not connected");
        }
    }
}
