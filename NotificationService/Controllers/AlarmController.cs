using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;
using Shared.Contracts;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/alarms")]
    public class AlarmsController : ControllerBase
    {
        private readonly IHubContext<AlarmHub> _hub;

        public AlarmsController(IHubContext<AlarmHub> hub)
        {
            _hub = hub;
        }

        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] AlarmNotification alarm)
        {
            Console.ForegroundColor = GetColor(alarm.AlarmPriority);
            Console.WriteLine(
                $"[{alarm.TimestampUtc:O}] ALARM Sensor={alarm.SensorId}, Temp={alarm.Temperature:F2}, Priority={alarm.AlarmPriority}");
            Console.ResetColor();

            await _hub.Clients.All.SendAsync("ReceiveAlarm", alarm);

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