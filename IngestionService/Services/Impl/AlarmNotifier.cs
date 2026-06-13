using System.Net.Http.Json;
using Shared.Contracts;

namespace IngestionService.Services.Impl
{
    public class AlarmNotifier : IAlarmNotifier
    {
        private readonly HttpClient _httpClient;

        public AlarmNotifier(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task NotifyAsync(SensorReadingRequest request)
        {
            var alarm = new AlarmNotification
            {
                SensorId = request.SensorId,
                Temperature = request.Temperature,
                AlarmPriority = request.AlarmPriority,
                TimestampUtc = request.TimestampUtc
            };

            try
            {
                await _httpClient.PostAsJsonAsync("/api/alarms", alarm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to notify NotificationService: {ex.Message}");
            }
        }
    }
}