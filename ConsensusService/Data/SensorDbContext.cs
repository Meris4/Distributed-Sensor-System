using Microsoft.EntityFrameworkCore;
using ConsensusService.Models;

namespace ConsensusService.Data
{
    public class SensorDbContext : DbContext
    {
        public SensorDbContext(DbContextOptions<SensorDbContext> options)
            : base(options)
        {
        }

        public DbSet<Sensor> Sensors => Set<Sensor>();

        public DbSet<SensorMeasurement> Measurements => Set<SensorMeasurement>();

        public DbSet<ConsensusValue> ConsensusValues => Set<ConsensusValue>();
    }
}
