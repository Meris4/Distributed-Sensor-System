namespace ConsensusService.Models

{
    public class SensorMeasurement
    {
        public Guid Id { get; set; }

        public Guid SensorId { get; set; }

        public Sensor Sensor { get; set; } = null!;

        public double Temperature { get; set; }

        public int AlarmPriority { get; set; }

        public DateTime Timestamp { get; set; }

        public bool IsConsensusValue { get; set; }
    }
}
