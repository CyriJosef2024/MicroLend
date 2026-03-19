using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicroLend.UI
{
    public class LenderDashboardForm : Form
    {
        private readonly int _userId;
        private DataGridView dgvInvestments;

        public LenderDashboardForm(int userId)
        {
            _userId = userId;
            Text = "Lender Dashboard";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            var top = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.FromArgb(2,100,170) };
            var lbl = new Label { Text = "Lender Dashboard", ForeColor = Color.White, Font = new Font("Segoe UI", 16F, FontStyle.Bold), AutoSize = true, Location = new Point(16, 18) };
            top.Controls.Add(lbl);
            Controls.Add(top);

            dgvInvestments = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
            Controls.Add(dgvInvestments);

            Load += LenderDashboardForm_Load;
        }

        private async void LenderDashboardForm_Load(object? sender, EventArgs e)
        {
            try
            {
                var repo = new MicroLend.DAL.Repositories.LoanFunderRepository();
                var list = await repo.GetByLenderIdAsync(_userId);
                dgvInvestments.DataSource = list.Select(i => new { i.Id, i.LoanId, Amount = i.Amount, ExpectedInterest = i.ExpectedInterest }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading lender dashboard: " + ex.Message);
            }
        }
    }
}
