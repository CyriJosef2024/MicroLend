using MicroLend.DAL.Entities;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MicroLend.UI
{
    public class BorrowerEditForm : Form
    {
        public Borrower Borrower { get; private set; }

        private TextBox txtName, txtContact, txtIncome;
        private ComboBox cmbBusiness;

        public BorrowerEditForm(Borrower? existing = null)
        {
            Text = existing == null ? "New Borrower" : "Edit Borrower";
            Width = 420;
            Height = 320;
            StartPosition = FormStartPosition.CenterParent;

            Borrower = existing ?? new Borrower();

            var lblName = new Label { Text = "Full name", Location = new Point(16, 16), AutoSize = true };
            txtName = new TextBox { Location = new Point(16, 36), Width = 360, Text = Borrower.Name };
            var lblContact = new Label { Text = "Contact number", Location = new Point(16, 72), AutoSize = true };
            txtContact = new TextBox { Location = new Point(16, 92), Width = 360, Text = Borrower.ContactNumber };
            var lblIncome = new Label { Text = "Monthly income", Location = new Point(16, 128), AutoSize = true };
            txtIncome = new TextBox { Location = new Point(16, 148), Width = 160, Text = Borrower.MonthlyIncome.ToString() };
            var lblBusiness = new Label { Text = "Business type", Location = new Point(200, 128), AutoSize = true };
            cmbBusiness = new ComboBox 
            { 
                Location = new Point(200, 148), 
                Width = 176, 
                DropDownStyle = ComboBoxStyle.DropDownList 
            };
            cmbBusiness.Items.AddRange(new[] { 
                "Personal", 
                "Sari-Sari Store", 
                "Farming", 
                "Food Stall / Carinderia", 
                "Transportation", 
                "Services (salon, repair, etc)", 
                "Manufacturing", 
                "Online Business", 
                "Other" 
            });
            
            // Set the existing business type if available
            if (!string.IsNullOrEmpty(Borrower.BusinessType) && cmbBusiness.Items.Contains(Borrower.BusinessType))
            {
                cmbBusiness.SelectedItem = Borrower.BusinessType;
            }
            else
            {
                cmbBusiness.SelectedIndex = 0;
            }

            var btnOk = new Button { Text = "OK", Location = new Point(216, 220), Size = new Size(80, 30) };
            var btnCancel = new Button { Text = "Cancel", Location = new Point(304, 220), Size = new Size(80, 30) };
            btnOk.Click += (s, e) => { if (Apply()) DialogResult = DialogResult.OK; };
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblContact);
            Controls.Add(txtContact);
            Controls.Add(lblIncome);
            Controls.Add(txtIncome);
            Controls.Add(lblBusiness);
            Controls.Add(cmbBusiness);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
        }

        private bool Apply()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Enter name"); return false; }
            Borrower.Name = txtName.Text.Trim();
            Borrower.ContactNumber = txtContact.Text.Trim();
            if (decimal.TryParse(txtIncome.Text, out var inc)) Borrower.MonthlyIncome = inc;
            Borrower.BusinessType = cmbBusiness.SelectedItem?.ToString() ?? "Personal";
            return true;
        }
    }
}
