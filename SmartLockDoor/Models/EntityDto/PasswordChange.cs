using System.ComponentModel.DataAnnotations;

namespace SmartLockDoor
{
    public class PasswordChange
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Mật khẩu cần có độ dài tối thiểu 6 ký tự")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu không trùng khớp")]
        public string CompareNewPassword { get; set; } = string.Empty;
    }
}
