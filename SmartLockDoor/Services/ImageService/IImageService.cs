namespace SmartLockDoor
{
    public interface IImageService
    {
        /// <summary>
        /// Lấy thời gian của ảnh cũ nhất
        /// </summary>
        /// <returns>Thời gian ảnh cũ nhất</returns>
        Task<DateTimeOffset?> GetOldestAsync();

        /// <summary>
        /// Tìm ảnh theo id
        /// </summary>
        /// <param name="id">id ảnh</param>
        /// <returns>Thông tin ảnh</returns>
        Task<ImageEntity?> FindByIdAsync(Guid id);

        /// <summary>
        /// Lọc lịch sử ảnh theo id thành viên hoặc thời gian
        /// </summary>
        /// <param name="memberId">id thành viên</param>
        /// <param name="startDate">Ngày bắt đầu</param>
        /// <param name="endDate">Ngày kết thúc</param>
        /// <returns></returns>
        Task<List<ImageEntity>> FilterAsync(Guid? memberId, DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Thêm ảnh mới
        /// </summary>
        /// <param name="imageEntityDto">Thông tin ảnh</param>
        /// <returns>Số lượng bản ghi thay đổi</returns>
        Task<int> InsertAsync(ImageEntityDto imageEntityDto);

        /// <summary>
        /// Xóa ảnh theo id
        /// </summary>
        /// <param name="imageId">id ảnh</param>
        /// <returns>Số lượng bản ghi thay đổi</returns>
        Task<int> DeleteAsync(Guid imageId);

        /// <summary>
        /// Xóa nhiều ảnh theo danh sách id
        /// </summary>
        /// <param name="imageIds">Danh sách id</param>
        /// <returns>Số lượng bản ghi thay đổi</returns>
        Task<int> DeleteManyAsync(List<Guid> imageIds);
    }
}
