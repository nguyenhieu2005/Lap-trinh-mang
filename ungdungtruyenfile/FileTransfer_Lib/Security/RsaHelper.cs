using System.Security.Cryptography;
using System.Text;

namespace FileTransfer_Lib.Security
{
    public static class RsaHelper
    {
        public static (string publicKey, string privateKey) GenerateKeyPair()
        {
            using RSA rsa = RSA.Create(2048);
            return (
                Convert.ToBase64String(rsa.ExportRSAPublicKey()),
                Convert.ToBase64String(rsa.ExportRSAPrivateKey())
            );
        }

        public static byte[] Encrypt(byte[] data, string base64PublicKey)
        {
            using RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(base64PublicKey), out _);
            return rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
        }

        public static byte[] Decrypt(byte[] data, string base64PrivateKey)
        {
            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(base64PrivateKey), out _);
            return rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
        }
    }
}
