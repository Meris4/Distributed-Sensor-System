using ConsensusService.Data;
using Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ConsensusService.Services.Impl
{
    public class ConsensusCalculationService : IConsensusCalculationService
    {
        private readonly SensorDbContext _db;

        public ConsensusCalculationService(SensorDbContext db)
        {
            _db = db;
        }

        public async Task<double?> CalculateConsensusAsync(DateTime start, DateTime end)
        {
            var values = await _db.Measurements
                .Include(x => x.Sensor)
                .Where(x =>
                    x.Timestamp >= start &&
                    x.Timestamp < end &&
                    x.Sensor.Quality == DataQuality.GOOD &&
                    x.Sensor.IsActive)
                .Select(x => x.Temperature)
                .OrderBy(x => x)
                .ToListAsync();

            if (values.Count < 3)
                return null;

            int trim = Math.Max(1, values.Count / 10);

            var trimmed = values
                .Skip(trim)
                .Take(values.Count - 2 * trim)
                .ToList();

            return trimmed[trimmed.Count / 2];
        }
    }
}