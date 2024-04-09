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

        public async Task<ImageEntity?> FindByIdAsync(Guid id)
        {
            var param = new
            {
                p_ImageId = id
            };

            var result = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<ImageEntity>("Proc_Image_GetById", param, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<List<ImageEntity>> FilterAsync(Guid? memberId, DateTime? startDate, DateTime? endDate)
        {
            var param = new
            {
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
                p_ImageLink = imageEntityDto.ImageData,
                p_CreatedDate = DateTime.Now,
                p_CreatedBy = "CX01"
            };

            var result = await _unitOfWork.Connection.ExecuteAsync("Proc_Image_Insert", param, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<int> DeleteAsync(Guid imageId)
        {
            var imageEntiy = await FindByIdAsync(imageId);

            if (imageEntiy != null)
            {
                await _firebaseService.DeleteImageAsync(imageEntiy.ImageLink);
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
