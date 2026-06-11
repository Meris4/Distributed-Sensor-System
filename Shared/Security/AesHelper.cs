using System.Security.Cryptography;
using System.Text;

namespace Shared.Security
{
    public static class AesHelper
    {
        public static (byte[] Key, byte[] Iv) GenerateKeyAndIv()
        {
            using var aes = Aes.Create();
            aes.GenerateKey();
            aes.GenerateIV();
            return (aes.Key, aes.IV);
        }

        public static string Encrypt(string plainText, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            using var enc = aes.CreateEncryptor();
            var plain = Encoding.UTF8.GetBytes(plainText);
            var cipher = enc.TransformFinalBlock(plain, 0, plain.Length);
            return Convert.ToBase64String(cipher);
        }

        public static string Decrypt(string cipherBase64, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            using var dec = aes.CreateDecryptor();
            var cipher = Convert.FromBase64String(cipherBase64);
            var plain = dec.TransformFinalBlock(cipher, 0, cipher.Length);
            return Encoding.UTF8.GetString(plain);
        }
    }
}