using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MicroLend.DAL;

namespace MicroLend.UI
{
    public class AdminDashboardForm : Form
    {
        private DataGridView dgv;
        private Label lblTotalUsersValue;
        private Label lblTotalBorrowersValue;
        private Label lblTotalLendersValue;
        private Label lblTotalLoansValue;
        
        // Filter controls
        private ComboBox cmbFilterType;
        private TextBox txtFilter;
        private Button btnFilter;
        private Button btnClearFilter;
        
        // Tab controls
        private TabControl tabControl;
        
        public AdminDashboardForm()
        {
            Text = "Admin Dashboard - MicroLend";
            Width = 1200;
            Height = 800;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(240, 248, 255);

            CreateMenuBar();
            
            // Create TabControl for different views
            tabControl = new TabControl
            {
                Location = new Point(10, 70),
                Size = new Size(1160, 680)
            };
            
            // Users Tab
            var tabUsers = new TabPage("Users");
            tabUsers.Controls.Add(CreateUsersPanel());
            tabControl.TabPages.Add(tabUsers);
            
            // Borrowers Tab
            var tabBorrowers = new TabPage("Borrowers");
            tabBorrowers.Controls.Add(CreateBorrowersPanel());
            tabControl.TabPages.Add(tabBorrowers);
            
            // Lenders Tab
            var tabLenders = new TabPage("Lenders");
            tabLenders.Controls.Add(CreateLendersPanel());
            tabControl.TabPages.Add(tabLenders);
            
            // Loans Tab
            var tabLoans = new TabPage("Loans");
            tabLoans.Controls.Add(CreateLoansPanel());
            tabControl.TabPages.Add(tabLoans);
            
            // Credit Scores Tab
            var tabCreditScores = new TabPage("Credit Scores");
            tabCreditScores.Controls.Add(CreateCreditScoresPanel());
            tabControl.TabPages.Add(tabCreditScores);
            
            // Contact Support Tab
            var tabSupport = new TabPage("Contact Support");
            tabSupport.Controls.Add(CreateSupportPanel());
            tabControl.TabPages.Add(tabSupport);
            
            Controls.Add(tabControl);
            
            Load += (s, e) => LoadData();
        }
        
        private Panel CreateUsersPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            
            // Summary cards
            var summaryPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Location = new Point(10, 10),
                Size = new Size(1100, 80),
                AutoSize = true
            };
            
            var card1 = CreateSummaryCard("Total Users", "0", Color.FromArgb(0, 120, 215), out lblTotalUsersValue);
            var card2 = CreateSummaryCard("Total Borrowers", "0", Color.FromArgb(0, 150, 136), out lblTotalBorrowersValue);
            var card3 = CreateSummaryCard("Total Lenders", "0", Color.FromArgb(255, 152, 0), out lblTotalLendersValue);
            var card4 = CreateSummaryCard("Total Loans", "0", Color.FromArgb(75, 181, 67), out lblTotalLoansValue);
            
            summaryPanel.Controls.Add(card1);
            summaryPanel.Controls.Add(card2);
            summaryPanel.Controls.Add(card3);
            summaryPanel.Controls.Add(card4);
            panel.Controls.Add(summaryPanel);
            
            // Filter section
            var filterPanel = new Panel
            {
                Location = new Point(10, 100),
                Size = new Size(1100, 45),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            
            var lblFilter = new Label { Text = "Filter:", Location = new Point(10, 12), AutoSize = true };
            filterPanel.Controls.Add(lblFilter);
            
            cmbFilterType = new ComboBox { Location = new Point(60, 8), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilterType.Items.AddRange(new[] { "All", "Admin", "Borrower", "Lender" });
            cmbFilterType.SelectedIndex = 0;
            filterPanel.Controls.Add(cmbFilterType);
            
            txtFilter = new TextBox { Location = new Point(220, 8), Width = 300, PlaceholderText = "Search by username..." };
            filterPanel.Controls.Add(txtFilter);
            
            btnFilter = new Button { Text = "Apply Filter", Location = new Point(530, 6), Size = new Size(100, 28), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnFilter.Click += (s, e) => FilterUsers();
            filterPanel.Controls.Add(btnFilter);
            
            btnClearFilter = new Button { Text = "Clear", Location = new Point(640, 6), Size = new Size(80, 28) };
            btnClearFilter.Click += (s, e) => { txtFilter.Text = ""; cmbFilterType.SelectedIndex = 0; LoadUsers(); };
            filterPanel.Controls.Add(btnClearFilter);
            
            panel.Controls.Add(filterPanel);
            
            // Management toolbar
            var toolPanel = new Panel
            {
                Location = new Point(10, 155),
                Size = new Size(1100, 38),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            var btnAdd = CreateStyledButton("Add User", Color.FromArgb(0, 120, 215), new Point(10, 4));
            btnAdd.Click += (s, e) => AddUser();
            toolPanel.Controls.Add(btnAdd);
            
            var btnEdit = CreateStyledButton("Edit User", Color.FromArgb(255, 152, 0), new Point(120, 4));
            btnEdit.Click += (s, e) => EditUser();
            toolPanel.Controls.Add(btnEdit);
            
            var btnDelete = CreateStyledButton("Delete User", Color.FromArgb(220, 53, 69), new Point(230, 4));
            btnDelete.Click += (s, e) => DeleteUser();
            toolPanel.Controls.Add(btnDelete);
            
            var btnExport = CreateStyledButton("Export Report", Color.FromArgb(40, 167, 69), new Point(340, 4));
            btnExport.Click += (s, e) => ExportReport();
            toolPanel.Controls.Add(btnExport);
            
            var btnRefresh = CreateStyledButton("Refresh", Color.FromArgb(108, 117, 125), new Point(460, 4));
            btnRefresh.Click += (s, e) => LoadUsers();
            toolPanel.Controls.Add(btnRefresh);
            
            var btnCleanup = CreateStyledButton("Batch Delete", Color.FromArgb(220, 53, 69), new Point(550, 4));
            btnCleanup.Click += (s, e) => BatchDeleteAccounts();
            toolPanel.Controls.Add(btnCleanup);
            
            panel.Controls.Add(toolPanel);
            
            dgv = new DataGridView
            {
                Location = new Point(10, 200),
                Size = new Size(1100, 440),
                ReadOnly = true,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };
            panel.Controls.Add(dgv);
            
            return panel;
        }
        
        private Panel CreateBorrowersPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            
            // Filter
            var filterPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(1100, 45),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            
            var txtSearch = new TextBox { Location = new Point(10, 8), Width = 300, PlaceholderText = "Search by name or contact..." };
            filterPanel.Controls.Add(txtSearch);
            
            var btnSearch = CreateStyledButton("Search", Color.FromArgb(0, 120, 215), new Point(320, 6));
            btnSearch.Click += (s, e) => {
                using var ctx = new MicroLendDbContext();
                var query = ctx.Borrowers.AsQueryable();
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    var search = txtSearch.Text.ToLower();
                    query = query.Where(b => b.Name.ToLower().Contains(search) || 
                                           (b.ContactNumber != null && b.ContactNumber.ToLower().Contains(search)));
                }
                var borrowers = query.Select(b => new { 
                    b.Id, b.Name, b.ContactNumber, b.MonthlyIncome, b.BusinessType, b.IsVerified 
                }).ToList();
                var dgvB = panel.Controls.OfType<DataGridView>().FirstOrDefault();
                if (dgvB != null) dgvB.DataSource = borrowers;
            };
            filterPanel.Controls.Add(btnSearch);
            
            var btnClear = CreateStyledButton("Clear", Color.FromArgb(108, 117, 125), new Point(420, 6));
            btnClear.Click += (s, e) => {
                txtSearch.Text = "";
                LoadBorrowers(panel);
            };
            filterPanel.Controls.Add(btnClear);
            
            panel.Controls.Add(filterPanel);
            
            var dgvB = new DataGridView
            {
                Location = new Point(10, 60),
                Size = new Size(1100, 570),
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            panel.Controls.Add(dgvB);
            
            Load += (s, e) => LoadBorrowers(panel);
            
            return panel;
        }
        
        private Panel CreateLendersPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            
            var filterPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(1100, 45),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            
            var txtSearch = new TextBox { Location = new Point(10, 8), Width = 300, PlaceholderText = "Search by username..." };
            filterPanel.Controls.Add(txtSearch);
            
            var btnSearch = CreateStyledButton("Search", Color.FromArgb(0, 120, 215), new Point(320, 6));
            btnSearch.Click += (s, e) => {
                using var ctx = new MicroLendDbContext();
                var query = ctx.Users.Where(u => u.Role != null && u.Role.ToLower() == "lender").AsQueryable();
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    var search = txtSearch.Text.ToLower();
                    query = query.Where(u => u.Username.ToLower().Contains(search));
                }
                var lenders = query.Select(u => new { 
                    u.Id, u.Username, u.InitialCreditScore, u.CreatedAt 
                }).ToList();
                var dgvL = panel.Controls.OfType<DataGridView>().FirstOrDefault();
                if (dgvL != null) dgvL.DataSource = lenders;
            };
            filterPanel.Controls.Add(btnSearch);
            
            panel.Controls.Add(filterPanel);
            
            var dgvL = new DataGridView
            {
                Location = new Point(10, 60),
                Size = new Size(1100, 570),
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            panel.Controls.Add(dgvL);
            
            Load += (s, e) => LoadLenders(panel);
            
            return panel;
        }
        
        private Panel CreateLoansPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            
            var filterPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(1100, 45),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            
            var lblStatus = new Label { Text = "Status:", Location = new Point(10, 12), AutoSize = true };
            filterPanel.Controls.Add(lblStatus);
            
            var cmbStatus = new ComboBox { Location = new Point(70, 8), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new[] { "All", "Pending", "Approved", "Verified", "Funding", "Active", "Rejected", "FullyRepaid" });
            cmbStatus.SelectedIndex = 0;
            filterPanel.Controls.Add(cmbStatus);
            
            var txtSearch = new TextBox { Location = new Point(230, 8), Width = 250, PlaceholderText = "Search by purpose..." };
            filterPanel.Controls.Add(txtSearch);
            
            var btnSearch = CreateStyledButton("Filter", Color.FromArgb(0, 120, 215), new Point(490, 6));
            btnSearch.Click += (s, e) => {
                using var ctx = new MicroLendDbContext();
                var query = ctx.Loans.AsQueryable();
                
                if (cmbStatus.SelectedIndex > 0)
                {
                    var status = cmbStatus.Items[cmbStatus.SelectedIndex].ToString();
                    query = query.Where(l => l.Status == status);
                }
                
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    var search = txtSearch.Text.ToLower();
                    query = query.Where(l => l.Purpose.ToLower().Contains(search));
                }
                
                var loans = query.Select(l => new { 
                    l.Id, l.BorrowerId, l.Purpose, l.TargetAmount, l.CurrentAmount, l.Status, l.RiskScore, l.CreatedAt 
                }).ToList();
                var dgvLoans = panel.Controls.OfType<DataGridView>().FirstOrDefault();
                if (dgvLoans != null) dgvLoans.DataSource = loans;
            };
            filterPanel.Controls.Add(btnSearch);
            
            var btnClear = CreateStyledButton("Clear", Color.FromArgb(108, 117, 125), new Point(590, 6));
            btnClear.Click += (s, e) => {
                cmbStatus.SelectedIndex = 0;
                txtSearch.Text = "";
                LoadLoans(panel);
            };
            filterPanel.Controls.Add(btnClear);
            
            var btnRefreshLoans = CreateStyledButton("Refresh", Color.FromArgb(40, 167, 69), new Point(680, 6));
            btnRefreshLoans.Click += (s, e) => LoadLoans(panel);
            filterPanel.Controls.Add(btnRefreshLoans);
            
            panel.Controls.Add(filterPanel);
            
            // Create DataGridView with button columns for approval actions
            var dgvLoans = new DataGridView
            {
                Location = new Point(10, 60),
                Size = new Size(1100, 500),
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            
            // Add action buttons column
            var colApprove = new DataGridViewButtonColumn
            {
                Name = "Approve",
                HeaderText = "Approve",
                Text = "Approve",
                UseColumnTextForButtonValue = true,
                Width = 70
            };
            dgvLoans.Columns.Add(colApprove);
            
            var colDecline = new DataGridViewButtonColumn
            {
                Name = "Decline",
                HeaderText = "Decline",
                Text = "Decline",
                UseColumnTextForButtonValue = true,
                Width = 70
            };
            dgvLoans.Columns.Add(colDecline);
            
            var colVerify = new DataGridViewButtonColumn
            {
                Name = "Verify",
                HeaderText = "Verify",
                Text = "Verify",
                UseColumnTextForButtonValue = true,
                Width = 70
            };
            dgvLoans.Columns.Add(colVerify);
            
            var colPending = new DataGridViewButtonColumn
            {
                Name = "Pending",
                HeaderText = "Pending",
                Text = "Set Pending",
                UseColumnTextForButtonValue = true,
                Width = 85
            };
            dgvLoans.Columns.Add(colPending);
            
            dgvLoans.CellClick += (s, e) => {
                if (e.RowIndex < 0) return;
                var row = dgvLoans.Rows[e.RowIndex];
                var loanId = Convert.ToInt32(row.Cells["Id"].Value);
                
                if (dgvLoans.Columns[e.ColumnIndex].Name == "Approve")
                {
                    ApproveLoan(loanId, panel);
                }
                else if (dgvLoans.Columns[e.ColumnIndex].Name == "Decline")
                {
                    DeclineLoan(loanId, panel);
                }
                else if (dgvLoans.Columns[e.ColumnIndex].Name == "Pending")
                {
                    SetLoanPending(loanId, panel);
                }
                else if (dgvLoans.Columns[e.ColumnIndex].Name == "Verify")
                {
                    VerifyLoan(loanId, panel);
                }
            };
            
            panel.Controls.Add(dgvLoans);
            
            // Legend panel
            var legendPanel = new Panel
            {
                Location = new Point(10, 565),
                Size = new Size(1100, 50),
                BackColor = Color.FromArgb(245, 247, 250)
            };
            
            var legendLabel = new Label
            {
                Text = "📋 Loan Ticketing System: Click 'Approve' to approve a loan application or 'Decline' to reject it. Approved loans move to Active status for funding.",
                Location = new Point(10, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(100, 100, 100),
                Font = new Font("Segoe UI", 9F)
            };
            legendPanel.Controls.Add(legendLabel);
            panel.Controls.Add(legendPanel);
            
            Load += (s, e) => LoadLoans(panel);
            
            return panel;
        }
        
        private void ApproveLoan(int loanId, Panel panel)
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                var loan = ctx.Loans.Find(loanId);
                if (loan == null)
                {
                    MessageBox.Show("Loan not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (loan.Status == "Approved")
                {
                    MessageBox.Show("This loan is already approved.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                if (loan.Status == "Rejected")
                {
                    MessageBox.Show("This loan was already declined. Reset to Pending first to approve.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Approve the loan - change status to Approved
                loan.Status = "Approved";
                ctx.SaveChanges();
                
                MessageBox.Show($"Loan #{loanId} has been approved!\n\nThe loan is now active and ready for funding.", 
                    "Loan Approved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Refresh the grid
                LoadLoans(panel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error approving loan: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void DeclineLoan(int loanId, Panel panel)
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                var loan = ctx.Loans.Find(loanId);
                if (loan == null)
                {
                    MessageBox.Show("Loan not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (loan.Status == "Rejected")
                {
                    MessageBox.Show("This loan is already declined.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                // Show input dialog for decline reason
                var reasonForm = new Form
                {
                    Width = 400,
                    Height = 180,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = "Decline Loan Application",
                    StartPosition = FormStartPosition.CenterParent,
                    MaximizeBox = false
                };
                
                var lblReason = new Label { Text = "Reason for declining (optional):", Location = new Point(20, 20), AutoSize = true };
                var txtReason = new TextBox { Location = new Point(20, 45), Width = 340, Height = 60, Multiline = true };
                
                var btnConfirm = new Button { Text = "Decline Loan", Location = new Point(200, 110), Size = new Size(160, 35) };
                var btnCancel = new Button { Text = "Cancel", Location = new Point(20, 110), Size = new Size(80, 35) };
                
                btnConfirm.BackColor = Color.FromArgb(200, 50, 50);
                btnConfirm.ForeColor = Color.White;
                btnConfirm.FlatStyle = FlatStyle.Flat;
                
                btnCancel.FlatStyle = FlatStyle.Flat;
                
                reasonForm.Controls.Add(lblReason);
                reasonForm.Controls.Add(txtReason);
                reasonForm.Controls.Add(btnConfirm);
                reasonForm.Controls.Add(btnCancel);
                
                btnConfirm.Click += (s, e) => {
                    // Decline the loan
                    loan.Status = "Rejected";
                    ctx.SaveChanges();
                    
                    MessageBox.Show($"Loan #{loanId} has been declined.\n\nReason: {(string.IsNullOrWhiteSpace(txtReason.Text) ? "Not provided" : txtReason.Text)}", 
                        "Loan Declined", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    reasonForm.Close();
                    LoadLoans(panel);
                };
                
                btnCancel.Click += (s, e) => reasonForm.Close();
                
                reasonForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error declining loan: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void SetLoanPending(int loanId, Panel panel)
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                var loan = ctx.Loans.Find(loanId);
                if (loan == null)
                {
                    MessageBox.Show("Loan not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // Set loan to pending status
                loan.Status = "Pending";
                ctx.SaveChanges();
                
                MessageBox.Show($"Loan #{loanId} has been set to Pending.\n\nThe loan application is now awaiting review.", 
                    "Loan Pending", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Refresh the grid
                LoadLoans(panel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting loan to pending: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void VerifyLoan(int loanId, Panel panel)
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                var loan = ctx.Loans.Find(loanId);
                if (loan == null)
                {
                    MessageBox.Show("Loan not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (loan.Status != "Approved")
                {
                    MessageBox.Show("Only approved loans can be verified.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Verify the loan - mark as verified
                loan.Status = "Verified";
                ctx.SaveChanges();
                
                MessageBox.Show($"Loan #{loanId} has been verified!\n\nThe loan is now confirmed and active for funding.", 
                    "Loan Verified", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Refresh the grid
                LoadLoans(panel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error verifying loan: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private Panel CreateCreditScoresPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            
            var lblInfo = new Label
            {
                Text = "Credit Scores Overview - All borrowers with their credit scores and quiz results",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            panel.Controls.Add(lblInfo);
            
            var dgvScores = new DataGridView
            {
                Location = new Point(10, 40),
                Size = new Size(1100, 590),
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            panel.Controls.Add(dgvScores);
            
            Load += (s, e) => {
                try
                {
                    using var ctx = new MicroLendDbContext();
                    var scores = ctx.CreditScores
                        .Join(ctx.Borrowers, cs => cs.UserId, b => b.UserId, (cs, b) => new { cs, b })
                        .Select(x => new { 
                            UserId = x.cs.UserId,
                            BorrowerName = x.b.Name,
                            x.cs.Score,
                            QuizDate = x.cs.QuizDate
                        })
                        .ToList();
                    
                    // Calculate risk level from score
                    var scoresWithRisk = scores.Select(s => new {
                        s.UserId,
                        s.BorrowerName,
                        s.Score,
                        RiskLevel = s.Score >= 70 ? "Low" : (s.Score >= 40 ? "Medium" : "High"),
                        s.QuizDate
                    }).ToList();
                    
                    dgvScores.DataSource = scoresWithRisk;
                    if (dgvScores.Columns.Count > 0)
                    {
                        dgvScores.Columns["UserId"].HeaderText = "User ID";
                        dgvScores.Columns["BorrowerName"].HeaderText = "Borrower Name";
                        dgvScores.Columns["Score"].HeaderText = "Credit Score";
                        dgvScores.Columns["RiskLevel"].HeaderText = "Risk Level";
                        dgvScores.Columns["QuizDate"].HeaderText = "Quiz Date";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading credit scores: " + ex.Message);
                }
            };
            
            return panel;
        }
        
        private Panel CreateSupportPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            
            var lblInfo = new Label
            {
                Text = "Contact Support - View support requests from borrowers and lenders",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            panel.Controls.Add(lblInfo);
            
            var btnRefresh = CreateStyledButton("Refresh", Color.FromArgb(0, 120, 215), new Point(10, 40));
            btnRefresh.Click += (s, e) => LoadSupportMessages(panel);
            panel.Controls.Add(btnRefresh);
            
            var dgvSupport = new DataGridView
            {
                Location = new Point(10, 75),
                Size = new Size(1100, 555),
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            panel.Controls.Add(dgvSupport);
            
            Load += (s, e) => LoadSupportMessages(panel);
            
            return panel;
        }
        
        private void LoadSupportMessages(Panel panel)
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                
                // Since we don't have a support tickets table, we'll show recent users who might need support
                // In a real app, you'd have a SupportTicket entity
                var recentActivity = ctx.Users
                    .OrderByDescending(u => u.UpdatedAt)
                    .Take(50)
                    .Select(u => new { 
                        u.Id, 
                        u.Username, 
                        u.Role, 
                        u.CreatedAt,
                        u.UpdatedAt,
                        Note = "User activity - check individual user for details"
                    })
                    .ToList();
                
                var dgv = panel.Controls.OfType<DataGridView>().FirstOrDefault();
                if (dgv != null)
                {
                    dgv.DataSource = recentActivity;
                    if (dgv.Columns.Count > 0)
                    {
                        dgv.Columns["Id"].HeaderText = "User ID";
                        dgv.Columns["Username"].HeaderText = "Username";
                        dgv.Columns["Role"].HeaderText = "Role";
                        dgv.Columns["CreatedAt"].HeaderText = "Created";
                        dgv.Columns["UpdatedAt"].HeaderText = "Last Activity";
                        dgv.Columns["Note"].HeaderText = "Notes";
                    }
                }
                
                MessageBox.Show("Note: Support ticket system is not yet implemented. Showing recent user activity for reference.", 
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading support data: " + ex.Message);
            }
        }
        
        private Button CreateStyledButton(string text, Color backColor, Point location)
        {
            return new Button
            {
                Text = text,
                Location = location,
                Size = new Size(100, 28),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
        }
        
        private void LoadBorrowers(Panel panel)
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                // Filter out auto-created borrowers (Auto Borrower X) that have no real names
                var borrowers = ctx.Borrowers
                    .Where(b => b.Name != null && !b.Name.StartsWith("Auto Borrower"))
                    .Select(b => new { 
                        b.Id, b.Name, b.ContactNumber, b.MonthlyIncome, b.BusinessType, b.IsVerified 
                    }).ToList();
                var dgv = panel.Controls.OfType<DataGridView>().FirstOrDefault();
                if (dgv != null)
                {
                    dgv.DataSource = borrowers;
                    if (dgv.Columns.Count > 0)
                    {
                        dgv.Columns["Id"].HeaderText = "ID";
                        dgv.Columns["Name"].HeaderText = "Full Name";
                        dgv.Columns["ContactNumber"].HeaderText = "Contact";
                        dgv.Columns["MonthlyIncome"].HeaderText = "Monthly Income";
                        dgv.Columns["BusinessType"].HeaderText = "Business Type";
                        dgv.Columns["IsVerified"].HeaderText = "Verified";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading borrowers: " + ex.Message);
            }
        }
        
        private void LoadLenders(Panel panel)
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                var lenders = ctx.Users
                    .Where(u => u.Role != null && u.Role.ToLower() == "lender")
                    .Select(u => new { u.Id, u.Username, u.InitialCreditScore, u.CreatedAt })
                    .ToList();
                var dgv = panel.Controls.OfType<DataGridView>().FirstOrDefault();
                if (dgv != null)
                {
                    dgv.DataSource = lenders;
                    if (dgv.Columns.Count > 0)
                    {
                        dgv.Columns["Id"].HeaderText = "ID";
                        dgv.Columns["Username"].HeaderText = "Username";
                        dgv.Columns["InitialCreditScore"].HeaderText = "Credit Score";
                        dgv.Columns["CreatedAt"].HeaderText = "Joined";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading lenders: " + ex.Message);
            }
        }
        
        private void LoadLoans(Panel panel)
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                var loans = ctx.Loans.Select(l => new { 
                    l.Id, l.BorrowerId, l.Purpose, l.TargetAmount, l.CurrentAmount, l.Status, l.RiskScore, l.CreatedAt 
                }).ToList();
                var dgv = panel.Controls.OfType<DataGridView>().FirstOrDefault();
                if (dgv != null)
                {
                    dgv.DataSource = loans;
                    if (dgv.Columns.Count > 0)
                    {
                        dgv.Columns["Id"].HeaderText = "Loan ID";
                        dgv.Columns["BorrowerId"].HeaderText = "Borrower ID";
                        dgv.Columns["Purpose"].HeaderText = "Purpose";
                        dgv.Columns["TargetAmount"].HeaderText = "Target Amount";
                        dgv.Columns["CurrentAmount"].HeaderText = "Funded";
                        dgv.Columns["Status"].HeaderText = "Status";
                        dgv.Columns["RiskScore"].HeaderText = "Risk Score";
                        dgv.Columns["CreatedAt"].HeaderText = "Created";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading loans: " + ex.Message);
            }
        }
        
        private Panel CreateSummaryCard(string title, string value, Color accentColor, out Label valueLabelOut)
        {
            var card = new Panel
            {
                Size = new Size(260, 70),
                BackColor = Color.White,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(102, 102, 102),
                Location = new Point(10, 8),
                AutoSize = true
            };
            
            var valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(10, 28),
                AutoSize = true
            };
            
            card.Controls.Add(titleLabel);
            card.Controls.Add(valueLabel);

            valueLabelOut = valueLabel;
            
            return card;
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
        
        private void LoadData()
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                
                lblTotalUsersValue.Text = ctx.Users.Count().ToString();
                lblTotalBorrowersValue.Text = ctx.Borrowers.Count().ToString();
                lblTotalLendersValue.Text = ctx.Users.Count(u => u.Role != null && u.Role.ToLower() == "lender").ToString();
                lblTotalLoansValue.Text = ctx.Loans.Count().ToString();
                
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading admin dashboard: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void FilterUsers()
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                var query = ctx.Users.AsQueryable();
                
                // Filter by role
                if (cmbFilterType.SelectedIndex > 0)
                {
                    var role = cmbFilterType.Items[cmbFilterType.SelectedIndex].ToString();
                    query = query.Where(u => u.Role == role);
                }
                
                // Filter by username search
                if (!string.IsNullOrWhiteSpace(txtFilter.Text))
                {
                    var search = txtFilter.Text.ToLower();
                    query = query.Where(u => u.Username.ToLower().Contains(search));
                }
                
                var users = query.Select(u => new { 
                    u.Id, 
                    u.Username, 
                    u.Role, 
                    u.InitialCreditScore,
                    u.CreatedAt
                }).ToList();
                
                dgv.DataSource = users;
                
                if (dgv.Columns.Count > 0)
                {
                    dgv.Columns["Id"].HeaderText = "ID";
                    dgv.Columns["Username"].HeaderText = "Username";
                    dgv.Columns["Role"].HeaderText = "Role";
                    dgv.Columns["InitialCreditScore"].HeaderText = "Credit Score";
                    dgv.Columns["CreatedAt"].HeaderText = "Created";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error filtering users: " + ex.Message);
            }
        }
        
        private void LoadUsers()
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                
                // Filter out auto-created users (username like "user" + number pattern that were auto-generated)
                var users = ctx.Users
                    .Where(u => !Regex.IsMatch(u.Username ?? "", @"^user\d+$"))
                    .Select(u => new { 
                        u.Id, 
                        u.Username, 
                        u.Role, 
                        u.InitialCreditScore,
                        u.CreatedAt
                    }).ToList();
                
                dgv.DataSource = users;
                
                if (dgv.Columns.Count > 0)
                {
                    dgv.Columns["Id"].HeaderText = "ID";
                    dgv.Columns["Username"].HeaderText = "Username";
                    dgv.Columns["Role"].HeaderText = "Role";
                    dgv.Columns["InitialCreditScore"].HeaderText = "Credit Score";
                    dgv.Columns["CreatedAt"].HeaderText = "Created";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading users: " + ex.Message);
            }
        }
        
        private void OpenSettings()
        {
            using var dlg = new Form { Text = "System Settings", Width = 600, Height = 400, StartPosition = FormStartPosition.CenterParent };
            var lbl = new Label { Text = "System configuration and logs", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            dlg.Controls.Add(lbl);
            dlg.ShowDialog();
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
        
        private void BatchDeleteAccounts()
        {
            // Create input dialog for batch deletion criteria
            using var inputForm = new Form { Text = "Batch Delete Accounts", Width = 400, Height = 220, StartPosition = FormStartPosition.CenterParent };
            inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputForm.MaximizeBox = false;
            inputForm.MinimizeBox = false;
            
            var lblCreditScore = new Label { Text = "Credit Score Threshold (0-100):", Location = new Point(20, 20), Width = 200 };
            var txtCreditScore = new TextBox { Location = new Point(20, 45), Width = 150, Text = "0" };
            var lblCreditHint = new Label { Text = "Delete accounts with score <= this value", Location = new Point(180, 45), Width = 180, ForeColor = Color.Gray };
            
            var lblLoanAmount = new Label { Text = "Loan Amount Threshold (PHP):", Location = new Point(20, 80), Width = 200 };
            var txtLoanAmount = new TextBox { Location = new Point(20, 105), Width = 150, Text = "0" };
            var lblLoanHint = new Label { Text = "Delete accounts with total loans >= this amount", Location = new Point(180, 105), Width = 180, ForeColor = Color.Gray };
            
            var btnOK = new Button { Text = "Delete", Location = new Point(200, 150), Width = 80 };
            var btnCancel = new Button { Text = "Cancel", Location = new Point(290, 150), Width = 80 };
            
            btnOK.Click += (s, e) => {
                if (!int.TryParse(txtCreditScore.Text, out int creditScore) || creditScore < 0 || creditScore > 100)
                {
                    MessageBox.Show("Please enter a valid credit score (0-100).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!decimal.TryParse(txtLoanAmount.Text, out decimal loanAmount) || loanAmount < 0)
                {
                    MessageBox.Show("Please enter a valid loan amount.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                inputForm.DialogResult = DialogResult.OK;
                inputForm.Close();
            };
            btnCancel.Click += (s, e) => { inputForm.DialogResult = DialogResult.Cancel; inputForm.Close(); };
            
            inputForm.Controls.AddRange(new Control[] { lblCreditScore, txtCreditScore, lblCreditHint, lblLoanAmount, txtLoanAmount, lblLoanHint, btnOK, btnCancel });
            
            if (inputForm.ShowDialog() != DialogResult.OK) return;
            
            int creditThreshold = int.Parse(txtCreditScore.Text);
            decimal loanThreshold = decimal.Parse(txtLoanAmount.Text);
            
            var confirm = MessageBox.Show(
                $"This will delete all borrower accounts that meet ANY of these criteria:\n\n" +
                $"- Credit score <= {creditThreshold}\n" +
                $"- Total loan amount >= PHP {loanThreshold:N2}\n\n" +
                "This will also delete all related loans, repayments, investments, documents, and credit scores. Continue?",
                "Confirm Batch Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            
            if (confirm != DialogResult.Yes) return;
            
            try
            {
                using var ctx = new MicroLendDbContext();
                
                // Get all borrowers with their credit scores and loan totals
                var borrowersToDelete = new List<Tuple<MicroLend.DAL.Entities.Borrower, int, decimal>>();
                
                var allBorrowers = ctx.Borrowers.ToList();
                foreach (var borrower in allBorrowers)
                {
                    // Get credit score
                    int? creditScore = null;
                    if (borrower.UserId.HasValue)
                    {
                        var cs = ctx.CreditScores.Where(c => c.UserId == borrower.UserId.Value)
                            .OrderByDescending(c => c.QuizDate)
                            .FirstOrDefault();
                        creditScore = cs?.Score;
                    }
                    
                    // Get total loan amount
                    var totalLoans = ctx.Loans
                        .Where(l => l.BorrowerId == borrower.Id)
                        .Sum(l => l.TargetAmount);
                    
                    // Check if meets deletion criteria
                    bool meetsCriteria = false;
                    if (creditScore.HasValue && creditScore.Value <= creditThreshold)
                        meetsCriteria = true;
                    if (totalLoans >= loanThreshold)
                        meetsCriteria = true;
                    
                    if (meetsCriteria)
                    {
                        borrowersToDelete.Add(Tuple.Create(borrower, creditScore ?? 0, totalLoans));
                    }
                }
                
                if (!borrowersToDelete.Any())
                {
                    MessageBox.Show("No accounts match the specified criteria.", "Info", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                int deletedCount = 0;
                foreach (var item in borrowersToDelete)
                {
                    var borrower = item.Item1;
                    var borrowerId = borrower.Id;
                    
                    // Delete related loans
                    var borrowerLoans = ctx.Loans.Where(l => l.BorrowerId == borrowerId).ToList();
                    foreach (var loan in borrowerLoans)
                    {
                        var loanId = loan.Id;
                        var repayments = ctx.Repayments.Where(r => r.LoanId == loanId).ToList();
                        ctx.Repayments.RemoveRange(repayments);
                        var investments = ctx.Investments.Where(i => i.LoanId == loanId).ToList();
                        ctx.Investments.RemoveRange(investments);
                        var documents = ctx.Documents.Where(d => d.LoanId == loanId).ToList();
                        ctx.Documents.RemoveRange(documents);
                    }
                    ctx.Loans.RemoveRange(borrowerLoans);
                    
                    // Delete credit scores
                    if (borrower.UserId.HasValue)
                    {
                        var creditScores = ctx.CreditScores.Where(c => c.UserId == borrower.UserId.Value).ToList();
                        ctx.CreditScores.RemoveRange(creditScores);
                    }
                    
                    // Delete borrower
                    ctx.Borrowers.Remove(borrower);
                    
                    // Delete associated user
                    if (borrower.UserId.HasValue)
                    {
                        var user = ctx.Users.Find(borrower.UserId.Value);
                        if (user != null)
                        {
                            ctx.Users.Remove(user);
                        }
                    }
                    
                    deletedCount++;
                }
                
                ctx.SaveChanges();
                
                // Refresh data
                LoadData();
                
                MessageBox.Show($"Successfully deleted {deletedCount} account(s) matching the criteria.", 
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during batch delete: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddUser()
        {
            using var edit = new UserEditForm();
            if (edit.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var ctx = new MicroLendDbContext();
                    var user = new MicroLend.DAL.Entities.User
                    {
                        Username = edit.Username,
                        PasswordHash = ComputeHash(edit.Password),
                        Role = edit.Role,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    ctx.Users.Add(user);
                    ctx.SaveChanges();
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding user: " + ex.Message);
                }
            }
        }

        private void EditUser()
        {
            if (dgv.CurrentRow == null) return;
            var id = Convert.ToInt32(dgv.CurrentRow.Cells["Id"].Value);
            using var ctx = new MicroLendDbContext();
            var user = ctx.Users.Find(id);
            if (user == null) return;
            using var edit = new UserEditForm(user.Username, user.Role);
            if (edit.ShowDialog() == DialogResult.OK)
            {
                user.Username = edit.Username;
                if (!string.IsNullOrEmpty(edit.Password)) user.PasswordHash = ComputeHash(edit.Password);
                user.Role = edit.Role;
                user.UpdatedAt = DateTime.Now;
                ctx.SaveChanges();
                LoadUsers();
            }
        }

        private void DeleteUser()
        {
            if (dgv.CurrentRow == null) return;
            var id = Convert.ToInt32(dgv.CurrentRow.Cells["Id"].Value);
            var confirm = MessageBox.Show($"Delete user ID {id}? This will also delete all related records (loans, investments, repayments).","Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;
            try
            {
                using var ctx = new MicroLendDbContext();
                var user = ctx.Users.Find(id);
                if (user == null)
                {
                    MessageBox.Show("User not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // Get related entities based on role
                if (user.Role == "Borrower")
                {
                    // Delete related borrower records
                    var borrower = ctx.Borrowers.FirstOrDefault(b => b.UserId == id);
                    if (borrower != null)
                    {
                        var borrowerId = borrower.Id;
                        // Delete related loans
                        var loans = ctx.Loans.Where(l => l.BorrowerId == borrowerId).ToList();
                        foreach (var loan in loans)
                        {
                            // Delete repayments
                            var repayments = ctx.Repayments.Where(r => r.LoanId == loan.Id).ToList();
                            ctx.Repayments.RemoveRange(repayments);
                            // Delete investments
                            var investments = ctx.Investments.Where(i => i.LoanId == loan.Id).ToList();
                            ctx.Investments.RemoveRange(investments);
                            // Delete documents
                            var documents = ctx.Documents.Where(d => d.LoanId == loan.Id).ToList();
                            ctx.Documents.RemoveRange(documents);
                        }
                        ctx.Loans.RemoveRange(loans);
                        ctx.Borrowers.Remove(borrower);
                    }
                }
                
                // Also delete any credit scores
                var creditScores = ctx.CreditScores.Where(c => c.UserId == id).ToList();
                ctx.CreditScores.RemoveRange(creditScores);
                
                // Delete the user
                ctx.Users.Remove(user);
                ctx.SaveChanges();
                
                // Refresh the data to update counts
                LoadData();
                
                MessageBox.Show("User deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting user: {ex.Message}\n\nInner exception: {ex.InnerException?.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportReport()
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                var users = ctx.Users.Select(u => new { u.Id, u.Username, u.Role, u.InitialCreditScore, u.CreatedAt }).ToList();
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Id,Username,Role,CreditScore,CreatedAt");
                foreach (var u in users)
                {
                    sb.AppendLine($"{u.Id},{u.Username},{u.Role},{u.InitialCreditScore},{u.CreatedAt:O}");
                }
                var fn = System.IO.Path.Combine(AppContext.BaseDirectory, "admin_users_export.csv");
                System.IO.File.WriteAllText(fn, sb.ToString());
                MessageBox.Show("Exported to: " + fn);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message);
            }
        }

        private string ComputeHash(string pwd)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pwd));
            return Convert.ToHexString(bytes);
        }
    }
}
