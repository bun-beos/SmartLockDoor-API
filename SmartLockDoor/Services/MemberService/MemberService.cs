using Dapper;
using System.Data;

namespace SmartLockDoor
{
    public class MemberService : IMemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IFirebaseService _firebaseService;
        private readonly TimeService _timeService;

        public MemberService(IUnitOfWork unitOfWork, IUserService userService, IFirebaseService firebaseService, TimeService timeService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _firebaseService = firebaseService;
            _timeService = timeService;
        }

        public async Task<List<MemberEntity>> GetAllByDeviceAsync(Guid deviceId)
        {
            var param = new
            {
                p_DeviceId = deviceId,
            };

            var result = await _unitOfWork.Connection.QueryAsync<MemberEntity>("Proc_Member_GetAllByDevice", param, commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<MemberEntity?> FindByIdAsync(Guid id)
        {
            var param = new
            {
                p_MemberId = id,
            };

            var result = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<MemberEntity?>("Proc_Member_GetById", param, commandType: CommandType.StoredProcedure);
                        
            return result;
        }

        public async Task<MemberEntity?> FindByNameAsync(Guid? id, string name)
        {
            var param = new
            {
                p_MemberId = id,
                p_MemberName = name,
            };

            var result = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<MemberEntity?>("Proc_Member_GetByName", param, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<MemberEntity> GetByIdAsync(Guid id)
        {
            var result = await FindByIdAsync(id);

            return result ?? throw new NotFoundException($"Không tìm thấy thành viên có id = '{id}'.", "Không tìm thấy dữ liệu thành viên.");
        }

        public async Task<MemberEntity?> InsertAsync(MemberEntityDto memberEntityDto)
        {
            memberEntityDto.MemberPhoto = await _firebaseService.UploadImageAsync(FolderEnum.Member, memberEntityDto.MemberPhoto) ?? "";
            var param = new
            {
                p_MemberId = Guid.NewGuid(),
                p_DeviceId = memberEntityDto.DeviceId,
                p_MemberName = memberEntityDto.MemberName,
                p_MemberPhoto = memberEntityDto.MemberPhoto,
                p_DateOfBirth = memberEntityDto.DateOfBirth,
                p_PhoneNumber = memberEntityDto.PhoneNumber,
                p_CreatedDate = _timeService.GetLocalTime(),
                p_CreatedBy = _userService.GetMyEmail(),
                p_ModifiedDate = _timeService.GetLocalTime(),
                p_ModifiedBy = _userService.GetMyEmail()
            };

            var result = await _unitOfWork.Connection.ExecuteAsync("Proc_Member_Insert", param, commandType: CommandType.StoredProcedure);

            if (result == 1)
            {
                return await FindByIdAsync(param.p_MemberId);
            }
            else return null;
        }

        public async Task<MemberEntity?> UpdateAsync(Guid memberId, MemberEntityDto memberEntityDto)
        {
            var memberEntity = await FindByIdAsync(memberId) ?? throw new NotFoundException($"Không tìm thấy thành viên có id = '{memberId}'.", "Không tìm thấy dữ liệu thành viên.");

            //var memberExist = await FindByNameAsync(memberId, memberEntityDto.MemberName);

            //if (memberExist != null)
            //{
            //    throw new ConflictException($"Trùng tên thành viên: {memberEntityDto.MemberName}.", "Tên thành viên đã tồn tại.");
            //}

            if (memberEntityDto.MemberPhoto != string.Empty)
            {
                memberEntityDto.MemberPhoto = await _firebaseService.UploadImageAsync(FolderEnum.Member, memberEntityDto.MemberPhoto) ?? "";

                await _firebaseService.DeleteImageAsync(memberEntity.MemberPhoto);
            }
            else
            {
                memberEntityDto.MemberPhoto = memberEntity.MemberPhoto;
            }

            var param = new
            {
                p_MemberId = memberId,
                p_MemberName = memberEntityDto.MemberName,
                p_MemberPhoto = memberEntityDto.MemberPhoto,
                p_DateOfBirth = memberEntityDto.DateOfBirth,
                p_PhoneNumber = memberEntityDto.PhoneNumber,
                p_ModifiedDate = _timeService.GetLocalTime(),
                p_ModifiedBy = _userService.GetMyEmail()
            };

            var result = await _unitOfWork.Connection.ExecuteAsync("Proc_Member_Update", param, commandType: CommandType.StoredProcedure);

            if (result == 1)
            {
                return await FindByIdAsync(memberId);
            }
            else return null;
        }

        public async Task<int> DeleteAsync(Guid memberId)
        {
            var param = new
            {
                p_MemberId = memberId
            };

            var memberEntity = await FindByIdAsync(memberId);

            if (memberEntity != null)
            {
                await _firebaseService.DeleteImageAsync(memberEntity.MemberPhoto);
            }

            var result = await _unitOfWork.Connection.ExecuteAsync("Proc_Member_Delete", param, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<int> DeleteManyAsync(List<Guid> memberIds)
        {
            string listId = string.Join(",", memberIds);

            var param = new
            {
                p_MemberIds = listId
            };

            var result = await _unitOfWork.Connection.ExecuteAsync("Proc_Member_DeleteMany", param, commandType: CommandType.StoredProcedure);

            return result;
        }
    }
}
