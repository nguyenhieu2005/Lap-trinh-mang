namespace FileTransferClient
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtSenderId;
        private System.Windows.Forms.TextBox txtReceiverId;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.ProgressBar progressBar1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            txtIP = new System.Windows.Forms.TextBox();
            txtPort = new System.Windows.Forms.TextBox();
            txtSenderId = new System.Windows.Forms.TextBox();
            txtReceiverId = new System.Windows.Forms.TextBox();
            txtFile = new System.Windows.Forms.TextBox();
            btnBrowse = new System.Windows.Forms.Button();
            btnSend = new System.Windows.Forms.Button();
            btnConnect = new System.Windows.Forms.Button();
            progressBar1 = new System.Windows.Forms.ProgressBar();

            btnConnect.Text = "Connect";
            btnSend.Text = "Send File";
            btnBrowse.Text = "Browse";

            btnConnect.Click += btnConnect_Click;
            btnSend.Click += btnSend_Click;
            btnBrowse.Click += btnBrowse_Click;

            Controls.AddRange(new System.Windows.Forms.Control[]
            {
                txtIP, txtPort, txtSenderId, txtReceiverId,
                txtFile, btnBrowse, btnSend, btnConnect, progressBar1
            });
        }
    }
}
