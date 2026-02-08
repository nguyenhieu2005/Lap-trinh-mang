using MySql.Data.MySqlClient;
using System;
using System.Security.Cryptography;

namespace FileTransfer.Server
{
    public static class DatabaseHelper
    {
        private static string connectionString =
            "Server=127.0.0.1;" +
            "Database=file_transfer_db;" +
            "Uid=root;" +
            "Pwd=;" +
            "SslMode=none;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        public static bool TestConnection()
        {
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Error: " + ex.Message);
                return false;
            }
        }

        public static void LogTransfer(string filename, long filesize, string status)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                string sql = "INSERT INTO transfers (filename, size, status, created_at) VALUES (@fn, @sz, @st, NOW())";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@fn", filename);
                cmd.Parameters.AddWithValue("@sz", filesize);
                cmd.Parameters.AddWithValue("@st", status);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Log Error: " + ex.Message);
            }
        }

        public static bool RegisterUser(string username, string password)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                // check exists
                using (var chk = new MySqlCommand("SELECT COUNT(*) FROM users WHERE username = @u", conn))
                {
                    chk.Parameters.AddWithValue("@u", username);
                    var c = Convert.ToInt32(chk.ExecuteScalar());
                    if (c > 0) return false;
                }

                // create salt + hash
                byte[] salt = new byte[16];
                using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
                var pbk = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, 100_000, System.Security.Cryptography.HashAlgorithmName.SHA256);
                var hash = pbk.GetBytes(32);

                using var cmd = new MySqlCommand("INSERT INTO users (username, password_hash, salt, created_at) VALUES (@u,@p,@s,NOW())", conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", Convert.ToBase64String(hash));
                cmd.Parameters.AddWithValue("@s", Convert.ToBase64String(salt));
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Register Error: " + ex.Message);
                return false;
            }
        }
    }
}
