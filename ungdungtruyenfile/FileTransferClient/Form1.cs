using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace FileTransferClient
{
    public partial class Form1 : Form
    {
        // 1. UI CONTROLS
        private TextBox txtUser, txtPass;
        private Button btnGui;
        private ProgressBar prgTienTrinh;
        private Label lblTrangThai;

        // 2. AES CONFIG (Khớp với Server)
        private readonly byte[] KHOA = "12345678901234561234567890123456"u8.ToArray();
        private readonly byte[] IV = "1234567890123456"u8.ToArray();
        private const int PORT = 8888;
        private const string SERVER_IP = "127.0.0.1";

        public Form1()
        {
            // --- GIAO DIỆN ---
            this.Text = "Client - Gửi File Bảo Mật (vFinal)";
            this.Size = new Size(450, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblU = new Label() { Text = "User:", Location = new Point(20, 25), AutoSize = true };
            txtUser = new TextBox() { Text = "admin", Location = new Point(80, 22), Width = 120 };

            Label lblP = new Label() { Text = "Pass:", Location = new Point(20, 60), AutoSize = true };
            txtPass = new TextBox() { Text = "123", Location = new Point(80, 58), Width = 120, PasswordChar = '*' };

            btnGui = new Button() { Text = "📤 CHỌN & GỬI FILE", Location = new Point(220, 20), Size = new Size(180, 60), BackColor = Color.LightSkyBlue, Font = new Font(this.Font, FontStyle.Bold) };
            btnGui.Click += BtnGui_Click;

            lblTrangThai = new Label() { Text = "Sẵn sàng...", Location = new Point(20, 110), AutoSize = true, ForeColor = Color.Blue };
            prgTienTrinh = new ProgressBar() { Location = new Point(20, 140), Width = 390, Height = 30 };

            this.Controls.Add(lblU); this.Controls.Add(txtUser);
            this.Controls.Add(lblP); this.Controls.Add(txtPass);
            this.Controls.Add(btnGui); this.Controls.Add(lblTrangThai); this.Controls.Add(prgTienTrinh);
        }

        private async void BtnGui_Click(object sender, EventArgs e)
        {
            try
            {
                btnGui.Enabled = false;
                using (TcpClient client = new TcpClient())
                {
                    lblTrangThai.Text = "🔌 Đang kết nối...";
                    await client.ConnectAsync(SERVER_IP, PORT);

                    using (NetworkStream stream = client.GetStream())
                    {
                        // BƯỚC 1: ĐĂNG NHẬP
                        byte[] loginData = Encoding.UTF8.GetBytes($"{txtUser.Text}|{txtPass.Text}");
                        await stream.WriteAsync(loginData);

                        byte[] respBuf = new byte[10];
                        int bytesRead = await stream.ReadAsync(respBuf);
                        string response = Encoding.UTF8.GetString(respBuf, 0, bytesRead);

                        if (response != "OK")
                        {
                            MessageBox.Show("❌ Sai tài khoản/mật khẩu!");
                            return;
                        }

                        // BƯỚC 2: CHỌN FILE
                        OpenFileDialog ofd = new OpenFileDialog();
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            lblTrangThai.Text = "📦 Đang gửi thông tin file...";

                            // --- [NEW] GỬI TÊN FILE TRƯỚC ---
                            string fileName = Path.GetFileName(ofd.FileName);
                            byte[] nameBytes = Encoding.UTF8.GetBytes(fileName);
                            byte[] lenBytes = BitConverter.GetBytes(nameBytes.Length);

                            await stream.WriteAsync(lenBytes); // Gửi độ dài tên (4 byte)
                            await stream.WriteAsync(nameBytes); // Gửi tên file
                            // --------------------------------

                            // BƯỚC 3: GỬI DỮ LIỆU FILE (MÃ HÓA)
                            lblTrangThai.Text = $"🚀 Đang gửi: {fileName}";
                            await GuiFileMaHoa(stream, ofd.FileName);

                            lblTrangThai.Text = "✅ Đã gửi xong!";
                            MessageBox.Show("Gửi file thành công!");
                            prgTienTrinh.Value = 0;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            finally { btnGui.Enabled = true; }
        }

        private async Task GuiFileMaHoa(NetworkStream netStream, string path)
        {
            var options = new FileStreamOptions { Mode = FileMode.Open, Access = FileAccess.Read };
            using (var fs = new FileStream(path, options))
            using (var aes = Aes.Create())
            {
                aes.Key = KHOA; aes.IV = IV;
                long totalSize = fs.Length;
                long totalSent = 0;

                // CryptoStreamMode.Write -> Mã hóa trước khi đẩy ra mạng
                using (var cs = new CryptoStream(netStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] buffer = new byte[8192]; // Chunk 8KB
                    int read;
                    while ((read = await fs.ReadAsync(buffer)) > 0)
                    {
                        await cs.WriteAsync(buffer.AsMemory(0, read));
                        totalSent += read;

                        // Cập nhật UI
                        int percent = (int)((totalSent * 100) / totalSize);
                        this.Invoke(() => { prgTienTrinh.Value = percent; });
                    }
                    if (!cs.HasFlushedFinalBlock) await cs.FlushFinalBlockAsync();
                }
            }
        }
    }
}