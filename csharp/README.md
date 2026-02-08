# FileTransfer C# Prototype

This workspace contains a minimal C# prototype for a File Transfer app:

- `csharp/Server` - simple TCP server that accepts a 4-byte header length, JSON header, then raw file bytes. Saves to `received_files`.
- `csharp/Client` - CLI client to send a file using the same protocol.
- `csharp/GuiClient` - WinForms GUI client with file chooser and progress bar.

Requirements: .NET SDK 6/7+ installed.

Quick run (PowerShell):

```powershell
# Run CA generator (one-time)
cd "c:/Users/pksai/Desktop/lập trình mạng/csharp/CA"
dotnet run

# Run server
cd "c:/Users/pksai/Desktop/lập trình mạng/csharp/Server"
dotnet run

# (Optional) Create MySQL database and table before running server
-- Create database and user (example):

```sql
CREATE DATABASE IF NOT EXISTS file_transfer_db;
USE file_transfer_db;
CREATE TABLE IF NOT EXISTS transfers (
	id INT AUTO_INCREMENT PRIMARY KEY,
	filename VARCHAR(512) NOT NULL,
	size BIGINT NOT NULL,
	status VARCHAR(64) NOT NULL,
	created_at DATETIME NOT NULL
);

CREATE TABLE IF NOT EXISTS users (
	id INT AUTO_INCREMENT PRIMARY KEY,
	username VARCHAR(128) NOT NULL UNIQUE,
	password_hash VARCHAR(256) NOT NULL,
	salt VARCHAR(256) NOT NULL,
	created_at DATETIME NOT NULL
);
```

# In another shell, run client
cd "c:/Users/pksai/Desktop/lập trình mạng/csharp/Client"
dotnet run -- 127.0.0.1 9000 C:\path\to\file.txt

# Or run GUI client
cd "c:/Users/pksai/Desktop/lập trình mạng/csharp/GuiClient"
dotnet run
```

Notes & next steps for full objectives:
- Add TLS/mTLS: configure Kestrel or wrap sockets with `SslStream` and use certificates (.pfx).
- Add resume: implement chunk indexing and server-side chunk tracking (DB or temp files).
- Add authentication + DB: create `FileTransfer.Server` ASP.NET Core project with EF Core and Identity.
- Add SignalR for realtime progress and multi-client coordination.

If you want, I can:
- Create a full `FileTransfer.sln` and add all projects.
- Implement TLS using a self-signed CA and sample scripts for cert generation.
- Add a simple SQLite DB and endpoints for auth/logs.

