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

        [HttpPost("{id}/block")]
        public IActionResult Block(string id)
        {
            _activityTracker.Block(id, TimeSpan.FromSeconds(30));
            return Ok($"Sensor {id} blocked for 30 seconds.");
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
    }
}
