using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MicroLend.DAL;
using MicroLend.DAL.Entities;

namespace MicroLend.UI
{
    public class LenderDashboardForm : Form
    {
        private readonly int _userId;
        private TabControl tabControl;
        private DataGridView dgvInvestments;
        private DataGridView dgvAvailableLoans;
        private DataGridView dgvMyFundedLoans;
        private Label lblTotalInvested;
        private Label lblTotalReturns;
        private Label lblActiveLoans;
        
        public LenderDashboardForm(int userId)
        {
            _userId = userId;
            Text = "Lender Dashboard - MicroLend";
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
            
            // Total Invested Card
            var card1 = CreateSummaryCard("Total Invested", "₱0.00", Color.FromArgb(0, 120, 215));
            lblTotalInvested = card1.Controls[1] as Label;
            summaryPanel.Controls.Add(card1);
            
            // Expected Returns Card
            var card2 = CreateSummaryCard("Expected Returns", "₱0.00", Color.FromArgb(75, 181, 67));
            lblTotalReturns = card2.Controls[1] as Label;
            summaryPanel.Controls.Add(card2);
            
            // Active Loans Card
            var card3 = CreateSummaryCard("Active Funded Loans", "0", Color.FromArgb(255, 152, 0));
            lblActiveLoans = card3.Controls[1] as Label;
            summaryPanel.Controls.Add(card3);
            
            Controls.Add(summaryPanel);
            
            // Tab Control
            tabControl = new TabControl
            {
                Location = new Point(10, 150),
                Size = new Size(970, 500)
            };
            
            // My Investments Tab
            var tabInvestments = new TabPage("My Investments");
            dgvInvestments = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            tabInvestments.Controls.Add(dgvInvestments);
            tabControl.TabPages.Add(tabInvestments);
            
            // Browse Loans Tab
            var tabBrowse = new TabPage("Browse Available Loans");
            var browsePanel = new Panel { Dock = DockStyle.Fill };
            
            var lblBrowse = new Label 
            { 
                Text = "Available loans to fund:", 
                Location = new Point(10, 10), 
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            browsePanel.Controls.Add(lblBrowse);
            
            var btnFund = new Button
            {
                Text = "Fund Selected Loan",
                Location = new Point(780, 5),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnFund.Click += BtnFund_Click;
            browsePanel.Controls.Add(btnFund);
            
            dgvAvailableLoans = new DataGridView
            {
                Location = new Point(10, 45),
                Size = new Size(920, 380),
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            browsePanel.Controls.Add(dgvAvailableLoans);
            tabBrowse.Controls.Add(browsePanel);
            tabControl.TabPages.Add(tabBrowse);
            
            // My Funded Loans Tab
            var tabFunded = new TabPage("My Funded Loans");
            dgvMyFundedLoans = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            tabFunded.Controls.Add(dgvMyFundedLoans);
            tabControl.TabPages.Add(tabFunded);
            
            Controls.Add(tabControl);
            
            Load += LenderDashboardForm_Load;
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
        
        private async void LenderDashboardForm_Load(object? sender, EventArgs e)
        {
            await LoadInvestmentsAsync();
            await LoadAvailableLoansAsync();
            await LoadFundedLoansAsync();
            UpdateSummary();
        }
        
        private async Task LoadInvestmentsAsync()
        {
            try
            {
                var ctx = new MicroLendDbContext();
                var investments = await Task.Run(() => ctx.LoanFunders
                    .Where(f => f.LenderId == _userId)
                    .Select(f => new 
                    { 
                        f.Id, 
                        LoanId = f.LoanId, 
                        Amount = f.Amount, 
                        ExpectedInterest = f.ExpectedInterest,
                        FundedDate = f.FundingDate
                    })
                    .ToList());
                
                dgvInvestments.DataSource = investments;
                
                if (dgvInvestments.Columns.Count > 0)
                {
                    dgvInvestments.Columns["Id"].HeaderText = "Investment ID";
                    dgvInvestments.Columns["LoanId"].HeaderText = "Loan ID";
                    dgvInvestments.Columns["Amount"].HeaderText = "Amount Funded (₱)";
                    dgvInvestments.Columns["ExpectedInterest"].HeaderText = "Expected Return (₱)";
                    dgvInvestments.Columns["FundedDate"].HeaderText = "Funded Date";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading investments: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private async Task LoadAvailableLoansAsync()
        {
            try
            {
                var ctx = new MicroLendDbContext();
                var loans = await Task.Run(() => ctx.Loans
                    .Where(l => l.Status == "Approved" && l.CurrentAmount < l.TargetAmount)
                    .Select(l => new
                    {
                        l.Id,
                        l.Purpose,
                        TargetAmount = l.TargetAmount,
                        CurrentAmount = l.CurrentAmount,
                        Remaining = l.TargetAmount - l.CurrentAmount,
                        l.Status,
                        l.RiskScore
                    })
                    .ToList());
                
                dgvAvailableLoans.DataSource = loans;
                
                if (dgvAvailableLoans.Columns.Count > 0)
                {
                    dgvAvailableLoans.Columns["Id"].HeaderText = "Loan ID";
                    dgvAvailableLoans.Columns["Purpose"].HeaderText = "Purpose";
                    dgvAvailableLoans.Columns["TargetAmount"].HeaderText = "Target (₱)";
                    dgvAvailableLoans.Columns["CurrentAmount"].HeaderText = "Funded (₱)";
                    dgvAvailableLoans.Columns["Remaining"].HeaderText = "Remaining (₱)";
                    dgvAvailableLoans.Columns["Status"].HeaderText = "Status";
                    dgvAvailableLoans.Columns["RiskScore"].HeaderText = "Risk Score";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading available loans: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private async Task LoadFundedLoansAsync()
        {
            try
            {
                var ctx = new MicroLendDbContext();
                
                // Get loan data separately since there's no navigation property
                var myFunders = await Task.Run(() => ctx.LoanFunders
                    .Where(f => f.LenderId == _userId)
                    .ToList());
                
                var myFundedLoans = myFunders.Select(f => new
                {
                    LoanId = f.LoanId,
                    f.Amount,
                    f.ExpectedInterest,
                    FundedDate = f.FundingDate,
                    LoanPurpose = "Loan #" + f.LoanId,
                    LoanStatus = "Active"
                }).ToList();
                
                dgvMyFundedLoans.DataSource = myFundedLoans;
                
                if (dgvMyFundedLoans.Columns.Count > 0)
                {
                    dgvMyFundedLoans.Columns["LoanId"].HeaderText = "Loan ID";
                    dgvMyFundedLoans.Columns["Amount"].HeaderText = "Funded Amount (₱)";
                    dgvMyFundedLoans.Columns["ExpectedInterest"].HeaderText = "Expected Return (₱)";
                    dgvMyFundedLoans.Columns["FundedDate"].HeaderText = "Funded Date";
                    dgvMyFundedLoans.Columns["LoanPurpose"].HeaderText = "Loan Purpose";
                    dgvMyFundedLoans.Columns["LoanStatus"].HeaderText = "Loan Status";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading funded loans: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void UpdateSummary()
        {
            try
            {
                var ctx = new MicroLendDbContext();
                
                var totalInvested = ctx.LoanFunders
                    .Where(f => f.LenderId == _userId)
                    .Sum(f => (decimal?)f.Amount) ?? 0;
                
                var totalReturns = ctx.LoanFunders
                    .Where(f => f.LenderId == _userId)
                    .Sum(f => (decimal?)f.ExpectedInterest) ?? 0;
                
                var activeLoans = ctx.LoanFunders
                    .Where(f => f.LenderId == _userId)
                    .Select(f => f.LoanId)
                    .Distinct()
                    .Count();
                
                lblTotalInvested.Text = $"₱{totalInvested:N2}";
                lblTotalReturns.Text = $"₱{totalReturns:N2}";
                lblActiveLoans.Text = activeLoans.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating summary: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void BtnFund_Click(object? sender, EventArgs e)
        {
            if (dgvAvailableLoans.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a loan to fund.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var selectedRow = dgvAvailableLoans.SelectedRows[0];
            var loanId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            var remaining = Convert.ToDecimal(selectedRow.Cells["Remaining"].Value);
            
            using var fundForm = new FundLoanForm(_userId, loanId, remaining);
            if (fundForm.ShowDialog() == DialogResult.OK)
            {
                // Refresh the data
                LenderDashboardForm_Load(null, EventArgs.Empty);
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
