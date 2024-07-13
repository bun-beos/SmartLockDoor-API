namespace SmartLockDoor
{
    public class TimeService
    {
        public DateTime GetLocalTime()
        {
            DateTime utcTime = DateTime.UtcNow;

            TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, localTimeZone);

            return localTime;
        }

        public DateTimeOffset GetLocalTimeOffset()
        {
            DateTime utcTime = DateTime.UtcNow;

            DateTimeOffset dateTimeOffset = new DateTimeOffset(utcTime, TimeSpan.Zero).ToOffset(TimeSpan.FromHours(7));

            return dateTimeOffset;
        }
    }
}
