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
        private SplitContainer _mainSplit;
        private TabControl tabControl;
        private FlowLayoutPanel _summaryPanel;
        private System.Collections.Generic.List<Panel> _summaryCards = new System.Collections.Generic.List<Panel>();
        private DataGridView dgvMyLoans;
        private DataGridView dgvRepayments;
        private DataGridView dgvDocuments;
        private DataGridView dgvLoanRecords;
        private Panel _loanBottomPanel;
        private Button _btnApplyLoanGlobal;
        private Button _btnTakeQuizGlobal;
        private Button _btnMakePaymentGlobal;
        private Label lblTotalBorrowed;
        private Label lblTotalRepaid;
        private Label lblOutstanding;
        private Label lblLendedMoney;
        private Panel loanDetailsPanel;
        // Payment controls for inline payment in Repayments tab
        private ComboBox cmbPaymentLoan;
        private TextBox txtPaymentAmount;
        private Button btnSubmitPayment;
        
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
                // make sure Documents table exists even if migrations did not run for some reason
                try { EnsureDocumentsTableExists(); } catch { }
                return true;
            }
            catch (Exception ex)
            {
                // attempt to provide helpful debugging info (DB path)
                try
                {
                    var dbPath = System.IO.Path.Combine(AppContext.BaseDirectory, "MicroLend.db");
                    MessageBox.Show("Database unavailable or migration failed: " + ex.Message + "\nDB Path: " + dbPath + (ex.InnerException != null ? "\nInner: " + ex.InnerException.Message : ""), "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch
                {
                    MessageBox.Show("Database unavailable or migration failed: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
        }

        private void EnsureDocumentsTableExists()
        {
            using var ctx = new MicroLendDbContext();
            var conn = ctx.Database.GetDbConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            // Create table if missing (columns mirror migration)
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Documents (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL,
    LoanId INTEGER,
    FileName TEXT NOT NULL,
    FilePath TEXT NOT NULL,
    UploadedAt TEXT NOT NULL,
    Status TEXT NOT NULL DEFAULT 'Pending',
    ReviewedBy INTEGER,
    ReviewedAt TEXT
);";
            cmd.ExecuteNonQuery();
            try { conn.Close(); } catch { }
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

            // Lended Money Card (CurrentAmount - the actual funded amount)
            var card4 = CreateSummaryCard("Lended Money", "₱0.00", Color.FromArgb(156, 39, 176));
            lblLendedMoney = card4.Controls[1] as Label;
            _summaryCards.Add(card4);
            _summaryPanel.Controls.Add(card4);

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
            var pRefresh = AddPrimaryButton("Refresh Data", Color.FromArgb(40, 167, 69), (s, e) => RefreshBorrowerData());
            Controls.Add(primaryPanel);

            // Instruction panel removed to match admin layout (no empty gray box).

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

            // Loan details panel (shown on the right). We'll host it in a SplitContainer so
            // the records grid can take the full available width on the left without creating
            // an empty whitespace area on the right.
            loanDetailsPanel = new Panel { BackColor = Color.White, Padding = new Padding(12) };
            loanDetailsPanel.Paint += (s, e) => { using var p = new Pen(Color.FromArgb(220, 220, 220)); e.Graphics.DrawRectangle(p, 0, 0, loanDetailsPanel.Width - 1, loanDetailsPanel.Height - 1); };
            var lblDetailsTitle = new Label { Text = "Loan Details", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(8, 8), AutoSize = true };
            var lblDetailsBody = new Label { Name = "DetailsBody", Location = new Point(8, 36), Size = new Size(332, 200), AutoSize = false };
            loanDetailsPanel.Controls.Add(lblDetailsTitle);
            loanDetailsPanel.Controls.Add(lblDetailsBody);

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

            // Bottom area: a larger read-only grid where loan records and status are shown (similar to Admin view)
            _loanBottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 200, BackColor = Color.White, Padding = new Padding(2), Margin = new Padding(0) };
            dgvLoanRecords = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Margin = new Padding(0),
                RowTemplate = { Height = 28 }
            };
            // columns: Purpose, TargetAmount, CurrentAmount, InterestRate, Status, IsCrowdfunded, DateGranted
            dgvLoanRecords.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvLoanRecords.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvLoanRecords.RowHeadersVisible = false;
            dgvLoanRecords.AllowUserToResizeRows = false;
            var colPurpose = new DataGridViewTextBoxColumn { Name = "Purpose", HeaderText = "Purpose", DataPropertyName = "Purpose" };
            var colTarget = new DataGridViewTextBoxColumn { Name = "TargetAmount", HeaderText = "Target", DataPropertyName = "TargetAmount", DefaultCellStyle = { Format = "N2" } };
            var colCurrent = new DataGridViewTextBoxColumn { Name = "CurrentAmount", HeaderText = "Current", DataPropertyName = "CurrentAmount", DefaultCellStyle = { Format = "N2" } };
            var colInterest = new DataGridViewTextBoxColumn { Name = "InterestRate", HeaderText = "InterestRate", DataPropertyName = "InterestRate", DefaultCellStyle = { Format = "N2" } };
            var colStatus = new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status" };
            var colCrowd = new DataGridViewCheckBoxColumn { Name = "IsCrowdfunded", HeaderText = "IsCrowdfunded", DataPropertyName = "IsCrowdfunded" };
            var colDate = new DataGridViewTextBoxColumn { Name = "DateGranted", HeaderText = "DateGranted", DataPropertyName = "DateGranted", DefaultCellStyle = { Format = "g" } };
            // Balance fill weights for nicer layout
            colPurpose.FillWeight = 220;
            colTarget.FillWeight = 80;
            colCurrent.FillWeight = 80;
            colInterest.FillWeight = 70;
            colStatus.FillWeight = 80;
            colCrowd.FillWeight = 50;
            colDate.FillWeight = 100;
            dgvLoanRecords.Columns.AddRange(new DataGridViewColumn[] { colPurpose, colTarget, colCurrent, colInterest, colStatus, colCrowd, colDate });
            dgvLoanRecords.Margin = new Padding(0);
            _loanBottomPanel.Controls.Add(dgvLoanRecords);

            dgvMyLoans = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Margin = new Padding(6),
                RowTemplate = { Height = 28 }
            };
            // Define explicit columns so headers are stable regardless of returned projection
            dgvMyLoans.Columns.Clear();
            var cId = new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Loan ID", DataPropertyName = "Id", Width = 80 };
            var cPurpose = new DataGridViewTextBoxColumn { Name = "Purpose", HeaderText = "Purpose", DataPropertyName = "Purpose" };
            var cTarget = new DataGridViewTextBoxColumn { Name = "TargetAmount", HeaderText = "Amount (₱)", DataPropertyName = "TargetAmount", DefaultCellStyle = { Format = "N2" } };
            var cCurrent = new DataGridViewTextBoxColumn { Name = "CurrentAmount", HeaderText = "Funded (₱)", DataPropertyName = "CurrentAmount", DefaultCellStyle = { Format = "N2" } };
            var cStatus = new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status" };
            var cRisk = new DataGridViewTextBoxColumn { Name = "RiskScore", HeaderText = "Risk Score", DataPropertyName = "RiskScore" };
            var cInterest = new DataGridViewTextBoxColumn { Name = "InterestRate", HeaderText = "Interest Rate (%)", DataPropertyName = "InterestRate", DefaultCellStyle = { Format = "N2" } };
            cPurpose.FillWeight = 220;
            cTarget.FillWeight = 90;
            cCurrent.FillWeight = 90;
            cStatus.FillWeight = 80;
            cRisk.FillWeight = 70;
            cInterest.FillWeight = 70;
            dgvMyLoans.Columns.AddRange(new DataGridViewColumn[] { cId, cPurpose, cTarget, cCurrent, cStatus, cRisk, cInterest });

            // Compose the loans area using a SplitContainer so left side holds the main grids
            // and right side holds the loan details. This avoids layout whitespace issues.
            var mainSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };
            mainSplit.SplitterDistance = Math.Max(400, this.ClientSize.Width - 360);
            mainSplit.Panel1.Padding = new Padding(0);
            mainSplit.Panel2.Padding = new Padding(0);

            // Left panel: toolbar at top, loans grid fill, bottom loan records docked bottom
            mainSplit.Panel1.Controls.Add(dgvMyLoans);
            mainSplit.Panel1.Controls.Add(_loanBottomPanel);
            mainSplit.Panel1.Controls.Add(loansButtonContainer);
            loansButtonContainer.Dock = DockStyle.Top;
            _loanBottomPanel.Dock = DockStyle.Bottom;
            dgvMyLoans.Dock = DockStyle.Fill;

            // Right panel: loan details
            loanDetailsPanel.Dock = DockStyle.Fill;
            mainSplit.Panel2.Controls.Add(loanDetailsPanel);

            loansPanel.Controls.Add(mainSplit);
            tabLoans.Controls.Add(loansPanel);
            tabControl.TabPages.Add(tabLoans);
            
            // Repayments Tab
            var tabRepayments = new TabPage("Repayments");
            var repaymentsPanel = new Panel { Dock = DockStyle.Fill };

            // ==================== MAKE A PAYMENT TAB ====================
            var tabMakePayment = new TabPage("Make a Payment");
            var makePaymentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // Payment container with TableLayoutPanel for better control
            var paymentContainer = new TableLayoutPanel
            {
                Location = new Point(20, 20),
                Size = new Size(600, 250),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ColumnCount = 2,
                RowCount = 4,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = new Padding(10)
            };
            paymentContainer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            paymentContainer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            for (int i = 0; i < 4; i++)
                paymentContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Payment title - spans both columns
            var lblPayTitle = new Label
            {
                Text = "Make a Payment",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 15)
            };
            paymentContainer.SetCellPosition(lblPayTitle, new TableLayoutPanelCellPosition(0, 0));
            paymentContainer.SetColumnSpan(lblPayTitle, 2);
            paymentContainer.Controls.Add(lblPayTitle, 0, 0);

            // Select Loan row
            var lblPayLoan = new Label
            {
                Text = "Select Loan:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Margin = new Padding(0, 10, 20, 10)
            };
            paymentContainer.Controls.Add(lblPayLoan, 0, 1);

            cmbPaymentLoan = new ComboBox
            {
                Size = new Size(320, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 10, 0, 10)
            };
            paymentContainer.Controls.Add(cmbPaymentLoan, 1, 1);

            // Amount row
            var lblPayAmount = new Label
            {
                Text = "Amount (₱):",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Margin = new Padding(0, 10, 20, 10)
            };
            paymentContainer.Controls.Add(lblPayAmount, 0, 2);

            txtPaymentAmount = new TextBox
            {
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 10, 0, 10)
            };
            paymentContainer.Controls.Add(txtPaymentAmount, 1, 2);

            // Submit Payment Button - spans both columns
            btnSubmitPayment = new Button
            {
                Text = "Submit Payment",
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Standard,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 20, 0, 0)
            };
            btnSubmitPayment.FlatAppearance.BorderSize = 1;
            btnSubmitPayment.FlatAppearance.BorderColor = Color.White;
            btnSubmitPayment.Click += BtnSubmitPayment_Click;
            paymentContainer.SetCellPosition(btnSubmitPayment, new TableLayoutPanelCellPosition(0, 3));
            paymentContainer.SetColumnSpan(btnSubmitPayment, 2);
            paymentContainer.Controls.Add(btnSubmitPayment);

            makePaymentPanel.Controls.Add(paymentContainer);
            tabMakePayment.Controls.Add(makePaymentPanel);
            tabControl.TabPages.Add(tabMakePayment);
            // ==================== END MAKE A PAYMENT TAB ====================

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

            // Inline Payment Panel - Add payment controls directly in the Repayments tab
            var paymentPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 140,
                Padding = new Padding(10),
                BackColor = Color.White
            };
            paymentPanel.Paint += (s, e) => {
                using var p = new Pen(Color.FromArgb(0, 120, 215), 2);
                e.Graphics.DrawRectangle(p, 0, 0, paymentPanel.Width - 1, paymentPanel.Height - 1);
            };

            // Title
            var lblPaymentTitle = new Label
            {
                Text = "Make a Payment",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(10, 10),
                AutoSize = true
            };
            paymentPanel.Controls.Add(lblPaymentTitle);

            // Row 1: Select Loan label and combo
            var lblSelectLoan = new Label
            {
                Text = "Select Loan:",
                Location = new Point(10, 40),
                Size = new Size(80, 20)
            };
            paymentPanel.Controls.Add(lblSelectLoan);

            cmbPaymentLoan = new ComboBox
            {
                Location = new Point(95, 38),
                Size = new Size(280, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            paymentPanel.Controls.Add(cmbPaymentLoan);

            // Row 2: Amount label, textbox
            var lblAmount = new Label
            {
                Text = "Amount (₱):",
                Location = new Point(10, 75),
                Size = new Size(80, 20)
            };
            paymentPanel.Controls.Add(lblAmount);

            txtPaymentAmount = new TextBox
            {
                Location = new Point(95, 73),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10)
            };
            paymentPanel.Controls.Add(txtPaymentAmount);

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
            repaymentsPanel.Controls.Add(paymentPanel);
            repaymentsPanel.Controls.Add(dgvRepayments);
            tabRepayments.Controls.Add(repaymentsPanel);
            tabControl.TabPages.Add(tabRepayments);
            
            // Credit Score Tab
            var tabCreditScore = new TabPage("Credit Score");
            var creditPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            // Credit Score Header
            var creditInfoLabel = new Label
            {
                Text = "📊 Your Credit Score",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(20, 20),
                AutoSize = true
            };
            creditPanel.Controls.Add(creditInfoLabel);
            
            // Credit Score Display Panel
            var scoreDisplayPanel = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(350, 180),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            scoreDisplayPanel.Paint += (s, e) => {
                using var p = new Pen(Color.FromArgb(200, 200, 200));
                e.Graphics.DrawRectangle(p, 0, 0, scoreDisplayPanel.Width - 1, scoreDisplayPanel.Height - 1);
            };
            
            var scoreTitleLabel = new Label
            {
                Text = "Current Score",
                Location = new Point(20, 15),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                AutoSize = true
            };
            
            var scoreValueLabel = new Label
            {
                Name = "ScoreValue",
                Text = "--",
                Location = new Point(20, 40),
                Font = new Font("Segoe UI", 42, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                AutoSize = true
            };
            
            var scoreMaxLabel = new Label
            {
                Text = "/ 100",
                Location = new Point(150, 55),
                Font = new Font("Segoe UI", 16),
                ForeColor = Color.Gray,
                AutoSize = true
            };
            
            // Progress bar for visual score
            var scoreProgressBar = new ProgressBar
            {
                Name = "ScoreProgress",
                Location = new Point(20, 95),
                Size = new Size(310, 20),
                Style = ProgressBarStyle.Continuous,
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };
            
            var scoreDescLabel = new Label
            {
                Name = "ScoreDesc",
                Text = "Take the credit quiz to calculate your score",
                Location = new Point(20, 125),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true
            };
            
            var btnTakeQuizCredit = new Button
            {
                Text = "Take Credit Quiz",
                Location = new Point(20, 150),
                Size = new Size(150, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 150, 80),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnTakeQuizCredit.FlatAppearance.BorderSize = 0;
            btnTakeQuizCredit.Click += BtnTakeQuiz_Click;
            
            scoreDisplayPanel.Controls.Add(scoreTitleLabel);
            scoreDisplayPanel.Controls.Add(scoreValueLabel);
            scoreDisplayPanel.Controls.Add(scoreMaxLabel);
            scoreDisplayPanel.Controls.Add(scoreProgressBar);
            scoreDisplayPanel.Controls.Add(scoreDescLabel);
            scoreDisplayPanel.Controls.Add(btnTakeQuizCredit);
            creditPanel.Controls.Add(scoreDisplayPanel);
            
            // Score Categories Panel
            var categoriesPanel = new Panel
            {
                Location = new Point(390, 70),
                Size = new Size(350, 180),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            categoriesPanel.Paint += (s, e) => {
                using var p = new Pen(Color.FromArgb(200, 200, 200));
                e.Graphics.DrawRectangle(p, 0, 0, categoriesPanel.Width - 1, categoriesPanel.Height - 1);
            };
            
            var catTitleLabel = new Label
            {
                Text = "Score Breakdown",
                Location = new Point(15, 15),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };
            
            var catQuizLabel = new Label
            {
                Text = "• Quiz Score (30%)",
                Location = new Point(15, 45),
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
            
            var catPaymentLabel = new Label
            {
                Text = "• On-time Payments (50%)",
                Location = new Point(15, 70),
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
            
            var catDebtLabel = new Label
            {
                Text = "• Debt-to-Income (20%)",
                Location = new Point(15, 95),
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
            
            var catTipLabel = new Label
            {
                Text = "Tips: Complete your credit quiz and make timely repayments to improve your score!",
                Location = new Point(15, 125),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true
            };
            
            categoriesPanel.Controls.Add(catTitleLabel);
            categoriesPanel.Controls.Add(catQuizLabel);
            categoriesPanel.Controls.Add(catPaymentLabel);
            categoriesPanel.Controls.Add(catDebtLabel);
            categoriesPanel.Controls.Add(catTipLabel);
            creditPanel.Controls.Add(categoriesPanel);
            
            // Score History Panel
            var historyPanel = new Panel
            {
                Location = new Point(20, 270),
                Size = new Size(720, 150),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            historyPanel.Paint += (s, e) => {
                using var p = new Pen(Color.FromArgb(200, 200, 200));
                e.Graphics.DrawRectangle(p, 0, 0, historyPanel.Width - 1, historyPanel.Height - 1);
            };
            
            var historyTitleLabel = new Label
            {
                Text = "Recent Quiz Results",
                Location = new Point(15, 15),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };
            
            var dgvScoreHistory = new DataGridView
            {
                Location = new Point(15, 45),
                Size = new Size(690, 90),
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            dgvScoreHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date", Width = 150 });
            dgvScoreHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Score", HeaderText = "Score", Width = 100 });
            dgvScoreHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", Width = 440 });
            
            historyPanel.Controls.Add(historyTitleLabel);
            historyPanel.Controls.Add(dgvScoreHistory);
            creditPanel.Controls.Add(historyPanel);
            
            // Description
            var creditDescLabel = new Label
            {
                Text = "Your credit score is calculated based on your financial behavior, repayment history, and credit quiz results.\n" +
                       "A higher score means better creditworthiness and may qualify you for larger loans with better terms.\n\n" +
                       "Take the credit quiz regularly to improve your score!",
                Location = new Point(20, 435),
                Size = new Size(700, 80),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray
            };
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
                    if (t == "Repayments") {
                        _ = LoadRepaymentsAsync();
                        PopulatePaymentLoanDropdown();
                    }
                    if (t == "Make a Payment") {
                        PopulatePaymentLoanDropdown();
                    }
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
            await LoadCreditScoreAsync();
            PopulatePaymentLoanDropdown();

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
                // populate bottom loan records grid
                try
                {
                    var loanRecords = await Task.Run(() => ctx.Loans
                        .Where(l => l.BorrowerId == borrower.Id)
                        .Select(l => new
                        {
                            l.Purpose,
                            l.TargetAmount,
                            l.CurrentAmount,
                            l.InterestRate,
                            l.Status,
                            l.IsCrowdfunded,
                            l.DateGranted
                        }).ToList());
                    dgvLoanRecords.DataSource = loanRecords;
                }
                catch { }
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
                
                // Lended Money = CurrentAmount (actual funded amount)
                var lendedMoney = loans.Sum(l => l.CurrentAmount);
                
                lblTotalBorrowed.Text = $"₱{totalBorrowed:N2}";
                lblTotalRepaid.Text = $"₱{totalRepaid:N2}";
                lblOutstanding.Text = $"₱{outstanding:N2}";
                lblLendedMoney.Text = $"₱{lendedMoney:N2}";
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
                // Copy file to local uploads first so we can persist the path atomically.
                var dest = System.IO.Path.Combine(AppContext.BaseDirectory, "uploads");
                System.IO.Directory.CreateDirectory(dest);
                var fileName = System.IO.Path.GetFileName(ofd.FileName);
                var dst = System.IO.Path.Combine(dest, Guid.NewGuid().ToString() + "_" + fileName);
                System.IO.File.Copy(ofd.FileName, dst, true);

                using var ctx = new MicroLendDbContext();
                var doc = new Document
                {
                    UserId = _userId,
                    FileName = fileName,
                    FilePath = dst,
                    UploadedAt = DateTime.Now
                };
                ctx.Documents.Add(doc);
                ctx.SaveChanges();

                LoadDocuments();
                MessageBox.Show("Document uploaded successfully.");
            }
            catch (Exception ex)
            {
                try
                {
                    var dbPath = System.IO.Path.Combine(AppContext.BaseDirectory, "MicroLend.db");
                    var inner = ex.InnerException != null ? "\nInner: " + ex.InnerException.Message : string.Empty;
                    MessageBox.Show($"Error uploading document: {ex.Message}{inner}\nDB Path: {dbPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch
                {
                    MessageBox.Show("Error uploading document: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
                // If loan records control exists ensure it refreshes (in case documents affect UI state)
                try { if (dgvLoanRecords != null) dgvLoanRecords.Refresh(); } catch { }
                // hide instruction panel if docs exist or loans exist
                var instr = FindControlByName(this, "InstructionPanel");
                if (instr != null) instr.Visible = !(docs.Any() || (dgvMyLoans.DataSource != null && ((System.Collections.IList)dgvMyLoans.DataSource).Count > 0));
                var primaryPanel = FindControlByName(this, "PrimaryActions");
                if (primaryPanel != null) primaryPanel.Visible = true;
            }
            catch { }
        }
        
        private async Task LoadCreditScoreAsync()
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                
                // Try to get credit score from CreditScores table
                var creditScores = await Task.Run(() => ctx.CreditScores
                    .Where(c => c.UserId == _userId)
                    .OrderByDescending(c => c.QuizDate)
                    .ToList());
                
                int score = 0;
                
                if (creditScores.Any())
                {
                    var latestScore = creditScores.First();
                    score = latestScore.Score;
                }
                else
                {
                    // Fallback to user's InitialCreditScore
                    var user = ctx.Users.Find(_userId);
                    if (user != null)
                    {
                        score = user.InitialCreditScore;
                    }
                }
                
                // Find score controls by looking through the form
                foreach (Control ctrl in this.Controls)
                {
                    // Check Credit Score tab
                    if (ctrl is TabControl tc)
                    {
                        foreach (TabPage tp in tc.TabPages)
                        {
                            if (tp.Text.Contains("Credit"))
                            {
                                foreach (Control pageCtrl in tp.Controls)
                                {
                                    if (pageCtrl is Panel mainPanel)
                                    {
                                        foreach (Control panel in mainPanel.Controls)
                                        {
                                            // Score display panel
                                            foreach (Control c in panel.Controls)
                                            {
                                                if (c.Name == "ScoreValue" && c is Label lblScore)
                                                {
                                                    lblScore.Text = score.ToString();
                                                    if (score >= 80) lblScore.ForeColor = Color.FromArgb(0, 150, 80);
                                                    else if (score >= 60) lblScore.ForeColor = Color.FromArgb(200, 150, 0);
                                                    else lblScore.ForeColor = Color.FromArgb(200, 50, 50);
                                                }
                                                if (c.Name == "ScoreProgress" && c is ProgressBar pb)
                                                {
                                                    pb.Value = Math.Min(100, Math.Max(0, score));
                                                }
                                                if (c.Name == "ScoreDesc" && c is Label lblDesc)
                                                {
                                                    if (score > 0)
                                                    {
                                                        if (score >= 80) lblDesc.Text = "Excellent! Your credit profile is strong.";
                                                        else if (score >= 60) lblDesc.Text = "Good. Keep improving your financial habits.";
                                                        else lblDesc.Text = "Fair. Consider taking the quiz to improve.";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Silently fail - score display will show default values
            }
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

        private void BtnSubmitPayment_Click(object? sender, EventArgs e)
        {
            // Validate loan selection
            if (cmbPaymentLoan.SelectedItem == null)
            {
                MessageBox.Show("Please select a loan to make payment for.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate amount
            if (!decimal.TryParse(txtPaymentAmount.Text, out var amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid payment amount.", "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get selected loan
            var selectedLoan = cmbPaymentLoan.SelectedItem as LoanItem;
            if (selectedLoan == null)
            {
                MessageBox.Show("Invalid loan selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var loanId = selectedLoan.Id;
            decimal outstanding = 0;

            try
            {
                using var ctx = new MicroLendDbContext();

                // Verify loan exists and belongs to borrower
                var borrower = ctx.Borrowers.FirstOrDefault(b => b.UserId == _userId);
                if (borrower == null)
                {
                    MessageBox.Show("Borrower profile not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var loan = ctx.Loans.FirstOrDefault(l => l.Id == loanId && l.BorrowerId == borrower.Id);
                if (loan == null)
                {
                    MessageBox.Show("Loan not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Check total repayments for this loan
                var totalRepaid = ctx.Repayments.Where(r => r.LoanId == loanId).Sum(r => (decimal?)r.Amount) ?? 0;
                outstanding = loan.TargetAmount - totalRepaid;

                if (amount > outstanding)
                {
                    var result = MessageBox.Show($"The outstanding balance is ₱{outstanding:N2}.\nDo you want to pay the full amount?", "Overpayment", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        amount = outstanding;
                    }
                    else
                    {
                        return;
                    }
                }

                // Show confirmation dialog before processing payment
                var confirmResult = MessageBox.Show(
                    $"Please confirm your payment details:\n\n" +
                    $"Loan: {loan.Purpose} (ID: {loan.Id})\n" +
                    $"Amount: ₱{amount:N2}\n" +
                    $"Payment Method: Online\n\n" +
                    "Do you want to proceed with this payment?",
                    "Confirm Payment",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult != DialogResult.Yes)
                {
                    MessageBox.Show("Payment cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Create repayment record
                var repayment = new MicroLend.DAL.Entities.Repayment
                {
                    LoanId = loanId,
                    Amount = amount,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = "Online",
                    PaymentReference = "PAY-" + DateTime.Now.ToString("yyyyMMddHHmmss")
                };

                ctx.Repayments.Add(repayment);
                ctx.SaveChanges();

                // Clear the amount field
                txtPaymentAmount.Text = string.Empty;

                // Show success message with payment reference
                MessageBox.Show(
                    $"Payment submitted successfully!\n\n" +
                    $"Payment Reference: {repayment.PaymentReference}\n" +
                    $"Amount: ₱{amount:N2}\n" +
                    $"Date: {repayment.PaymentDate:g}\n\n" +
                    "The dashboard will now refresh to show the updated balances.",
                    "Payment Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Refresh all data in real-time
                RefreshBorrowerData();

                // Also reload repayments grid to show the new payment
                _ = LoadRepaymentsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing payment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper class for loan dropdown items
        private class LoanItem
        {
            public int Id { get; set; }
            public string DisplayText { get; set; } = string.Empty;
            public override string ToString() => DisplayText;
        }

        private void PopulatePaymentLoanDropdown()
        {
            try
            {
                if (!EnsureDatabaseMigrated()) return;

                using var ctx = new MicroLendDbContext();
                var borrower = ctx.Borrowers.FirstOrDefault(b => b.UserId == _userId);
                if (borrower == null) return;

                var loans = ctx.Loans
                    .Where(l => l.BorrowerId == borrower.Id && l.Status != "Pending")
                    .ToList();

                // Get repayments for each loan to calculate outstanding
                var loanItems = loans.Select(l =>
                {
                    var totalRepaid = ctx.Repayments
                        .Where(r => r.LoanId == l.Id)
                        .Sum(r => (decimal?)r.Amount) ?? 0;
                    var outstanding = l.TargetAmount - totalRepaid;
                    return new LoanItem
                    {
                        Id = l.Id,
                        DisplayText = $"Loan #{l.Id} - {l.Purpose} (Outstanding: ₱{outstanding:N2})"
                    };
                }).ToList();

                cmbPaymentLoan.Items.Clear();
                cmbPaymentLoan.Items.AddRange(loanItems.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading loans: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void RefreshBorrowerData()
        {
            try
            {
                // Refresh loans - find the DataGridView directly
                if (dgvMyLoans != null)
                {
                    using var ctx = new MicroLendDbContext();
                    var borrower = ctx.Borrowers.FirstOrDefault(b => b.UserId == _userId);
                    if (borrower != null)
                    {
                        var loans = ctx.Loans
                            .Where(l => l.BorrowerId == borrower.Id)
                            .Select(l => new {
                                l.Id,
                                l.Purpose,
                                l.TargetAmount,
                                l.CurrentAmount,
                                l.Status,
                                l.RiskScore,
                                l.CreatedAt
                            })
                            .ToList();
                        dgvMyLoans.DataSource = loans;
                    }
                }
                
                // Refresh summary
                using var ctx2 = new MicroLendDbContext();
                var borrower2 = ctx2.Borrowers.FirstOrDefault(b => b.UserId == _userId);
                if (borrower2 != null)
                {
                    var loans = ctx2.Loans.Where(l => l.BorrowerId == borrower2.Id).ToList();
                    var totalBorrowed = loans.Sum(l => l.TargetAmount);
                    var repayments = ctx2.Repayments.Where(r => loans.Select(l => l.Id).Contains(r.LoanId)).ToList();
                    var totalRepaid = repayments.Sum(r => r.Amount);
                    var outstanding = totalBorrowed - totalRepaid;
                    var lendedMoney = loans.Sum(l => l.CurrentAmount);

                    if (lblTotalBorrowed != null) lblTotalBorrowed.Text = $"₱{totalBorrowed:N2}";
                    if (lblTotalRepaid != null) lblTotalRepaid.Text = $"₱{totalRepaid:N2}";
                    if (lblOutstanding != null) lblOutstanding.Text = $"₱{outstanding:N2}";
                    if (lblLendedMoney != null) lblLendedMoney.Text = $"₱{lendedMoney:N2}";
                }
                
                // Refresh credit score
                LoadCreditScoreAsync();

                // Refresh payment loan dropdown
                PopulatePaymentLoanDropdown();

                MessageBox.Show("Data refreshed successfully!", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
