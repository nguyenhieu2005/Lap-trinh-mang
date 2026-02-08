using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuiClient {
    public class MainForm : Form {
        // Connection fields
        TextBox txtServerIp = new TextBox() { Text = "127.0.0.1", Width = 140 };
        NumericUpDown numPort = new NumericUpDown() { Minimum = 1, Maximum = 65535, Value = 9000 };
        TextBox txtSenderId = new TextBox() { Text = "user01", Width = 140 };
        TextBox txtReceiverId = new TextBox() { Text = "server", Width = 140 };
        
        // File transfer fields
        Label lblFile = new Label() { Text = "No file selected", AutoSize = true };
        ProgressBar progress = new ProgressBar() { Width = 360 };
        Button btnChooseFile = new Button() { Text = "Browse" };
        Button btnConnect = new Button() { Text = "Connect" };
        Button btnSendFile = new Button() { Text = "Send File", Enabled = false };
        string? filepath;

        public MainForm() {
            Text = "FILE TRANSFER CLIENT";
            Width = 480; 
            Height = 350;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = System.Drawing.Color.WhiteSmoke;

            int topPos = 15;
            int labelWidth = 90;
            int inputWidth = 150;
            int leftCol1 = 10;
            int leftCol2 = 270;

            // Server IP
            var lblServerIp = new Label() { Text = "Server IP:", AutoSize = true };
            Controls.Add(lblServerIp); 
            lblServerIp.Left = leftCol1; 
            lblServerIp.Top = topPos;
            Controls.Add(txtServerIp); 
            txtServerIp.Left = leftCol1 + labelWidth; 
            txtServerIp.Top = topPos - 3;
            txtServerIp.Width = inputWidth;

            // Port
            var lblPort = new Label() { Text = "Port:", AutoSize = true };
            Controls.Add(lblPort); 
            lblPort.Left = leftCol2; 
            lblPort.Top = topPos;
            Controls.Add(numPort); 
            numPort.Left = leftCol2 + labelWidth; 
            numPort.Top = topPos - 3;
            numPort.Width = inputWidth;
            numPort.Minimum = 1; 
            numPort.Maximum = 65535; 
            numPort.Value = 9000;

            topPos += 35;

            // Sender ID
            var lblSenderId = new Label() { Text = "Sender ID:", AutoSize = true };
            Controls.Add(lblSenderId); 
            lblSenderId.Left = leftCol1; 
            lblSenderId.Top = topPos;
            Controls.Add(txtSenderId); 
            txtSenderId.Left = leftCol1 + labelWidth; 
            txtSenderId.Top = topPos - 3;
            txtSenderId.Width = inputWidth;

            // Receiver ID
            var lblReceiverId = new Label() { Text = "Receiver ID:", AutoSize = true };
            Controls.Add(lblReceiverId); 
            lblReceiverId.Left = leftCol2; 
            lblReceiverId.Top = topPos;
            Controls.Add(txtReceiverId); 
            txtReceiverId.Left = leftCol2 + labelWidth; 
            txtReceiverId.Top = topPos - 3;
            txtReceiverId.Width = inputWidth;

            topPos += 35;

            // File selection
            var lblFileLabel = new Label() { Text = "File:", AutoSize = true };
            Controls.Add(lblFileLabel); 
            lblFileLabel.Left = leftCol1; 
            lblFileLabel.Top = topPos;
            Controls.Add(lblFile); 
            lblFile.Left = leftCol1 + labelWidth; 
            lblFile.Top = topPos;
            lblFile.Width = 250;
            Controls.Add(btnChooseFile); 
            btnChooseFile.Left = leftCol1 + labelWidth + 260; 
            btnChooseFile.Top = topPos - 3;
            btnChooseFile.Width = 70;
            btnChooseFile.Click += BtnChooseFile_Click;

            topPos += 35;

            // Connect and Send buttons
            Controls.Add(btnConnect); 
            btnConnect.Left = leftCol1; 
            btnConnect.Top = topPos;
            btnConnect.Width = 100;
            btnConnect.Click += BtnConnect_Click;
            
            Controls.Add(btnSendFile); 
            btnSendFile.Left = leftCol1 + 110; 
            btnSendFile.Top = topPos;
            btnSendFile.Width = 100;
            btnSendFile.Click += BtnSendFile_Click;

            topPos += 40;

            // Progress bar
            var lblProgress = new Label() { Text = "Progress:", AutoSize = true };
            Controls.Add(lblProgress); 
            lblProgress.Left = leftCol1; 
            lblProgress.Top = topPos;
            
            Controls.Add(progress); 
            progress.Left = leftCol1; 
            progress.Top = topPos + 25;
            progress.Width = 440;
            progress.Height = 25;
            progress.Style = ProgressBarStyle.Continuous;
        }

        private void BtnChooseFile_Click(object? sender, EventArgs e) {
            using var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK) {
                filepath = dlg.FileName;
                lblFile.Text = Path.GetFileName(filepath);
                btnSendFile.Enabled = true;
            }
        }

        private void BtnConnect_Click(object? sender, EventArgs e) {
            string ip = txtServerIp.Text;
            int port = (int)numPort.Value;
            
            btnConnect.Enabled = false;
            btnConnect.Text = "Connecting...";
            
            _ = Task.Run(async () => {
                try {
                    using var client = new TcpClient();
                    await client.ConnectAsync(ip, port);
                    client.Close();
                    
                    MessageBox.Show(this, $"Connected to {ip}:{port}", "Success");
                    Invoke((Action)(() => {
                        btnConnect.Enabled = true;
                        btnConnect.Text = "Connect";
                        btnSendFile.Enabled = filepath != null;
                    }));
                } catch (Exception ex) {
                    MessageBox.Show(this, $"Connection failed: {ex.Message}", "Error");
                    Invoke((Action)(() => {
                        btnConnect.Enabled = true;
                        btnConnect.Text = "Connect";
                    }));
                }
            });
        }

        private void BtnSendFile_Click(object? sender, EventArgs e) {
            if (filepath == null) return;
            btnSendFile.Enabled = false;
            _ = Task.Run(() => SendFileAsync(txtServerIp.Text, (int)numPort.Value, filepath));
        }

        private async Task SendFileAsync(string serverIp, int port, string path) {
            try {
                var headerObj = new { 
                    senderId = txtSenderId.Text,
                    receiverId = txtReceiverId.Text,
                    filename = Path.GetFileName(path), 
                    filesize = new FileInfo(path).Length 
                };
                var headerBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(headerObj));
                int netLen = IPAddress.HostToNetworkOrder(headerBytes.Length);

                using var client = new TcpClient();
                await client.ConnectAsync(serverIp, port);
                using var ns = client.GetStream();
                await ns.WriteAsync(BitConverter.GetBytes(netLen), 0, 4);
                await ns.WriteAsync(headerBytes, 0, headerBytes.Length);

                long filesize = new FileInfo(path).Length;
                progress.Invoke((Action)(() => { 
                    progress.Value = 0; 
                    progress.Maximum = (int)Math.Min(filesize, int.MaxValue); 
                }));
                
                byte[] buffer = new byte[65536];
                using var fs = File.OpenRead(path);
                int r; 
                long sent = 0;
                while ((r = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                    await ns.WriteAsync(buffer, 0, r);
                    sent += r;
                    long finalSent = sent;
                    progress.Invoke((Action)(() => {
                        int progressValue = (int)Math.Min(finalSent, progress.Maximum);
                        int percentage = (int)((finalSent * 100) / filesize);
                        progress.Value = progressValue;
                        Text = $"FILE TRANSFER CLIENT - {percentage}%";
                    }));
                }

                byte[] resp = new byte[4];
                int got = await ns.ReadAsync(resp, 0, resp.Length);
                string respStr = Encoding.ASCII.GetString(resp, 0, got);
                MessageBox.Show(this, respStr == "OK" ? "File sent successfully!" : "Server error: " + respStr);
            } catch (Exception ex) {
                MessageBox.Show(this, "Error: " + ex.Message);
            } finally {
                Invoke((Action)(() => {
                    btnSendFile.Enabled = filepath != null;
                    Text = "FILE TRANSFER CLIENT";
                    progress.Value = 0;
                }));
            }
        }
    }
}
