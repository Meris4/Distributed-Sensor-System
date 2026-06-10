using IngestionService.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;

namespace IngestionService.Controllers
{
    [ApiController]
    [Route("api/ingest")]
    public class IngestionController : ControllerBase
    {
        private readonly SensorMessageValidator _validator;
        private readonly ISensorReadingWriter _sensorReadingWriter;
        private readonly IAlarmNotifier _alarmNotifier;

        public IngestionController(
        SensorMessageValidator validator,
        ISensorReadingWriter sensorReadingWriter,
        IAlarmNotifier alarmNotifier)
        {
            _validator = validator;
            _sensorReadingWriter = sensorReadingWriter;
            _alarmNotifier = alarmNotifier;
        }

        [HttpPost]
        public async Task<IActionResult> Ingest([FromBody] SensorReadingRequest request)
        {
            var validationError = _validator.Validate(request);

            if (validationError is not null)
                return BadRequest(validationError);

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
