using ConsensusService.Data;
using ConsensusService.Models;
using Microsoft.EntityFrameworkCore;

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
            var now = DateTime.UtcNow;

            var sensors = await _db.Sensors
                .Include(s => s.Measurements)
                .ToListAsync();

            foreach (var sensor in sensors)
            {
                var last = sensor.LastSeen;

                // 1. timeout rule
                if (now - last > TimeSpan.FromSeconds(10))
                {
                    sensor.Quality = SensorQuality.BAD;
                    continue;
                }

                var lastMeasurement = sensor.Measurements
                    .OrderByDescending(m => m.Timestamp)
                    .FirstOrDefault();

                if (lastMeasurement == null)
                    continue;

                // 2. invalid values rule
                if (lastMeasurement.Temperature < -100 ||
                    lastMeasurement.Temperature > 500)
                {
                    sensor.Quality = SensorQuality.BAD;
                    continue;
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}
