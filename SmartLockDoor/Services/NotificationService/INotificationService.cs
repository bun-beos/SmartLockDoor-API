namespace SmartLockDoor
{
    public interface INotificationService
    {
        Task<List<NotificationEntity>> FilterAsync(Guid accountId, Guid? deviceId);

        Task<int> InsertAsync(NotificationEntity entity);

        Task<int> UpdateAsync(Guid notifId);

        Task<int> UpdateManyAsync(List<Guid> notifIds);

        Task<int> DeleteAsync(Guid notifId);

        Task<int> DeleteManyAsync(List<Guid> notifIds);
    }
}
