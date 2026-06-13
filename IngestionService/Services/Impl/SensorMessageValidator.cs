using Shared.Contracts;
using Shared.Security;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;

namespace IngestionService.Services.Impl
{
    public class SensorMessageValidator : ISensorMessageValidator
    {
        private readonly ConcurrentDictionary<string, long> _lastMessageIds = new();
        private readonly ConcurrentDictionary<string, List<DateTime>> _messageTimes = new();

        private readonly Dictionary<string, RSA> _publicKeys = new();
        private readonly RSA _serverPrivateKey;

        public SensorMessageValidator()
        {
            var keysDir = Path.Combine(AppContext.BaseDirectory, "keys");

            foreach (var path in Directory.GetFiles(keysDir, "sensor-*-public.pem"))
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                var sensorId = fileName.Replace("sensor-", "").Replace("-public", "");
                _publicKeys[sensorId] = RsaKeyLoader.LoadPublicKey(path);
            }

            var serverPrivateKeyPath = Path.Combine(keysDir, "private", "server-private.pem");
            _serverPrivateKey = RsaKeyLoader.LoadPrivateKey(serverPrivateKeyPath);
        }

        public string? Validate(SensorReadingRequest request)
        {
            var error = CheckValues(request);
            if (error != null)
                return error;

            error = DecryptInfo(request);
            if (error != null)
                return error;

            _lastMessageIds[request.SensorId] = request.MessageId;
            return null;
        }

        private string? CheckValues(SensorReadingRequest request)
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

            if (!_publicKeys.TryGetValue(request.SensorId, out var publicKey))
                return "Unknown sensor.";

            if (!SignatureHelper.Verify(request, publicKey))
                return "Invalid signature.";

            return null;
        }

        private string? DecryptInfo(SensorReadingRequest request)
        {
            try
            {
                var aesKey = RsaHelper.DecryptAesKey(request.EncryptedKey, _serverPrivateKey);
                var iv = Convert.FromBase64String(request.Iv);
                var json = AesHelper.Decrypt(request.EncryptedPayload, aesKey, iv);
                var payload = JsonSerializer.Deserialize<SensorReadingPayload>(json);

                if (payload is null)
                    return "Invalid payload.";

                request.Temperature = payload.Temperature;
                request.Quality = payload.Quality;
                request.AlarmPriority = payload.AlarmPriority;
                return null;
            }
            catch
            {
                return "Decryption failed.";
            }
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
