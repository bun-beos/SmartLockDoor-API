namespace SmartLockDoor
{
    public class ImageEntity
    {
        public Guid ImageId { get; set; }

        public string ImageLink { get; set; } = string.Empty;

        public DateTimeOffset CreatedDate { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public Guid? NotifId { get; set; }

        public Guid MemberId { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public string MemberPhoto { get; set; } = string.Empty;
    }
}
