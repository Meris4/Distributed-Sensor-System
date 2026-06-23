using IngestionService.Data;
using Shared.Models;
using Shared.Contracts;

namespace IngestionService.Services.Impl
{
    public class SensorReadingWriter : ISensorReadingWriter
    {
        private readonly IngestionDbContext _db;

        public SensorReadingWriter(IngestionDbContext db)
        {
            _db = db;
        }

        public async Task SaveAsync(SensorReadingRequest request)
        {
            var sensorId = request.SensorId;

            var sensor = await _db.Sensors.FindAsync(sensorId);

            if (sensor == null)
            {
                sensor = new Sensor
                {
                    Id = sensorId,
                    Name = $"Sensor-{sensorId}",
                    LastSeen = DateTime.UtcNow,
                    Quality = DataQuality.GOOD,
                    IsActive = true
                };

                _db.Sensors.Add(sensor);
            }
            else
            {
                sensor.LastSeen = DateTime.UtcNow;
            }

            _db.Measurements.Add(new SensorMeasurement
            {
                Id = Guid.NewGuid(),
                SensorId = sensorId,
                Temperature = request.Temperature,
                AlarmPriority = request.AlarmPriority,
                Timestamp = request.TimestampUtc
            });

            await _db.SaveChangesAsync();
        }
    }
}