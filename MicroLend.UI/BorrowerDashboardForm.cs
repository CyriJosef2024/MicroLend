using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MicroLend.DAL;
using MicroLend.DAL.Entities;

namespace MicroLend.UI
{
    public class BorrowerDashboardForm : Form
    {
        private readonly int _userId;
        private TabControl tabControl;
        private DataGridView dgvMyLoans;
        private DataGridView dgvRepayments;
        private Label lblTotalBorrowed;
        private Label lblTotalRepaid;
        private Label lblOutstanding;
        
        public BorrowerDashboardForm(int userId)
        {
            _userId = userId;
            Text = "Borrower Dashboard - MicroLend";
            Width = 1000;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(240, 248, 255);
            
            CreateMenuBar();
            CreateDashboard();
        }
        
        private void CreateMenuBar()
        {
            var menuStrip = new MenuStrip();
            
            var fileMenu = new ToolStripMenuItem("Account");
            fileMenu.DropDownItems.Add("Settings", null, (s, e) => OpenSettings());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Logout", null, (s, e) => Logout());
            fileMenu.DropDownItems.Add("Exit", null, (s, e) => { Close(); });
            
            menuStrip.Items.Add(fileMenu);
            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);
        }
        
        private void CreateDashboard()
        {
            // Summary Cards Panel
            var summaryPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Location = new Point(10, 40),
                Size = new Size(970, 100),
                AutoSize = true
            };
            
            // Total Borrowed Card
            var card1 = CreateSummaryCard("Total Borrowed", "₱0.00", Color.FromArgb(0, 120, 215));
            lblTotalBorrowed = card1.Controls[1] as Label;
            summaryPanel.Controls.Add(card1);
            
            // Total Repaid Card
            var card2 = CreateSummaryCard("Total Repaid", "₱0.00", Color.FromArgb(75, 181, 67));
            lblTotalRepaid = card2.Controls[1] as Label;
            summaryPanel.Controls.Add(card2);
            
            // Outstanding Card
            var card3 = CreateSummaryCard("Outstanding Balance", "₱0.00", Color.FromArgb(255, 152, 0));
            lblOutstanding = card3.Controls[1] as Label;
            summaryPanel.Controls.Add(card3);
            
            Controls.Add(summaryPanel);
            
            // Tab Control
            tabControl = new TabControl
            {
                Location = new Point(10, 150),
                Size = new Size(970, 500)
            };
            
            // My Loans Tab
            var tabLoans = new TabPage("My Loans");
            var loansPanel = new Panel { Dock = DockStyle.Fill };
            
            var btnApplyLoan = new Button
            {
                Text = "Apply for New Loan",
                Location = new Point(780, 10),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnApplyLoan.Click += BtnApplyLoan_Click;
            loansPanel.Controls.Add(btnApplyLoan);
            
            var btnTakeQuiz = new Button
            {
                Text = "Take Credit Quiz",
                Location = new Point(620, 10),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(75, 181, 67),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTakeQuiz.Click += BtnTakeQuiz_Click;
            loansPanel.Controls.Add(btnTakeQuiz);
            
            dgvMyLoans = new DataGridView
            {
                Location = new Point(10, 50),
                Size = new Size(920, 400),
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            loansPanel.Controls.Add(dgvMyLoans);
            tabLoans.Controls.Add(loansPanel);
            tabControl.TabPages.Add(tabLoans);
            
            // Repayments Tab
            var tabRepayments = new TabPage("Repayments");
            var repaymentsPanel = new Panel { Dock = DockStyle.Fill };
            
            var btnMakePayment = new Button
            {
                Text = "Make Payment",
                Location = new Point(780, 10),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnMakePayment.Click += BtnMakePayment_Click;
            repaymentsPanel.Controls.Add(btnMakePayment);
            
            dgvRepayments = new DataGridView
            {
                Location = new Point(10, 50),
                Size = new Size(920, 400),
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            repaymentsPanel.Controls.Add(dgvRepayments);
            tabRepayments.Controls.Add(repaymentsPanel);
            tabControl.TabPages.Add(tabRepayments);
            
            // Credit Score Tab
            var tabCreditScore = new TabPage("Credit Score");
            var creditPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                Padding = new Padding(20)
            };
            
            var creditInfoLabel = new Label
            {
                Text = "Your Credit Score",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                AutoSize = true
            };
            
            var creditDescLabel = new Label
            {
                Text = "Your credit score is calculated based on your financial behavior, repayment history, and credit quiz results.\n" +
                       "A higher score means better creditworthiness and may qualify you for larger loans with better terms.\n\n" +
                       "Take the credit quiz regularly to improve your score!",
                AutoSize = false,
                Size = new Size(800, 100),
                Font = new Font("Segoe UI", 11)
            };
            
            creditPanel.Controls.Add(creditInfoLabel);
            creditPanel.Controls.Add(new Label { Height = 20 });
            creditPanel.Controls.Add(creditDescLabel);
            
            tabCreditScore.Controls.Add(creditPanel);
            tabControl.TabPages.Add(tabCreditScore);
            
            Controls.Add(tabControl);
            
            Load += BorrowerDashboardForm_Load;
        }
        
        private Panel CreateSummaryCard(string title, string value, Color accentColor)
        {
            var card = new Panel
            {
                Size = new Size(300, 90),
                BackColor = Color.White,
                Padding = new Padding(15)
            };
            card.Paint += (s, e) => {
                using var p = new Pen(accentColor, 3);
                e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
            };
            
            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(102, 102, 102),
                Location = new Point(15, 10),
                AutoSize = true
            };
            
            var valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(15, 35),
                AutoSize = true
            };
            
            card.Controls.Add(titleLabel);
            card.Controls.Add(valueLabel);
            
            return card;
        }
        
        private async void BorrowerDashboardForm_Load(object? sender, EventArgs e)
        {
            await LoadMyLoansAsync();
            await LoadRepaymentsAsync();
            UpdateSummary();
        }
        
        private async Task LoadMyLoansAsync()
        {
            try
            {
                var ctx = new MicroLendDbContext();
                
                // Get borrower ID for this user
                var borrower = await Task.Run(() => ctx.Borrowers.FirstOrDefault(b => b.UserId == _userId));
                
                if (borrower == null)
                {
                    dgvMyLoans.DataSource = null;
                    return;
                }
                
                var loans = await Task.Run(() => ctx.Loans
                    .Where(l => l.BorrowerId == borrower.Id)
                    .Select(l => new
                    {
                        l.Id,
                        l.Purpose,
                        TargetAmount = l.TargetAmount,
                        CurrentAmount = l.CurrentAmount,
                        l.Status,
                        l.RiskScore,
                        l.InterestRate
                    })
                    .ToList());
                
                dgvMyLoans.DataSource = loans;
                
                if (dgvMyLoans.Columns.Count > 0)
                {
                    dgvMyLoans.Columns["Id"].HeaderText = "Loan ID";
                    dgvMyLoans.Columns["Purpose"].HeaderText = "Purpose";
                    dgvMyLoans.Columns["TargetAmount"].HeaderText = "Amount (₱)";
                    dgvMyLoans.Columns["CurrentAmount"].HeaderText = "Funded (₱)";
                    dgvMyLoans.Columns["Status"].HeaderText = "Status";
                    dgvMyLoans.Columns["RiskScore"].HeaderText = "Risk Score";
                    dgvMyLoans.Columns["InterestRate"].HeaderText = "Interest Rate (%)";
                    dgvMyLoans.Columns["TermMonths"].HeaderText = "Term (Months)";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading loans: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private async Task LoadRepaymentsAsync()
        {
            try
            {
                var ctx = new MicroLendDbContext();
                
                var borrower = await Task.Run(() => ctx.Borrowers.FirstOrDefault(b => b.UserId == _userId));
                
                if (borrower == null)
                {
                    dgvRepayments.DataSource = null;
                    return;
                }
                
                var loans = await Task.Run(() => ctx.Loans
                    .Where(l => l.BorrowerId == borrower.Id)
                    .Select(l => l.Id)
                    .ToList());
                
                var repayments = await Task.Run(() => ctx.Repayments
                    .Where(r => loans.Contains(r.LoanId))
                    .Select(r => new
                    {
                        r.Id,
                        r.LoanId,
                        r.Amount,
                        r.PaymentDate,
                        r.PaymentMethod,
                        r.PaymentReference
                    })
                    .ToList());
                
                dgvRepayments.DataSource = repayments;
                
                if (dgvRepayments.Columns.Count > 0)
                {
                    dgvRepayments.Columns["Id"].HeaderText = "Payment ID";
                    dgvRepayments.Columns["LoanId"].HeaderText = "Loan ID";
                    dgvRepayments.Columns["AmountPaid"].HeaderText = "Amount (₱)";
                    dgvRepayments.Columns["PaymentDate"].HeaderText = "Date";
                    dgvRepayments.Columns["PaymentMethod"].HeaderText = "Method";
                    dgvRepayments.Columns["PaymentReference"].HeaderText = "Reference";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading repayments: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void UpdateSummary()
        {
            try
            {
                var ctx = new MicroLendDbContext();
                
                var borrower = ctx.Borrowers.FirstOrDefault(b => b.UserId == _userId);
                
                if (borrower == null) return;
                
                var loans = ctx.Loans.Where(l => l.BorrowerId == borrower.Id).ToList();
                
                var totalBorrowed = loans.Sum(l => l.TargetAmount);
                var totalRepaid = ctx.Repayments
                    .Where(r => loans.Select(l => l.Id).Contains(r.LoanId))
                    .Sum(r => (decimal?)r.Amount) ?? 0;
                
                var outstanding = totalBorrowed - totalRepaid;
                
                lblTotalBorrowed.Text = $"₱{totalBorrowed:N2}";
                lblTotalRepaid.Text = $"₱{totalRepaid:N2}";
                lblOutstanding.Text = $"₱{outstanding:N2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating summary: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void BtnApplyLoan_Click(object? sender, EventArgs e)
        {
            using var loansForm = new LoansForm(_userId);
            if (loansForm.ShowDialog() == DialogResult.OK)
            {
                BorrowerDashboardForm_Load(null, EventArgs.Empty);
            }
        }
        
        private void BtnTakeQuiz_Click(object? sender, EventArgs e)
        {
            using var f = new CreditQuizForm(_userId);
            f.ShowDialog(this);
        }
        
        private void BtnMakePayment_Click(object? sender, EventArgs e)
        {
            if (dgvMyLoans.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a loan to make payment for.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var selectedRow = dgvMyLoans.SelectedRows[0];
            var loanId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            
            using var repayForm = new RepaymentMethodForm(loanId);
            if (repayForm.ShowDialog() == DialogResult.OK)
            {
                BorrowerDashboardForm_Load(null, EventArgs.Empty);
            }
        }
        
        private void OpenSettings()
        {
            using var settingsForm = new AccountSettingsForm(_userId);
            settingsForm.ShowDialog();
        }
        
        private void Logout()
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}
