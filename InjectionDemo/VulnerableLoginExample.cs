// FOR THE REPORT ONLY - this file is NOT included in the .csproj and is not
// part of the real app. It exists so Task 6 has something concrete to screenshot:
// the "before" (vulnerable) version next to the real, parameterized version.
//
// This mirrors bug #1 in the sample Login_System project: the query is built
// with string concatenation instead of parameters.

using System;
using System.Data.SqlClient;

namespace LoginSystem_24_59275_3.InjectionDemo
{
    public class VulnerableLoginExample
    {
        // DO NOT USE THIS PATTERN - user input is pasted directly into the SQL text.
        public bool VulnerableCheckLogin(SqlConnection con, string username, string password)
        {
            string sql = "SELECT COUNT(*) FROM dbo.Users WHERE Username = '" + username +
                         "' AND PasswordHash = '" + password + "'";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                con.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        // THE EXPLOIT
        // Typing this into the password box:
        //     ' OR '1'='1
        // turns the query the server actually receives into:
        //     ... WHERE Username = 'anything' AND PasswordHash = '' OR '1'='1'
        // '1'='1' is always true, so COUNT(*) comes back greater than zero and
        // the login "succeeds" with no valid password at all. Take a screenshot
        // of this happening for part (a) of the Task 6 report.

        // THE FIX
        // The real app never builds SQL this way. DatabaseHelper.TryLogin() does
        // this instead:
        //
        //   SqlCommand cmd = new SqlCommand(
        //       "SELECT UserID, FullName FROM dbo.Users WHERE Username = @username AND PasswordHash = @hash", con);
        //   cmd.Parameters.AddWithValue("@username", username);
        //   cmd.Parameters.AddWithValue("@hash", passwordHash);
        //
        // WHY THIS STOPS THE ATTACK
        // With @parameters, the value travels to SQL Server separately from the
        // SQL text itself - it is sent as data, not as part of the command that
        // gets parsed. The server never re-parses ' OR '1'='1 as SQL, so it is
        // just compared as a (wrong) string value and the login correctly fails.
        // Take a screenshot of the same input failing against the fixed version
        // for part (c) of the report.
    }
}
