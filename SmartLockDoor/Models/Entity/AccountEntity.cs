using System.ComponentModel.DataAnnotations;

namespace SmartLockDoor
{
    public class AccountEntity
    {
        public Guid AccountId { get; set; }

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        public string VerifyToken { get; set; } = string.Empty;

        public DateTimeOffset VerifyTokenExpires { get; set; }

        public DateTimeOffset? VerifiedDate { get; set; }

        public string? RefreshToken { get; set; }

        public DateTimeOffset? RefreshTokenCreated { get; set; }

        public DateTimeOffset? RefreshTokenExpires { get; set; }

        public string? PasswordToken { get; set; }

        public DateTimeOffset? PasswordTokenExpires { get; set; }

        public DateTimeOffset? ModifiedDate { get; set; }
    }
}
