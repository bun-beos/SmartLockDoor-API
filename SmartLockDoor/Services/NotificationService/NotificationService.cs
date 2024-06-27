
using Dapper;
using System.Data;

namespace SmartLockDoor
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<NotificationEntity>> FilterAsync(Guid accountId, Guid? deviceId)
        {
            var param = new
            {
                p_AccountId = accountId,
                p_DeviceId = deviceId
            };

            var result = await _unitOfWork.Connection.QueryAsync<NotificationEntity>("Proc_Notification_Filter", param, commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<int> InsertAsync(NotificationEntity notificationEntity)
        {
            var param = new
            {
                p_NotifId = notificationEntity.NotifId,
                p_AccountId = notificationEntity.AccountId,
                p_DeviceId = notificationEntity.DeviceId,
                p_NotifTitle = notificationEntity.NotifTitle,
                p_NotifBody = notificationEntity.NotifBody,
                p_Createddate = notificationEntity.CreatedDate
            };

            return await _unitOfWork.Connection.ExecuteAsync("Proc_Notification_Insert", param, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> DeleteAsync(Guid notifId)
        {
            var param = new
            {
                p_NotifId = notifId,
            };

            return await _unitOfWork.Connection.ExecuteAsync("Proc_Notification_Delete", param, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> DeleteManyAsync(List<Guid> notifIds)
        {
            string listId = string.Join(",", notifIds);

            var param = new
            {
                p_NotifIds = listId
            };

            return await _unitOfWork.Connection.ExecuteAsync("Proc_Notification_DeleteMany", param, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateAsync(Guid notifId)
        {
            var param = new
            {
                p_NotifId = notifId
            };

            return await _unitOfWork.Connection.ExecuteAsync("Proc_Notification_Update", param, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateManyAsync(List<Guid> notifIds)
        {
            string listId = string.Join (",", notifIds);

            var param = new
            {
                p_NotifIds = listId
            };

            return await _unitOfWork.Connection.ExecuteAsync("Proc_Notification_UpdateMany", param, commandType: CommandType.StoredProcedure);
        }
    }
}
