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

                if (now - sensor.LastSeen > TimeSpan.FromSeconds(10))
                {
                    sensor.IsActive = false;
                    continue;
                }

                sensor.IsActive = true;

                var recentMeasurements = sensor.Measurements
                    .OrderByDescending(x => x.Timestamp)
                    .Take(3)
                    .ToList();

                if (recentMeasurements.Count == 0)
                    continue;

                var invalidCount = recentMeasurements.Count(x =>
                    x.Temperature < -100 ||
                    x.Temperature > 500);

                if (invalidCount == 3)
                {
                    sensor.Quality = DataQuality.BAD;
                    sensor.IsActive = false;
                }
                else
                {
                    sensor.Quality = DataQuality.GOOD;
                }
            }

            var activeGood = sensors
                .Where(s => s.IsActive &&
                            s.Quality == DataQuality.GOOD)
                .ToList();

            if (activeGood.Count > 5)
            {
                foreach (var sensor in activeGood.Skip(5))
                {
                    sensor.IsActive = false;
                }
            }
            else if (activeGood.Count < 5)
            {
                var replacements = sensors
                    .Where(s =>
                        !s.IsActive &&
                        s.Quality == DataQuality.GOOD &&
                        now - s.LastSeen <= TimeSpan.FromSeconds(10))
                    .Take(5 - activeGood.Count);

                foreach (var sensor in replacements)
                {
                    sensor.IsActive = true;
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}