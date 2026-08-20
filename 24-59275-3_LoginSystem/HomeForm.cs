using System;
using System.Windows.Forms;

namespace LoginSystem_24_59275_3
{
    public partial class HomeForm : Form
    {
        // The LoginHistory row created when this user logged in (0 if it
        // failed to write, e.g. if the bonus table isn't there for some reason).
        private readonly int historyId;

        public HomeForm(string fullName, int historyId)
        {
            InitializeComponent();
            this.historyId = historyId;
            lblWelcome.Text = "Welcome, " + fullName;
        }

        private void HomeForm_Load(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                // Never bind PasswordHash here - the grid only asks for
                // UserID, Username, Email, CreatedAt.
                dgvUsers.DataSource = DatabaseHelper.GetAllUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load users.\n\n" + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                DatabaseHelper.LogLogoutTime(historyId);
            }
            catch
            {
                // Not fatal - the user still gets logged out either way.
            }

            // Closing (not exiting the app) triggers LoginForm.ResetAndShow()
            // through the FormClosed handler wired up in LoginForm.
            this.Close();
        }
    }
}
