namespace SmartLockDoor
{
    public class NotFoundException : Exception
    {
        public string DevMessage { get; set; }
        public string UserMessage { get; set; }

        public NotFoundException(string devMessage, string userMessage)
        {
            DevMessage = devMessage;
            UserMessage = userMessage;
        }
    }
}
