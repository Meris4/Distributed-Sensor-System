namespace ConsensusService.DTOs
{
    public class SensorDataDto
    {
        public string SensorId { get; set; }

        public double Temperature { get; set; }

        public int AlarmPriority { get; set; }

        public DateTime Timestamp { get; set; }

        public long MessageId { get; set; }
    }
}
