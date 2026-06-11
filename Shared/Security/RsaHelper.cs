using System.Security.Cryptography;

namespace Shared.Security
{
    public static class RsaHelper
    {
        private static readonly RSAEncryptionPadding Padding = RSAEncryptionPadding.OaepSHA256;

        public static string EncryptAesKey(byte[] aesKey, RSA serverPublicKey)
            => Convert.ToBase64String(serverPublicKey.Encrypt(aesKey, Padding));

        public static byte[] DecryptAesKey(string encryptedKeyBase64, RSA serverPrivateKey)
            => serverPrivateKey.Decrypt(Convert.FromBase64String(encryptedKeyBase64), Padding);
    }
}