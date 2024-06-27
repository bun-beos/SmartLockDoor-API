
using Dapper;
using System.Data;

namespace SmartLockDoor
{
    public class DeviceService : IDeviceService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeviceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<DeviceEntity>> GetAllAsync(Guid accountId)
        {
            var param = new
            {
                p_AccountId = accountId
            };

            var result =  await _unitOfWork.Connection.QueryAsync<DeviceEntity>("Proc_Device_GetAll", param, commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<List<DeviceEntity>> GetByAccountAsync(Guid accountId)
        {
            var param = new
            {
                p_AccountId = accountId
            };

            var result = await _unitOfWork.Connection.QueryAsync<DeviceEntity>("Proc_Device_GetByAccount", param, commandType: CommandType.StoredProcedure);

            return result.ToList(); 
        }

        public async Task<int> InsertAsync(DeviceEntity deviceEntity)
        {
            var param = new
            {
                p_DeviceId = deviceEntity.DeviceId,
                p_DeviceName = deviceEntity.DeviceName,
                p_DeviceState = 1,
                p_CreatedDate = DateTime.Now
            };

            return await _unitOfWork.Connection.ExecuteAsync("Proc_Device_Insert", param, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateAsync(DeviceEntity deviceEntity)
        {
            var param = new
            {
                p_DeviceId = deviceEntity.DeviceId,
                p_DeviceName = deviceEntity.DeviceName,
                p_DeviceState = deviceEntity.DeviceState,
                p_AccountId = deviceEntity.AccountId
            };

            return await _unitOfWork.Connection.ExecuteAsync("Proc_Device_Update", param, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> DeleteAsync(Guid deviceId)
        {
            var param = new { p_DeviceId = deviceId };

            return await _unitOfWork.Connection.ExecuteAsync("Proc_Device_Delete", param, commandType: CommandType.StoredProcedure);
        }
    }
}
