using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FileTransfer.Shared;

namespace FileTransfer.Server
{
    public static class ConnectionHandler
    {
        public static async Task HandleClientAsync(TcpClient client)
        {
            var ep = client.Client.RemoteEndPoint;
            try
            {
                using var ns = client.GetStream();
                byte[] lenBuf = new byte[4];
                int got = await ReadExactAsync(ns, lenBuf, 0, 4);
                if (got != 4) { client.Close(); return; }
                int hdrLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBuf, 0));
                byte[] hdrBuf = new byte[hdrLen];
                got = await ReadExactAsync(ns, hdrBuf, 0, hdrLen);
                if (got != hdrLen) { client.Close(); return; }
                var header = JsonSerializer.Deserialize<Header>(Encoding.UTF8.GetString(hdrBuf));
                string filename = Path.GetFileName(header.filename);
                long filesize = header.filesize;
                string dest = Path.Combine("received_files", filename);

                using var fs = File.Create(dest);
                long remaining = filesize;
                byte[] buffer = new byte[65536];
                while (remaining > 0)
                {
                    int toRead = (int)Math.Min(buffer.Length, remaining);
                    int r = await ns.ReadAsync(buffer, 0, toRead);
                    if (r == 0) break;
                    await fs.WriteAsync(buffer, 0, r);
                    remaining -= r;
                }

                if (remaining == 0)
                {
                    Console.WriteLine($"Received {filename} ({filesize}) from {ep}");
                    await ns.WriteAsync(Encoding.ASCII.GetBytes("OK"));
                    // Log to DB
                    try { DatabaseHelper.LogTransfer(filename, filesize, "Completed"); } catch (Exception ex) { Console.WriteLine("DB log error: " + ex.Message); }
                }
                else
                {
                    Console.WriteLine($"Incomplete transfer from {ep}");
                    await ns.WriteAsync(Encoding.ASCII.GetBytes("ERR"));
                    try { DatabaseHelper.LogTransfer(filename, filesize, "Incomplete"); } catch { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error handling client: " + ex.Message);
            }
            finally
            {
                client.Close();
            }
        }

        static async Task<int> ReadExactAsync(NetworkStream ns, byte[] buffer, int offset, int size)
        {
            int total = 0;
            while (total < size)
            {
                int r = await ns.ReadAsync(buffer, offset + total, size - total);
                if (r == 0) break;
                total += r;
            }
            return total;
        }
    }
}
