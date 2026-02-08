using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileTransfer.Server
{
    public static class RegistrationHttpServer
    {
        public static async Task StartAsync(string prefix = "http://localhost:5000/")
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();
            Console.WriteLine("Registration HTTP server listening on " + prefix);
            while (true)
            {
                var ctx = await listener.GetContextAsync();
                _ = Task.Run(() => HandleContext(ctx));
            }
        }

        static async Task HandleContext(HttpListenerContext ctx)
        {
            try
            {
                var req = ctx.Request;
                var resp = ctx.Response;
                if (req.HttpMethod != "POST" || req.Url.AbsolutePath != "/register")
                {
                    resp.StatusCode = 404;
                    resp.Close();
                    return;
                }

                using var ms = new MemoryStream();
                await req.InputStream.CopyToAsync(ms);
                var body = Encoding.UTF8.GetString(ms.ToArray());
                var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                var username = root.GetProperty("username").GetString();
                var password = root.GetProperty("password").GetString();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    resp.StatusCode = 400;
                    await WriteJson(resp, new { error = "username and password required" });
                    return;
                }

                var created = DatabaseHelper.RegisterUser(username, password);
                if (!created)
                {
                    resp.StatusCode = 409;
                    await WriteJson(resp, new { error = "user exists or db error" });
                    return;
                }

                // create client cert
                string outDir = Path.Combine(AppContext.BaseDirectory, "client_certs");
                string pfxPath = CertManager.CreateClientCertificate(username, outDir);
                var pfxBytes = await File.ReadAllBytesAsync(pfxPath);
                var pfxB64 = Convert.ToBase64String(pfxBytes);

                resp.StatusCode = 200;
                await WriteJson(resp, new { pfx = pfxB64, pfxPassword = "pfxpassword" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Registration error: " + ex.Message);
            }
        }

        static async Task WriteJson(HttpListenerResponse resp, object obj)
        {
            resp.ContentType = "application/json";
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));
            resp.ContentLength64 = bytes.Length;
            await resp.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            resp.Close();
        }
    }
}
