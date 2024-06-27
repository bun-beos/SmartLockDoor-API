namespace SmartLockDoor
{
    public class MqttPublishRequest
    {
        public string Topic { get; set; } = string.Empty;

        public string Payload { get; set; } = string.Empty;
    }
}
