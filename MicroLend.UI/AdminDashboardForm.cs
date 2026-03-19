using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicroLend.UI
{
    public class AdminDashboardForm : Form
    {
        public AdminDashboardForm()
        {
            Text = "Admin Dashboard";
            Width = 1000;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;

            var top = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.FromArgb(2,100,170) };
            var lbl = new Label { Text = "Admin Dashboard", ForeColor = Color.White, Font = new Font("Segoe UI", 16F, FontStyle.Bold), AutoSize = true, Location = new Point(16, 18) };
            top.Controls.Add(lbl);
            Controls.Add(top);

            var dgv = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
            Controls.Add(dgv);

            Load += async (s, e) => {
                try
                {
                    var ctx = new MicroLend.DAL.MicroLendDbContext();
                    dgv.DataSource = ctx.Users.Select(u => new { u.Id, u.Username, u.Role, u.InitialCreditScore }).ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading admin dashboard: " + ex.Message);
                }
            };
        }
    }
}
