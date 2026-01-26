using System;
using System.IO;

public class FileHelper
{
    // Kích thước mỗi gói tin (Chunk size). 
    // Thường để 4KB (4096 bytes) hoặc 8KB là tối ưu cho mạng LAN/Wifi.
    private const int BUFFER_SIZE = 4096;

    public static void SplitAndSendFile(string filePath)
    {
        // Kiểm tra file có tồn tại không
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File khong ton tai!");
            return;
        }

        // Mở file ra để đọc (FileMode.Open) và chỉ đọc (FileAccess.Read)
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            byte[] buffer = new byte[BUFFER_SIZE]; // Cái "xô" để múc dữ liệu
            int bytesRead; // Số lượng byte thực tế múc được (vì gói cuối có thể vơi)
            long totalBytes = fs.Length; // Tổng dung lượng file
            long currentBytes = 0; // Dung lượng đã gửi

            int packetID = 1; // Đánh số thứ tự gói tin

            // Vòng lặp: Múc từng gáo nước (buffer) cho đến khi cạn (bytesRead = 0)
            while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
            {
                // --- ĐÂY LÀ CHỖ BẠN SẼ GỬI DỮ LIỆU ĐI (SOCKET) ---

                // Giả lập việc gửi
                Console.WriteLine($"Dang gui Goi {packetID}: Kich thuoc {bytesRead} bytes");

                // Cập nhật tiến độ
                currentBytes += bytesRead;
                packetID++;

                // Tính % hoàn thành (để sau này vẽ lên thanh Progress Bar)
                double progress = (double)currentBytes / totalBytes * 100;
                Console.WriteLine($"--> Tien do: {progress:F2}%");

                // Giả lập độ trễ mạng (để nhìn kịp log)
                System.Threading.Thread.Sleep(50);
            }
        }
        Console.WriteLine("--- DA GUI XONG TOAN BO FILE ---");
    }
}