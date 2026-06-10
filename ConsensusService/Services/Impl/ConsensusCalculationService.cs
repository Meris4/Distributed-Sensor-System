using ConsensusService.Data;
using ConsensusService.Models;
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
                    x.Timestamp <= end &&
                    x.Sensor.Quality == SensorQuality.GOOD)
                .Select(x => x.Temperature)
                .ToListAsync();

            if (!values.Any())
                return null;

            values.Sort();

            return values[values.Count / 2]; // median (BFT simplification)
        }
    }
}
