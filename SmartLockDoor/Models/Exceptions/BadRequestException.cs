namespace SmartLockDoor
{
    public class BadRequestException : Exception
    {
        public string DevMessage { get; set; }
        public string UserMessage { get; set; }

        public BadRequestException(string devMessage, string userMessage)
        {
            DevMessage = devMessage;
            UserMessage = userMessage;
        }
    }
}
