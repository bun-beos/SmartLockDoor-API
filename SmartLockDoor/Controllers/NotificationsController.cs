using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartLockDoor.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        [Route("{deviceId}")]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<List<NotificationEntity>> GetAllByDeviceAsync(Guid deviceId)
        {
            return await _notificationService.GetAllByDeviceAsync(deviceId);
        }

        [HttpPost]
        public async Task<int> InsertAsync(NotificationEntity notificationEntity)
        {
            return await _notificationService.InsertAsync(notificationEntity);
        }

        [HttpPut]
        [Route("{notifId}")]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<int> UpdateAsync(Guid notifId)
        {
            return await _notificationService.UpdateAsync(notifId);
        }
        
        [HttpPut]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<int> UpdateManyAsync(List<Guid> notifIds)
        {
            return await _notificationService.UpdateManyAsync(notifIds);
        }

        [HttpDelete]
        [Route("{notifId}")]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<int> DeleteAsync(Guid notifId)
        {
            return await _notificationService.DeleteAsync(notifId);
        }

        [HttpDelete]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<int> DeleteManyAsync(List<Guid> notifIds)
        {
            return await _notificationService.DeleteManyAsync(notifIds);
        }
    }
}
