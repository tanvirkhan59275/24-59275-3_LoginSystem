using System;
using System.Windows.Forms;

namespace LoginSystem_24_59275_3
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirm = txtConfirmPassword.Text;
            string email = txtEmail.Text.Trim();
            string fullName = txtFullName.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirm) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullName))
            {
                lblStatus.Text = "All fields are required.";
                return;
            }

            if (password.Length < 6)
            {
                lblStatus.Text = "Password must be at least 6 characters.";
                return;
            }

            if (password != confirm)
            {
                lblStatus.Text = "Passwords do not match.";
                return;
            }

            if (!email.Contains("@"))
            {
                lblStatus.Text = "Enter a valid email address.";
                return;
            }

            try
            {
                if (DatabaseHelper.UsernameExists(username))
                {
                    lblStatus.Text = "Username already taken.";
                    return;
                }

                string hash = DatabaseHelper.HashPassword(password);
                DatabaseHelper.RegisterUser(username, hash, email, fullName);

                MessageBox.Show("Registration successful. You can now log in.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearForm();
                this.Close();
            }
            catch (Exception ex)
            {
                // Covers things like a UNIQUE constraint violation on a race
                // condition, or the database being unreachable.
                MessageBox.Show("Could not register.\n\n" + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ClearForm()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
            txtEmail.Clear();
            txtFullName.Clear();
            lblStatus.Text = "";
        }
    }
}
