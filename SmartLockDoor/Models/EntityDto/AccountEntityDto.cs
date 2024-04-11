using System.ComponentModel.DataAnnotations;

namespace SmartLockDoor
{
    public class AccountEntityDto
    {
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Username { get; set; }

        [MinLength(6, ErrorMessage = "Mật khẩu cần có độ dài tối thiểu 6 ký tự")]
        public string Password { get; set; } = string.Empty;
    }
}
