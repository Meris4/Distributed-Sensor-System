using Shared.Contracts;

namespace IngestionService.Services
{
    public interface IAlarmNotifier
    {
        Task NotifyAsync(SensorReadingRequest request);
    }
}
