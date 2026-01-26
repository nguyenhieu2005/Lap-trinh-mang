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
    // LỚP FORM1 (Đã sửa tên cho khớp với Visual Studio)
    public partial class Form1 : Form
    {
        // 1. KHAI BÁO GIAO DIỆN (UI)
        private ListBox lstLog;

        // 2. CẤU HÌNH BẢO MẬT (AES 256-bit)
        // Sử dụng cú pháp u8 của .NET 10 để tối ưu bộ nhớ
        private readonly byte[] KHOA = "12345678901234561234567890123456"u8.ToArray();
        private readonly byte[] IV = "1234567890123456"u8.ToArray();
        // --- LOGIC SERVER ---
        private async Task KhoiChayServer()
        {
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Any, 8888);
                listener.Start();
                GhiLog(">>> SERVER ĐANG CHẠY TẠI CỔNG 8888...");
                GhiLog(">>> Đang chờ kết nối từ Client...");

                while (true)
                {
                    // Chấp nhận kết nối bất đồng bộ (không treo giao diện)
                    TcpClient client = await listener.AcceptTcpClientAsync();

                    // Tạo một luồng riêng để xử lý khách hàng này
                    _ = Task.Run(() => XuLyKhachHang(client));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể khởi động Server: " + ex.Message);
            }
        }

        private async Task XuLyKhachHang(TcpClient client)
        {
            string diaChiIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    // BƯỚC 1: XÁC THỰC (ĐĂNG NHẬP)
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer);
                    string thongTinDangNhap = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    if (thongTinDangNhap == "admin|123")
                    {
                        // Gửi phản hồi OK về cho Client
                        await stream.WriteAsync("OK"u8.ToArray());
                        GhiLog($"[+] Kết nối mới từ {diaChiIP}: Đăng nhập thành công.");

                        // BƯỚC 2: NHẬN FILE VÀ GIẢI MÃ
                        string tenFile = $"Data_{DateTime.Now:yyyyMMdd_HHmmss}.dat";

                        // Tạo thư mục nếu chưa có
                        Directory.CreateDirectory("ServerFiles");
                        string duongDanLuu = Path.Combine("ServerFiles", tenFile);

                        await NhanVaGiaiMaFile(stream, duongDanLuu);

                        GhiLog($"[V] Đã nhận và giải mã file: {tenFile}");
                        GhiLog($"    Lưu tại: {Path.GetFullPath(duongDanLuu)}");
                    }
                    else
                    {
                        GhiLog($"[!] Cảnh báo: {diaChiIP} sai mật khẩu ('{thongTinDangNhap}')");
                    }
                }
            }
            catch (Exception ex)
            {
                GhiLog($"[X] Lỗi kết nối với {diaChiIP}: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        // HÀM GIẢI MÃ AES (Quan trọng)
        private async Task NhanVaGiaiMaFile(NetworkStream netStream, string savePath)
        {
            // Mở file để ghi
            using (FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
            using (Aes aes = Aes.Create())
            {
                aes.Key = KHOA;
                aes.IV = IV;

                // CryptoStream đóng vai trò trung gian: Nhận từ Mạng -> Giải mã -> Ghi xuống File
                using (CryptoStream cs = new CryptoStream(fs, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    byte[] buffer = new byte[16384]; // Buffer 16KB
                    int bytesRead;
                    while ((bytesRead = await netStream.ReadAsync(buffer)) > 0)
                    {
                        await cs.WriteAsync(buffer.AsMemory(0, bytesRead));
                    }
                }
            }
        }

        // HÀM GHI LOG LÊN GIAO DIỆN (An toàn với đa luồng)
        private void GhiLog(string noidung)
        {
            // Vì hàm này được gọi từ luồng phụ, cần dùng Invoke để vẽ lên giao diện chính
            if (IsDisposed) return; // Tránh lỗi khi đóng form

            this.Invoke(() => {
                string thoiGian = DateTime.Now.ToString("HH:mm:ss");
                lstLog.Items.Add($"{thoiGian} - {noidung}");
                lstLog.TopIndex = lstLog.Items.Count - 1; // Tự cuộn xuống dòng cuối
            });
        }
    }
}