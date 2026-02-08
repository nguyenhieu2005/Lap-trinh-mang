using FileTransfer_Lib.Network;
using FileTransfer_Lib.Security;
using System.Net.Security;
using System.Net.Sockets;

namespace FileTransferClient
{
    public class Client
    {
        public TcpClient Tcp { get; private set; }
        public SslStream Stream { get; private set; }

        public void Connect(string serverIp, int port)
        {
            Tcp = new TcpClient();
            Tcp.Connect(serverIp, port);
            Stream = SslHelper.CreateClientSslStream(Tcp.GetStream());
        }

        public void Send(Packet packet)
        {
            NetworkHelper.SendPacket(Stream, packet);
        }

        public Packet Receive()
        {
            return NetworkHelper.ReceivePacket(Stream);
        }
    }
}
