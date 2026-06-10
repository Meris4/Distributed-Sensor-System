namespace ConsensusService.Models
{
    public enum SensorQuality
    {
        GOOD,
        BAD,
        UNCERTAIN
    }

    public class Sensor
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public SensorQuality Quality { get; set; } = SensorQuality.GOOD;

        public DateTime LastSeen { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<SensorMeasurement> Measurements { get; set; }
            = new List<SensorMeasurement>();
    }
}
