using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace LoginSystem_24_59275_3
{
    // Bonus (Task 8, option 4): all the database code lives in this one class.
    // LoginForm / RegisterForm / HomeForm never open a SqlConnection themselves -
    // they just call these methods.
    public static class DatabaseHelper
    {
        // Read from App.config every time - never hard-coded.
        private static string ConnString
        {
            get { return ConfigurationManager.ConnectionStrings["LoginDB"].ConnectionString; }
        }

        // Used on startup to fail nicely instead of crashing if SQL Server is down.
        public static bool TestConnection(out string errorMessage)
        {
            errorMessage = "";
            try
            {
                using (SqlConnection con = new SqlConnection(ConnString))
                {
                    con.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        // SHA-256 hash of the password, as a hex string. We store this, never the
        // real password.
        public static string HashPassword(string plainPassword)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plainPassword));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static bool UsernameExists(string username)
        {
            using (SqlConnection con = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.Users WHERE Username = @username", con))
            {
                cmd.Parameters.AddWithValue("@username", username);
                con.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public static void RegisterUser(string username, string passwordHash, string email, string fullName)
        {
            using (SqlConnection con = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO dbo.Users (Username, PasswordHash, Email, FullName) " +
                "VALUES (@username, @hash, @email, @fullName)", con))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@hash", passwordHash);
                cmd.Parameters.AddWithValue("@email", (object)email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@fullName", (object)fullName ?? DBNull.Value);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Returns the matching user (id + full name) or null if the username/hash
        // pair doesn't match anything.
        public static UserRecord TryLogin(string username, string passwordHash)
        {
            using (SqlConnection con = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT UserID, FullName FROM dbo.Users WHERE Username = @username AND PasswordHash = @hash", con))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@hash", passwordHash);
                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string fullName = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        return new UserRecord(id, fullName);
                    }
                }
            }
            return null;
        }

        public static DataTable GetAllUsers()
        {
            DataTable table = new DataTable();
            using (SqlConnection con = new SqlConnection(ConnString))
            using (SqlDataAdapter adapter = new SqlDataAdapter(
                "SELECT UserID, Username, Email, CreatedAt FROM dbo.Users ORDER BY UserID", con))
            {
                adapter.Fill(table);
            }
            return table;
        }

        // ---- Bonus (Task 8, option 1): LoginHistory ----

        // Call when a login succeeds. Returns the new HistoryID so HomeForm can
        // stamp the LogoutTime on the same row when the user logs out.
        public static int LogLoginStart(int userId)
        {
            using (SqlConnection con = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO dbo.LoginHistory (UserID, LoginTime) " +
                "OUTPUT INSERTED.HistoryID VALUES (@userId, GETDATE())", con))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public static void LogLogoutTime(int historyId)
        {
            if (historyId <= 0) return;

            using (SqlConnection con = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE dbo.LoginHistory SET LogoutTime = GETDATE() WHERE HistoryID = @id", con))
            {
                cmd.Parameters.AddWithValue("@id", historyId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }

    // Small holder for what TryLogin needs to hand back.
    public class UserRecord
    {
        public int UserID { get; private set; }
        public string FullName { get; private set; }

        public UserRecord(int userId, string fullName)
        {
            UserID = userId;
            FullName = fullName;
        }
    }
}
