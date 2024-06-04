using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartLockDoor
{
    public class MemberEntity
    {
        public Guid MemberId { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public string MemberPhoto { get; set; } = string.Empty;

        public DateTimeOffset? DateOfBirth { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public DateTimeOffset ModifiedDate { get; set; }

        public string ModifiedBy { get; set;} = string.Empty;
    }
}
