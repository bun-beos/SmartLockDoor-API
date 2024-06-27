namespace SmartLockDoor
{
    public class DeviceEntity
    {
        public Guid DeviceId { get; set; }

        public Guid? AccountId { get; set; }

        public string DeviceName { get; set; } = string.Empty;

        public DeviceEnum DeviceState { get; set; } 

        public DateTime CreatedDate { get; set; }
    }
}
