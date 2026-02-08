using System.Security.Cryptography;

namespace FileTransfer_Lib.Security
{
    public static class AesHelper
    {
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            using var encryptor = aes.CreateEncryptor();
            return encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public static void GenerateKey(out byte[] key, out byte[] iv)
        {
            using Aes aes = Aes.Create();
            key = aes.Key;
            iv = aes.IV;
        }
    }
}
