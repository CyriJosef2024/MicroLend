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
        private FlowLayoutPanel _summaryPanel;
        private System.Collections.Generic.List<Panel> _summaryCards = new System.Collections.Generic.List<Panel>();
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
            // Role-based admin access: only show admin dashboard shortcut when the current user is an admin.
            try
            {
                var role = LookupUserRole(_userId);
                if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    fileMenu.DropDownItems.Add(new ToolStripSeparator());
                    fileMenu.DropDownItems.Add("Open Admin Dashboard", null, (s, e) => OpenAdminDashboard());
                }
            }
            catch
            {
                // If role lookup fails, do not expose the admin menu.
            }
            
            menuStrip.Items.Add(fileMenu);
            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);
        }
        
        private void CreateDashboard()
        {
            // Summary Cards Panel
            _summaryPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Top,
                AutoSize = true,
                // Allow cards to wrap onto the next row on narrow windows
                WrapContents = true,
                Padding = new Padding(10),
                Margin = new Padding(0)
            };
            
            // Total Borrowed Card
            var card1 = CreateSummaryCard("Total Borrowed", "₱0.00", Color.FromArgb(0, 120, 215));
            lblTotalBorrowed = card1.Controls[1] as Label;
            _summaryCards.Add(card1);
            _summaryPanel.Controls.Add(card1);
            
            // Total Repaid Card
            var card2 = CreateSummaryCard("Total Repaid", "₱0.00", Color.FromArgb(75, 181, 67));
            lblTotalRepaid = card2.Controls[1] as Label;
            _summaryCards.Add(card2);
            _summaryPanel.Controls.Add(card2);
            
            // Outstanding Card
            var card3 = CreateSummaryCard("Outstanding Balance", "₱0.00", Color.FromArgb(255, 152, 0));
            lblOutstanding = card3.Controls[1] as Label;
            _summaryCards.Add(card3);
            _summaryPanel.Controls.Add(card3);

            Controls.Add(_summaryPanel);

            // Handle resize to collapse summary cards into a single column on small widths
            this.Resize += BorrowerDashboardForm_Resize;
            UpdateSummaryPanelLayout();
            
            // Tab Control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };
            
            // My Loans Tab
            var tabLoans = new TabPage("My Loans");
            var loansPanel = new Panel { Dock = DockStyle.Fill };

            var loansButtonContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Padding = new Padding(10),
                WrapContents = false
            };

            var btnApplyLoan = new Button
            {
                Text = "Apply for New Loan",
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5, 0, 0, 0)
            };
            btnApplyLoan.Click += BtnApplyLoan_Click;
            loansButtonContainer.Controls.Add(btnApplyLoan);

            var btnTakeQuiz = new Button
            {
                Text = "Take Credit Quiz",
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(75, 181, 67),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5, 0, 0, 0)
            };
            btnTakeQuiz.Click += BtnTakeQuiz_Click;
            loansButtonContainer.Controls.Add(btnTakeQuiz);

            dgvMyLoans = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Margin = new Padding(10)
            };
            // Define explicit columns so headers are stable regardless of returned projection
            dgvMyLoans.Columns.Clear();
            dgvMyLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Loan ID", DataPropertyName = "Id", Width = 80 });
            dgvMyLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "Purpose", HeaderText = "Purpose", DataPropertyName = "Purpose" });
            dgvMyLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "TargetAmount", HeaderText = "Amount (₱)", DataPropertyName = "TargetAmount", DefaultCellStyle = { Format = "N2" } });
            dgvMyLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "CurrentAmount", HeaderText = "Funded (₱)", DataPropertyName = "CurrentAmount", DefaultCellStyle = { Format = "N2" } });
            dgvMyLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status" });
            dgvMyLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "RiskScore", HeaderText = "Risk Score", DataPropertyName = "RiskScore" });
            dgvMyLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "InterestRate", HeaderText = "Interest Rate (%)", DataPropertyName = "InterestRate", DefaultCellStyle = { Format = "N2" } });

            loansPanel.Controls.Add(dgvMyLoans);
            loansPanel.Controls.Add(loansButtonContainer);
            tabLoans.Controls.Add(loansPanel);
            tabControl.TabPages.Add(tabLoans);
            
            // Repayments Tab
            var tabRepayments = new TabPage("Repayments");
            var repaymentsPanel = new Panel { Dock = DockStyle.Fill };

            var repaymentsButtonContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Padding = new Padding(10),
                WrapContents = false
            };

            var btnMakePayment = new Button
            {
                Text = "Make Payment",
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5, 0, 0, 0)
            };
            btnMakePayment.Click += BtnMakePayment_Click;
            repaymentsButtonContainer.Controls.Add(btnMakePayment);

            dgvRepayments = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Margin = new Padding(10)
            };
            dgvRepayments.Columns.Clear();
            dgvRepayments.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Payment ID", DataPropertyName = "Id", Width = 80 });
            dgvRepayments.Columns.Add(new DataGridViewTextBoxColumn { Name = "LoanId", HeaderText = "Loan ID", DataPropertyName = "LoanId", Width = 80 });
            dgvRepayments.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "Amount (₱)", DataPropertyName = "Amount", DefaultCellStyle = { Format = "N2" } });
            dgvRepayments.Columns.Add(new DataGridViewTextBoxColumn { Name = "PaymentDate", HeaderText = "Date", DataPropertyName = "PaymentDate", DefaultCellStyle = { Format = "g" } });
            dgvRepayments.Columns.Add(new DataGridViewTextBoxColumn { Name = "PaymentMethod", HeaderText = "Method", DataPropertyName = "PaymentMethod" });
            dgvRepayments.Columns.Add(new DataGridViewTextBoxColumn { Name = "PaymentReference", HeaderText = "Reference", DataPropertyName = "PaymentReference" });
            repaymentsPanel.Controls.Add(dgvRepayments);
            repaymentsPanel.Controls.Add(repaymentsButtonContainer);
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
                
                // Bind to explicitly defined columns (DataPropertyName maps to projection fields)
                dgvMyLoans.DataSource = loans;
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
                // DataGridView has explicit columns; setting DataSource will map values by DataPropertyName
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
        
        private void OpenAdminDashboard()
        {
            // Enforce runtime role check before opening admin UI
            try
            {
                var role = LookupUserRole(_userId);
                if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Access denied. Admins only.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Unable to verify user role. Access denied.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var admin = new AdminDashboardForm();
            admin.ShowDialog(this);
        }

        private string LookupUserRole(int userId)
        {
            try
            {
                using var ctx = new MicroLend.DAL.MicroLendDbContext();
                var user = ctx.Users.FirstOrDefault(u => u.Id == userId);
                return user?.Role ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void BorrowerDashboardForm_Resize(object? sender, EventArgs e)
        {
            UpdateSummaryPanelLayout();
        }

        private void UpdateSummaryPanelLayout()
        {
            // If window narrow, stack cards vertically; otherwise, show horizontally with wrapping
            if (this.ClientSize.Width < 600)
            {
                _summaryPanel.FlowDirection = FlowDirection.TopDown;
                foreach (var c in _summaryCards) c.Width = _summaryPanel.ClientSize.Width - 20;
            }
            else
            {
                _summaryPanel.FlowDirection = FlowDirection.LeftToRight;
                foreach (var c in _summaryCards) c.Width = 300;
            }
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
