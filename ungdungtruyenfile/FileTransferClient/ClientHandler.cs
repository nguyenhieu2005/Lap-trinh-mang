using FileTransfer_Lib.DTO;
using FileTransfer_Lib.Network;
using System.Text.Json;

namespace FileTransferClient
{
    public class ClientHandler
    {
        private readonly Client _client;

        public ClientHandler(Client client)
        {
            _client = client;
        }

        public bool Login(string username, string password)
        {
            var dto = new UserDTO { Username = username, Password = password };
            var packet = new Packet
            {
                Type = PacketType.LoginRequest,
                Data = JsonSerializer.SerializeToUtf8Bytes(dto)
            };

            _client.Send(packet);
            var response = _client.Receive();
            return response.Type == PacketType.LoginResponse;
        }
    }
}
