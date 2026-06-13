using System.Text.Json.Serialization;

namespace Shared.Contracts
{
    public class SensorReadingRequest
    {
        public string SensorId { get; set; } = string.Empty;
        public DateTime TimestampUtc { get; set; }
        public long MessageId { get; set; }

        // Encrypted Temperature, Quality, AlarmPriority
        public string EncryptedPayload { get; set; } = string.Empty;
        public string EncryptedKey { get; set; } = string.Empty;
        public string Iv { get; set; } = string.Empty;

        public string Signature { get; set; } = string.Empty;
        
        // Filled by server after decryption
        [JsonIgnore] public double Temperature { get; set; }
        [JsonIgnore] public DataQuality Quality { get; set; }
        [JsonIgnore] public int AlarmPriority { get; set; }
    }
}