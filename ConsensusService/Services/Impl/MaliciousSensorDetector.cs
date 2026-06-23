using ConsensusService.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;

namespace ConsensusService.Services.Impl
{
    public class MaliciousSensorDetector : IMaliciousSensorDetector
    {
        private readonly SensorDbContext _db;

        public MaliciousSensorDetector(SensorDbContext db)
        {
            _db = db;
        }

        public async Task EvaluateAsync()
        {
            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);

            var recentMeasurements = await _db.Measurements
                .Where(m => m.Timestamp >= oneMinuteAgo)
                .ToListAsync();

            var activeSensors = await _db.Sensors.Where(s => s.IsActive).ToListAsync();

            if (recentMeasurements.Count == 0) return;

            var allTemperatures = recentMeasurements.Select(m => m.Temperature).OrderBy(t => t).ToList();
            double medianConsensusTemp = allTemperatures[allTemperatures.Count / 2];

            foreach (var sensor in activeSensors)
            {
                var sensorReadings = recentMeasurements.Where(m => m.SensorId == sensor.Id).ToList();

                if (sensorReadings.Any())
                {
                    var sensorAvg = sensorReadings.Average(r => r.Temperature);

                    if (Math.Abs(sensorAvg - medianConsensusTemp) > 30.0)
                    {
                        sensor.Quality = DataQuality.BAD;
                        sensor.IsActive = false;
                    }
                    else
                    {
                        sensor.Quality = DataQuality.GOOD;
                        sensor.IsActive = true;
                    }
                }
            }

            var activeGood = await _db.Sensors
                .Where(s => s.IsActive && s.Quality == DataQuality.GOOD)
                .ToListAsync();

            if (activeGood.Count < 5)
            {
                var replacements = await _db.Sensors
                    .Where(s => !s.IsActive && s.Quality == DataQuality.GOOD)
                    .OrderBy(s => s.LastSeen)
                    .Take(5 - activeGood.Count)
                    .ToListAsync();

                foreach (var r in replacements)
                    r.IsActive = true;
            }

            await _db.SaveChangesAsync();
        }
    }
}