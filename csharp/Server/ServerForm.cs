using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileTransferServer {
    public class ServerForm : Form {
        private NumericUpDown numPort = new NumericUpDown() { Minimum = 1, Maximum = 65535, Value = 9000 };
        private Button btnStartServer = new Button() { Text = "Start Server" };
        private ListBox lbClients = new ListBox() { Width = 500, Height = 100 };
        private ListBox lbLogs = new ListBox() { Width = 500, Height = 150 };
        private TcpListener? listener;
        private bool isRunning = false;

        public ServerForm() {
            Text = "FILE TRANSFER SERVER";
            Width = 560;
            Height = 450;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = System.Drawing.Color.WhiteSmoke;

            int leftMargin = 15;
            int topPos = 15;

            // Port configuration
            var lblPort = new Label() { Text = "Port:", AutoSize = true };
            Controls.Add(lblPort);
            lblPort.Left = leftMargin;
            lblPort.Top = topPos;

            Controls.Add(numPort);
            numPort.Left = leftMargin + 50;
            numPort.Top = topPos - 3;
            numPort.Width = 80;

            Controls.Add(btnStartServer);
            btnStartServer.Left = leftMargin + 140;
            btnStartServer.Top = topPos - 3;
            btnStartServer.Width = 120;
            btnStartServer.Click += BtnStartServer_Click;

            topPos += 40;

            // Connected Clients Section
            var lblClients = new Label() { Text = "Connected Clients", AutoSize = true };
            Controls.Add(lblClients);
            lblClients.Left = leftMargin;
            lblClients.Top = topPos;
            lblClients.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);

            topPos += 25;

            Controls.Add(lbClients);
            lbClients.Left = leftMargin;
            lbClients.Top = topPos;
            lbClients.Width = 510;
            lbClients.Height = 100;
            lbClients.BorderStyle = BorderStyle.FixedSingle;

            topPos += 110;

            // Transfer Logs Section
            var lblLogs = new Label() { Text = "Transfer Logs", AutoSize = true };
            Controls.Add(lblLogs);
            lblLogs.Left = leftMargin;
            lblLogs.Top = topPos;
            lblLogs.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);

            topPos += 25;

            Controls.Add(lbLogs);
            lbLogs.Left = leftMargin;
            lbLogs.Top = topPos;
            lbLogs.Width = 510;
            lbLogs.Height = 150;
            lbLogs.BorderStyle = BorderStyle.FixedSingle;

            AddLog("Server initialized. Press 'Start Server' to begin.");
        }

        private void BtnStartServer_Click(object? sender, EventArgs e) {
            if (isRunning) {
                isRunning = false;
                btnStartServer.Text = "Start Server";
                AddLog("Server stopped.");
            } else {
                isRunning = true;
                btnStartServer.Text = "Stop Server";
                int port = (int)numPort.Value;
                numPort.Enabled = false;
                _ = Task.Run(() => StartServerAsync(port));
            }
        }

        private async Task StartServerAsync(int port) {
            try {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                AddLog($"Server listening on port {port}...");

                while (isRunning) {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    string clientId = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
                    AddClientToList(clientId);
                    AddLog($"Client connected: {clientId}");

                    _ = HandleClientAsync(client, clientId);
                }
            } catch (Exception ex) {
                if (isRunning) {
                    AddLog($"Server error: {ex.Message}");
                }
            } finally {
                listener?.Stop();
                AddLog("Server stopped.");
            }
        }

        private async Task HandleClientAsync(TcpClient client, string clientId) {
            try {
                using var ns = client.GetStream();
                byte[] lenBuf = new byte[4];
                int got = await ReadExactAsync(ns, lenBuf, 0, 4);

                if (got != 4) {
                    RemoveClientFromList(clientId);
                    AddLog($"Client {clientId}: Invalid header size");
                    return;
                }

                int hdrLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBuf, 0));
                byte[] hdrBuf = new byte[hdrLen];
                await ReadExactAsync(ns, hdrBuf, 0, hdrLen);

                var header = JsonSerializer.Deserialize<TransferHeader>(Encoding.UTF8.GetString(hdrBuf));
                if (header == null) {
                    RemoveClientFromList(clientId);
                    AddLog($"Client {clientId}: Invalid header data");
                    return;
                }

                string receivedFilePath = Path.Combine("received_files", Path.GetFileName(header.filename ?? "unknown"));
                Directory.CreateDirectory("received_files");

                AddLog($"Client {clientId} sending: {header.filename} ({FormatFileSize(header.filesize)})");

                using var fs = File.Create(receivedFilePath);
                long remaining = header.filesize;
                byte[] buffer = new byte[65536];

                while (remaining > 0) {
                    int toRead = (int)Math.Min(buffer.Length, remaining);
                    int r = await ns.ReadAsync(buffer, 0, toRead);
                    if (r == 0) break;

                    await fs.WriteAsync(buffer, 0, r);
                    remaining -= r;
                }

                // Send response
                if (remaining == 0) {
                    await ns.WriteAsync(Encoding.ASCII.GetBytes("OK"));
                    AddLog($"Client {clientId}: {header.filename} received successfully");
                } else {
                    await ns.WriteAsync(Encoding.ASCII.GetBytes("ERR"));
                    AddLog($"Client {clientId}: Transfer incomplete for {header.filename}");
                }
            } catch (Exception ex) {
                AddLog($"Client {clientId} error: {ex.Message}");
            } finally {
                client.Close();
                RemoveClientFromList(clientId);
                AddLog($"Client {clientId} disconnected");
            }
        }

        private async Task<int> ReadExactAsync(NetworkStream ns, byte[] buf, int off, int size) {
            int total = 0;
            while (total < size) {
                int r = await ns.ReadAsync(buf, off + total, size - total);
                if (r == 0) break;
                total += r;
            }
            return total;
        }

        private void AddClientToList(string clientId) {
            Invoke((Action)(() => {
                lbClients.Items.Add($"{clientId} | Online");
            }));
        }

        private void RemoveClientFromList(string clientId) {
            Invoke((Action)(() => {
                for (int i = lbClients.Items.Count - 1; i >= 0; i--) {
                    if (lbClients.Items[i]?.ToString()?.StartsWith(clientId) ?? false) {
                        lbClients.Items.RemoveAt(i);
                        break;
                    }
                }
            }));
        }

        private void AddLog(string message) {
            string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            Invoke((Action)(() => {
                lbLogs.Items.Add(logEntry);
                if (lbLogs.Items.Count > 100) {
                    lbLogs.Items.RemoveAt(0);
                }
                lbLogs.TopIndex = lbLogs.Items.Count - 1;
            }));
        }

        private string FormatFileSize(long bytes) {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1) {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        protected override void OnFormClosing(FormClosingEventArgs e) {
            isRunning = false;
            listener?.Stop();
            base.OnFormClosing(e);
        }
    }

    public class TransferHeader {
        public string? filename { get; set; }
        public long filesize { get; set; }
        public string? senderId { get; set; }
        public string? receiverId { get; set; }
    }
}
