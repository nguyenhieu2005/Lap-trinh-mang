using System;
using System.IO;
using System.Windows.Forms;
using FileTransfer_Lib.DTO;
using FileTransfer_Lib.Network;
using System.Text.Json;

namespace FileTransferClient
{
    public partial class Form1 : Form
    {
        Client client;
        ClientHandler handler;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            client = new Client();
            client.Connect(txtServerIP.Text, 9000);
            handler = new ClientHandler(client);
            MessageBox.Show("Connected");
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (handler.Login(txtUser.Text, txtPass.Text))
                MessageBox.Show("Login OK");
            else
                MessageBox.Show("Login Failed");
        }

        private void btnChooseFiles_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Multiselect = true
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                listFiles.Items.Clear();
                foreach (var f in dlg.FileNames)
                    listFiles.Items.Add(f);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            foreach (string path in listFiles.Items)
            {
                FileInfo fi = new FileInfo(path);
                var meta = new FileInfoDTO
                {
                    FileName = fi.Name,
                    FileSize = fi.Length
                };

                client.Send(new Packet
                {
                    Type = PacketType.FileMeta,
                    Data = JsonSerializer.SerializeToUtf8Bytes(meta)
                });

                using FileStream fs = new FileStream(path, FileMode.Open);
                byte[] buffer = new byte[1024 * 1024];
                int read;
                while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    client.Send(new Packet
                    {
                        Type = PacketType.FileChunk,
                        Data = buffer[..read]
                    });
                }

                client.Send(new Packet { Type = PacketType.FileComplete });
            }

            MessageBox.Show("Send done");
        }
    }
}
