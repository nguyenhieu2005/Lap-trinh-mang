using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

class Program
{
    static void Main(string[] args)
    {
        string outPath = Path.Combine(Path.GetFullPath(".."), "Server", "ca.pfx");
        string password = "capassword";
        Console.WriteLine("Generating CA at: " + outPath);

        using RSA rsa = RSA.Create(4096);
        var req = new CertificateRequest("CN=FileTransferRootCA", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        req.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

        var now = DateTimeOffset.UtcNow;
        var cert = req.CreateSelfSigned(now.AddDays(-1), now.AddYears(10));
        var export = cert.Export(X509ContentType.Pfx, password);
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
        File.WriteAllBytes(outPath, export);
        Console.WriteLine("CA generated.");
    }
}
