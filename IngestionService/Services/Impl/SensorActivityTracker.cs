namespace IngestionService.Services.Impl
{
    public class SensorActivityTracker
    {
        private readonly Dictionary<string, DateTime> _lastSeenBySensor = new();

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
    }
}
