using System.Security.Cryptography;

var basePath = @"C:\Users\Administrator\Documents\0_III Godina\SNUS\Projekat\DistributedSensorSystem\keys";

Directory.CreateDirectory(Path.Combine(basePath, "private"));
Directory.CreateDirectory(Path.Combine(basePath, "public"));

for (int i = 6; i <= 10; i++)
{
    using var rsa = RSA.Create(2048);

    var privateKey = rsa.ExportRSAPrivateKeyPem();
    var publicKey = rsa.ExportRSAPublicKeyPem();

    File.WriteAllText(Path.Combine(basePath, $"private/sensor-{i}-private.pem"), privateKey);
    File.WriteAllText(Path.Combine(basePath, $"public/sensor-{i}-public.pem"), publicKey);

    Console.WriteLine($"Generated keys for sensor {i}");
}