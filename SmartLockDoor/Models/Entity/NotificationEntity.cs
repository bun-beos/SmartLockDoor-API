namespace SmartLockDoor
{
    public class NotificationEntity
    {
        public Guid NotifId { get; set; }

        public Guid? DeviceId { get; set; }

        public string NotifTitle { get; set; } = string.Empty;

        public string NotifBody { get; set; } = string.Empty;

        public NotificationEnum NotifState { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
