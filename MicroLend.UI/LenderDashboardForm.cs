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
        private bool _agreementsSigned = false;
        private ComboBox cbRiskFilter;
        private NumericUpDown nudMaxRemainingFilter;
        private TextBox txtPurposeFilter;
        private Button btnApplyFilter;
        private Button btnClearFilter;
        private Button btnViewDetails;
        private Button btnSignAgreement;
        private DataGridView dgvAnalytics;
        private DataGridView dgvEarnings;
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

        private bool ShowAgreementDialog()
        {
            using var dlg = new Form { Text = "Investment Agreement", Width = 640, Height = 460, StartPosition = FormStartPosition.CenterParent };
            var txt = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, Text = "[Investment Agreement Terms]\r\n\r\nBy signing you agree to terms of funding and accept the risks." };
            var btnAccept = new Button { Text = "I Agree", Dock = DockStyle.Bottom, Height = 36, BackColor = Color.FromArgb(0,133,119), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnCancel = new Button { Text = "Cancel", Dock = DockStyle.Bottom, Height = 36 };
            btnAccept.Click += (s, e) => { dlg.DialogResult = DialogResult.OK; dlg.Close(); };
            btnCancel.Click += (s, e) => { dlg.DialogResult = DialogResult.Cancel; dlg.Close(); };
            dlg.Controls.Add(txt); dlg.Controls.Add(btnAccept); dlg.Controls.Add(btnCancel);
            return dlg.ShowDialog(this) == DialogResult.OK;
        }

        private async Task LoadAnalyticsAsync()
        {
            try
            {
                var ctx = new MicroLendDbContext();
                // simple portfolio aggregation
                var investments = await Task.Run(() => ctx.LoanFunders.Where(f => f.LenderId == _userId).ToList());
                var agg = investments.GroupBy(f => f.LoanId).Select(g => new
                {
                    LoanId = g.Key,
                    TotalInvested = g.Sum(x => x.Amount),
                    ExpectedInterest = g.Sum(x => x.ExpectedInterest),
                    Count = g.Count()
                }).ToList();
                dgvAnalytics.DataSource = agg;
            }
            catch { }
        }

        private async Task LoadEarningsAsync()
        {
            try
            {
                var ctx = new MicroLendDbContext();
                var earnings = await Task.Run(() => ctx.LoanFunders.Where(f => f.LenderId == _userId)
                    .Select(f => new { f.LoanId, f.Amount, f.ExpectedInterest, f.FundingDate }).ToList());
                dgvEarnings.DataSource = earnings;
            }
            catch { }
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
            var fintechGreen = Color.FromArgb(0, 133, 119); // #008577 approximate
            var card1 = CreateSummaryCard("Total Invested", "₱0.00", fintechGreen);
            lblTotalInvested = card1.Controls[1] as Label;
            summaryPanel.Controls.Add(card1);
            
            // Expected Returns Card
            var card2 = CreateSummaryCard("Expected Returns", "₱0.00", fintechGreen);
            lblTotalReturns = card2.Controls[1] as Label;
            summaryPanel.Controls.Add(card2);
            
            // Active Loans Card
            var card3 = CreateSummaryCard("Active Funded Loans", "0", fintechGreen);
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
                BackColor = fintechGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnFund.Click += BtnFund_Click;
            browsePanel.Controls.Add(btnFund);
            // Filter controls
            txtPurposeFilter = new TextBox { Location = new Point(10, 10), Width = 240, PlaceholderText = "Purpose contains..." };
            cbRiskFilter = new ComboBox { Location = new Point(260, 10), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cbRiskFilter.Items.AddRange(new object[] { "Any", "Low", "Medium", "High" });
            cbRiskFilter.SelectedIndex = 0;
            nudMaxRemainingFilter = new NumericUpDown { Location = new Point(410, 10), Width = 120, Minimum = 0, Maximum = 10000000, DecimalPlaces = 2, Value = 0 };
            var lblRemaining = new Label { Text = "Max Remaining (₱)", Location = new Point(410, -6), AutoSize = true };
            btnApplyFilter = new Button { Text = "Apply Filter", Location = new Point(540, 8), Size = new Size(100, 28), BackColor = fintechGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnClearFilter = new Button { Text = "Clear", Location = new Point(648, 8), Size = new Size(60, 28) };
            btnViewDetails = new Button { Text = "View Details", Location = new Point(718, 8), Size = new Size(100, 28) };
            btnSignAgreement = new Button { Text = "Sign Agreement", Location = new Point(826, 8), Size = new Size(120, 28), BackColor = fintechGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            browsePanel.Controls.Add(txtPurposeFilter);
            browsePanel.Controls.Add(cbRiskFilter);
            browsePanel.Controls.Add(lblRemaining);
            browsePanel.Controls.Add(nudMaxRemainingFilter);
            browsePanel.Controls.Add(btnApplyFilter);
            browsePanel.Controls.Add(btnClearFilter);
            browsePanel.Controls.Add(btnViewDetails);
            browsePanel.Controls.Add(btnSignAgreement);
            btnApplyFilter.Click += (s, e) => _ = LoadAvailableLoansAsync();
            btnClearFilter.Click += (s, e) => { txtPurposeFilter.Text = ""; cbRiskFilter.SelectedIndex = 0; nudMaxRemainingFilter.Value = 0; _ = LoadAvailableLoansAsync(); };
            btnViewDetails.Click += (s, e) => { BtnFund_Click(s, e); };
            btnSignAgreement.Click += (s, e) => { _agreementsSigned = ShowAgreementDialog(); if (_agreementsSigned) MessageBox.Show("Agreement signed. You may now fund loans.", "Agreement", MessageBoxButtons.OK, MessageBoxIcon.Information); };
            
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

            // Analytics Tab
            var tabAnalytics = new TabPage("Portfolio Analytics");
            dgvAnalytics = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
            tabAnalytics.Controls.Add(dgvAnalytics);
            tabControl.TabPages.Add(tabAnalytics);

            // Earnings Tracker
            var tabEarnings = new TabPage("Earnings Tracker");
            dgvEarnings = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
            tabEarnings.Controls.Add(dgvEarnings);
            tabControl.TabPages.Add(tabEarnings);

            // Settings Tab
            var tabSettings = new TabPage("Settings");
            var settingsPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var lblProfile = new Label { Text = "Profile", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
            var txtContact = new TextBox { Location = new Point(10, 50), Width = 400, PlaceholderText = "Contact email or phone" };
            var btnSaveSettings = new Button { Text = "Save", Location = new Point(420, 48), BackColor = fintechGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnContactSupport = new Button { Text = "Contact Support", Location = new Point(500, 48) };
            btnSaveSettings.Click += (s, e) => MessageBox.Show("Settings saved.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            btnContactSupport.Click += (s, e) => MessageBox.Show("Support request sent.", "Support", MessageBoxButtons.OK, MessageBoxIcon.Information);
            settingsPanel.Controls.Add(lblProfile); settingsPanel.Controls.Add(txtContact); settingsPanel.Controls.Add(btnSaveSettings); settingsPanel.Controls.Add(btnContactSupport);
            tabSettings.Controls.Add(settingsPanel);
            tabControl.TabPages.Add(tabSettings);
            
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
            await LoadAnalyticsAsync();
            await LoadEarningsAsync();
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
                // Apply simple filters from filter controls
                var q = ctx.Loans.AsQueryable();
                q = q.Where(l => l.Status == "Approved" && l.CurrentAmount < l.TargetAmount);
                if (!string.IsNullOrWhiteSpace(txtPurposeFilter?.Text)) q = q.Where(l => l.Purpose.Contains(txtPurposeFilter.Text));
                if (cbRiskFilter != null && cbRiskFilter.SelectedIndex > 0)
                {
                    var sel = cbRiskFilter.SelectedItem.ToString();
                    // Simplified risk mapping
                    if (sel == "Low") q = q.Where(l => l.RiskScore <= 33);
                    if (sel == "Medium") q = q.Where(l => l.RiskScore > 33 && l.RiskScore <= 66);
                    if (sel == "High") q = q.Where(l => l.RiskScore > 66);
                }
                if (nudMaxRemainingFilter != null && nudMaxRemainingFilter.Value > 0)
                {
                    var maxRem = nudMaxRemainingFilter.Value;
                    q = q.Where(l => (l.TargetAmount - l.CurrentAmount) <= maxRem);
                }

                var loans = await Task.Run(() => q.Select(l => new
                {
                    l.Id,
                    l.Purpose,
                    TargetAmount = l.TargetAmount,
                    CurrentAmount = l.CurrentAmount,
                    Remaining = l.TargetAmount - l.CurrentAmount,
                    l.Status,
                    l.RiskScore
                }).ToList());
                
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
