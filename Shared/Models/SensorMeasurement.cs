namespace Shared.Models

{
    public class SensorMeasurement
    {
        public Guid Id { get; set; }
        public string SensorId { get; set; }
        public Sensor Sensor { get; set; }

        public double Temperature { get; set; }
        public int AlarmPriority { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
