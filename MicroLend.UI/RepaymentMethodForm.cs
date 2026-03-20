using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MicroLend.DAL;
using MicroLend.DAL.Entities;

namespace MicroLend.UI
{
    public class RepaymentMethodForm : Form
    {
        public string Method => cmbMethods.SelectedItem?.ToString() ?? "";
        public string Reference => txtRef.Text;
        
        private readonly int _loanId;
        private readonly decimal _outstandingAmount;
        
        private ComboBox cmbMethods;
        private TextBox txtRef;
        private TextBox txtAmount;
        private Label lblAmount;

        public RepaymentMethodForm(int loanId)
        {
            _loanId = loanId;
            
            // Get outstanding amount
            using var ctx = new MicroLendDbContext();
            var loan = ctx.Loans.Find(loanId);
            var totalRepaid = ctx.Repayments.Where(r => r.LoanId == loanId).Sum(r => r.Amount);
            _outstandingAmount = (loan?.TargetAmount ?? 0) - totalRepaid;
            
            InitializeComponent();
        }
        
        // Original constructor for backward compatibility
        public RepaymentMethodForm() : this(0)
        {
        }

        private void InitializeComponent()
        {
            Text = "Make a Payment";
            Width = 450;
            Height = 300;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(240, 248, 255);

            var lblTitle = new Label
            {
                Text = "Make a Payment",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(140, 15),
                AutoSize = true
            };
            
            var lblLoanInfo = new Label
            {
                Text = $"Loan ID: {_loanId}",
                Location = new Point(20, 45),
                AutoSize = true
            };
            
            lblAmount = new Label
            {
                Text = $"Outstanding Amount: ₱{_outstandingAmount:N2}",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(255, 152, 0),
                Location = new Point(20, 65),
                AutoSize = true
            };
            
            var lblPayAmount = new Label { Text = "Payment Amount (₱)", Location = new Point(20, 100), AutoSize = true };
            txtAmount = new TextBox { Location = new Point(20, 120), Width = 390 };
            txtAmount.Text = _outstandingAmount.ToString("F2");

            var lblMethod = new Label { Text = "Payment Method", Location = new Point(20, 150), AutoSize = true };
            cmbMethods = new ComboBox { Location = new Point(20, 170), Width = 390, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbMethods.Items.AddRange(new[] { "Cash", "GCash", "Bank Transfer", "PayMaya", "Credit Card", "Other" });
            cmbMethods.SelectedIndex = 0;

            var lblRef = new Label { Text = "Reference / Transaction ID", Location = new Point(20, 200), AutoSize = true };
            txtRef = new TextBox { Location = new Point(20, 220), Width = 390 };

            var btnOk = new Button 
            { 
                Text = "Submit Payment", 
                Location = new Point(250, 230), 
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            var btnCancel = new Button { Text = "Cancel", Location = new Point(355, 230), Size = new Size(80, 35) };
            
            btnOk.Click += BtnOk_Click;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(lblTitle);
            Controls.Add(lblLoanInfo);
            Controls.Add(lblAmount);
            Controls.Add(lblPayAmount);
            Controls.Add(txtAmount);
            Controls.Add(lblMethod);
            Controls.Add(cmbMethods);
            Controls.Add(lblRef);
            Controls.Add(txtRef);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
        }
        
        private void BtnOk_Click(object? sender, EventArgs e)
        {
            if (!decimal.TryParse(txtAmount.Text, out var amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid payment amount.", "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(Method))
            {
                MessageBox.Show("Please select a payment method.", "No Method", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                using var ctx = new MicroLendDbContext();
                
                var repayment = new Repayment
                {
                    LoanId = _loanId,
                    Amount = amount,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = Method,
                    PaymentReference = Reference
                };
                
                ctx.Repayments.Add(repayment);
                
                // Update loan status if fully repaid
                var loan = ctx.Loans.Find(_loanId);
                if (loan != null)
                {
                    var totalRepaid = ctx.Repayments
                        .Where(r => r.LoanId == _loanId)
                        .Sum(r => r.Amount) + amount;
                    
                    if (totalRepaid >= loan.TargetAmount)
                    {
                        loan.Status = "FullyRepaid";
                    }
                }
                
                ctx.SaveChanges();
                
                MessageBox.Show("Payment submitted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing payment: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
