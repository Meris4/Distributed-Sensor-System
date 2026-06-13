using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shared.Contracts;

namespace SensorSimulator
{
    public class SensorConfig
    {
        public string SensorId { get; set; }
        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set; }
        public DataQuality Quality { get; set; }

        public double AlarmLimitPriority1 { get; set; }
        public double AlarmLimitPriority2 { get; set; }
        public double AlarmLimitPriority3 { get; set; }
    }
}
