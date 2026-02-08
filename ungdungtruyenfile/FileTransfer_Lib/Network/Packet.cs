using System.Text.Json;

namespace FileTransfer_Lib.Network
{
    public class Packet
    {
        public PacketType Type { get; set; }
        public byte[] Data { get; set; }

        public static byte[] Serialize(Packet packet)
        {
            return JsonSerializer.SerializeToUtf8Bytes(packet);
        }

        public static Packet Deserialize(byte[] buffer)
        {
            return JsonSerializer.Deserialize<Packet>(buffer);
        }
    }
}
