using Shared.Contracts;

namespace IngestionService.Services.Impl
{
    public class SensorReadingWriter : ISensorReadingWriter
    {
        public Task SaveAsync(SensorReadingRequest request)
        {
            Console.WriteLine(
                $"Reading prepared for database: Sensor={request.SensorId}, Temperature={request.Temperature:F2}, Alarm={request.AlarmPriority}");

            return Task.CompletedTask;
        }
    }
}
