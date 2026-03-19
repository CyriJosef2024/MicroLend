using System;
using System.Drawing;
using System.Windows.Forms;

namespace MicroLend.UI
{
    public class FundLoanForm : Form
    {
        public string LenderUsername => txtUser.Text;
        public decimal Amount => decimal.TryParse(txtAmount.Text, out var v) ? v : 0m;

        private TextBox txtUser, txtAmount;

        public FundLoanForm(MicroLend.DAL.Entities.Loan loan)
        {
            Text = $"Fund Loan #{loan.Id}";
            Width = 420;
            Height = 220;
            StartPosition = FormStartPosition.CenterParent;

            var lblUser = new Label { Text = "Lender username", Location = new Point(16, 16), AutoSize = true };
            txtUser = new TextBox { Location = new Point(16, 36), Width = 360 };
            var lblAmt = new Label { Text = "Amount", Location = new Point(16, 72), AutoSize = true };
            txtAmount = new TextBox { Location = new Point(16, 92), Width = 160 };

            var btnOk = new Button { Text = "Fund", Location = new Point(216, 132), Size = new Size(80, 30) };
            var btnCancel = new Button { Text = "Cancel", Location = new Point(304, 132), Size = new Size(80, 30) };
            btnOk.Click += (s, e) => { if (Amount <= 0) { MessageBox.Show("Enter valid amount"); return; } DialogResult = DialogResult.OK; };
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(lblUser); Controls.Add(txtUser); Controls.Add(lblAmt); Controls.Add(txtAmount);
            Controls.Add(btnOk); Controls.Add(btnCancel);
        }
    }
}
