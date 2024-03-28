namespace SmartLockDoor
{
    public class MemberEntity
    {
        public Guid MemberId { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public byte[] MemberPhoto { get; set; } = Array.Empty<byte>();

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset ModifiedDate { get; set; }
    }
}
