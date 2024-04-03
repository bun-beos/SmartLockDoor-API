using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartLockDoor.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        public MembersController(IUnitOfWork unitOfWork, ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet]
        //[Authorize(Roles = "User")]
        public async Task<List<MemberEntity>> GetAllAsync()
        {
            var sql = "SELECT * FROM member ORDER BY MemberName ASC";

            var result = await _unitOfWork.Connection.QueryAsync<MemberEntity>(sql);

            return result.ToList();
        }

        [HttpGet]
        [Route("{name}")]
        //[Authorize(Roles = "User")]
        public async Task<MemberEntity?> GetMemberAsync(string name)
        {
            var param = new
            {
                name,
            };

            var sql = "SELECT * FROM member WHERE MemberName = @name";

            var result = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<MemberEntity>(sql, param);

            if (result != null)
            {
                return result;
            }
            else throw new NotFoundException($"Không tìm thấy thành viên có tên '{name}'.", "Không tìm thấy dữ liệu thành viên.");
        }

        [HttpPost]
        //[Authorize(Roles = "User")]
        public async Task<IActionResult> InsertMemberAsync(MemberEntityDto memberEntityDto)
        {
            var param = new
            {
                memberId = Guid.NewGuid(),
                memberName = memberEntityDto.MemberName,
                memberPhoto = _cloudinaryService.UploadImage(memberEntityDto.MemberPhoto),
                createdDate = DateTime.Now,
            };

            var sql = "INSERT INTO member (MemberId, MemberName, MemberPhoto, CreatedDate) VALUES (@memberId, @memberName, @memberPhoto, @createdDate)";

            var memberEntity = await GetMemberAsync(param.memberName);
            if (memberEntity != null)
            {
                throw new ConflictException($"Trùng tên thành viên 'MemberName = {param.memberName}.", "Tên thành viên đã tồn tại.");
            }
            else
            {
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

        //[HttpPut]
        //[Route("{memberId")]
        //[Authorize(Roles = "User")]
        //public async Task<IActionResult> UpdateMemberAsync(MemberEntityDto memberEntityDto)
        //{

        //}
    }
}
