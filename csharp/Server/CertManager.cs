using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace FileTransfer.Server
{
    public static class CertManager
    {
        private static string CaPath => Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "CA", "..", "Server", "ca.pfx");
        private static string CaPassword => "capassword";

        public static string CreateClientCertificate(string commonName, string outDir)
        {
            Directory.CreateDirectory(outDir);
            string outPath = Path.Combine(outDir, commonName + ".pfx");

            var caFullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "ca.pfx"));
            if (!File.Exists(caFullPath))
            {
                // try relative
                caFullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "CA", "ca.pfx"));
            }

            if (!File.Exists(caFullPath)) throw new FileNotFoundException("CA file not found: " + caFullPath);

            var caCert = new X509Certificate2(caFullPath, CaPassword, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            using var caKey = caCert.GetRSAPrivateKey();

            using RSA rsa = RSA.Create(2048);
            var req = new CertificateRequest($"CN={commonName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
            req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));
            req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

            var notBefore = DateTimeOffset.UtcNow.AddDays(-1);
            var notAfter = notBefore.AddYears(2);
            var serial = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(serial);

            var cert = req.Create(caCert, notBefore, notAfter, serial);
            var certWithKey = cert.CopyWithPrivateKey(rsa);
            var export = certWithKey.Export(X509ContentType.Pfx, "pfxpassword");
            File.WriteAllBytes(outPath, export);
            return Path.GetFullPath(outPath);
        }
    }
}
