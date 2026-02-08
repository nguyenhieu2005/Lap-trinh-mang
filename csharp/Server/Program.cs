using System.Windows.Forms;
using FileTransferServer;

namespace FileTransferServer {
    static class Program {
        [STAThread]
        static void Main() {
            ApplicationConfiguration.Initialize();
            Application.Run(new ServerForm());
        }
    }
}

