using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts
{
    public class SensorReadingRequest
    {
        public string SensorId { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public DataQuality Quality { get; set; }
        public int AlarmPriority { get; set; }
        public DateTime TimestampUtc { get; set; }
        public long MessageId { get; set; }
        public string Signature { get; set; } = string.Empty;
    }
}
