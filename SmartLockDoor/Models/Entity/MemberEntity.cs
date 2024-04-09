namespace SmartLockDoor
{
    public class MemberEntity
    {
        public Guid MemberId { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public string MemberPhoto { get; set; } = string.Empty;

        public DateTimeOffset CreatedDate { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public DateTimeOffset ModifiedDate { get; set; }

        public string ModifiedBy { get; set;} = string.Empty;
    }
}
