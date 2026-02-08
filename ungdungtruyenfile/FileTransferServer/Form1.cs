using System;
using System.Windows.Forms;

namespace FileTransferServer
{
    public partial class Form1 : Form
    {
        Server server;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            server = new Server("Certificates/server.pfx", "123");
            server.Start(9000);
            MessageBox.Show("Server running");
        }
    }
}
