using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SmartLockDoor.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountService _accountService;
        private readonly IEmailService _emailService;
        private readonly ICloudinaryService _cloudinaryService;

        public AccountsController(IConfiguration configuration, IUserService userService, IUnitOfWork unitOfWork, IAccountService accountService, ICloudinaryService cloudinaryService, IEmailService emailService)
        {
            _configuration = configuration;
            _userService = userService;
            _unitOfWork = unitOfWork;
            _accountService = accountService;
            _emailService = emailService;
            _cloudinaryService = cloudinaryService;
        }

        /// <summary>
        /// Lấy ra danh sách tài khoản
        /// </summary>
        /// <returns>Danh sách tài khoản</returns>
        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<List<AccountEntity>> GetAllAsync()
        {
            var sql = "SELECT * FROM account ORDER BY VerifiedDate DESC";

            var accountEntities = await _unitOfWork.Connection.QueryAsync<AccountEntity>(sql);

            return accountEntities.ToList();
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

            if (accountEntity == null)
            {
                var verifyToken = await _accountService.RegisterAsync(accountEntityDto);

                verifyToken = verifyToken.Replace("+", "%2B").Replace("/", "%2F").Replace("=", "%3D");

                var verifyUrl = $"https://localhost:7106/api/v1/Accounts/VerifyAccount?token={verifyToken}";

                var emailDto = new EmailDto
                {
                    To = "trantrungkien532@gmail.com",
                    Subject = "Xác thực tài khoản",
                    Body = _emailService.GetVerifyBodyEmail(verifyUrl)
                };

                _emailService.SendEmail(emailDto);

                return 1;
            }

            if (accountEntity.VerifiedDate == null)
            {
                var verifyToken = await _accountService.UpdateRegisterAsync(accountEntityDto);

                verifyToken = verifyToken.Replace("+", "%2B").Replace("/", "%2F").Replace("=", "%3D");

                var verifyUrl = $"https://localhost:7106/api/v1/Accounts/VerifyAccount?token={verifyToken}";

                var emailDto = new EmailDto
                {
                    To = "trantrungkien532@gmail.com",
                    Subject = "Xác thực tài khoản",
                    Body = _emailService.GetVerifyBodyEmail(verifyUrl)
                };

                _emailService.SendEmail(emailDto);

                return 1;
            }
            else throw new ConflictException($"Email '{accountEntityDto.Email}' đã tồn tại.", "Email đã đăng ký tài khoản.");
        }

        /// <summary>
        /// Xác thực tài khoản
        /// </summary>
        [HttpGet]
        [Route("VerifyAccount")]
        public async Task<ActionResult<string>> VerifyAccountAsync(string token)
        {
            var accountEntity = await _accountService.GetAccountAsync("VerifyToken", token);

            if (accountEntity == null || accountEntity.VerifyTokenExpires < DateTime.Now)
            {
                return BadRequest("Mã xác thực không hợp lệ");
            }

            var result = await _accountService.UpdateVerifiedAsync(accountEntity.Email);

            if (result == 1)
            {
                return Ok();
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
        public async Task<ActionResult<Token>> LoginAsync(AccountEntityDto accountEntityDto)
        {
            var accountEntity = await _accountService.GetAccountAsync("Email", accountEntityDto.Email);

            if (accountEntity == null)
            {
                return BadRequest("Email chưa đăng ký tài khoản.");
            }

            if (accountEntity.VerifiedDate == null)
            {
                return BadRequest("Email chưa được xác thực.");
            }

            if (!_accountService.VerifyPasswordHash(accountEntityDto.Password, accountEntity.PasswordHash, accountEntity.PasswordSalt))
            {
                return BadRequest("Mật khẩu không đúng.");
            }

            var accessToken = _accountService.CreateAccessToken(accountEntityDto.Email, "User");

            var newRefreshToken = _accountService.CreateRefreshToken();

            var token = new Token
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token
            };

            var result = await _accountService.UpdateTokenAsync(accountEntityDto.Email, newRefreshToken.Token, newRefreshToken.Expires, "RefreshToken");

            if (result == 1)
            {
                return Ok(token);
            }
            else throw new Exception("Cập nhập RefreshToken thất bại.");
        }

        /// <summary>
        /// Lấy mã xác thực đặt lại mật khẩu
        /// </summary>
        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPasswordAsync(string email)
        {
            var accountEntity = await _accountService.GetAccountAsync("Email", email);

            if (accountEntity == null)
            {
                return BadRequest("Email chưa đăng ký");
            }

            if (accountEntity.VerifiedDate == null)
            {
                return BadRequest("Email chưa được xác thực");
            }

            var resetPasswordToken = _accountService.CreatePasswordResetToken();

            var result = await _accountService.UpdateTokenAsync(email, resetPasswordToken.Token, resetPasswordToken.Expires, "PasswordToken");

            if (result == 1)
            {
                return Ok();
            }
            else throw new Exception("Cập nhập PasswordToken thất bại.");
        }

        /// <summary>
        /// Đặt lại mật khẩu
        /// </summary>
        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPasswordAsync(PasswordReset passwordReset)
        {
            var accountEntity = await _accountService.GetAccountAsync("PasswordToken", passwordReset.PasswordToken);

            if (accountEntity == null || accountEntity.PasswordTokenExpires < DateTime.Now)
            {
                return BadRequest("Mã xác thực không hợp lệ");
            }

            _accountService.CreatePasswordHash(passwordReset.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

            var result = await _accountService.UpdatePasswordAsync(accountEntity.Email, passwordHash, passwordSalt);

            if (result == 1)
            {
                return Ok();
            }
            else throw new Exception("Cập nhập mật khẩu thất bại.");
        }

        /// <summary>
        /// Đổi tên người dùng
        /// </summary>
        [HttpPut]
        [Route("Username")]
        [Authorize(Roles = "User")]
        public async Task<int> UpdateUsernameAsync(string username)
        {
            var email = _userService.GetMyEmail();

            var result = await _accountService.UpdateUserInfoAsync(email, username, null);

            return result;
        }

        /// <summary>
        /// Đổi ảnh người dùng
        /// </summary>
        [HttpPut]
        [Route("UserImage")]
        [Authorize(Roles = "User")]
        public async Task<int> UpdateUserImageAsync([FromBody] string imageBase64Data)
        {
            var email = _userService.GetMyEmail();

            var imageUri = _cloudinaryService.UploadImage(imageBase64Data);

            var result = await _accountService.UpdateUserInfoAsync(email, null, imageUri);

            return result;
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        [HttpPut]
        [Route("Password")]
        [Authorize(Roles = "User")]
        public async Task<int> UpdatePasswordAsync([FromBody] PasswordChange passwordChange)
        {
            var email = _userService.GetMyEmail();

            var accountEntity = await _accountService.GetAccountAsync("Email", email);

            if (accountEntity != null && !_accountService.VerifyPasswordHash(passwordChange.CurrentPassword, accountEntity.PasswordHash, accountEntity.PasswordSalt))
            {
                throw new BadHttpRequestException("Mật khẩu hiện tại không đúng.");
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
        public async Task<ActionResult<Token>> GetNewAccessTokenAsync(string refreshToken)
        {
            var accountEntity = await _accountService.GetAccountAsync("RefreshToken", refreshToken);

            if (accountEntity == null)
            {
                return Unauthorized("Refresh token không hợp lệ.");
            }

            if (accountEntity.RefreshTokenExpires < DateTime.Now)
            {
                return Unauthorized("Refresh token đã hết hiệu lực.");
            }

            string accessToken = _accountService.CreateAccessToken(accountEntity.Email, "User");
            var newRefreshToken = _accountService.CreateRefreshToken();

            var token = new Token
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token
            };

            if (accountEntity.RefreshTokenExpires != null)
            {
                newRefreshToken.Expires = accountEntity.RefreshTokenExpires.Value.DateTime;
            }

            var result = await _accountService.UpdateTokenAsync(accountEntity.Email, newRefreshToken.Token, newRefreshToken.Expires, "RefreshToken");

            if (result == 1)
            {
                return Ok(token);
            }
            else throw new Exception("Cập nhập RefreshToken thất bại.");
        }
    }
}
