using MicroLend.DAL;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MicroLend.UI
{
    public class DashboardForm : Form
    {
        private DataGridView dgvRepayments;
        private ListView lvPurposes;

        public DashboardForm()
        {
            Text = "Risk Dashboard";
            Width = 1000;
            Height = 640;
            StartPosition = FormStartPosition.CenterParent;

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 320 };
            Controls.Add(split);

            dgvRepayments = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
            dgvRepayments.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date", DataPropertyName = "PaymentDate", Width = 180 });
            dgvRepayments.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Amount", DataPropertyName = "Amount", Width = 120 });
            dgvRepayments.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "LoanId", DataPropertyName = "LoanId", Width = 80 });
            dgvRepayments.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "UserId", DataPropertyName = "UserId", Width = 80 });
            split.Panel1.Controls.Add(dgvRepayments);

            lvPurposes = new ListView { Dock = DockStyle.Fill, View = View.Details }; 
            lvPurposes.Columns.Add("Purpose", 400);
            lvPurposes.Columns.Add("Count", 100);
            split.Panel2.Controls.Add(lvPurposes);

            Load += DashboardForm_Load;
        }

        private void DashboardForm_Load(object? sender, EventArgs e)
        {
            try
            {
                var ctx = new MicroLendDbContext();
                var repayments = ctx.Repayments.OrderBy(r => r.PaymentDate).Take(100).ToList();
                dgvRepayments.DataSource = repayments.Select(r => new
                {
                    PaymentDate = r.PaymentDate.ToString("g"),
                    Amount = r.Amount.ToString("C"),
                    r.LoanId,
                    r.UserId
                }).ToList();

                var purposes = ctx.Loans.GroupBy(l => l.Purpose).Select(g => new { Purpose = g.Key, Count = g.Count() }).ToList();
                lvPurposes.Items.Clear();
                foreach (var p in purposes)
                {
                    var it = new ListViewItem(new[] { p.Purpose ?? "(none)", p.Count.ToString() });
                    lvPurposes.Items.Add(it);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading dashboard: " + ex.Message);
            }
        }
    }
}
