using Shared.Contracts;

namespace IngestionService.Services
{
    public interface ISensorReadingWriter
    {
        Task SaveAsync(SensorReadingRequest request);
    }
}
