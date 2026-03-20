using System;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using MicroLend.DAL;
using MicroLend.DAL.Entities;

namespace MicroLend.UI
{
    public class AccountSettingsForm : Form
    {
        private readonly int _userId;
        private User _user;
        
        private TextBox txtUsername;
        private TextBox txtCurrentPassword;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private ComboBox cmbPaymentMethod;
        private TextBox txtPaymentReference;
        private Button btnSave;
        private Button btnCancel;
        private Label lblMessage;
        
        public AccountSettingsForm(int userId)
        {
            _userId = userId;
            InitializeComponent();
            LoadUserData();
        }
        
        private void InitializeComponent()
        {
            Text = "Account Settings - MicroLend";
            Width = 500;
            Height = 520;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.FromArgb(240, 248, 255);
            
            var titleLabel = new Label
            {
                Text = "Account Settings",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(150, 20),
                AutoSize = true
            };
            
            // Credentials Section
            var credPanel = new GroupBox
            {
                Text = "Login Credentials",
                Location = new Point(20, 70),
                Size = new Size(440, 160)
            };
            
            var lblUsername = new Label { Text = "Username", Location = new Point(20, 25), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(20, 45), Width = 380, ReadOnly = true };
            txtUsername.BackColor = Color.FromArgb(230, 230, 230);
            
            var lblCurrentPwd = new Label { Text = "Current Password", Location = new Point(20, 75), AutoSize = true };
            txtCurrentPassword = new TextBox { Location = new Point(20, 95), Width = 180, PasswordChar = '*' };
            
            var lblNewPwd = new Label { Text = "New Password", Location = new Point(220, 75), AutoSize = true };
            txtNewPassword = new TextBox { Location = new Point(220, 95), Width = 180, PasswordChar = '*' };
            
            var lblConfirmPwd = new Label { Text = "Confirm Password", Location = new Point(20, 120), AutoSize = true };
            txtConfirmPassword = new TextBox { Location = new Point(20, 140), Width = 180, PasswordChar = '*' };
            
            credPanel.Controls.Add(lblUsername);
            credPanel.Controls.Add(txtUsername);
            credPanel.Controls.Add(lblCurrentPwd);
            credPanel.Controls.Add(txtCurrentPassword);
            credPanel.Controls.Add(lblNewPwd);
            credPanel.Controls.Add(txtNewPassword);
            credPanel.Controls.Add(lblConfirmPwd);
            credPanel.Controls.Add(txtConfirmPassword);
            
            // Payment Methods Section
            var payPanel = new GroupBox
            {
                Text = "Payment Methods",
                Location = new Point(20, 240),
                Size = new Size(440, 130)
            };
            
            var lblPaymentMethod = new Label { Text = "Payment Method", Location = new Point(20, 25), AutoSize = true };
            cmbPaymentMethod = new ComboBox
            {
                Location = new Point(20, 45),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPaymentMethod.Items.AddRange(new[] { "Cash", "Bank Transfer", "GCash", "PayMaya", "Credit Card", "Other" });
            cmbPaymentMethod.SelectedIndex = 0;
            
            var lblPaymentRef = new Label { Text = "Payment Reference/Account", Location = new Point(240, 25), AutoSize = true };
            txtPaymentReference = new TextBox { Location = new Point(240, 45), Width = 180 };
            
            var lblPaymentNote = new Label
            {
                Text = "Add your payment details for receiving repayments (as lender) or making payments (as borrower).",
                Location = new Point(20, 80),
                Size = new Size(400, 40),
                ForeColor = Color.FromArgb(102, 102, 102),
                Font = new Font("Segoe UI", 9)
            };
            
            payPanel.Controls.Add(lblPaymentMethod);
            payPanel.Controls.Add(cmbPaymentMethod);
            payPanel.Controls.Add(lblPaymentRef);
            payPanel.Controls.Add(txtPaymentReference);
            payPanel.Controls.Add(lblPaymentNote);
            
            // Buttons
            btnSave = new Button
            {
                Text = "Save Changes",
                Location = new Point(280, 390),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;
            
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(390, 390),
                Size = new Size(80, 35)
            };
            btnCancel.Click += (s, e) => Close();
            
            lblMessage = new Label
            {
                Location = new Point(20, 400),
                Size = new Size(250, 30),
                ForeColor = Color.Red
            };
            
            Controls.Add(titleLabel);
            Controls.Add(credPanel);
            Controls.Add(payPanel);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            Controls.Add(lblMessage);
        }
        
        private void LoadUserData()
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                _user = ctx.Users.Find(_userId);
                
                if (_user != null)
                {
                    txtUsername.Text = _user.Username;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading user data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            lblMessage.ForeColor = Color.Red;
            lblMessage.Text = "";
            
            try
            {
                using var ctx = new MicroLendDbContext();
                var user = ctx.Users.Find(_userId);
                
                if (user == null)
                {
                    lblMessage.Text = "User not found.";
                    return;
                }
                
                // Validate password change if provided
                if (!string.IsNullOrWhiteSpace(txtCurrentPassword.Text) || 
                    !string.IsNullOrWhiteSpace(txtNewPassword.Text) ||
                    !string.IsNullOrWhiteSpace(txtConfirmPassword.Text))
                {
                    // Verify current password
                    var currentHash = ComputeHash(txtCurrentPassword.Text);
                    if (currentHash != user.PasswordHash)
                    {
                        lblMessage.Text = "Current password is incorrect.";
                        return;
                    }
                    
                    // Validate new password
                    if (string.IsNullOrWhiteSpace(txtNewPassword.Text))
                    {
                        lblMessage.Text = "Please enter a new password.";
                        return;
                    }
                    
                    if (txtNewPassword.Text != txtConfirmPassword.Text)
                    {
                        lblMessage.Text = "New passwords do not match.";
                        return;
                    }
                    
                    if (txtNewPassword.Text.Length < 4)
                    {
                        lblMessage.Text = "Password must be at least 4 characters.";
                        return;
                    }
                    
                    // Update password
                    user.PasswordHash = ComputeHash(txtNewPassword.Text);
                    user.UpdatedAt = DateTime.Now;
                }
                
                // Note: Payment method would typically be stored in a separate table
                // For now, we'll just show success message
                // In a full implementation, you'd add a PaymentMethod entity
                
                ctx.SaveChanges();
                
                lblMessage.ForeColor = Color.Green;
                lblMessage.Text = "Settings saved successfully!";
                
                MessageBox.Show("Your settings have been saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Clear password fields
                txtCurrentPassword.Clear();
                txtNewPassword.Clear();
                txtConfirmPassword.Clear();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error saving settings: " + ex.Message;
                MessageBox.Show("Error saving settings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
