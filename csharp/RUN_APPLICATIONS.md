# Hướng dẫn chạy ứng dụng File Transfer

## 📋 Kiến trúc

```
Server GUI (WinForms)          Client GUI (WinForms)
  - Port: 9000                  - Connect to Server
  - Listen for clients          - Send files
  - Display logs               - Show progress
```

---

## 🚀 Cách chạy

### **Option 1: Chạy từ IDE (Visual Studio)**

1. Mở solution `csharp.slnx` trong Visual Studio
2. **Chạy Server:**
   - Right-click project `Server` → Set as Startup Project
   - Press `F5` (Debug) hoặc `Ctrl+F5` (Release)
   - Cửa sổ File Transfer SERVER sẽ xuất hiện
   - Nhập port (mặc định: 9000) → Click "Start Server"

3. **Chạy Client:**
   - Right-click project `GuiClient` → Set as Startup Project
   - Press `F5` hoặc `Ctrl+F5`
   - Cửa sổ File Transfer CLIENT sẽ xuất hiện
   - Nhập Server IP: `127.0.0.1` (nếu local)
   - Port: `9000`
   - Sender ID: `user01`
   - Receiver ID: `server`
   - Click "Connect"
   - Browse file → Click "Send File"

---

### **Option 2: Chạy EXE trực tiếp**

#### **Server:**
```powershell
C:\Users\pksai\Desktop\lập trình mạng\csharp\Server\bin\Debug\net7.0-windows\Server.exe
```

#### **Client:**
```powershell
C:\Users\pksai\Desktop\lập trình mạng\csharp\GuiClient\bin\Debug\net7.0-windows\GuiClient.exe
```

---

### **Option 3: Dùng `dotnet run`**

#### **Server:**
```bash
cd C:\Users\pksai\Desktop\lập trình mạng\csharp\Server
dotnet run -c Debug
```

#### **Client:**
```bash
cd C:\Users\pksai\Desktop\lập trình mạng\csharp\GuiClient
dotnet run -c Debug
```

---

## 📝 Server UI Layout

```
+--------------------------------------+
|      FILE TRANSFER SERVER           |
+--------------------------------------+
| Port: [9000]  [Start Server]        |
|                                      |
| Connected Clients                    |
| ----------------------------------   |
| 127.0.0.1:12345 | Online            |
| 127.0.0.1:12346 | Online            |
|                                      |
| Transfer Logs                        |
| ----------------------------------   |
| [12:34:56] Server listening...       |
| [12:35:10] Client connected: 127... |
| [12:35:15] Client sending: file.zip |
+--------------------------------------+
```

---

## 📝 Client UI Layout

```
+--------------------------------------+
|    FILE TRANSFER CLIENT             |
+--------------------------------------+
| Server IP:   [127.0.0.1]            |
| Port:        [9000]                 |
| Sender ID:   [user01]               |
| Receiver ID: [server]               |
|                                      |
| File: [C:\data\file.zip] [Browse]   |
|                                      |
| [Connect]    [Send File]            |
|                                      |
| Progress:                           |
| [████████░░░░░░░░░░░░] 40%          |
+--------------------------------------+
```

---

## 🔧 Troubleshooting

### Server không start
- Kiểm tra port 9000 đã được sử dụng: `netstat -ano | Select-String 9000`
- Thay đổi port khác (< 65535)

### Client kết nối thất bại
- Đảm bảo Server đang chạy
- Kiểm tra Server IP và Port chính xác
- Firewall có thể chặn - add exception cho port 9000

### File transfer bị lỗi
- Kiểm tra quyền ghi folder `received_files/`
- Đảm bảo file tồn tại trước khi gửi
- Kiểm trap file size không quá lớn (test < 100MB trước)

---

## 📦 File Cơ bản

```
csharp/
├── Server/
│   ├── bin/Debug/net7.0-windows/Server.exe    ← Server GUI
│   ├── ServerForm.cs                          ← UI Form
│   ├── Program.cs                             ← Entry point
│   └── ...
├── GuiClient/
│   ├── bin/Debug/net7.0-windows/GuiClient.exe ← Client GUI
│   ├── MainForm.cs                            ← UI Form
│   └── Program.cs                             ← Entry point
├── Shared/
│   ├── Header.cs                              ← Shared DTOs
│   └── ...
└── received_files/                            ← Files received by server
```

---

## 🎯 Tính năng hiện tại

✅ Server GUI với port config  
✅ Client GUI với Server connection  
✅ File transfer TCP protocol  
✅ Progress bar  
✅ Transfer logs  
✅ Connected clients list  
✅ Sender/Receiver ID tracking  

---

## 📌 Ghi chú

- **net7.0-windows**: Framework .NET 7.0 (out of support từ 2024)
- Khuyên cập nhật sang `.NET 8` hoặc `.NET 9` trong production
- MySQL dependency đang setup cho future database logging

---

**Tạo ngày:** Feb 8, 2026  
**Status:** ✅ Working - Giao diện WinForms hoàn thiện
