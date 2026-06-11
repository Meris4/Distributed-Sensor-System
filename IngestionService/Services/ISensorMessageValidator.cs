using Shared.Contracts;

namespace IngestionService.Services
{
    public interface ISensorMessageValidator
    {
        string? Validate(SensorReadingRequest request);
    }
}
