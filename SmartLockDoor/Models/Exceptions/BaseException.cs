using System.Text.Json;

namespace SmartLockDoor
{
    public class BaseException
    {
        #region Properties
        public string? DevMessage { get; set; }

        public string? UserMessage { get; set; }
        #endregion

        #region Methods
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
        #endregion
    }
}
