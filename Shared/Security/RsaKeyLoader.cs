using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Security
{
    public static class RsaKeyLoader
    {
        public static RSA LoadPrivateKey(string path)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(File.ReadAllText(path));
            return rsa;
        }

        public static RSA LoadPublicKey(string path)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(File.ReadAllText(path));
            return rsa;
        }
    }
}
