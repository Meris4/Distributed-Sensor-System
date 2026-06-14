using System.Collections.Concurrent;

namespace IngestionService.Services.Impl
{
    public class SensorActivityTracker
    {
        private readonly ConcurrentDictionary<string, DateTime> _lastSeenBySensor = new();
        private readonly ConcurrentDictionary<string, DateTime> _blockedUntil = new();

        public void Block(string sensorId, TimeSpan duration)
        {
            _blockedUntil[sensorId] = DateTime.UtcNow.Add(duration);
        }

        public bool IsBlocked(string sensorId)
        {
            return _blockedUntil.TryGetValue(sensorId, out var until) && until > DateTime.UtcNow;
        }

        public void MarkMessageReceived(string sensorId, DateTime timestampUtc)
        {
            _lastSeenBySensor[sensorId] = timestampUtc;
        }

        public IReadOnlyDictionary<string, DateTime> GetAllLastSeen()
        {
            return _lastSeenBySensor;
        }

        public bool IsInactive(string sensorId)
        {
            if (!_lastSeenBySensor.TryGetValue(sensorId, out var lastSeen))
                return true;

            return DateTime.UtcNow - lastSeen > TimeSpan.FromSeconds(10);
        }

        public void RegisterSensor(string sensorId)
        {
            _lastSeenBySensor.TryAdd(sensorId, DateTime.UtcNow);
        }
    }
}
