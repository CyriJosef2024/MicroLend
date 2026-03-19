using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicroLend.UI
{
    public class BorrowerDashboardForm : Form
    {
        private readonly int _userId;
        private DataGridView dgvLoans;
        private Button btnTakeQuiz;

        public BorrowerDashboardForm(int userId)
        {
            _userId = userId;
            Text = "Borrower Dashboard";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            var top = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.FromArgb(2,100,170) };
            var lbl = new Label { Text = "Borrower Dashboard", ForeColor = Color.White, Font = new Font("Segoe UI", 16F, FontStyle.Bold), AutoSize = true, Location = new Point(16, 18) };
            top.Controls.Add(lbl);
            Controls.Add(top);

            dgvLoans = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
            Controls.Add(dgvLoans);

            btnTakeQuiz = new Button { Text = "Take Credit Quiz", Size = new Size(140, 36), Location = new Point(700, 72), BackColor = Color.FromArgb(75,181,67), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnTakeQuiz.Click += BtnTakeQuiz_Click;
            Controls.Add(btnTakeQuiz);

            Load += BorrowerDashboardForm_Load;
        }

        private async void BorrowerDashboardForm_Load(object? sender, EventArgs e)
        {
            try
            {
                var loanRepo = new MicroLend.DAL.Repositories.LoanRepository();
                var loans = await loanRepo.GetLoansByBorrowerAsync(_userId);
                dgvLoans.DataSource = loans.Select(l => new { l.Id, l.Purpose, l.TargetAmount, l.CurrentAmount, l.Status }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading borrower dashboard: " + ex.Message);
            }
        }

        private void BtnTakeQuiz_Click(object? sender, EventArgs e)
        {
            using var f = new CreditQuizForm(_userId);
            f.ShowDialog(this);
        }
    }
}
