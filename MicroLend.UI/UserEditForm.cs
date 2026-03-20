using System;
using System.Drawing;
using System.Windows.Forms;

namespace MicroLend.UI
{
    public class UserEditForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private ComboBox cmbRole;

        public string Username => txtUsername.Text.Trim();
        public string Password => txtPassword.Text;
        public string Role => cmbRole.SelectedItem?.ToString() ?? "Borrower";

        public UserEditForm(string username = "", string role = "Borrower")
        {
            Text = string.IsNullOrEmpty(username) ? "Create User" : "Edit User";
            Width = 420;
            Height = 220;
            StartPosition = FormStartPosition.CenterParent;

            var lblUser = new Label { Text = "Username", Location = new Point(16, 16), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(16, 36), Width = 360, Text = username };

            var lblPwd = new Label { Text = "Password", Location = new Point(16, 72), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(16, 92), Width = 360, PasswordChar = '*' };

            var lblRole = new Label { Text = "Role", Location = new Point(16, 128), AutoSize = true };
            cmbRole = new ComboBox { Location = new Point(16, 148), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new[] { "Borrower", "Lender", "Admin" });
            cmbRole.SelectedItem = role;

            var btnOk = new Button { Text = "OK", Location = new Point(196, 180), Size = new Size(80, 30) };
            var btnCancel = new Button { Text = "Cancel", Location = new Point(284, 180), Size = new Size(80, 30) };
            btnOk.Click += (s, e) => { if (ValidateForm()) DialogResult = DialogResult.OK; };
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(lblUser); Controls.Add(txtUsername);
            Controls.Add(lblPwd); Controls.Add(txtPassword);
            Controls.Add(lblRole); Controls.Add(cmbRole);
            Controls.Add(btnOk); Controls.Add(btnCancel);
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text)) { MessageBox.Show("Enter username"); return false; }
            return true;
        }
    }
}
