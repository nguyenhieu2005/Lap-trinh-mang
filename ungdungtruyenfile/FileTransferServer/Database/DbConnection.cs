using MySql.Data.MySqlClient;

namespace FileTransferServer.Database
{
    public static class DbConnection
    {
        public static MySqlConnection Get()
        {
            return new MySqlConnection(
                "server=localhost;database=file_transfer;uid=root;pwd=;");
        }
    }
}
