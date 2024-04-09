namespace SmartLockDoor
{
    public interface IMemberService
    {
        /// <summary>
        /// Lấy tất cả thành viên
        /// </summary>
        /// <returns>Danh sách thành viên</returns>
        Task<List<MemberEntity>> GetAllAsync();

        /// <summary>
        /// Tìm thành viên theo id
        /// </summary>
        /// <param name="id">id thành viên</param>
        /// <returns>Thông tin thành viên hoặc null</returns>
        Task<MemberEntity?> FindByIdAsync(Guid id);

        /// <summary>
        /// Lấy thành viên theo id
        /// </summary>
        /// <param name="id">Id thành viên</param>
        /// <returns>Thông tin thành viên</returns>
        Task<MemberEntity> GetByIdAsync(Guid id);

        /// <summary>
        /// Thêm mới thành viên
        /// </summary>
        /// <param name="memberEntityDto">Thông tin thành viên thêm mới</param>
        /// <returns>Số lượng thành viên đã được thêm(1/0)</returns>
        Task<int> InsertAsync(MemberEntityDto memberEntityDto);

        /// <summary>
        /// Cập nhập thông tin thành viên 
        /// </summary>
        /// <param name="memberId">Id thành viên</param>
        /// <param name="memberEntityDto">Thông tin thành viên</param>
        /// <returns>Số lượng thành viên thay đổi</returns>
        Task<int> UpdateAsync(Guid memberId, MemberEntityDto memberEntityDto);

        /// <summary>
        /// Xóa thành viên theo id
        /// </summary>
        /// <param name="memberId">Id thành viên</param>
        /// <returns>Số lượng thành viên bị xóa</returns>
        Task<int> DeleteAsync(Guid memberId);

        /// <summary>
        /// Xóa nhiều thành viên theo danh sách id
        /// </summary>
        /// <param name="memberIds">Danh sách id</param>
        /// <returns>Số lượng thành viên bị xóa</returns>
        Task<int> DeleteManyAsync(List<Guid> memberIds);

       

    }
}
