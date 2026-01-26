using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace FileTransferServer
{
    public partial class Form1 : Form
    {
        // Khai báo biến
        private ListBox lstLogMoi;
        private TcpListener _serverListener;
        private bool _isServerRunning = false;

        // CẤU HÌNH AES
        private readonly byte[] KEY_AES = "12345678901234561234567890123456"u8.ToArray();
        private readonly byte[] IV_AES = "1234567890123456"u8.ToArray();
        private const int PORT_NUMBER = 8888;

        public Form1()
        {
            // --- ĐÃ SỬA LỖI TẠI ĐÂY ---
            // Mình đã XÓA dòng InitializeComponent(); để tránh lỗi CS0103.
            // Chỉ dùng hàm này để vẽ giao diện:
            SetupCustomUI();

            // Tự động chạy Server
            this.Load += (s, e) => _ = StartServerAsync();
            this.FormClosing += (s, e) => { _isServerRunning = false; _serverListener?.Stop(); };
        }

        private void SetupCustomUI()
        {
            this.Text = "Server - Nhận File (Final)";
            this.Size = new Size(600, 450);

            // Khởi tạo ListBox Log
            lstLogMoi = new ListBox();
            lstLogMoi.Dock = DockStyle.Fill;
            lstLogMoi.Font = new Font("Consolas", 10);
            lstLogMoi.BackColor = Color.Black;
            lstLogMoi.ForeColor = Color.LightGreen;

            this.Controls.Add(lstLogMoi);
        }

        private async Task StartServerAsync()
        {
            if (!Directory.Exists("ReceivedFiles")) Directory.CreateDirectory("ReceivedFiles");

            _serverListener = new TcpListener(IPAddress.Any, PORT_NUMBER);
            _serverListener.Start();
            _isServerRunning = true;

            LogToScreen($"✅ SERVER ĐANG CHẠY - Port {PORT_NUMBER}...");

            while (_isServerRunning)
            {
                try
                {
                    TcpClient client = await _serverListener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClientAsync(client));
                }
                catch { }
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            string clientIP = client.Client.RemoteEndPoint.ToString();
            LogToScreen($"[+] Kết nối từ: {clientIP}");

            try
            {
                using (client)
                using (NetworkStream netStream = client.GetStream())
                {
                    // 1. XÁC THỰC
                    byte[] buffer = new byte[1024];
                    int bytesRead = await netStream.ReadAsync(buffer);
                    string loginInfo = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    if (loginInfo != "admin|123")
                    {
                        LogToScreen($"⚠️ Sai mật khẩu: {clientIP}");
                        return;
                    }
                    await netStream.WriteAsync("OK"u8.ToArray());

                    // 2. NHẬN TÊN FILE
                    byte[] lenBytes = new byte[4];
                    await netStream.ReadExactlyAsync(lenBytes, 0, 4);
                    int nameLen = BitConverter.ToInt32(lenBytes, 0);

                    byte[] nameBytes = new byte[nameLen];
                    await netStream.ReadExactlyAsync(nameBytes, 0, nameLen);
                    string fileName = Encoding.UTF8.GetString(nameBytes);

                    LogToScreen($"📄 Đang nhận file: {fileName}");

                    string savePath = Path.Combine("ReceivedFiles", fileName);
                    if (File.Exists(savePath))
                    {
                        string timestamp = DateTime.Now.ToString("HHmmss");
                        string nameOnly = Path.GetFileNameWithoutExtension(fileName);
                        string ext = Path.GetExtension(fileName);
                        savePath = Path.Combine("ReceivedFiles", $"{nameOnly}_{timestamp}{ext}");
                    }

                    // 3. GIẢI MÃ & LƯU
                    await DecryptAndSaveFile(netStream, savePath);
                    LogToScreen($"✅ Đã lưu xong: {Path.GetFileName(savePath)}");
                }
            }
            catch (Exception ex)
            {
                LogToScreen($"❌ Lỗi: {ex.Message}");
            }
        }

        private async Task DecryptAndSaveFile(NetworkStream netStream, string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            using (var aes = Aes.Create())
            {
                aes.Key = KEY_AES; aes.IV = IV_AES;
                aes.Padding = PaddingMode.PKCS7;

                using (var cs = new CryptoStream(netStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    await cs.CopyToAsync(fs);
                }
            }
        }

        private void LogToScreen(string msg)
        {
            if (this.IsDisposed) return;
            Invoke(new Action(() => {
                lstLogMoi.Items.Add($"[{DateTime.Now:HH:mm:ss}] {msg}");
                lstLogMoi.TopIndex = lstLogMoi.Items.Count - 1;
            }));
        }
    }
}