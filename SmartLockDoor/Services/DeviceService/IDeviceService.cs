namespace SmartLockDoor
{
    public interface IDeviceService
    {
        public Task<List<DeviceEntity>> GetAllAsync();

        public Task<List<DeviceEntity>> GetByAccountAsync(Guid accountId);

        public Task<int> InsertAsync(DeviceEntity deviceEntity);

        public Task<int> UpdateAsync(DeviceEntity deviceEntity);

        public Task<int> DeleteAsync(Guid deviceId);
    }
}
