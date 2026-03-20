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
            
            var card1 = CreateSummaryCard("Total Users", "0", Color.FromArgb(0, 120, 215));
            var card2 = CreateSummaryCard("Total Borrowers", "0", Color.FromArgb(0, 150, 136));
            var card3 = CreateSummaryCard("Total Lenders", "0", Color.FromArgb(255, 152, 0));
            var card4 = CreateSummaryCard("Total Loans", "0", Color.FromArgb(75, 181, 67));
            
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

            dgv = new DataGridView
            {
                Location = new Point(10, 205),
                Size = new Size(970, 440),
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false
            };
            Controls.Add(dgv);

            Load += (s, e) => LoadData();
        }
        
        private Panel CreateSummaryCard(string title, string value, Color accentColor)
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
                AutoSize = true,
                Name = "ValueLabel"
            };
            
            card.Controls.Add(titleLabel);
            card.Controls.Add(valueLabel);
            
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
                var lblTotalUsers = FindControlByName(this, "ValueLabel") as Label;
                // This is a simplified approach - in production you'd want to find the specific controls
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
            MessageBox.Show("Admin settings panel would open here.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }
}
