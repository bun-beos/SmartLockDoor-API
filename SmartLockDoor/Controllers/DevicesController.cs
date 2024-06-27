using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartLockDoor.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        public DevicesController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpGet]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<List<DeviceEntity>> GetAllAsync(Guid accountId)
        {
            return await _deviceService.GetAllAsync(accountId);
        }

        [HttpGet]
        [Route("{accountId}")]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<List<DeviceEntity>> GetByAccountAsync(Guid accountId)
        {
            return await _deviceService.GetByAccountAsync(accountId);
        }

        [HttpPost]
        public async Task<int> InsertAsync(DeviceEntity deviceEntity)
        {
            return await _deviceService.InsertAsync(deviceEntity);
        }

        [HttpPut]
        public async Task<int> UpdateAsync(DeviceEntity deviceEntity)
        {
            return await _deviceService.UpdateAsync(deviceEntity);
        }

        [HttpDelete]
        [Route("{deviceId}")]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<int> DeleteAsync(Guid deviceId)
        {
            return await _deviceService.DeleteAsync(deviceId);
        }
    }
}
