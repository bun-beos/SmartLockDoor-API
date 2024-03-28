namespace SmartLockDoor
{
    public class ImageEntity
    {
        public Guid ImageId { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public byte[] ImageData { get; set; } = Array.Empty<byte>();

        public DateTimeOffset CreatedDate { get; set; }
    }
}
