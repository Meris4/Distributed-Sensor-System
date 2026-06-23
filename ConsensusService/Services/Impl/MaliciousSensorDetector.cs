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
            var now = DateTime.UtcNow;

            var sensors = await _db.Sensors
                .Include(s => s.Measurements)
                .ToListAsync();

            foreach (var sensor in sensors)
            {
                // timeout
                if (now - sensor.LastSeen > TimeSpan.FromSeconds(10))
                {
                    sensor.IsActive = false;
                    continue;
                }

                var recent = await _db.Measurements
                    .Where(x => x.SensorId == sensor.Id)
                    .OrderByDescending(x => x.Timestamp)
                    .Take(5)
                    .ToListAsync();

                if (recent.Count >= 3)
                {
                    var badRatio = recent.Count(x =>
                        x.Temperature < 20 || x.Temperature > 120)
                        / (double)recent.Count;

                    if (badRatio > 0.6)
                    {
                        sensor.Quality = DataQuality.BAD;
                        sensor.IsActive = false;
                        continue;
                    }
                }

                sensor.Quality = DataQuality.GOOD;
                sensor.IsActive = true;
            }

                await _db.SaveChangesAsync();
        }
    }
}