namespace SmartLockDoor
{
    public class ConflictException : Exception
    {
        public string DevMessage { get; set; }
        public string UserMessage { get; set; }

        public ConflictException(string devMessage, string userMessage)
        {
            DevMessage = devMessage;
            UserMessage = userMessage;
        }
    }
}
