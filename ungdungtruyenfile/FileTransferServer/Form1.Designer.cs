namespace FileTransferServer
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Button btnStart;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            txtPort = new System.Windows.Forms.TextBox();
            btnStart = new System.Windows.Forms.Button();

            btnStart.Text = "Start Server";
            btnStart.Click += btnStart_Click;

            Controls.Add(txtPort);
            Controls.Add(btnStart);
        }
    }
}
