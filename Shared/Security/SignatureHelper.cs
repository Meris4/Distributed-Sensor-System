using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Shared.Contracts;
using System.Globalization;

namespace Shared.Security
{
    public static class SignatureHelper
    {
        public static string CreatePayload(SensorReadingRequest request)
        {
            return string.Join("|",
                request.SensorId,
                request.Temperature.ToString("F4", CultureInfo.InvariantCulture),
                request.Quality,
                request.AlarmPriority,
                request.TimestampUtc.ToUniversalTime().ToString("O"),
                request.MessageId);
        }

        public static string Sign(SensorReadingRequest request, RSA privateKey)
        {
            var payload = CreatePayload(request);
            var bytes = Encoding.UTF8.GetBytes(payload);

            var signature = privateKey.SignData(
                bytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signature);
        }

        public static bool Verify(SensorReadingRequest request, RSA publicKey)
        {
            try
            {
                var payload = CreatePayload(request);
                var bytes = Encoding.UTF8.GetBytes(payload);
                var signature = Convert.FromBase64String(request.Signature);

                return publicKey.VerifyData(
                    bytes,
                    signature,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);
            }
            catch
            {
                return false;
            }
        }
    }
}

