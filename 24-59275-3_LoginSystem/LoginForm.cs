using System;
using System.Windows.Forms;

namespace LoginSystem_24_59275_3
{
    public partial class LoginForm : Form
    {
        private int failedAttempts = 0;
        private const int MaxAttempts = 3;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblStatus.Text = "Enter a username and password.";
                return;
            }

            string hash = DatabaseHelper.HashPassword(password);

            UserRecord user;
            try
            {
                user = DatabaseHelper.TryLogin(username, hash);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not reach the database.\n\n" + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (user != null)
            {
                failedAttempts = 0;

                // Bonus: start a LoginHistory row. Not fatal if this fails.
                int historyId = 0;
                try
                {
                    historyId = DatabaseHelper.LogLoginStart(user.UserID);
                }
                catch
                {
                    historyId = 0;
                }

                HomeForm home = new HomeForm(user.FullName, historyId);
                home.FormClosed += (s, args) => ResetAndShow();
                home.Show();
                this.Hide();
            }
            else
            {
                failedAttempts++;
                int remaining = MaxAttempts - failedAttempts;

                if (remaining <= 0)
                {
                    MessageBox.Show("Too many failed attempts. Login has been disabled.",
                        "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lblStatus.Text = "Login disabled after 3 failed attempts.";
                    btnLogin.Enabled = false;
                }
                else
                {
                    MessageBox.Show("Incorrect username or password. " + remaining + " attempt(s) left.",
                        "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lblStatus.Text = remaining + " attempt(s) left.";
                }

                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        private void btnGoToRegister_Click(object sender, EventArgs e)
        {
            using (RegisterForm reg = new RegisterForm())
            {
                reg.ShowDialog();
            }
        }

        // Called when HomeForm closes (logout). Clears the form, re-enables the
        // login button, and shows the login form again - the app never exits here.
        public void ResetAndShow()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            lblStatus.Text = "";
            failedAttempts = 0;
            btnLogin.Enabled = true;
            this.Show();
            txtUsername.Focus();
        }
    }
}
