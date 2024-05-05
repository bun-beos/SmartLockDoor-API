namespace SmartLockDoor
{
    public class Token
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public DateTimeOffset RefreshTokenExpires { get; set; }
    }
}
