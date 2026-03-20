using MicroLend.DAL;
using MicroLend.DAL.Entities;
using System;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace MicroLend.UI
{
    public class Form1 : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnSignup;
        private Label lblMessage;

        public Form1()
        {
            Text = "MicroLend - Login";
            Width = 420;
            Height = 320;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var lblTitle = new Label
            {
                Text = "MicroLend",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(130, 20),
                AutoSize = true
            };

            var lblUser = new Label { Text = "Username", Location = new Point(30, 70), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(30, 90), Width = 340 };

            var lblPwd = new Label { Text = "Password", Location = new Point(30, 120), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(30, 140), Width = 340, PasswordChar = '*' };

            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(30, 180),
                Size = new Size(160, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.Click += BtnLogin_Click;

            btnSignup = new Button
            {
                Text = "Sign Up",
                Location = new Point(210, 180),
                Size = new Size(160, 35),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSignup.Click += BtnSignup_Click;

            lblMessage = new Label
            {
                Location = new Point(30, 230),
                Size = new Size(340, 40),
                ForeColor = Color.Red,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };

            AcceptButton = btnLogin;

            Controls.Add(lblTitle);
            Controls.Add(lblUser);
            Controls.Add(txtUsername);
            Controls.Add(lblPwd);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
            Controls.Add(btnSignup);
            Controls.Add(lblMessage);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblMessage.Text = "Please enter both username and password.";
                return;
            }

            try
            {
                using var ctx = new MicroLendDbContext();
                var user = ctx.Users.FirstOrDefault(u => u.Username == username);

                if (user == null || !VerifyPassword(password, user.PasswordHash))
                {
                    lblMessage.Text = "Invalid username or password.";
                    return;
                }

                // Hide login form and show appropriate dashboard
                this.Hide();
                ShowDashboard(user);
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error: " + ex.Message;
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            // Simple hash comparison - in production use proper password hashing
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var computedHash = Convert.ToHexString(bytes);
            return computedHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }

        private void ShowDashboard(User user)
        {
            Form? dashboard = null;

            switch (user.Role?.ToLower())
            {
                case "admin":
                    dashboard = new AdminDashboardForm();
                    break;
                case "officer":
                    dashboard = new DashboardForm();
                    break;
                case "lender":
                    dashboard = new LenderDashboardForm(user.Id);
                    break;
                case "borrower":
                default:
                    dashboard = new BorrowerDashboardForm(user.Id);
                    break;
            }

            if (dashboard != null)
            {
                dashboard.FormClosed += (s, e) => this.Show();
                dashboard.ShowDialog();
            }
            else
            {
                this.Show();
            }
        }

        private void BtnSignup_Click(object? sender, EventArgs e)
        {
            using var signupForm = new SignupForm();
            if (signupForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var ctx = new MicroLendDbContext();

                    // Check if username already exists
                    if (ctx.Users.Any(u => u.Username == signupForm.Username))
                    {
                        lblMessage.Text = "Username already exists.";
                        return;
                    }

                    // Hash password
                    using var sha256 = SHA256.Create();
                    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(signupForm.Password));
                    var passwordHash = Convert.ToHexString(bytes);

                    // Create user
                    var user = new User
                    {
                        Username = signupForm.Username,
                        PasswordHash = passwordHash,
                        Role = signupForm.Role,
                        CreatedAt = DateTime.Now
                    };

                    ctx.Users.Add(user);
                    ctx.SaveChanges();

                    // Create borrower record if role is Borrower
                    if (signupForm.Role.Equals("Borrower", StringComparison.OrdinalIgnoreCase))
                    {
                        var borrower = new Borrower
                        {
                            UserId = user.Id,
                            Name = signupForm.FullName,
                            ContactNumber = signupForm.Contact,
                            MonthlyIncome = signupForm.MonthlyIncome,
                            BusinessType = signupForm.BusinessType
                        };
                        ctx.Borrowers.Add(borrower);
                        ctx.SaveChanges();
                    }

                    lblMessage.ForeColor = Color.Green;
                    lblMessage.Text = "Account created successfully! Please login.";
                    txtUsername.Text = signupForm.Username;
                    txtPassword.Clear();
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Error creating account: " + ex.Message;
                }
            }
        }
    }
}
