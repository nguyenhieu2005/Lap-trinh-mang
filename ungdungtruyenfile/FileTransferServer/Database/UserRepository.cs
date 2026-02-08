using MySql.Data.MySqlClient;

namespace FileTransferServer.Database
{
    public static class UserRepository
    {
        public static bool Login(string user, string pass)
        {
            using var conn = DbConnection.Get();
            conn.Open();

            var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM users WHERE username=@u AND password=@p", conn);
            cmd.Parameters.AddWithValue("@u", user);
            cmd.Parameters.AddWithValue("@p", pass);

            return (long)cmd.ExecuteScalar() > 0;
        }
    }
}
