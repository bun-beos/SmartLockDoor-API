namespace SmartLockDoor
{
    public class MemberEntityDto
    {
        public string MemberName { get; set; } = string.Empty;

        public string MemberPhoto { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        public string? PhoneNumber { get; set; }
    }
}
