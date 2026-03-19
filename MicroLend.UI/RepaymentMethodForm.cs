using System;
using System.Drawing;
using System.Windows.Forms;

namespace MicroLend.UI
{
    public class RepaymentMethodForm : Form
    {
        public string Method => cmbMethods.SelectedItem?.ToString() ?? "";
        public string Reference => txtRef.Text;

        private ComboBox cmbMethods;
        private TextBox txtRef;

        public RepaymentMethodForm()
        {
            Text = "Select payment method";
            Width = 420;
            Height = 200;
            StartPosition = FormStartPosition.CenterParent;

            var lbl = new Label { Text = "Choose payment method", Location = new Point(16, 16), AutoSize = true };
            cmbMethods = new ComboBox { Location = new Point(16, 36), Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbMethods.Items.AddRange(new[] { "GCash", "Bank Transfer", "Mobile Wallet", "Card" });
            cmbMethods.SelectedIndex = 0;

            var lblRef = new Label { Text = "Reference / transaction id", Location = new Point(16, 76), AutoSize = true };
            txtRef = new TextBox { Location = new Point(16, 96), Width = 360 };

            var btnOk = new Button { Text = "OK", Location = new Point(216, 132), Size = new Size(80, 30) };
            var btnCancel = new Button { Text = "Cancel", Location = new Point(304, 132), Size = new Size(80, 30) };
            btnOk.Click += (s, e) => { if (string.IsNullOrWhiteSpace(Method)) { MessageBox.Show("Select method"); return; } DialogResult = DialogResult.OK; };
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(lbl); Controls.Add(cmbMethods); Controls.Add(lblRef); Controls.Add(txtRef); Controls.Add(btnOk); Controls.Add(btnCancel);
        }
    }
}
