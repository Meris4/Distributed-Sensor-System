using System.Collections.Concurrent;
using Shared.Contracts;
using System.Security.Cryptography;
using Shared.Security;

namespace IngestionService.Services.Impl
{
    public class SensorMessageValidator : ISensorMessageValidator
    {
        private readonly ConcurrentDictionary<string, long> _lastMessageIds = new();
        private readonly ConcurrentDictionary<string, List<DateTime>> _messageTimes = new();

        private readonly RSA _publicKey;

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

            if (string.IsNullOrWhiteSpace(request.Signature))
                return "Signature is required.";

            if (!SignatureHelper.Verify(request, _publicKey))
                return "Invalid signature.";

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

        public SensorMessageValidator()
        {
            var publicKeyPath = Path.Combine(AppContext.BaseDirectory, "keys", "sensor-public.pem");

            _publicKey = RsaKeyLoader.LoadPublicKey(publicKeyPath);
        }
    }
}
