using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.CodeDom;

namespace SmartLockDoor.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public MembersController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        /// <summary>
        /// Lấy tất cả thành viên
        /// </summary>
        /// <returns>Danh sách thành viên</returns>
        [HttpGet]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<List<MemberEntity>> GetAllAsync()
        {
            var result = await _memberService.GetAllAsync();

            return result;
        }

        /// <summary>
        /// Lấy thành viên theo id
        /// </summary>
        /// <param name="id">id thành viên</param>
        /// <returns>Thông tin thành viên</returns>
        [HttpGet]
        [Route("{id}")]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<MemberEntity> GetMemberAsync(Guid id)
        {
            var result = await _memberService.GetByIdAsync(id);

            return result;
        }

        /// <summary>
        /// Thêm mới thành viên
        /// </summary>
        /// <param name="memberEntityDto">Thông tin thành viên</param>
        /// <returns>Số bản ghi thay đổi</returns>
        [HttpPost]
        [Route("NewMember")]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<int> InsertMemberAsync(MemberEntityDto memberEntityDto)
        {
            var memberEntity = await _memberService.FindByNameAsync(memberEntityDto.MemberName);

            if (memberEntity != null)
            {
                throw new ConflictException($"Trùng tên thành viên '{memberEntityDto.MemberName}'.", "Tên thành viên đã tồn tại.");
            } else {
            var result = await _memberService.InsertAsync(memberEntityDto);

            return result;
            }
        }

        /// <summary>
        /// Thay đổi thông tin thành viên theo id
        /// </summary>
        /// <param name="memberId">id thành viên</param>
        /// <param name="memberEntityDto">Thông tin mới</param>
        /// <returns>Số bản ghi thay đổi</returns>
        [HttpPut]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<int> UpdateMemberAsync(Guid memberId, MemberEntityDto memberEntityDto)
        {
            var result = await _memberService.UpdateAsync(memberId, memberEntityDto);

            return result;
        }

        /// <summary>
        /// Xóa thành viên theo id
        /// </summary>
        /// <param name="id">id thành viên</param>
        /// <returns>Số bản ghi thay đổi</returns>
        [HttpDelete]
        [Route("{id}")]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<int> DeleteMemberAsync(Guid id)
        {
            var result = await _memberService.DeleteAsync(id);

            return result;
        }

        /// <summary>
        /// Xóa nhiều thành viên theo list id
        /// </summary>
        /// <param name="ids">list id</param>
        /// <returns>Số bản ghi thay đổi</returns>
        [HttpDelete]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<int> DeleteManyMemberAsync(List<Guid> ids)
        {
            var result = await _memberService.DeleteManyAsync(ids);

            return result;
        }
    }
}
