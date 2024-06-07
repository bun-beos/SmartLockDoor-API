namespace SmartLockDoor
{
    public class ImageEntityDto
    {
        public Guid MemberId { get; set; }

        public Guid? DeviceId { get; set; }

        public Guid? NotifId { get; set; }

        public string ImageData { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
    }
}
