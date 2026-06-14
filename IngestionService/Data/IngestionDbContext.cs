using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace IngestionService.Data
{
    public class IngestionDbContext : DbContext
    {
        public IngestionDbContext(DbContextOptions<IngestionDbContext> options)
            : base(options)
        {
        }

        public DbSet<SensorMeasurement> Measurements { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
    }
}