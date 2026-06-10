using Shared.Contracts;

namespace IngestionService.Services.Impl
{
    public class SensorMessageValidator : ISensorMessageValidator
    {
        private readonly Dictionary<string, long> _lastMessageIds = new();
        private readonly Dictionary<string, List<DateTime>> _messageTimes = new();

        public string? Validate(SensorReadingRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SensorId))
                return "SensorId is required.";

            if (request.TimestampUtc < DateTime.UtcNow.AddMinutes(-2) ||
                request.TimestampUtc > DateTime.UtcNow.AddMinutes(1))
                return "Invalid timestamp.";

            if (_lastMessageIds.TryGetValue(request.SensorId, out var lastMessageId) &&
                request.MessageId <= lastMessageId)
                return "Replay attack detected.";

            if (IsRateLimited(request.SensorId))
                return "Sensor is sending too many messages.";

            _lastMessageIds[request.SensorId] = request.MessageId;

            return null;
        }

        private bool IsRateLimited(string sensorId)
        {
            var now = DateTime.UtcNow;

            if (!_messageTimes.ContainsKey(sensorId))
                _messageTimes[sensorId] = new List<DateTime>();

            _messageTimes[sensorId].RemoveAll(x => x < now.AddSeconds(-1));
            _messageTimes[sensorId].Add(now);

            return _messageTimes[sensorId].Count > 10;
        }
    }
}
