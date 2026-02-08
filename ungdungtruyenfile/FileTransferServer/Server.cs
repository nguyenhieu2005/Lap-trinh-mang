using FileTransfer_Lib.Network;
using FileTransfer_Lib.Security;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace FileTransferServer
{
    public class Server
    {
        TcpListener listener;
        X509Certificate2 cert;

        public Server(string certPath, string certPass)
        {
            cert = new X509Certificate2(certPath, certPass);
        }

        public void Start(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Thread t = new Thread(() =>
                {
                    var ssl = SslHelper.CreateServerSslStream(client.GetStream(), cert);
                    var handler = new ServerHandler(client, ssl);
                    handler.Handle();
                });
                t.Start();
            }
        }
    }
}
