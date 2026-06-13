using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using SensorSimulator;
using Shared.Contracts;
using Shared.Security;

var serverUrl = Environment.GetEnvironmentVariable("ServerUrl") ?? "http://localhost:5019";
var httpClient = new HttpClient
{
    BaseAddress = new Uri(serverUrl)
};

var sensors = new List<SensorConfig>
{
    new() { SensorId = "1", MinTemperature = 20, MaxTemperature = 120, Quality = DataQuality.GOOD, AlarmLimitPriority1 = 60, AlarmLimitPriority2 = 80, AlarmLimitPriority3 = 100 },
    new() { SensorId = "2", MinTemperature = 20, MaxTemperature = 120, Quality = DataQuality.GOOD, AlarmLimitPriority1 = 60, AlarmLimitPriority2 = 80, AlarmLimitPriority3 = 100 },
    new() { SensorId = "3", MinTemperature = 20, MaxTemperature = 120, Quality = DataQuality.GOOD, AlarmLimitPriority1 = 60, AlarmLimitPriority2 = 80, AlarmLimitPriority3 = 100 },
    new() { SensorId = "4", MinTemperature = 20, MaxTemperature = 120, Quality = DataQuality.GOOD, AlarmLimitPriority1 = 60, AlarmLimitPriority2 = 80, AlarmLimitPriority3 = 100 },
    new() { SensorId = "5", MinTemperature = 20, MaxTemperature = 120, Quality = DataQuality.GOOD, AlarmLimitPriority1 = 60, AlarmLimitPriority2 = 80, AlarmLimitPriority3 = 100 }
};

var random = new Random();
var messageIds = sensors.ToDictionary(x => x.SensorId, _ => 0L);
var blockedUntil = new Dictionary<string, DateTime>();

var keysDir = Path.Combine(AppContext.BaseDirectory, "keys");

var privateKeys = new Dictionary<string, RSA>();
foreach (var sensor in sensors)
{
    var path = Path.Combine(keysDir, "private", $"sensor-{sensor.SensorId}-private.pem");
    privateKeys[sensor.SensorId] = RsaKeyLoader.LoadPrivateKey(path);
}

using var serverPublicKey = RsaKeyLoader.LoadPublicKey(
    Path.Combine(keysDir, "public", "server-public.pem"));

while (true)
{
    foreach (var sensor in sensors)
    {
        if (IsBlocked(sensor.SensorId))
        {
            Console.WriteLine($"{sensor.SensorId} is temporarily blocked.");
            continue;
        }

        var measurement = Measure(sensor, random);
        PrintMeasurement(sensor, measurement);

        var request = BuildSignedRequest(sensor, measurement, serverPublicKey, privateKeys[sensor.SensorId], messageIds);
        await SendRequest(httpClient, request);
    }

    Console.WriteLine();
    await Task.Delay(TimeSpan.FromSeconds(random.Next(1, 11)));
}

SensorReadingRequest BuildSignedRequest(
    SensorConfig sensor,
    Measurement m,
    RSA serverPublicKey,
    RSA sensorPrivateKey,
    Dictionary<string, long> messageIds)
{
    var encrypted = EncryptPayload(sensor, m, serverPublicKey);

    var request = new SensorReadingRequest
    {
        SensorId = sensor.SensorId,
        TimestampUtc = DateTime.UtcNow,
        MessageId = ++messageIds[sensor.SensorId],
        EncryptedPayload = encrypted.Payload,
        EncryptedKey = encrypted.Key,
        Iv = encrypted.Iv,
        Signature = string.Empty
    };

    request.Signature = SignatureHelper.Sign(request, sensorPrivateKey);
    return request;
}

EncryptedMessage EncryptPayload(SensorConfig sensor, Measurement m, RSA serverPublicKey)
{
    var payload = new SensorReadingPayload
    {
        Temperature = m.Temperature,
        Quality = sensor.Quality,
        AlarmPriority = m.AlarmPriority
    };
    var payloadJson = JsonSerializer.Serialize(payload);

    var (aesKey, iv) = AesHelper.GenerateKeyAndIv();
    var encryptedPayload = AesHelper.Encrypt(payloadJson, aesKey, iv);
    var encryptedKey = RsaHelper.EncryptAesKey(aesKey, serverPublicKey);

    return new EncryptedMessage(encryptedPayload, encryptedKey, Convert.ToBase64String(iv));
}

async Task SendRequest(HttpClient httpClient, SensorReadingRequest request)
{
    try
    {
        var response = await httpClient.PostAsJsonAsync("/api/ingest", request);

        if (response.IsSuccessStatusCode)
            Console.WriteLine($"Server response: {(int)response.StatusCode} OK");
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Server response: {(int)response.StatusCode} - {error}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to send data: {ex.Message}");
    }
}

bool IsBlocked(string sensorId)
    => blockedUntil.TryGetValue(sensorId, out var until) && until > DateTime.UtcNow;

Measurement Measure(SensorConfig sensor, Random random)
{
    var temperature = GenerateTemperature(sensor, random);
    var alarmPriority = CalculateAlarmPriority(sensor, temperature);
    return new Measurement(temperature, alarmPriority);
}

void PrintMeasurement(SensorConfig sensor, Measurement m)
{
    Console.ForegroundColor = GetColor(m.AlarmPriority);
    Console.WriteLine($"[{DateTime.UtcNow:O}] {sensor.SensorId} measured {m.Temperature:F2} C, alarm priority {m.AlarmPriority}");
    Console.ResetColor();
}

static double GenerateTemperature(SensorConfig sensor, Random random)
{
    return sensor.MinTemperature +
           random.NextDouble() * (sensor.MaxTemperature - sensor.MinTemperature);
}

static int CalculateAlarmPriority(SensorConfig sensor, double temperature)
{
    if (temperature >= sensor.AlarmLimitPriority3)
        return 3;

    if (temperature >= sensor.AlarmLimitPriority2)
        return 2;

    if (temperature >= sensor.AlarmLimitPriority1)
        return 1;

    return 0;
}

static ConsoleColor GetColor(int priority)
{
    return priority switch
    {
        1 => ConsoleColor.Yellow,
        2 => ConsoleColor.DarkYellow,
        3 => ConsoleColor.Red,
        _ => ConsoleColor.Gray
    };
}

record Measurement(double Temperature, int AlarmPriority);
record EncryptedMessage(string Payload, string Key, string Iv);