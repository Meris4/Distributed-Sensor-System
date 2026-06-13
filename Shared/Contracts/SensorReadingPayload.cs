namespace Shared.Contracts
{
    public class SensorReadingPayload
    {
        public double Temperature { get; set; }
        public DataQuality Quality { get; set; }
        public int AlarmPriority { get; set; }
    }
}
