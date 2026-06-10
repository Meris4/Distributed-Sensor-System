using System.Net.Http.Json;
using SensorSimulator;
using Shared.Contracts;

var httpClient = new HttpClient
{
    BaseAddress = new Uri("http://localhost:5019")
};

var sensors = new List<SensorConfig>
{
    new()
    {
        SensorId = "SENSOR-001",
        MinTemperature = 20,
        MaxTemperature = 120,
        Quality = DataQuality.GOOD,
        AlarmLimitPriority1 = 60,
        AlarmLimitPriority2 = 80,
        AlarmLimitPriority3 = 100
    },
    new()
    {
        SensorId = "SENSOR-002",
        MinTemperature = 20,
        MaxTemperature = 120,
        Quality = DataQuality.GOOD,
        AlarmLimitPriority1 = 60,
        AlarmLimitPriority2 = 80,
        AlarmLimitPriority3 = 100
    },
    new()
    {
        SensorId = "SENSOR-003",
        MinTemperature = 20,
        MaxTemperature = 120,
        Quality = DataQuality.GOOD,
        AlarmLimitPriority1 = 60,
        AlarmLimitPriority2 = 80,
        AlarmLimitPriority3 = 100
    },
    new()
    {
        SensorId = "SENSOR-004",
        MinTemperature = 20,
        MaxTemperature = 120,
        Quality = DataQuality.GOOD,
        AlarmLimitPriority1 = 60,
        AlarmLimitPriority2 = 80,
        AlarmLimitPriority3 = 100
    },
    new()
    {
        SensorId = "SENSOR-005",
        MinTemperature = 20,
        MaxTemperature = 120,
        Quality = DataQuality.GOOD,
        AlarmLimitPriority1 = 60,
        AlarmLimitPriority2 = 80,
        AlarmLimitPriority3 = 100
    }
};

var random = new Random();
var messageIds = sensors.ToDictionary(x => x.SensorId, _ => 0L);

var blockedUntil = new Dictionary<string, DateTime>();

_ = Task.Run(() =>
{
    while (true)
    {
        Console.WriteLine("Enter sensor ID to block for 30 seconds, for example SENSOR-001:");

        var sensorId = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(sensorId))
            continue;

        if (!sensors.Any(x => x.SensorId == sensorId))
        {
            Console.WriteLine($"Sensor {sensorId} does not exist.");
            continue;
        }

        blockedUntil[sensorId] = DateTime.UtcNow.AddSeconds(30);
        Console.WriteLine($"Sensor {sensorId} blocked for 30 seconds.");
    }
});

while (true)
{
    foreach (var sensor in sensors)
    {
        if (blockedUntil.TryGetValue(sensor.SensorId, out var until) && until > DateTime.UtcNow)
        {
            Console.WriteLine($"{sensor.SensorId} is temporarily blocked.");
            continue;
        }

        var temperature = GenerateTemperature(sensor, random);
        var alarmPriority = CalculateAlarmPriority(sensor, temperature);

        Console.ForegroundColor = GetColor(alarmPriority);
        Console.WriteLine(
            $"[{DateTime.UtcNow:O}] {sensor.SensorId} measured {temperature:F2} C, alarm priority {alarmPriority}");
        Console.ResetColor();

        var request = new SensorReadingRequest
        {
            SensorId = sensor.SensorId,
            Temperature = temperature,
            Quality = sensor.Quality,
            AlarmPriority = alarmPriority,
            TimestampUtc = DateTime.UtcNow,
            MessageId = ++messageIds[sensor.SensorId],
            Signature = "temporary-signature"
        };

        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/ingest", request);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Server response: {(int)response.StatusCode} OK");
            }
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

    await Task.Delay(TimeSpan.FromSeconds(random.Next(1, 11)));
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