using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace FileTransfer_Lib.Security
{
    public static class SslHelper
    {
        public static SslStream CreateServerSslStream(NetworkStream stream, string certPath, string password)
        {
            X509Certificate2 cert = new X509Certificate2(certPath, password);
            SslStream ssl = new SslStream(stream, false);
            ssl.AuthenticateAsServer(cert, false, false);
            return ssl;
        }

        public static SslStream CreateClientSslStream(NetworkStream stream)
        {
            SslStream ssl = new SslStream(stream, false,
                (sender, certificate, chain, errors) => true);
            ssl.AuthenticateAsClient("FileTransferServer");
            return ssl;
        }
    }
}
