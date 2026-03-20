using System;
using System.Drawing;
using System.Windows.Forms;
using MicroLend.DAL;
using MicroLend.DAL.Entities;

namespace MicroLend.UI
{
    public class FundLoanForm : Form
    {
        public int LenderId { get; }
        public int LoanId { get; }
        public decimal Amount => decimal.TryParse(txtAmount.Text, out var v) ? v : 0m;
        
        private TextBox txtAmount;
        private Label lblLoanId;
        private Label lblMaxAmount;
        private decimal _maxAmount;

        public FundLoanForm(int lenderId, int loanId, decimal maxAmount)
        {
            LenderId = lenderId;
            LoanId = loanId;
            _maxAmount = maxAmount;
            
            Text = $"Fund Loan #{loanId}";
            Width = 420;
            Height = 220;
            StartPosition = FormStartPosition.CenterParent;

            var lblTitle = new Label 
            { 
                Text = $"Fund Loan #{loanId}", 
                Location = new Point(16, 16), 
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            
            lblLoanId = new Label { Text = $"Loan ID: {loanId}", Location = new Point(16, 45), AutoSize = true };
            lblMaxAmount = new Label { Text = $"Maximum amount: ₱{maxAmount:N2}", Location = new Point(16, 65), AutoSize = true };
            
            var lblAmt = new Label { Text = "Amount to Fund (₱)", Location = new Point(16, 95), AutoSize = true };
            txtAmount = new TextBox { Location = new Point(16, 115), Width = 360 };

            var btnOk = new Button { Text = "Fund", Location = new Point(216, 155), Size = new Size(80, 30), BackColor = Color.FromArgb(0, 150, 136), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnCancel = new Button { Text = "Cancel", Location = new Point(304, 155), Size = new Size(80, 30) };
            btnOk.Click += BtnOk_Click;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(lblTitle);
            Controls.Add(lblLoanId);
            Controls.Add(lblMaxAmount);
            Controls.Add(lblAmt);
            Controls.Add(txtAmount);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
        }
        
        // Original constructor for backward compatibility
        public FundLoanForm(Loan loan) : this(0, loan.Id, loan.TargetAmount - loan.CurrentAmount)
        {
        }
        
        private void BtnOk_Click(object? sender, EventArgs e)
        {
            if (Amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount.", "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (Amount > _maxAmount)
            {
                MessageBox.Show($"Amount exceeds the remaining funding needed (₱{_maxAmount:N2}).", "Amount Too High", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Save the funding
            try
            {
                using var ctx = new MicroLendDbContext();
                
                var funder = new LoanFunder
                {
                    LoanId = LoanId,
                    LenderId = LenderId,
                    Amount = Amount,
                    ExpectedInterest = Amount * 0.05m, // 5% expected interest
                    FundingDate = DateTime.Now
                };
                
                ctx.LoanFunders.Add(funder);
                
                // Update loan current amount
                var loan = ctx.Loans.Find(LoanId);
                if (loan != null)
                {
                    loan.CurrentAmount += Amount;
                    
                    // Check if fully funded
                    if (loan.CurrentAmount >= loan.TargetAmount)
                    {
                        loan.Status = "FullyFunded";
                    }
                }
                
                ctx.SaveChanges();
                
                MessageBox.Show("Loan funded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error funding loan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
