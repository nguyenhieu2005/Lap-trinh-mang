using FileTransfer_Lib.DTO;
using FileTransfer_Lib.Network;
using System.Text.Json;
using System.IO;

namespace FileTransferServer
{
    public class ServerHandler
    {
        TcpClient client;
        SslStream stream;
        FileStream currentFile;

        public ServerHandler(TcpClient c, SslStream s)
        {
            client = c;
            stream = s;
        }

        public void Handle()
        {
            while (true)
            {
                Packet p = NetworkHelper.ReceivePacket(stream);

                switch (p.Type)
                {
                    case PacketType.LoginRequest:
                        HandleLogin(p);
                        break;

                    case PacketType.FileMeta:
                        HandleFileMeta(p);
                        break;

                    case PacketType.FileChunk:
                        currentFile.Write(p.Data);
                        break;

                    case PacketType.FileComplete:
                        currentFile.Close();
                        break;
                }
            }
        }

        void HandleLogin(Packet p)
        {
            var dto = JsonSerializer.Deserialize<UserDTO>(p.Data);
            bool ok = Database.UserRepository.Login(dto.Username, dto.Password);

            NetworkHelper.SendPacket(stream, new Packet
            {
                Type = ok ? PacketType.LoginResponse : PacketType.Error
            });
        }

        void HandleFileMeta(Packet p)
        {
            var meta = JsonSerializer.Deserialize<FileInfoDTO>(p.Data);
            string path = Path.Combine("Storage/Uploads", meta.FileName);
            currentFile = new FileStream(path, FileMode.Append);
        }
    }
}
