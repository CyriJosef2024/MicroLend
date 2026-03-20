using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MicroLend.DAL;
using Microsoft.EntityFrameworkCore;
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
        private DataGridView dgvDocuments;
        private Button _btnApplyLoanGlobal;
        private Button _btnTakeQuizGlobal;
        private Button _btnMakePaymentGlobal;
        private Label lblTotalBorrowed;
        private Label lblTotalRepaid;
        private Label lblOutstanding;
        private Panel loanDetailsPanel;
        
        public BorrowerDashboardForm(int userId)
        {
            _userId = userId;
            Text = "Borrower Dashboard - MicroLend";
            Width = 1000;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(240, 248, 255);
            // Allow the form to scroll if the window is too small for content
            this.AutoScroll = true;
            this.AutoScaleMode = AutoScaleMode.Dpi;

            CreateMenuBar();
            CreateDashboard();
        }

        private void OpenSupport()
        {
            using var dlg = new Form { Text = "Contact Support", Width = 520, Height = 320, StartPosition = FormStartPosition.CenterParent };
            var lbl = new Label { Text = "Describe your issue and contact details. Support will respond via email.", Location = new Point(12, 12), Size = new Size(480, 24) };
            var txt = new TextBox { Multiline = true, Location = new Point(12, 40), Size = new Size(480, 180) };
            var btnSend = new Button { Text = "Send Ticket", Location = new Point(340, 230), Size = new Size(80, 30) };
            var btnCancel = new Button { Text = "Cancel", Location = new Point(430, 230), Size = new Size(60, 30) };
            btnSend.Click += (s, e) => { MessageBox.Show("Support ticket sent. We will contact you."); dlg.Close(); };
            btnCancel.Click += (s, e) => dlg.Close();
            dlg.Controls.Add(lbl); dlg.Controls.Add(txt); dlg.Controls.Add(btnSend); dlg.Controls.Add(btnCancel);
            dlg.ShowDialog(this);
        }

        private bool EnsureDatabaseMigrated()
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                ctx.Database.Migrate();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database unavailable or migration failed: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        
        private void CreateMenuBar()
        {
            var menuStrip = new MenuStrip();
            menuStrip.Dock = DockStyle.Top;
            
            var fileMenu = new ToolStripMenuItem("Account");
            // Diagnostics for troubleshooting visibility/data
            fileMenu.DropDownItems.Add("Diagnostics", null, (s, e) => ShowDiagnostics());
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
            // ensure menu is at the top of z-order
            menuStrip.BringToFront();
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

            // Primary action toolbar (always visible) with large, clear buttons
            var primaryPanel = new Panel { Dock = DockStyle.Top, Height = 64, Padding = new Padding(12), BackColor = Color.White, Name = "PrimaryActions" };
            var primaryX = 12;
            Button AddPrimaryButton(string text, Color back, EventHandler onClick)
            {
                var b = new Button { Text = text, Size = new Size(160, 40), Location = new Point(primaryX, 12), BackColor = back, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                b.FlatAppearance.BorderSize = 0;
                b.Click += onClick;
                primaryX += b.Width + 12;
                primaryPanel.Controls.Add(b);
                return b;
            }
            var pUpload = AddPrimaryButton("Upload Document", Color.FromArgb(0, 120, 215), (s, e) => { foreach (TabPage tp in tabControl.TabPages) if (tp.Text == "Documents") { tabControl.SelectedTab = tp; break; } BtnUploadDocument_Click(s, e); });
            var pApply = AddPrimaryButton("Apply for Loan", Color.FromArgb(0, 150, 136), (s, e) => { foreach (TabPage tp in tabControl.TabPages) if (tp.Text == "My Loans") { tabControl.SelectedTab = tp; break; } BtnApplyLoan_Click(s, e); });
            var pQuiz = AddPrimaryButton("Take Credit Quiz", Color.FromArgb(75, 181, 67), (s, e) => { foreach (TabPage tp in tabControl.TabPages) if (tp.Text == "My Loans") { tabControl.SelectedTab = tp; break; } BtnTakeQuiz_Click(s, e); });
            var pPay = AddPrimaryButton("Make Payment", Color.FromArgb(0, 102, 204), (s, e) => { foreach (TabPage tp in tabControl.TabPages) if (tp.Text == "Repayments") { tabControl.SelectedTab = tp; break; } BtnMakePayment_Click(s, e); });
            var pSupport = AddPrimaryButton("Contact Support", Color.FromArgb(128, 128, 128), (s, e) => OpenSupport());
            Controls.Add(primaryPanel);

            // Instruction panel shown when there are no loans/documents to guide user
            var instrPanel = new Panel { Dock = DockStyle.Top, Height = 96, Padding = new Padding(12), BackColor = Color.WhiteSmoke, Name = "InstructionPanel" };
            var instrLabel = new Label { Text = "Get started: upload your documents, then apply for a loan or take the credit quiz.", AutoSize = false, Size = new Size(600, 40), Location = new Point(12, 12), Font = new Font("Segoe UI", 10) };
            instrPanel.Controls.Add(instrLabel);

            // Action buttons for quick access
            var btnUploadInstr = new Button { Text = "Upload Document", Size = new Size(140, 34), Location = new Point(630, 12), BackColor = Color.FromArgb(0,120,215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUploadInstr.Click += (s, e) => {
                // navigate to Documents tab and open file dialog
                foreach (TabPage tp in tabControl.TabPages) if (tp.Text == "Documents") { tabControl.SelectedTab = tp; break; }
                BtnUploadDocument_Click(s, e);
            };
            var btnApplyInstr = new Button { Text = "Apply for Loan", Size = new Size(140, 34), Location = new Point(780, 12), BackColor = Color.FromArgb(0,150,136), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnApplyInstr.Click += (s, e) => { foreach (TabPage tp in tabControl.TabPages) if (tp.Text == "My Loans") { tabControl.SelectedTab = tp; break; } BtnApplyLoan_Click(s, e); };
            var btnQuizInstr = new Button { Text = "Take Credit Quiz", Size = new Size(140, 34), Location = new Point(630, 50), BackColor = Color.FromArgb(75,181,67), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnQuizInstr.Click += (s, e) => { foreach (TabPage tp in tabControl.TabPages) if (tp.Text == "My Loans") { tabControl.SelectedTab = tp; break; } BtnTakeQuiz_Click(s, e); };
            var btnSupport = new Button { Text = "Contact Support", Size = new Size(140, 34), Location = new Point(780, 50), BackColor = Color.FromArgb(0,102,204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSupport.Click += (s, e) => OpenSupport();

            instrPanel.Controls.Add(btnUploadInstr);
            instrPanel.Controls.Add(btnApplyInstr);
            instrPanel.Controls.Add(btnQuizInstr);
            instrPanel.Controls.Add(btnSupport);
            Controls.Add(instrPanel);

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

            // Ensure loan details panel exists
            loanDetailsPanel = new Panel { Dock = DockStyle.Right, Width = 360, BackColor = Color.White, Padding = new Padding(12) };
            loanDetailsPanel.Paint += (s, e) => { using var p = new Pen(Color.FromArgb(220, 220, 220)); e.Graphics.DrawRectangle(p, 0, 0, loanDetailsPanel.Width - 1, loanDetailsPanel.Height - 1); };
            var lblDetailsTitle = new Label { Text = "Loan Details", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(8, 8), AutoSize = true };
            var lblDetailsBody = new Label { Name = "DetailsBody", Location = new Point(8, 36), Size = new Size(332, 200), AutoSize = false };
            var btnViewDocs = new Button { Text = "View Documents", Size = new Size(140, 36), Location = new Point(8, 250) };
            btnViewDocs.Click += (s, e) => { if (dgvMyLoans.CurrentRow != null) OpenLoanDocuments(); };
            var btnSchedule = new Button { Text = "Schedule Payment", Size = new Size(140, 36), Location = new Point(160, 250) };
            btnSchedule.Click += (s, e) => { if (dgvMyLoans.CurrentRow != null) BtnMakePayment_Click(s, e); };
            loanDetailsPanel.Controls.Add(lblDetailsTitle);
            loanDetailsPanel.Controls.Add(lblDetailsBody);
            loanDetailsPanel.Controls.Add(btnViewDocs);
            loanDetailsPanel.Controls.Add(btnSchedule);

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
            // expose for instruction panel
            _btnApplyLoanGlobal = btnApplyLoan;
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
            _btnTakeQuizGlobal = btnTakeQuiz;
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

            // Add controls in docking order: top toolbar, right details, then fill grid
            loansPanel.Controls.Add(loansButtonContainer);
            loansPanel.Controls.Add(loanDetailsPanel);
            loansPanel.Controls.Add(dgvMyLoans);
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
            _btnMakePaymentGlobal = btnMakePayment;
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
            // Repayments: add toolbar first so it docks to top, then grid fills remaining space
            repaymentsPanel.Controls.Add(repaymentsButtonContainer);
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

            // Profile Tab
            var tabProfile = new TabPage("Profile");
            var profilePanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var lblProfileTitle = new Label { Text = "Your Profile", Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true };
            var btnEditProfile = new Button { Text = "Edit Profile", Size = new Size(120, 30), Location = new Point(20, 60) };
            btnEditProfile.Click += (s, e) => OpenSettings();
            profilePanel.Controls.Add(lblProfileTitle);
            profilePanel.Controls.Add(btnEditProfile);
            tabProfile.Controls.Add(profilePanel);
            tabControl.TabPages.Add(tabProfile);

            // Documents Tab
            var tabDocs = new TabPage("Documents");
            var docsPanel = new Panel { Dock = DockStyle.Fill };
            var docsToolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10) };
            var btnUpload = new Button { Text = "Upload Document", Size = new Size(140, 30), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpload.Click += BtnUploadDocument_Click;
            docsToolbar.Controls.Add(btnUpload);
            dgvDocuments = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false, AllowUserToAddRows = false };
            dgvDocuments.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Width = 60 });
            dgvDocuments.Columns.Add(new DataGridViewTextBoxColumn { Name = "FileName", HeaderText = "File", DataPropertyName = "FileName" });
            dgvDocuments.Columns.Add(new DataGridViewTextBoxColumn { Name = "Uploaded", HeaderText = "Uploaded", DataPropertyName = "UploadedAt" });
            // Documents: toolbar at top, grid fills
            docsPanel.Controls.Add(docsToolbar);
            docsPanel.Controls.Add(dgvDocuments);
            tabDocs.Controls.Add(docsPanel);
            tabControl.TabPages.Add(tabDocs);
            
            Controls.Add(tabControl);
            // bring primary actions to front
            var primary = FindControlByName(this, "PrimaryActions");
            if (primary != null) primary.BringToFront();
            
            Load += BorrowerDashboardForm_Load;
            tabControl.SelectedIndexChanged += (s, e) => {
                // refresh visible tab content
                if (tabControl.SelectedTab != null)
                {
                    var t = tabControl.SelectedTab.Text;
                    if (t == "My Loans") _ = LoadMyLoansAsync();
                    if (t == "Repayments") _ = LoadRepaymentsAsync();
                    if (t == "Documents") LoadDocuments();
                }
            };
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
            // Ensure DB is migrated before any operations
            if (!EnsureDatabaseMigrated()) return;

            await LoadMyLoansAsync();
            await LoadRepaymentsAsync();
            LoadDocuments();

#if DEBUG
            // For developer testing: if borrower has no documents and no loans, create sample entries so UI is visible
            try
            {
                using var ctx = new MicroLendDbContext();
                var borrower = ctx.Borrowers.FirstOrDefault(b => b.UserId == _userId);
                if (borrower == null)
                {
                    borrower = new Borrower { UserId = _userId, Name = ctx.Users.Find(_userId)?.Username ?? ("user" + _userId), ContactNumber = "", MonthlyIncome = 0m, BusinessType = "" };
                    ctx.Borrowers.Add(borrower);
                    ctx.SaveChanges();
                }

                var hasDocs = ctx.Documents.Any(d => d.UserId == _userId);
                var hasLoans = ctx.Loans.Any(l => l.BorrowerId == borrower.Id);

                if (!hasDocs)
                {
                    // create a placeholder document record and a placeholder file
                    var doc = new MicroLend.DAL.Entities.Document { UserId = _userId, FileName = "sample-id.pdf", FilePath = "", UploadedAt = DateTime.Now };
                    ctx.Documents.Add(doc);
                    ctx.SaveChanges();
                    try
                    {
                        var dest = System.IO.Path.Combine(AppContext.BaseDirectory, "uploads");
                        System.IO.Directory.CreateDirectory(dest);
                        var dst = System.IO.Path.Combine(dest, doc.Id + "_" + doc.FileName);
                        System.IO.File.WriteAllText(dst, "Sample document placeholder");
                        doc.FilePath = dst;
                        ctx.SaveChanges();
                    }
                    catch { }
                }

                if (!hasLoans)
                {
                    var loan = new MicroLend.DAL.Entities.Loan { Purpose = "Sample loan application", TargetAmount = 1000m, CurrentAmount = 0m, InterestRate = 5m, Status = "Pending", IsCrowdfunded = true, BorrowerId = borrower.Id, DateGranted = DateTime.Now, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };
                    ctx.Loans.Add(loan);
                    ctx.SaveChanges();
                }
            }
            catch { }
#endif

            UpdateSummary();
        }
        
        private async Task LoadMyLoansAsync()
        {
            try
            {
                if (!EnsureDatabaseMigrated()) return;
                var ctx = new MicroLendDbContext();
                
                // Get borrower ID for this user
                var borrower = await Task.Run(() => ctx.Borrowers.FirstOrDefault(b => b.UserId == _userId));

                if (borrower == null)
                {
                    // If borrower profile is missing, create a minimal one so the dashboard becomes usable
                    try
                    {
                        var user = ctx.Users.Find(_userId);
                        var name = user?.Username ?? ("user" + _userId);
                        borrower = new Borrower
                        {
                            UserId = _userId,
                            Name = name,
                            ContactNumber = string.Empty,
                            MonthlyIncome = 0m,
                            BusinessType = string.Empty
                        };
                        ctx.Borrowers.Add(borrower);
                        ctx.SaveChanges();
                    }
                    catch
                    {
                        // if creation fails, just show empty grids
                        dgvMyLoans.DataSource = null;
                        return;
                    }
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
                dgvMyLoans.SelectionChanged -= DgvMyLoans_SelectionChanged;
                dgvMyLoans.SelectionChanged += DgvMyLoans_SelectionChanged;

                // after loading loans, refresh repayments and summary
                _ = LoadRepaymentsAsync();
                UpdateSummary();
                var instr = FindControlByName(this, "InstructionPanel");
                if (instr != null) instr.Visible = ! (loans.Any() || (dgvDocuments.DataSource != null && ((System.Collections.IList)dgvDocuments.DataSource).Count > 0));
                var primaryPanel = FindControlByName(this, "PrimaryActions");
                if (primaryPanel != null) primaryPanel.Visible = true;
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
                if (!EnsureDatabaseMigrated()) { dgvRepayments.DataSource = null; return; }
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
                dgvRepayments.Refresh();
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

        private void BtnUploadDocument_Click(object? sender, EventArgs e)
        {
            if (!EnsureDatabaseMigrated()) return;
            using var ofd = new OpenFileDialog { Filter = "PDF or Image|*.pdf;*.jpg;*.jpeg;*.png|All files|*.*" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var ctx = new MicroLendDbContext();
                var doc = new Document
                {
                    UserId = _userId,
                    FileName = System.IO.Path.GetFileName(ofd.FileName),
                    UploadedAt = DateTime.Now
                };
                ctx.Documents.Add(doc);
                ctx.SaveChanges();

                // Save file marker locally for demo purposes
                var dest = System.IO.Path.Combine(AppContext.BaseDirectory, "uploads");
                System.IO.Directory.CreateDirectory(dest);
                var dst = System.IO.Path.Combine(dest, doc.Id + "_" + doc.FileName);
                System.IO.File.Copy(ofd.FileName, dst, true);

                LoadDocuments();
                MessageBox.Show("Document uploaded successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error uploading document: " + ex.Message);
            }
        }

        private void LoadDocuments()
        {
            try
            {
                if (!EnsureDatabaseMigrated()) { dgvDocuments.DataSource = null; return; }
                using var ctx = new MicroLendDbContext();
                var docs = ctx.Documents.Where(d => d.UserId == _userId)
                    .Select(d => new { d.Id, d.FileName, UploadedAt = d.UploadedAt })
                    .ToList();
                dgvDocuments.DataSource = docs;
                // hide instruction panel if docs exist or loans exist
                var instr = FindControlByName(this, "InstructionPanel");
                if (instr != null) instr.Visible = !(docs.Any() || (dgvMyLoans.DataSource != null && ((System.Collections.IList)dgvMyLoans.DataSource).Count > 0));
                var primaryPanel = FindControlByName(this, "PrimaryActions");
                if (primaryPanel != null) primaryPanel.Visible = true;
            }
            catch { }
        }

        private void OpenLoanDocuments()
        {
            if (!EnsureDatabaseMigrated()) { MessageBox.Show("Database unavailable."); return; }
            if (dgvMyLoans.SelectedRows.Count == 0) return;
            var loanId = Convert.ToInt32(dgvMyLoans.SelectedRows[0].Cells["Id"].Value);
            // For demo, show documents for current user; in a full app, we'd link documents to loan records.
            using var dlg = new Form { Text = $"Documents for Loan #{loanId}", Width = 600, Height = 400, StartPosition = FormStartPosition.CenterParent };
            var lv = new ListView { Dock = DockStyle.Fill, View = View.Details };
            lv.Columns.Add("ID", 60);
            lv.Columns.Add("File", 400);
            using var ctx = new MicroLendDbContext();
            var docs = ctx.Documents.Where(d => d.UserId == _userId).ToList();
            foreach (var d in docs) lv.Items.Add(new ListViewItem(new[] { d.Id.ToString(), d.FileName }));
            dlg.Controls.Add(lv);
            dlg.ShowDialog();
        }

        private void DgvMyLoans_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvMyLoans.SelectedRows.Count == 0) return;
            var row = dgvMyLoans.SelectedRows[0];
            var id = Convert.ToInt32(row.Cells["Id"].Value);
            var purpose = row.Cells["Purpose"].Value?.ToString() ?? string.Empty;
            var amt = Convert.ToDecimal(row.Cells["TargetAmount"].Value);
            var status = row.Cells["Status"].Value?.ToString() ?? string.Empty;

            var lbl = loanDetailsPanel.Controls.Find("DetailsBody", true).FirstOrDefault() as Label;
            if (lbl != null)
            {
                lbl.Text = $"Loan ID: {id}\r\nPurpose: {purpose}\r\nAmount: ₱{amt:N2}\r\nStatus: {status}";
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

        private Control? FindControlByName(Control parent, string name)
        {
            foreach (Control c in parent.Controls)
            {
                if (c.Name == name) return c;
                var found = FindControlByName(c, name);
                if (found != null) return found;
            }
            return null;
        }

        private void ShowDiagnostics()
        {
            try
            {
                if (!EnsureDatabaseMigrated()) return;
                using var ctx = new MicroLendDbContext();
                var hasBorrower = ctx.Borrowers.Any(b => b.UserId == _userId);
                var docCount = ctx.Set<MicroLend.DAL.Entities.Document>().Count(d => d.UserId == _userId);
                var borrower = ctx.Borrowers.FirstOrDefault(b => b.UserId == _userId);
                var loans = borrower != null ? ctx.Loans.Where(l => l.BorrowerId == borrower.Id).ToList() : new System.Collections.Generic.List<MicroLend.DAL.Entities.Loan>();
                MessageBox.Show($"Diagnostics:\nHasBorrower={hasBorrower}\nDocuments={docCount}\nLoans={loans.Count}", "Diagnostics");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Diagnostics error: " + ex.Message);
            }
        }
    }
}
