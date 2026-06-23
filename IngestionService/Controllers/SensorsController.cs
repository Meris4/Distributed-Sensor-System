using IngestionService.Data;
using IngestionService.Services;
using IngestionService.Services.Impl;
using Microsoft.AspNetCore.Mvc;

namespace IngestionService.Controllers
{
    [ApiController]
    [Route("api/sensors")]
    public class SensorsController : ControllerBase
    {
        private readonly SensorActivityTracker _activityTracker;
        private readonly IngestionDbContext _db;

        public SensorsController(SensorActivityTracker activityTracker, IngestionDbContext db)
        {
            _activityTracker = activityTracker;
            _db = db;
        }

        [HttpGet("activity")]
        public IActionResult GetActivity()
        {
            var result = _activityTracker.GetAllLastSeen()
                .Select(sensor => new
                {
                    SensorId = sensor.Key,
                    LastSeenUtc = sensor.Value,
                    IsInactive = DateTime.UtcNow - sensor.Value > TimeSpan.FromSeconds(10)
                });

            return Ok(result);
        }

        [HttpPost("block/{sensorId}")]
        public async Task<IActionResult> Block(string sensorId)
        {
            var sensor = await _db.Sensors.FindAsync(sensorId);

            if (sensor == null)
                return NotFound();

            sensor.BlockedUntil = DateTime.UtcNow.AddSeconds(30);

            await _db.SaveChangesAsync();

            return Ok($"Sensor {sensorId} blocked for 30s");
        }
    }
}
