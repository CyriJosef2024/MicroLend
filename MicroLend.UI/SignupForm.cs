using System;
using System.Drawing;
using System.Windows.Forms;

namespace MicroLend.UI
{
    public class SignupForm : Form
    {
        public string Username => txtUsername.Text;
        public string Password => txtPassword.Text;
        public string Role => cmbRole.SelectedItem?.ToString() ?? "Borrower";
        public string FullName => txtFullname.Text;
        public string Contact => txtContact.Text;
        public decimal MonthlyIncome => decimal.TryParse(txtIncome.Text, out var v) ? v : 0m;
        public string BusinessType => txtBusiness.Text;

        private TextBox txtUsername, txtPassword, txtFullname, txtContact, txtIncome, txtBusiness;
        private ComboBox cmbRole;

        public SignupForm()
        {
            Text = "Sign up";
            Width = 460;
            Height = 420;
            StartPosition = FormStartPosition.CenterParent;

            var lblUser = new Label { Text = "Username", Location = new Point(16, 16), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(16, 36), Width = 360 };
            var lblPwd = new Label { Text = "Password", Location = new Point(16, 72), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(16, 92), Width = 360 };

            var lblRole = new Label { Text = "Role", Location = new Point(16, 128), AutoSize = true };
            cmbRole = new ComboBox { Location = new Point(16, 148), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new[] { "Borrower", "Lender" });
            cmbRole.SelectedIndex = 0;

            var lblFull = new Label { Text = "Full name", Location = new Point(16, 184), AutoSize = true };
            txtFullname = new TextBox { Location = new Point(16, 204), Width = 360 };
            var lblContact = new Label { Text = "Contact number", Location = new Point(16, 240), AutoSize = true };
            txtContact = new TextBox { Location = new Point(16, 260), Width = 360 };

            var lblIncome = new Label { Text = "Monthly income", Location = new Point(16, 296), AutoSize = true };
            txtIncome = new TextBox { Location = new Point(16, 316), Width = 160 };
            var lblBus = new Label { Text = "Business type", Location = new Point(200, 296), AutoSize = true };
            txtBusiness = new TextBox { Location = new Point(200, 316), Width = 176 };

            var btnOk = new Button { Text = "Create", Location = new Point(216, 352), Size = new Size(80, 30) };
            var btnCancel = new Button { Text = "Cancel", Location = new Point(304, 352), Size = new Size(80, 30) };
            btnOk.Click += (s, e) => { if (ValidateForm()) DialogResult = DialogResult.OK; };
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(lblUser); Controls.Add(txtUsername); Controls.Add(lblPwd); Controls.Add(txtPassword);
            Controls.Add(lblRole); Controls.Add(cmbRole);
            Controls.Add(lblFull); Controls.Add(txtFullname); Controls.Add(lblContact); Controls.Add(txtContact);
            Controls.Add(lblIncome); Controls.Add(txtIncome); Controls.Add(lblBus); Controls.Add(txtBusiness);
            Controls.Add(btnOk); Controls.Add(btnCancel);
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text)) { MessageBox.Show("Enter username"); return false; }
            if (string.IsNullOrWhiteSpace(txtPassword.Text)) { MessageBox.Show("Enter password"); return false; }
            return true;
        }
    }
}
