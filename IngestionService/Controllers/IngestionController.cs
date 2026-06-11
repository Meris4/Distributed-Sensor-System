using IngestionService.Services;
using IngestionService.Services.Impl;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;

namespace IngestionService.Controllers
{
    [ApiController]
    [Route("api/ingest")]
    public class IngestionController : ControllerBase
    {
        private readonly ISensorMessageValidator _validator;
        private readonly ISensorReadingWriter _sensorReadingWriter;
        private readonly IAlarmNotifier _alarmNotifier;
        private readonly SensorActivityTracker _activityTracker;

        public IngestionController(
        ISensorMessageValidator validator,
        ISensorReadingWriter sensorReadingWriter,
        IAlarmNotifier alarmNotifier,
        SensorActivityTracker activityTracker)
        {
            _validator = validator;
            _sensorReadingWriter = sensorReadingWriter;
            _alarmNotifier = alarmNotifier;
            _activityTracker = activityTracker;
        }

        [HttpPost]
        public async Task<IActionResult> Ingest([FromBody] SensorReadingRequest request)
        {
            var validationError = _validator.Validate(request);

            if (validationError is not null)
                return BadRequest(validationError);

            _activityTracker.MarkMessageReceived(request.SensorId, DateTime.UtcNow);

            Console.ForegroundColor = GetColor(request.AlarmPriority);
            Console.WriteLine(
                $"[{request.TimestampUtc:O}] Sensor={request.SensorId}, Temp={request.Temperature}, Quality={request.Quality}, Alarm={request.AlarmPriority}");
            Console.ResetColor();

            await _sensorReadingWriter.SaveAsync(request);

            if (request.AlarmPriority > 0)
                await _alarmNotifier.NotifyAsync(request);

            return Ok();
        }

        private static ConsoleColor GetColor(int priority)
        {
            return priority switch
            {
                1 => ConsoleColor.Yellow,
                2 => ConsoleColor.DarkYellow,
                3 => ConsoleColor.Red,
                _ => ConsoleColor.Gray
            };
        }
    }
}
