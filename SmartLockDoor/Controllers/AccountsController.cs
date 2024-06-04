using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartLockDoor.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly IEmailService _emailService;
        private readonly IFirebaseService _firebaseService;
        private readonly string _host;

        public AccountsController(IConfiguration configuration, IUserService userService, IAccountService accountService, IEmailService emailService, IFirebaseService firebaseService)
        {
            _userService = userService;
            _accountService = accountService;
            _emailService = emailService;
            _firebaseService = firebaseService;
            _host = configuration["Host"];
        }

        /// <summary>
        /// Lấy ra danh sách tài khoản
        /// </summary>
        /// <returns>Danh sách tài khoản</returns>
        [HttpGet]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<List<AccountEntity>> GetAllAsync()
        {
            var result = await _accountService.GetAllAsync();

            return result;
        }

        /// <summary>
        /// Lấy thông tin tài khoản
        /// </summary>
        /// <returns>Thông tin tài khoản người dùng</returns>
        [HttpGet]
        [Route("Info")]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<AccountEntity?> GetAccountAsync()
        {
            var email = _userService.GetMyEmail();

            var result = await _accountService.GetAccountAsync("Email", email);

            return result;
        }

        /// <summary>
        /// Đăng ký tài khoản
        /// </summary>
        /// <param name="accountEntityDto">Email và mật khẩu</param>
        /// <returns>Mã xác thực tài khoản</returns>
        [HttpPost]
        [Route("Registration")]
        public async Task<int> RegisterAsync(AccountEntityDto accountEntityDto)
        {
            if (accountEntityDto.Username == null || accountEntityDto.Username == string.Empty)
            {
                accountEntityDto.Username = accountEntityDto.Email.Split("@")[0];
            }

            var accountEntity = await _accountService.GetAccountAsync("Email", accountEntityDto.Email);

            string verifyToken;

            if (accountEntity == null)
            {
                verifyToken = await _accountService.RegisterAsync(accountEntityDto);
            }
            else if (accountEntity.VerifiedDate == null)
            {
                verifyToken = await _accountService.UpdateRegisterAsync(accountEntityDto);
            }
            else throw new ConflictException($"Email '{accountEntityDto.Email}' đã tồn tại.", "Email đã đăng ký tài khoản.");

            verifyToken = verifyToken.Replace("+", "%2B").Replace("/", "%2F").Replace("=", "%3D");

            var verifyUrl = $"{_host}/api/v1/Accounts/VerifyAccount?code={verifyToken}";

            var emailDto = new EmailDto
            {
                To = accountEntityDto.Email,
                Subject = "Xác thực tài khoản",
                Body = _emailService.GetVerifyTokenBody(verifyUrl)
            };

            _emailService.SendEmail(emailDto);

            return 1;
        }

        /// <summary>
        /// Xác thực tài khoản
        /// </summary>
        [HttpGet]
        [Route("VerifyAccount")]
        public async Task<IActionResult> VerifyAccountAsync(string code)
        {
            var accountEntity = await _accountService.GetAccountAsync("VerifyToken", code);

            if (accountEntity == null || accountEntity.VerifyTokenExpires < DateTime.Now)
            {
                return Redirect($"{_host}/api/v1/Accounts/Verify/failure");
            }

            if (accountEntity.VerifiedDate != null)
            {
                return Redirect($"{_host}/api/v1/Accounts/Verify/success");
            }

            var result = await _accountService.UpdateVerifiedAsync(accountEntity.Email);

            if (result == 1)
            {
                return Redirect($"{_host}/api/v1/Accounts/Verify/success");
            }
            else throw new Exception("Cập nhập dữ liệu thất bại.");
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="accountEntityDto">Email và mật khẩu</param>
        /// <returns>Access token</returns>
        [HttpPost]
        [Route("Login")]
        public async Task<Token> LoginAsync(AccountEntityDto accountEntityDto)
        {
            var accountEntity = await _accountService.GetAccountAsync("Email", accountEntityDto.Email);

            if (accountEntity == null)
            {
                throw new BadRequestException($"Không tìm thấy email: '{accountEntityDto.Email}'.", "Email chưa đăng ký tài khoản.");
            }

            if (accountEntity.VerifiedDate == null)
            {
                throw new BadRequestException($"Tài khoản '{accountEntityDto.Email}' chưa xác thực.", "Email chưa được xác thực.");
            }

            if (!_accountService.VerifyPasswordHash(accountEntityDto.Password, accountEntity.PasswordHash, accountEntity.PasswordSalt))
            {
                throw new BadRequestException("Mật khẩu không hợp lệ.", "Mật khẩu không đúng.");
            }

            var accessToken = _accountService.CreateAccessToken(accountEntityDto.Email, nameof(RolesEnum.User));

            var newRefreshToken = _accountService.CreateRefreshToken();

            var token = new Token
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpires = newRefreshToken.Expires
            };

            var result = await _accountService.UpdateTokenAsync(accountEntityDto.Email, newRefreshToken.Token, newRefreshToken.Expires, "RefreshToken");

            if (result == 1)
            {
                return token;
            }
            else throw new Exception("Cập nhập RefreshToken thất bại.");
        }

        /// <summary>
        /// Lấy mã xác thực đặt lại mật khẩu
        /// </summary>
        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<int> ForgotPasswordAsync([FromBody] string email)
        {
            var accountEntity = await _accountService.GetAccountAsync("Email", email);

            if(accountEntity == null)
            {
                throw new BadRequestException($"Không tìm thấy email: '{email}'.", "Email chưa đăng ký tài khoản.");
            }

            if (accountEntity.VerifiedDate == null)
            {
                throw new BadRequestException($"Tài khoản '{email}' chưa xác thực.", "Email chưa được xác thực.");
            }

            var resetPasswordToken = _accountService.CreatePasswordResetToken();

            var result = await _accountService.UpdateTokenAsync(email, resetPasswordToken.Token, resetPasswordToken.Expires, "PasswordToken");

            var emailDto = new EmailDto
            {
                To = email,
                Subject = $"Mã xác thực của bạn là {resetPasswordToken.Token}",
                Body = _emailService.GetPasswordTokenBody(resetPasswordToken.Token)
            };

            _emailService.SendEmail(emailDto);

            return result;
        }

        /// <summary>
        /// Đặt lại mật khẩu
        /// </summary>
        [HttpPost]
        [Route("ResetPassword")]
        public async Task<int> ResetPasswordAsync(PasswordReset passwordReset)
        {
            var accountEntity = await _accountService.GetAccountAsync("PasswordToken", passwordReset.PasswordToken);

            if (accountEntity == null || accountEntity.PasswordTokenExpires < DateTime.Now)
            {
                throw new BadRequestException($"Mã xác thực {passwordReset.PasswordToken} không hợp lệ.", "Mã xác thực không hợp lệ.");
            }

            _accountService.CreatePasswordHash(passwordReset.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

            var result = await _accountService.UpdatePasswordAsync(accountEntity.Email, passwordHash, passwordSalt);

            return result;
        }

        /// <summary>
        /// Đổi tên người dùng
        /// </summary>
        [HttpPut]
        [Route("Username")]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<int> UpdateUsernameAsync(string name)
        {
            var email = _userService.GetMyEmail();

            var result = await _accountService.UpdateUserInfoAsync(email, name, null);

            return result;
        }

        /// <summary>
        /// Đổi ảnh người dùng
        /// </summary>
        [HttpPut]
        [Route("UserImage")]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<int> UpdateUserImageAsync([FromBody] string imageBase64Data)
        {
            var email = _userService.GetMyEmail();
            var accountEntity = await _accountService.GetAccountAsync("Email", email);

            var imageUri = await _firebaseService.UploadImageAsync(FolderEnum.Account, imageBase64Data);
            if (accountEntity != null)
            {
                await _firebaseService.DeleteImageAsync(accountEntity.UserImage);
            }

            var result = await _accountService.UpdateUserInfoAsync(email, null, imageUri);

            return result;
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        [HttpPut]
        [Route("Password")]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<int> UpdatePasswordAsync(PasswordChange passwordChange)
        {
            var email = _userService.GetMyEmail();

            var accountEntity = await _accountService.GetAccountAsync("Email", email);

            if (accountEntity != null && !_accountService.VerifyPasswordHash(passwordChange.CurrentPassword, accountEntity.PasswordHash, accountEntity.PasswordSalt))
            {
                throw new BadRequestException("Mật khẩu hiện tại không đúng.", "Sai mật khẩu.");
            }

            _accountService.CreatePasswordHash(passwordChange.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

            var result = await _accountService.UpdatePasswordAsync(email, passwordHash, passwordSalt);

            if (result == 1)
            {
                return result;
            }
            else throw new Exception("Cập nhập mật khẩu thất bại.");
        }

        /// <summary>
        /// Tạo access token mới
        /// </summary>
        [HttpPost]
        [Route("NewAccessToken")]
        public async Task<Token> GetNewAccessTokenAsync([FromBody] string refreshToken)
        {
            var accountEntity = await _accountService.GetAccountAsync("RefreshToken", refreshToken) ?? throw new BadRequestException("Refresh token không hợp lệ.", "Refresh token không hợp lệ.");

            if (accountEntity.RefreshTokenExpires < DateTime.Now)
            {
                throw new BadRequestException("Refresh token đã hết hiệu lực.", "Refresh token không hợp lệ.");
            }

            var accessToken = _accountService.CreateAccessToken(accountEntity.Email, nameof(RolesEnum.User));
            var newRefreshToken = _accountService.CreateRefreshToken();

            if (accountEntity.RefreshTokenExpires != null)
            {
                newRefreshToken.Expires = accountEntity.RefreshTokenExpires.Value.DateTime;
            }

            var token = new Token
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpires = newRefreshToken.Expires
            };

            var result = await _accountService.UpdateTokenAsync(accountEntity.Email, newRefreshToken.Token, newRefreshToken.Expires, "RefreshToken");

            if (result == 1)
            {
                return token;
            }
            else throw new Exception("Cập nhập RefreshToken thất bại.");
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        /// <param name="refreshToken">refresh token</param>
        /// <returns>Số bản ghi thay đổi</returns>
        [HttpPost]
        [Route("LogOut")]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<int> LogOutAsync([FromBody] string refreshToken)
        {
            var result = await _accountService.DeleteRefreshTokenAsync(refreshToken);

            return result;
        }

        /// <summary>
        /// Xóa tài khoản
        /// </summary>
        /// <param name="password">Mật khẩu</param>
        /// <returns></returns>
        /// <exception cref="BadHttpRequestException">Mật khẩu không đúng</exception>
        [HttpDelete]
        [Route("Deletion")]
        [Authorize(Roles = nameof(RolesEnum.User))]
        public async Task<int> DeleteAccountAsync([FromBody] string password)
        {
            var email = _userService.GetMyEmail();

            var accountEntity = await _accountService.GetAccountAsync("Email", email);

            if (!_accountService.VerifyPasswordHash(password, accountEntity!.PasswordHash, accountEntity.PasswordSalt))
            {
                throw new BadHttpRequestException("Mật khẩu không đúng.");
            }

            var result = await _accountService.DeleteAccountAsync(email);

            return result;
        }

        /// <summary>
        /// Thông báo xác thực thành công
        /// </summary>
        /// <returns>html content</returns>
        [HttpGet]
        [Route("Verify/success")]
        public IActionResult GetSuccess()
        {
            string htmlContent = "<head> <meta charset=\"UTF-8\"> <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"> <style>body {margin: 0;} .header {display: flex; align-items: center; background-color: #3dafdc; color: #fff; height: 70px; padding-left: 30px; font-size: 22px;} img {width: 50px; height: 50px;} span {margin-top: 14px;} .body {padding: 50px 100px;}.success {font-size: 22px; color: #42bded; margin-bottom: 50px;} .notification {font-size: 18px; margin-top: 40px; line-height: 1.5em;} @media only screen and (max-width: 600px) {.body {padding: 50px 50px;}} </style> </head>  <body> <div class=\"header\"> <img src=\"https://firebasestorage.googleapis.com/v0/b/smart-doorbell-ffebe.appspot.com/o/Smart%20Lock%20Door%2FApp%2Fapp_logo.png?alt=media&token=a6bf0bc1-7240-4432-a7d8-e6b89c0f1ed6\" alt=\"Logo\"> <span>Smart Lock Door</span> </div>  <div class=\"body\"> <div class=\"success\">Đã xác thực thành công</div> <hr> <div class=\"notification\">Chúc mừng!<br><br>Tài khoản của bạn đã được xác thực. Quay lại ứng dụng để có thể đăng nhập ngay bây giờ.</div> </div> </body>";

            return Content(htmlContent, "text/html");
        }

        /// <summary>
        /// Thông báo xác thực thất bại
        /// </summary>
        /// <returns>html content</returns>
        [HttpGet]
        [Route("Verify/failure")]
        public IActionResult GetFailure()
        {
            string htmlContent = "<head> <meta charset=\"UTF-8\"> <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"> <style>body {margin: 0;} .header {display: flex; align-items: center; background-color: #3dafdc; color: #fff; height: 70px; padding-left: 30px; font-size: 22px;} img {width: 50px; height: 50px;} span {margin-top: 14px;} .body {padding: 50px 100px;}.failure {font-size: 22px; color: #ed4248; margin-bottom: 50px;} .notification {font-size: 18px; margin-top: 40px; line-height: 1.5em;} @media only screen and (max-width: 600px) {.body {padding: 50px 50px;}} </style> </head>  <body> <div class=\"header\"> <img src=\"https://firebasestorage.googleapis.com/v0/b/smart-doorbell-ffebe.appspot.com/o/Smart%20Lock%20Door%2FApp%2Fapp_logo.png?alt=media&token=a6bf0bc1-7240-4432-a7d8-e6b89c0f1ed6\" alt=\"Logo\"> <span>Smart Lock Door</span> </div>  <div class=\"body\"> <div class=\"failure\">Xác thực thất bại</div> <hr> <div class=\"notification\">Mã xác thực của bạn đã hết hạn hoặc không hợp lệ.<br>Vui lòng đăng ký tài khoản trên ứng dụng để nhận mã xác thực mới.</div> </div> </body>";

            return Content(htmlContent, "text/html");
        }
    }
}
