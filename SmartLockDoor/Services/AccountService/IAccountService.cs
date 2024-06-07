namespace SmartLockDoor
{
    public interface IAccountService
    {
        /// <summary>
        /// Lấy ra tất cả tài khoản
        /// </summary>
        /// <returns>Danh sách tài khoản</returns>
        Task<List<AccountEntity>> GetAllAsync();

        /// <summary>
        /// Lấy tài khoản theo Email, VerifyToken, RefreshToken, PasswordToken
        /// </summary>
        /// <param name="column">Tên cột</param>
        /// <param name="value">Giá trị của cột</param>
        /// <returns>Thông tin tài khoản hoặc null</returns>
        Task<AccountEntity?> GetAccountAsync(string columnName, string columnValue);
        
        /// <summary>
        /// Đăng ký tài khoản
        /// </summary>
        /// <param name="accountEntityDto">Thông tin tài khoản</param>
        /// <returns>Mã xác thực tài khoản</returns>
        Task<string> RegisterAsync(AccountEntityDto accountEntityDto);

        /// <summary>
        /// Cập nhập thông tin đăng ký nếu trước đó đã đăng ký nhưng chưa xác thực
        /// </summary>
        /// <param name="accountEntityDto">Thông tin tài khoản</param>
        /// <returns>Mã xác thực tài khoản</returns>
        Task<string> UpdateRegisterAsync(AccountEntityDto accountEntityDto);

        /// <summary>
        /// Cập nhập ngày xác thực tài khoản
        /// </summary>
        /// <param name="email">Email tài khoản</param>
        /// <returns>1-Thành công, 0-Thất bại</returns>
        Task<int> UpdateVerifiedAsync(string email);

        /// <summary>
        /// Cập nhập RefreshToken hoặc PasswordToken
        /// </summary>
        /// <param name="email">Email tài khoản</param>
        /// <param name="token">Giá trị token</param>
        /// <param name="tokenExpires">Thời hạn</param>
        /// <param name="tokenType">Loại token</param>
        /// <returns>1-Thành công, 0-Thất bại</returns>
        Task<int> UpdateTokenAsync(string email, string token, DateTime tokenExpires, string tokenType, string phoneToken);

        /// <summary>
        /// Cập nhập Username hoặc UserImage
        /// </summary>
        /// <param name="email">Email tài khoản</param>
        /// <param name="username">giá trị Username</param>
        /// <param name="userImage">giá trị UserImage</param>
        /// <returns>1-Thành công, 0-Thất bại</returns>
        Task<int> UpdateUserInfoAsync(string email, string? username, string? userImage);

        /// <summary>
        /// Cập nhập mật khẩu
        /// </summary>
        /// <param name="email">Email đăng ký</param>
        /// <param name="passwordHash">PasswordHash</param>
        /// <param name="passwordSalt">PasswordSalt</param>
        /// <returns>1-Thành công, 0-Thất bại</returns>
        Task<int> UpdatePasswordAsync(string email, byte[] passwordHash, byte[] passwordSalt);

        /// <summary>
        /// Xóa refresh token khi đăng xuất
        /// </summary>
        /// <param name="refreshToken">refresh token</param>
        /// <returns>Số bản ghi thay đổi</returns>
        Task<int> DeleteRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Xóa tài khoản
        /// </summary>
        /// <param name="email">email người dùng</param>
        /// <returns>Số bản ghi thay đổi</returns>
        Task<int> DeleteAccountAsync(string email);


        /// <summary>
        /// Tạo PasswordHash
        /// </summary>
        /// <param name="password">mật khẩu người dùng</param>
        /// <param name="paswordHash">PasswordHash</param>
        /// <param name="passwordSalt">PasswordSalt</param>
        void CreatePasswordHash(string password, out byte[] paswordHash, out byte[] passwordSalt);

        /// <summary>
        /// Xác thực mật khẩu
        /// </summary>
        /// <param name="password">mật khẩu</param>
        /// <param name="passwordHash">PasswordHash</param>
        /// <param name="passwordSalt">PasswordSalt</param>
        /// <returns>true/false</returns>
        bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);

        /// <summary>
        /// Tạo mã xác thực Email
        /// </summary>
        /// <returns>VerifyToken</returns>
        VerifyToken CreateRegistrationVerifyToken();

        /// <summary>
        /// Tạo AccessToken
        /// </summary>
        /// <param name="email">Email tài khoản</param>
        /// <param name="roleValue">Vai trò tài khoản</param>
        /// <returns>AccessToken</returns>
        string CreateAccessToken(string email, string roleValue);

        /// <summary>
        /// Tạo RefreshToken
        /// </summary>
        /// <returns>RefreshToken</returns>
        RefreshToken CreateRefreshToken();

        /// <summary>
        /// Tạo mã xác thực đặt quên mật khẩu
        /// </summary>
        /// <returns>VerifyToken</returns>
        VerifyToken CreatePasswordResetToken();
    }
}
