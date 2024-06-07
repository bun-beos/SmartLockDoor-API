
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SmartLockDoor
{
    public class AccountService : IAccountService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<AccountEntity>> GetAllAsync()
        {
            var result = await _unitOfWork.Connection.QueryAsync<AccountEntity>("Proc_Account_GetAll", commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<AccountEntity?> GetAccountAsync(string columnName, string columnValue)
        {
            var param = new
            {
                p_ColumnName = columnName,
                p_ColumnValue = columnValue
            };

            var account = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<AccountEntity>("Proc_Account_Get", param, commandType: CommandType.StoredProcedure);

            return account;
        }

        public async Task<string> RegisterAsync(AccountEntityDto accountEntityDto)
        {
            CreatePasswordHash(accountEntityDto.Password, out byte[] p_PasswordHash, out byte[] p_PasswordSalt);

            var newVerifyToken = CreateRegistrationVerifyToken();
            var param = new
            {
                p_AccountId = Guid.NewGuid(),
                p_Email = accountEntityDto.Email,
                p_Username = accountEntityDto.Username,
                p_PasswordHash,
                p_PasswordSalt,
                p_VerifyToken = newVerifyToken.Token,
                p_VerifyTokenExpires = newVerifyToken.Expires,
                p_ModifiedDate = DateTime.Now
            };

            var result = await _unitOfWork.Connection.ExecuteAsync("Proc_Account_Register", param, commandType: CommandType.StoredProcedure);

            if (result == 1)
            {
                return newVerifyToken.Token;
            }
            else return string.Empty;
        }

        public async Task<int> UpdateTokenAsync(string email, string token, DateTime tokenExpires, string tokenType, string phoneToken)
        {
            var param = new
            {
                p_Email = email,
                p_Token = token,
                p_TokenExpires = tokenExpires,
                p_TokenType = tokenType,
                p_ModifiedDate = DateTime.Now,
                p_PhoneToken = phoneToken
            };

            var result = await _unitOfWork.Connection.ExecuteAsync("Proc_Account_UpdateToken", param, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<int> UpdatePasswordAsync(string email, byte[] passwordHash, byte[] passwordSalt)
        {
            var param = new
            {
                p_Email = email,
                p_PasswordHash = passwordHash,
                p_PasswordSalt = passwordSalt,
                p_ModifiedDate = DateTime.Now
            };

            var result = await _unitOfWork.Connection.ExecuteAsync("Proc_Account_UpdatePassword", param, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<string> UpdateRegisterAsync(AccountEntityDto accountEntityDto)
        {
            CreatePasswordHash(accountEntityDto.Password, out byte[] p_PasswordHash, out byte[] p_PasswordSalt);

            var newVerifyToken = CreateRegistrationVerifyToken();
            var param = new
            {
                p_Email = accountEntityDto.Email,
                p_Username = accountEntityDto.Username,
                p_PasswordHash,
                p_PasswordSalt,
                p_VerifyToken = newVerifyToken.Token,
                p_VerifyTokenExpires = newVerifyToken.Expires,
                p_ModifiedDate = DateTime.Now
            };

            await _unitOfWork.Connection.ExecuteAsync("Proc_Account_UpdateRegister", param, commandType: CommandType.StoredProcedure);

            return newVerifyToken.Token;
        }

        public async Task<int> UpdateUserInfoAsync(string email, string? username, string? userImage)
        {
            var param = new
            {
                p_Email = email,
                p_Username = username,
                p_UserImage = userImage,
                p_ModifiedDate = DateTime.Now
            };

            var result = await _unitOfWork.Connection.ExecuteAsync("Proc_Account_UpdateUserInfo", param, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<int> UpdateVerifiedAsync(string email)
        {
            var param = new
            {
                p_Email = email,
                p_VerifiedDate = DateTime.Now,
                p_ModifiedDate = DateTime.Now,
            };

            var result = await _unitOfWork.Connection.ExecuteAsync("Proc_Account_UpdateVerified", param, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<int> DeleteRefreshTokenAsync(string refreshToken)
        {
            var param = new
            {
                p_RefreshToken = refreshToken
            };

            var result = await _unitOfWork.Connection.ExecuteAsync("Proc_Account_DeleteRefreshToken", param, commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<int> DeleteAccountAsync(string email)
        {
            var param = new
            {
                p_Email = email
            };

            var result = await _unitOfWork.Connection.ExecuteAsync("Proc_Account_Delete", param, commandType: CommandType.StoredProcedure);

            return result;
        }


        public void CreatePasswordHash(string password, out byte[] paswordHash, out byte[] passwordSalt)
        {
            var hmac = new HMACSHA512();

            passwordSalt = hmac.Key;
            paswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            var hmac = new HMACSHA512(passwordSalt);

            var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            return computeHash.SequenceEqual(passwordHash);
        }

        public VerifyToken CreateRegistrationVerifyToken()
        {
            var verifyToken = new VerifyToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
                Expires = DateTime.Now.AddHours(1),
            };

            return verifyToken;
        }

        public string CreateAccessToken(string email, string roleValue)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, roleValue),
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(2),
                signingCredentials: cred);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public RefreshToken CreateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(128)),
                Expires = DateTime.Now.AddDays(7),
            };

            return refreshToken;
        }

        public VerifyToken CreatePasswordResetToken()
        {
            var verifyToken = new VerifyToken
            {
                Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)),
                Expires = DateTime.Now.AddMinutes(3),
            };

            return verifyToken;
        }
    }
}
