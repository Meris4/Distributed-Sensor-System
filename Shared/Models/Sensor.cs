using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Contracts;

namespace Shared.Models
{
    public class Sensor
    {
        public string Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DataQuality Quality { get; set; } = DataQuality.GOOD;

        public DateTime LastSeen { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<SensorMeasurement> Measurements { get; set; }
            = new List<SensorMeasurement>();
    }
}
