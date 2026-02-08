using System.Net.Security;

namespace FileTransfer_Lib.Network
{
    public static class NetworkHelper
    {
        public static void SendPacket(SslStream stream, Packet packet)
        {
            byte[] data = Packet.Serialize(packet);
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

            stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public static Packet ReceivePacket(SslStream stream)
        {
            byte[] lengthPrefix = new byte[4];
            ReadFull(stream, lengthPrefix, 4);

            int length = BitConverter.ToInt32(lengthPrefix, 0);
            byte[] buffer = new byte[length];
            ReadFull(stream, buffer, length);

            return Packet.Deserialize(buffer);
        }

        private static void ReadFull(SslStream stream, byte[] buffer, int size)
        {
            int read = 0;
            while (read < size)
            {
                int r = stream.Read(buffer, read, size - read);
                if (r <= 0) throw new IOException("Disconnected");
                read += r;
            }
        }
    }
}
