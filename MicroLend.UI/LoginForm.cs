using MicroLend.DAL.Repositories;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicroLend.UI
{
    public partial class LoginForm : Form
    {
        private readonly UserRepository _userRepo = new UserRepository();

        public int? LoggedInUserId { get; private set; }
        public string LoggedInRole { get; private set; } = string.Empty;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            _ = TryLoginAsync();
        }

        private async Task TryLoginAsync()
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text; // in-memory demo: password equals username

            var user = await _userRepo.GetByUsernameAsync(username);
            if (user == null)
            {
                MessageBox.Show("User not found.");
                return;
            }

            // Simple demo authentication: password must equal username for seeded users
            if (password != username)
            {
                MessageBox.Show("Invalid credentials.");
                return;
            }

            LoggedInUserId = user.Id;
            LoggedInRole = user.Role;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
