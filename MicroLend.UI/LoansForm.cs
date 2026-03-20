using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MicroLend.DAL;
using MicroLend.DAL.Entities;

namespace MicroLend.UI
{
    public class LoansForm : Form
    {
        private readonly int _userId;
        private TextBox txtPurpose;
        private TextBox txtAmount;
        private ComboBox cmbStatus;
        private Button btnApply;
        private DataGridView dgvLoans;
        
        public LoansForm(int userId)
        {
            _userId = userId;
            InitializeComponent();
            LoadLoans();
        }
        
        private void InitializeComponent()
        {
            Text = "Apply for Loan - MicroLend";
            Width = 800;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(240, 248, 255);
            
            var titleLabel = new Label
            {
                Text = "Apply for a Loan",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(300, 20),
                AutoSize = true
            };
            
            // Application Form Panel
            var formPanel = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(350, 200),
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            formPanel.Paint += (s, e) => {
                using var p = new Pen(Color.FromArgb(0, 120, 215));
                e.Graphics.DrawRectangle(p, 0, 0, formPanel.Width - 1, formPanel.Height - 1);
            };
            
            var lblPurpose = new Label { Text = "Loan Purpose", Location = new Point(15, 15), AutoSize = true };
            txtPurpose = new TextBox { Location = new Point(15, 35), Width = 310 };
            
            var lblAmount = new Label { Text = "Amount (₱)", Location = new Point(15, 70), AutoSize = true };
            txtAmount = new TextBox { Location = new Point(15, 90), Width = 150 };
            
            var lblStatus = new Label { Text = "Loan Type", Location = new Point(180, 70), AutoSize = true };
            cmbStatus = new ComboBox { Location = new Point(180, 90), Width = 145, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new[] { "Personal", "Business", "Emergency", "Education" });
            cmbStatus.SelectedIndex = 0;
            
            btnApply = new Button
            {
                Text = "Submit Application",
                Location = new Point(15, 140),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnApply.Click += BtnApply_Click;
            
            formPanel.Controls.Add(lblPurpose);
            formPanel.Controls.Add(txtPurpose);
            formPanel.Controls.Add(lblAmount);
            formPanel.Controls.Add(txtAmount);
            formPanel.Controls.Add(lblStatus);
            formPanel.Controls.Add(cmbStatus);
            formPanel.Controls.Add(btnApply);
            
            // Loans List Panel
            var listPanel = new Panel
            {
                Location = new Point(390, 70),
                Size = new Size(390, 480),
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            
            var lblMyLoans = new Label
            {
                Text = "My Loan Applications",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            
            dgvLoans = new DataGridView
            {
                Location = new Point(10, 40),
                Size = new Size(370, 430),
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false
            };
            
            listPanel.Controls.Add(lblMyLoans);
            listPanel.Controls.Add(dgvLoans);
            
            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(680, 555),
                Size = new Size(100, 30)
            };
            btnClose.Click += (s, e) => Close();
            
            Controls.Add(titleLabel);
            Controls.Add(formPanel);
            Controls.Add(listPanel);
            Controls.Add(btnClose);
        }
        
        private void LoadLoans()
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                var borrower = ctx.Borrowers.FirstOrDefault(b => b.UserId == _userId);
                
                if (borrower == null)
                {
                    MessageBox.Show("Borrower profile not found. Please contact support.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                var loans = ctx.Loans
                    .Where(l => l.BorrowerId == borrower.Id)
                    .Select(l => new
                    {
                        l.Id,
                        l.Purpose,
                        l.TargetAmount,
                        l.CurrentAmount,
                        l.Status,
                        l.RiskScore
                    })
                    .ToList();
                
                dgvLoans.DataSource = loans;
                
                if (dgvLoans.Columns.Count > 0)
                {
                    dgvLoans.Columns["Id"].HeaderText = "Loan ID";
                    dgvLoans.Columns["Purpose"].HeaderText = "Purpose";
                    dgvLoans.Columns["TargetAmount"].HeaderText = "Amount (₱)";
                    dgvLoans.Columns["CurrentAmount"].HeaderText = "Funded (₱)";
                    dgvLoans.Columns["Status"].HeaderText = "Status";
                    dgvLoans.Columns["RiskScore"].HeaderText = "Risk Score";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading loans: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void BtnApply_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPurpose.Text))
            {
                MessageBox.Show("Please enter loan purpose.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (!decimal.TryParse(txtAmount.Text, out var amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                using var ctx = new MicroLendDbContext();
                var borrower = ctx.Borrowers.FirstOrDefault(b => b.UserId == _userId);
                
                if (borrower == null)
                {
                    MessageBox.Show("Borrower profile not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                var loan = new Loan
                {
                    Purpose = $"[{cmbStatus.SelectedItem}] {txtPurpose.Text}",
                    TargetAmount = amount,
                    CurrentAmount = 0,
                    Status = "Pending",
                    InterestRate = 5.0m,
                    IsCrowdfunded = true,
                    BorrowerId = borrower.Id,
                    RiskScore = 0,
                    DateGranted = DateTime.Now
                };
                
                ctx.Loans.Add(loan);
                ctx.SaveChanges();
                
                MessageBox.Show("Loan application submitted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                txtPurpose.Clear();
                txtAmount.Clear();
                
                LoadLoans();
                
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error submitting loan application: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}