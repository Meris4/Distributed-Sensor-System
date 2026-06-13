namespace Shared.Contracts
{
    public class AlarmNotification
    {
        public string SensorId { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public int AlarmPriority { get; set; }
        public DateTime TimestampUtc { get; set; }
    }
}