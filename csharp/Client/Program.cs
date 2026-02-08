using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security;

class Program {
    static async Task<int> Main(string[] args) {
        // support subcommands: send or register
        if (args.Length >= 1 && args[0].ToLower() == "register")
        {
            if (args.Length != 3) { Console.WriteLine("Usage: dotnet run -- register <serverUrl> <username>"); return 1; }
            string serverUrl = args[1].TrimEnd('/');
            string username = args[2];
            Console.Write("Password: ");
            var pwd = ReadPassword();
            var obj = new { username = username, password = pwd };
            var http = System.Text.Json.JsonSerializer.Serialize(obj);
            using var wc = new System.Net.WebClient();
            wc.Headers[System.Net.HttpRequestHeader.ContentType] = "application/json";
            var res = wc.UploadString(serverUrl + "/register", http);
            Console.WriteLine("Response: " + res);
            return 0;
        }

        if (args.Length != 3) {
            Console.WriteLine("Usage: dotnet run -- <serverIP> <port> <file>");
            return 1;
        }

        string serverIp = args[0];
        int port = int.Parse(args[1]);
        string path = args[2];

        if (!File.Exists(path)) {
            Console.WriteLine("File not found: " + path);
            return 1;
        }

        var headerObj = new { filename = Path.GetFileName(path), filesize = new FileInfo(path).Length };
        var headerBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(headerObj));
        int netLen = IPAddress.HostToNetworkOrder(headerBytes.Length);

        using var client = new TcpClient();
        await client.ConnectAsync(serverIp, port);
        using var ns = client.GetStream();

        await ns.WriteAsync(BitConverter.GetBytes(netLen), 0, 4);
        await ns.WriteAsync(headerBytes, 0, headerBytes.Length);

        byte[] buffer = new byte[65536];
        using var fs = File.OpenRead(path);
        int read;
        while ((read = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0) {
            await ns.WriteAsync(buffer, 0, read);
        }

        byte[] resp = new byte[4];
        int got = await ns.ReadAsync(resp, 0, resp.Length);
        Console.WriteLine("Server response: " + Encoding.ASCII.GetString(resp, 0, got));
        return 0;
    }

    static string ReadPassword()
    {
        var pwd = new System.Text.StringBuilder();
        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace && pwd.Length > 0)
            {
                pwd.Length--;
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                pwd.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        Console.WriteLine();
        return pwd.ToString();
    }
}
