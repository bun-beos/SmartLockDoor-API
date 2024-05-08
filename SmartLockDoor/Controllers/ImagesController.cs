using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartLockDoor.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImagesController(IImageService imageService)
        {
            _imageService = imageService;
        }

        /// <summary>
        /// Lấy thời gian ảnh cũ nhất
        /// </summary>
        /// <returns>Thời gian của ảnh cũ nhất</returns>
        [HttpGet]
        [Route("OldestTime")]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<DateTimeOffset?> GetOldestAsync()
        {
            var result = await _imageService.GetOldestAsync();

            return result;
        }

        /// <summary>
        /// Lấy ảnh theo thành viên hoặc thời gian
        /// </summary>
        /// <param name="memberId">id thành viên</param>
        /// <param name="startDate">ngày bắt đầu</param>
        /// <param name="endDate">ngày kết thúc</param>
        /// <returns>Danh sách ảnh</returns>
        [HttpGet]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<List<ImageEntity>> FilterImageAsync(Guid? memberId, DateTime? startDate, DateTime? endDate)
        {
            var result = await _imageService.FilterAsync(memberId, startDate, endDate);

            return result.ToList();
        }

        /// <summary>
        /// Thêm ảnh mới
        /// </summary>
        /// <param name="imageEntityDto">Thông tin ảnh</param>
        /// <returns>Số bản ghi thay đổi</returns>
        [HttpPost]
        [Route("NewImage")]
        public async Task<int> InsertImageAsync(ImageEntityDto imageEntityDto)
        {
            var result = await _imageService.InsertAsync(imageEntityDto);

            return result;
        }

        /// <summary>
        /// Xóa ảnh theo id
        /// </summary>
        /// <param name="id">id ảnh</param>
        /// <returns>Số bản ghi thay đổi</returns>
        [HttpDelete]
        [Route("{id}")]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<int> DeleteImageAsync(Guid id)
        {
            var result = await _imageService.DeleteAsync(id);

            return result;
        }

        /// <summary>
        /// Xóa nhiều ảnh theo danh sách id
        /// </summary>
        /// <param name="ids">danh sách id</param>
        /// <returns>Số bản ghi thay đổi</returns>
        [HttpDelete]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<int> DeleteImageAsync(List<Guid> ids)
        {
            var result = await _imageService.DeleteManyAsync(ids);

            return result;
        }
    }
}
