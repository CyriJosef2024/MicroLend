using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MicroLend.DAL;

namespace MicroLend.UI
{
    public class AdminDashboardForm : Form
    {
        private DataGridView dgv;
        private Label lblTotalUsersValue;
        private Label lblTotalBorrowersValue;
        private Label lblTotalLendersValue;
        private Label lblTotalLoansValue;
        
        public AdminDashboardForm()
        {
            Text = "Admin Dashboard - MicroLend";
            Width = 1000;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(240, 248, 255);

            CreateMenuBar();
            
            var top = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.FromArgb(0, 102, 204) };
            var lbl = new Label 
            { 
                Text = "Admin Dashboard", 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI", 16F, FontStyle.Bold), 
                AutoSize = true, 
                Location = new Point(16, 18) 
            };
            top.Controls.Add(lbl);
            Controls.Add(top);

            // Summary cards
            var summaryPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Location = new Point(10, 80),
                Size = new Size(970, 80),
                AutoSize = true
            };
            
            var card1 = CreateSummaryCard("Total Users", "0", Color.FromArgb(0, 120, 215), out lblTotalUsersValue);
            var card2 = CreateSummaryCard("Total Borrowers", "0", Color.FromArgb(0, 150, 136), out lblTotalBorrowersValue);
            var card3 = CreateSummaryCard("Total Lenders", "0", Color.FromArgb(255, 152, 0), out lblTotalLendersValue);
            var card4 = CreateSummaryCard("Total Loans", "0", Color.FromArgb(75, 181, 67), out lblTotalLoansValue);
            
            summaryPanel.Controls.Add(card1);
            summaryPanel.Controls.Add(card2);
            summaryPanel.Controls.Add(card3);
            summaryPanel.Controls.Add(card4);
            
            Controls.Add(summaryPanel);
            
            var lblUsers = new Label
            {
                Text = "All Users",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 175),
                AutoSize = true
            };
            Controls.Add(lblUsers);

            // Management toolbar
            var toolPanel = new Panel
            {
                Location = new Point(10, 200),
                Size = new Size(970, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var btnAdd = new Button { Text = "Add User", Size = new Size(100, 28), Location = new Point(0, 0) };
            btnAdd.Click += (s, e) => AddUser();
            var btnEdit = new Button { Text = "Edit User", Size = new Size(100, 28), Location = new Point(110, 0) };
            btnEdit.Click += (s, e) => EditUser();
            var btnDelete = new Button { Text = "Delete User", Size = new Size(100, 28), Location = new Point(220, 0) };
            btnDelete.Click += (s, e) => DeleteUser();
            var btnLogs = new Button { Text = "View Logs", Size = new Size(100, 28), Location = new Point(330, 0) };
            btnLogs.Click += (s, e) => ViewLogs();
            var btnExport = new Button { Text = "Export Report", Size = new Size(110, 28), Location = new Point(440, 0) };
            btnExport.Click += (s, e) => ExportReport();
            var btnSettings = new Button { Text = "System Settings", Size = new Size(120, 28), Location = new Point(560, 0) };
            btnSettings.Click += (s, e) => OpenSettings();

            toolPanel.Controls.Add(btnAdd);
            toolPanel.Controls.Add(btnEdit);
            toolPanel.Controls.Add(btnDelete);
            toolPanel.Controls.Add(btnLogs);
            toolPanel.Controls.Add(btnExport);
            toolPanel.Controls.Add(btnSettings);
            Controls.Add(toolPanel);

            dgv = new DataGridView
            {
                Location = new Point(10, 240),
                Size = new Size(970, 405),
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false
            };
            Controls.Add(dgv);

            Load += (s, e) => LoadData();
        }
        
        private Panel CreateSummaryCard(string title, string value, Color accentColor, out Label valueLabelOut)
        {
            var card = new Panel
            {
                Size = new Size(230, 70),
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            card.Paint += (se, ev) => {
                using var p = new Pen(accentColor, 3);
                ev.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
            };
            
            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(102, 102, 102),
                Location = new Point(10, 8),
                AutoSize = true
            };
            
            var valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(10, 28),
                AutoSize = true
            };
            
            card.Controls.Add(titleLabel);
            card.Controls.Add(valueLabel);

            valueLabelOut = valueLabel;
            
            return card;
        }
        
        private void CreateMenuBar()
        {
            var menuStrip = new MenuStrip();
            
            var fileMenu = new ToolStripMenuItem("Account");
            fileMenu.DropDownItems.Add("Settings", null, (s, e) => OpenSettings());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Logout", null, (s, e) => Logout());
            fileMenu.DropDownItems.Add("Exit", null, (s, e) => { Close(); });
            
            menuStrip.Items.Add(fileMenu);
            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);
        }
        
        private void LoadData()
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                
                var users = ctx.Users.Select(u => new 
                { 
                    u.Id, 
                    u.Username, 
                    u.Role, 
                    u.InitialCreditScore,
                    u.CreatedAt
                }).ToList();
                
                dgv.DataSource = users;
                
                if (dgv.Columns.Count > 0)
                {
                    dgv.Columns["Id"].HeaderText = "ID";
                    dgv.Columns["Username"].HeaderText = "Username";
                    dgv.Columns["Role"].HeaderText = "Role";
                    dgv.Columns["InitialCreditScore"].HeaderText = "Credit Score";
                    dgv.Columns["CreatedAt"].HeaderText = "Created";
                }
                
                // Update summary cards
                lblTotalUsersValue.Text = ctx.Users.Count().ToString();
                lblTotalBorrowersValue.Text = ctx.Borrowers.Count().ToString();
                lblTotalLendersValue.Text = ctx.Users.Count(u => u.Role != null && u.Role.ToLower() == "lender").ToString();
                lblTotalLoansValue.Text = ctx.Loans.Count().ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading admin dashboard: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private Control? FindControlByName(Control parent, string name)
        {
            foreach (Control c in parent.Controls)
            {
                if (c.Name == name) return c;
                var found = FindControlByName(c, name);
                if (found != null) return found;
            }
            return null;
        }
        
        private void OpenSettings()
        {
            // Admin settings - could be expanded
            using var dlg = new Form { Text = "System Settings", Width = 600, Height = 400, StartPosition = FormStartPosition.CenterParent };
            var lbl = new Label { Text = "System configuration and logs", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            dlg.Controls.Add(lbl);
            dlg.ShowDialog();
        }
        
        private void Logout()
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void AddUser()
        {
            using var edit = new UserEditForm();
            if (edit.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var ctx = new MicroLendDbContext();
                    var user = new MicroLend.DAL.Entities.User
                    {
                        Username = edit.Username,
                        PasswordHash = ComputeHash(edit.Password),
                        Role = edit.Role,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    ctx.Users.Add(user);
                    ctx.SaveChanges();
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding user: " + ex.Message);
                }
            }
        }

        private void EditUser()
        {
            if (dgv.CurrentRow == null) return;
            var id = Convert.ToInt32(dgv.CurrentRow.Cells["Id"].Value);
            using var ctx = new MicroLendDbContext();
            var user = ctx.Users.Find(id);
            if (user == null) return;
            using var edit = new UserEditForm(user.Username, user.Role);
            if (edit.ShowDialog() == DialogResult.OK)
            {
                user.Username = edit.Username;
                if (!string.IsNullOrEmpty(edit.Password)) user.PasswordHash = ComputeHash(edit.Password);
                user.Role = edit.Role;
                user.UpdatedAt = DateTime.Now;
                ctx.SaveChanges();
                LoadData();
            }
        }

        private void DeleteUser()
        {
            if (dgv.CurrentRow == null) return;
            var id = Convert.ToInt32(dgv.CurrentRow.Cells["Id"].Value);
            var confirm = MessageBox.Show($"Delete user ID {id}?","Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;
            try
            {
                using var ctx = new MicroLendDbContext();
                var user = ctx.Users.Find(id);
                if (user != null)
                {
                    ctx.Users.Remove(user);
                    ctx.SaveChanges();
                }
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting user: " + ex.Message);
            }
        }

        private void ExportReport()
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                var users = ctx.Users.Select(u => new { u.Id, u.Username, u.Role, u.InitialCreditScore, u.CreatedAt }).ToList();
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Id,Username,Role,CreditScore,CreatedAt");
                foreach (var u in users)
                {
                    sb.AppendLine($"{u.Id},{u.Username},{u.Role},{u.InitialCreditScore},{u.CreatedAt:O}");
                }
                var fn = System.IO.Path.Combine(AppContext.BaseDirectory, "admin_users_export.csv");
                System.IO.File.WriteAllText(fn, sb.ToString());
                MessageBox.Show("Exported to: " + fn);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message);
            }
        }

        private void ViewLogs()
        {
            try
            {
                var log = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MicroLend_seeder_log.txt");
                if (!System.IO.File.Exists(log)) { MessageBox.Show("No logs found."); return; }
                var txt = System.IO.File.ReadAllText(log);
                using var dlg = new Form { Text = "Seeder Log", Width = 800, Height = 600, StartPosition = FormStartPosition.CenterParent };
                var tb = new TextBox { Multiline = true, ReadOnly = true, WordWrap = false, ScrollBars = ScrollBars.Both, Dock = DockStyle.Fill, Text = txt };
                dlg.Controls.Add(tb);
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading logs: " + ex.Message);
            }
        }

        private string ComputeHash(string pwd)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pwd));
            return Convert.ToHexString(bytes);
        }
    }
}
