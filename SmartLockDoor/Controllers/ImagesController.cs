using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartLockDoor.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;

        public ImagesController(IUnitOfWork unitOfWork, ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet]
        public async Task<List<ImageEntity>> GetAllImageAsync()
        {
            var sql = "SELECT * FROM image";

            var result = await _unitOfWork.Connection.QueryAsync<ImageEntity>(sql);

            return result.ToList();
        }

        [HttpGet]
        [Route("Latest")]
        public async Task<IActionResult> GetLatestImageAsync()
        {
            var sql = "SELECT * FROM image ORDER BY CreatedDate DESC LIMIT 1";
            try
            {
                var image = await _unitOfWork.Connection.QueryFirstAsync<ImageEntity>(sql);
                return File(image.ImageData, "image/jpeg");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> InsertImageAsync(ImageEntityDto imageEntityDto)
        {
            var param = new
            {
                imageId = Guid.NewGuid(),
                memberName = imageEntityDto.MemberName,
                imageData = _cloudinaryService.UploadImage(imageEntityDto.ImageData),
                createdDate = DateTime.Now,
            };

            var sql = "INSERT INTO image (ImageId, MemberName, ImageData, CreatedDate) VALUES (@imageId, @memberName, @imageData, @createdDate)";

            try
            {
                await _unitOfWork.Connection.ExecuteAsync(sql, param);
                return Ok("Upload OK");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
