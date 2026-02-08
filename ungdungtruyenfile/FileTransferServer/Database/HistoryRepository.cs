using MySql.Data.MySqlClient;

namespace FileTransferServer.Database
{
    public static class HistoryRepository
    {
        public static void Save(string sender, string file)
        {
            using var conn = DbConnection.Get();
            conn.Open();

            var cmd = new MySqlCommand(
                "INSERT INTO history(sender,file) VALUES(@s,@f)", conn);
            cmd.Parameters.AddWithValue("@s", sender);
            cmd.Parameters.AddWithValue("@f", file);
            cmd.ExecuteNonQuery();
        }
    }
}
