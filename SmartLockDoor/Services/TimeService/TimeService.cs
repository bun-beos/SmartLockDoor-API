namespace SmartLockDoor
{
    public class TimeService
    {
        public DateTime GetLocalTime()
        {
            // Lấy thời gian hiện tại theo UTC
            DateTime utcTime = DateTime.UtcNow;

            // Chuyển đổi UTC sang múi giờ địa phương (ví dụ UTC+7)
            TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, localTimeZone);

            return localTime;
        }
    }
}
