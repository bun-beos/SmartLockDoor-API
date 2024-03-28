using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SmartLockDoor.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;

        public AccountsController(IConfiguration configuration, IUserService userService, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _userService = userService;
            _unitOfWork = unitOfWork;
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
        /// <returns>1-Thành công, 0-Thất bại</returns>
        [HttpPost]
        [Route("Registration")]
        public async Task<int> RegisterAsync(AccountEntityDto accountEntityDto)
        {
            if (accountEntityDto.Username == null || accountEntityDto.Username == string.Empty)
            {
                accountEntityDto.Username = accountEntityDto.Email.Split("@")[0];
            }

            var paramEmail = new
            {
                email = accountEntityDto.Email,
            };

            var sql = $"SELECT * FROM account WHERE Email = @email";

            var accountEntity = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<AccountEntity>(sql, paramEmail);

            if (accountEntity == null)
            {
                CreatePasswordHash(accountEntityDto.Password, out byte[] passwordHash, out byte[] passwordSalt);
                var newVerifyToken = CreateRegistrationVerifyToken();
                var paramAccount = new
                {
                    accountId = Guid.NewGuid(),
                    email = accountEntityDto.Email,
                    passwordHash,
                    passwordSalt,
                    username = accountEntityDto.Username,
                    verifyToken = newVerifyToken.Token,
                    verifyTokenExpires = newVerifyToken.Expires,
                };

                var sqlNewAcc = @"INSERT INTO account (AccountId, Email, PasswordHash, PasswordSalt, Username, VerifyToken, VerifyTokenExpires) VALUES (@accountId, @email, @passwordHash, @passwordSalt, @username, @verifyToken, @verifyTokenExpires)";

                var result = await _unitOfWork.Connection.ExecuteAsync(sqlNewAcc, paramAccount);

                return result;
            }

            if (accountEntity.VerifiedDate == null)
            {
                var newVerifyToken = CreateRegistrationVerifyToken();

                var paramAccount = new
                {
                    email = accountEntityDto.Email,
                    verifyToken = newVerifyToken.Token,
                    verifyTokenExpires = newVerifyToken.Expires,
                };

                var sqlUpdateAcc = @"UPDATE account SET VerifyToken = @verifyToken, VerifyTokenExpires = @verifyTokenExpires WHERE Email = @email";

                var result = await _unitOfWork.Connection.ExecuteAsync(sqlUpdateAcc, paramAccount);

                return result;
            }

            return 0;
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="accountEntityDto">Email và mật khẩu</param>
        /// <returns>Access token</returns>
        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<string>> LoginAsync(AccountEntityDto accountEntityDto)
        {
            var param = new
            {
                email = accountEntityDto.Email,
            };

            var sql = $"SELECT * FROM account WHERE Email = @email";

            var accountEntity = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<AccountEntity>(sql, param);

            if (accountEntity == null)
            {
                return BadRequest("Email chưa đăng ký tài khoản");
            }

            if (accountEntity.VerifiedDate == null)
            {
                return BadRequest("Tài khoản chưa được xác thực");
            }

            if (!VerifyPasswordHash(accountEntityDto.Password, accountEntity.PasswordHash, accountEntity.PasswordSalt))
            {
                return BadRequest("Mật khẩu không đúng");
            }

            string token = CreateAccessToken(accountEntity, "User");

            var newRefreshToken = CreateRefreshToken();
            SetRefreshToken(newRefreshToken);

            var param1 = new
            {
                email = accountEntity.Email,
                token = newRefreshToken.Token,
                created = newRefreshToken.Created,
                expries = newRefreshToken.Expires,
            };

            var sql1 = @"UPDATE account SET RefreshToken = @token, RefreshTokenCreated = @created, RefreshTokenExpires = @expries WHERE Email = @email";

            await _unitOfWork.Connection.ExecuteAsync(sql1, param1);

            return Ok(token);
        }

        /// <summary>
        /// Đổi tên người dùng
        /// </summary>
        [HttpPut]
        [Route("Username")]
        [Authorize(Roles = "User")]
        public async Task<int> UpdateUsernameAsync(string? newUsername)
        {
            var param = new
            {
                email = _userService.GetMyEmail(),
                newUsername,
            };

            var sql = @"UPDATE account SET Username = @newUsername WHERE Email = @email";

            var result = await _unitOfWork.Connection.ExecuteAsync(sql, param);

            return result;
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        [HttpPut]
        [Route("Password")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<string>> UpdatePasswordAsync([FromBody] PasswordChange passwordChange)
        {
            var paramEmail = new
            {
                email = _userService.GetMyEmail()
            };

            var sqlAcc = $"SELECT * FROM account WHERE Email = @email";

            var accountEntity = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<AccountEntity>(sqlAcc, paramEmail);

            if (accountEntity != null && !VerifyPasswordHash(passwordChange.CurrentPassword, accountEntity.PasswordHash, accountEntity.PasswordSalt))
            {
                return BadRequest("Mật khẩu hiện tại không đúng");
            }

            CreatePasswordHash(passwordChange.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

            var paramPassword = new
            {
                passwordHash,
                passwordSalt,
            };

            var sqlPassword = $"UPDATE account SET PasswordHash = @passwordHash, PasswordSalt = @passwordSalt WHERE Email = '{paramEmail.email}'";

            try
            {
                await _unitOfWork.Connection.ExecuteAsync(sqlPassword, paramPassword);
                return Ok();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Tạo access token mới
        /// </summary>
        [HttpPost]
        [Route("NewAccessToken")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<string>> GetNewAccessTokenAsync()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var sql = $"SELECT * FROM account WHERE RefreshToken = '{refreshToken}'";

            var account = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<AccountEntity>(sql);

            if (account == null || account.Email != _userService.GetMyEmail())
            {
                return Unauthorized("Refresh token không hợp lệ");
            }
            else if (account.RefreshTokenExpires < DateTime.Now)
            {
                return Unauthorized("Refresh token đã hết hiệu lực");
            }

            string token = CreateAccessToken(account, "User");

            return Ok(token);
        }

        /// <summary>
        /// Xác thực tài khoản
        /// </summary>
        [HttpPost]
        [Route("VerifyAccount")]
        public async Task<ActionResult<string>> VerifyAccountAsync(string token)
        {
            var paramToken = new
            {
                verifyToken = token
            };

            var sqlAcc = $"SELECT * FROM account WHERE VerifyToken = @verifyToken";

            var accountEntity = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<AccountEntity>(sqlAcc, paramToken);

            if (accountEntity == null || accountEntity.VerifyTokenExpires < DateTime.Now)
            {
                return BadRequest("Mã xác thực không hợp lệ");
            }

            var paramAcc = new
            {
                verifiedDate = DateTime.Now,
            };

            var sqlUpdate = $"UPDATE account SET VerifiedDate = @verifiedDate WHERE VerifyToken = '{paramToken.verifyToken}'";

            try
            {
                await _unitOfWork.Connection.ExecuteAsync(sqlUpdate, paramAcc);
                return Ok();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Lấy mã xác thực đặt lại mật khẩu
        /// </summary>
        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPasswordAsync(string email)
        {
            var paramEmail = new
            {
                email
            };

            var sql = $"SELECT * FROM account WHERE Email = @email";

            var accountEntity = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<AccountEntity>(sql, paramEmail);

            if (accountEntity == null)
            {
                return BadRequest("Email chưa đăng ký");
            }

            var resetPasswordToken = CreatePasswordResetToken();

            var paramToken = new
            {
                passwordToken = resetPasswordToken.Token,
                passwordTokenExpires = resetPasswordToken.Expires,
            };

            var sqlUpdate = $"UPDATE account SET passwordToken = @passwordToken, passwordTokenExpires = @passwordTokenExpires WHERE Email = '{paramEmail.email}'";

            try
            {
                await _unitOfWork.Connection.ExecuteAsync(sqlUpdate, paramToken);
                return Ok();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Đặt lại mật khẩu
        /// </summary>
        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPasswordAsync(PasswordReset passwordReset)
        {
            var paramToken = new
            {
                passwordToken = passwordReset.PasswordToken,
            };

            var sqlAcc = $"SELECT * FROM account WHERE PasswordToken = @passwordToken";

            var accountEntity = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<AccountEntity>(sqlAcc, paramToken);

            if (accountEntity == null || accountEntity.PasswordTokenExpires < DateTime.Now)
            {
                return BadRequest("Mã xác thực không hợp lệ");
            }

            CreatePasswordHash(passwordReset.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

            var paramPassword = new
            {
                passwordHash,
                passwordSalt,
            };

            var sqlPassword = $"UPDATE account SET PasswordHash = @passwordHash, PasswordSalt = @passwordSalt WHERE Email = '{accountEntity.Email}'";

            try
            {
                await _unitOfWork.Connection.ExecuteAsync(sqlPassword, paramPassword);
                return Ok();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        /// <summary>
        /// Băm mật khẩu
        /// </summary>
        private void CreatePasswordHash(string password, out byte[] paswordHash, out byte[] passwordSalt)
        {
            var hmac = new HMACSHA512();

            passwordSalt = hmac.Key;
            paswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }

        /// <summary>
        /// Xác thực mật khẩu
        /// </summary>
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            var hmac = new HMACSHA512(passwordSalt);

            var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            return computeHash.SequenceEqual(passwordHash);
        }

        /// <summary>
        /// Tạo mã xác thực email
        /// </summary>
        private VerifyToken CreateRegistrationVerifyToken()
        {
            var verifyToken = new VerifyToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
                Expires = DateTime.Now.AddMinutes(10),
            };

            return verifyToken;
        }

        /// <summary>
        /// Tạo access token
        /// </summary>
        private string CreateAccessToken(AccountEntity accountEntity, string roleValue)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Email, accountEntity.Email),
                new Claim(ClaimTypes.Role, roleValue),
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: cred);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        /// <summary>
        /// Tạo refresh token
        /// </summary>
        private RefreshToken CreateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Created = DateTime.Now,
                Expires = DateTime.Now.AddDays(7),
            };

            return refreshToken;
        }

        /// <summary>
        /// Set refresh token
        /// </summary>
        private void SetRefreshToken(RefreshToken newRefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires,
            };
            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);
        }

        /// <summary>
        /// Tạo mã xác thực quên mật khẩu
        /// </summary>
        private VerifyToken CreatePasswordResetToken()
        {
            var verifyToken = new VerifyToken
            {
                Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)),
                Expires = DateTime.Now.AddMinutes(10),
            };

            return verifyToken;
        }
    }
}
