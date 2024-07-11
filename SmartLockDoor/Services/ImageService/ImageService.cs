using Dapper;
using System.Data;

namespace SmartLockDoor
{
    public class ImageService : IImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFirebaseService _firebaseService;
        private readonly IMemberService _memberService;

        public ImageService(IUnitOfWork unitOfWork, IFirebaseService firebaseService, IMemberService memberService)
        {
            _unitOfWork = unitOfWork;
            _firebaseService = firebaseService;
            _memberService = memberService;
        }

        public async Task<DateTimeOffset?> GetOldestAsync(Guid deviceId)
        {
            var param = new
            {
                p_DeviceId = deviceId
            };

            var imageEntity = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<ImageEntity>("Proc_Image_GetOldest", param, commandType: CommandType.StoredProcedure);

            if (imageEntity == null)
            {
                return DateTimeOffset.Now.AddMonths(1);
            } else
            {
                return imageEntity.CreatedDate;
            }
        }

        public async Task<ImageEntity?> FindByIdAsync(Guid id)
        {
            var param = new
            {
                p_ImageId = id
            };

            var result = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<ImageEntity>("Proc_Image_GetById", param, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<ImageEntity?> FindByNotifIdAsync(Guid notifId)
        {
            var param = new
            {
                p_Notifid = notifId
            };

            var result = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<ImageEntity>("Proc_Image_GetByNotifId", param, commandType: CommandType.StoredProcedure);

            return result;
        }
        
        public async Task<List<ImageEntity>> FilterAsync(Guid deviceId, Guid? memberId, DateTime? startDate, DateTime? endDate)
        {
            var param = new
            {
                p_DeviceId = deviceId,
                p_MemberId = memberId,
                p_StartDate = startDate,
                p_EndDate = endDate
            };

            var result = await _unitOfWork.Connection.QueryAsync<ImageEntity>("Proc_Image_History", param, commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<int> InsertAsync(ImageEntityDto imageEntityDto)
        {
            var memberEntity = await _memberService.FindByIdAsync(imageEntityDto.MemberId);

            if (memberEntity != null)
            {
                imageEntityDto.ImageData = await _firebaseService.UploadImageAsync(FolderEnum.History, imageEntityDto.ImageData);
            }
            else throw new BadRequestException($"Không tìm thấy thành viên có id = '{imageEntityDto.MemberId}'.", "Thông tin thành viên không hợp lệ.");


            var param = new
            {
                p_ImageId = Guid.NewGuid(),
                p_MemberId = imageEntityDto.MemberId,
                p_DeviceId = imageEntityDto.DeviceId,
                p_ImageLink = imageEntityDto.ImageData,
                p_CreatedDate = imageEntityDto.CreatedDate,
                p_CreatedBy = "CX01",
                p_NotifId = imageEntityDto.NotifId
            };

            var result = await _unitOfWork.Connection.ExecuteAsync("Proc_Image_Insert", param, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<int> DeleteAsync(Guid imageId)
        {
            var imageEntity = await FindByIdAsync(imageId);

            if (imageEntity != null)
            {
                await _firebaseService.DeleteImageAsync(imageEntity.ImageLink);
            }

            var param = new
            {
                p_ImageId = imageId,
            };

            var result = await _unitOfWork.Connection.ExecuteAsync("Proc_Image_Delete", param, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<int> DeleteManyAsync(List<Guid> imageIds)
        {
            string listId = string.Join(",", imageIds);

            var param = new
            {
                p_ImageIds = listId
            };

            var result = await _unitOfWork.Connection.ExecuteAsync("Proc_Image_DeleteMany", param, commandType: CommandType.StoredProcedure);

            return result;
        }
    }
}
