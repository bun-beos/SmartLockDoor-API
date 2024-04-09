namespace SmartLockDoor
{
    public interface IFirebaseService
    {
        /// <summary>
        /// Upload ảnh lên Firebase
        /// </summary>
        /// <param name="folderEnum">Tên folder</param>
        /// <param name="imageBase64Data">Dữ liệu ảnh</param>
        /// <returns></returns>
        Task<string> UploadImageAsync(FolderEnum folderEnum, string imageBase64Data);

        /// <summary>
        /// Xóa ảnh 
        /// </summary>
        /// <param name="imageUrl">Đường link ảnh</param>
        /// <returns></returns>
        Task DeleteImageAsync(string imageUrl);
    }
}
