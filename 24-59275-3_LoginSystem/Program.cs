using System;
using System.Windows.Forms;

namespace LoginSystem_24_59275_3
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Fail nicely up front if SQL Server isn't reachable, instead of
            // crashing the first time a form tries to query it.
            string error;
            if (!DatabaseHelper.TestConnection(out error))
            {
                MessageBox.Show(
                    "Could not connect to the database.\n\n" + error +
                    "\n\nCheck the connection string in App.config, then try again.",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Application.Run(new LoginForm());
        }
    }
}
