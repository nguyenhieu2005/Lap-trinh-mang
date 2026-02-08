using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CA
{
    public static class CAHelper
    {
        public static X509Certificate2 CreateCA(string name)
        {
            using var rsa = RSA.Create(2048);
            var req = new CertificateRequest(name, rsa, HashAlgorithmName.SHA256);
            return req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(10));
        }
    }
}
