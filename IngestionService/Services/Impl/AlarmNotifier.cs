using Shared.Contracts;

namespace IngestionService.Services.Impl
{
    public class AlarmNotifier : IAlarmNotifier
    {
        public Task NotifyAsync(SensorReadingRequest request)
        {
            Console.WriteLine(
                $"Alarm prepared for NotificationService: Sensor={request.SensorId}, Priority={request.AlarmPriority}, Value={request.Temperature:F2}");

            return Task.CompletedTask;
        }
    }
}
