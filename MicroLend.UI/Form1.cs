using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicroLend.UI
{
    public partial class Form1 : Form
    {
        // Controls are created in the Designer partial but we declare them here so the compiler sees them
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnListUsers;
        private System.Windows.Forms.TextBox txtLoanPurpose;
        private System.Windows.Forms.TextBox txtLoanAmount;
        private System.Windows.Forms.Button btnCreateLoan;
        private System.Windows.Forms.TextBox txtRepaymentAmount;
        private System.Windows.Forms.Button btnMakeRepayment;
        private System.Windows.Forms.DataGridView dgvLoans;
        private System.Windows.Forms.Label lblSelectedLoan;
        private System.Windows.Forms.Label lblPrediction;
        private System.Windows.Forms.Button btnApproveLoan;

        private readonly MicroLend.DAL.Repositories.UserRepository _userRepo = new MicroLend.DAL.Repositories.UserRepository();
        private readonly MicroLend.DAL.Repositories.BorrowerRepository _borrowerRepo = new MicroLend.DAL.Repositories.BorrowerRepository();
        private readonly MicroLend.DAL.Repositories.LoanRepository _loanRepo = new MicroLend.DAL.Repositories.LoanRepository();

        private MicroLend.DAL.Entities.Borrower? _currentBorrower;
        private MicroLend.DAL.Entities.User? _currentUser;

        public Form1()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            try
            {
                var username = (txtUsername.Text ?? string.Empty).Trim();
                var password = txtPassword.Text ?? string.Empty;

                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Please enter username or user id.");
                    return;
                }

                var allUsers = await _userRepo.GetAllAsync();
                var user = allUsers.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
                if (user == null && int.TryParse(username, out var uid)) user = allUsers.FirstOrDefault(u => u.Id == uid);

                if (user == null)
                {
                    MessageBox.Show("User not found. Click 'List Users' to see available users.");
                    return;
                }

                var stored = (user.PasswordHash ?? string.Empty).Trim();
                var entered = (password ?? string.Empty).Trim();
                if (!string.Equals(stored, entered, StringComparison.Ordinal))
                {
                    MessageBox.Show($"Invalid credentials. Entered length={entered.Length}, stored length={stored.Length}.");
                    return;
                }

                // record current user
                _currentUser = user;

                // If user is a borrower, load borrower profile; lenders can log in without borrower profile
                var borrowers = await _borrowerRepo.GetAllAsync();
                var borrower = borrowers.FirstOrDefault(b => b.UserId == user.Id);
                if (borrower != null)
                {
                    _currentBorrower = borrower;
                    await LoadBorrowerLoansAsync(borrower.Id);
                }
                else
                {
                    // no borrower linked; allow login for lender/officer/admin roles
                    _currentBorrower = null;
                    MessageBox.Show($"Signed in as {user.Username} ({user.Role}). No borrower profile linked.");
                }

                UpdateSignedInState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login error: " + ex.Message);
            }
        }

        private void UpdateSignedInState()
        {
            try
            {
                var lbl = this.Controls.Find("lblCurrentUser", true).FirstOrDefault() as Label;
                if (_currentUser != null)
                {
                    lbl.Text = $"Signed in: {_currentUser.Username} ({_currentUser.Role})";
                }
                else
                {
                    lbl.Text = "Not signed in";
                }
            }
            catch { }
        }

        private async void BtnSignUp_Click(object? sender, EventArgs e)
        {
            try
            {
                using var f = new SignupForm();
                if (f.ShowDialog(this) != DialogResult.OK) return;

                var user = new MicroLend.DAL.Entities.User
                {
                    Username = f.Username,
                    PasswordHash = f.Password,
                    Role = f.Role,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                var repo = new MicroLend.DAL.Repositories.Repository<MicroLend.DAL.Entities.User>();
                await repo.AddAsync(user);

                if (f.Role == "Borrower")
                {
                    var borrower = new MicroLend.DAL.Entities.Borrower
                    {
                        UserId = user.Id,
                        Name = f.FullName,
                        ContactNumber = f.Contact,
                        MonthlyIncome = f.MonthlyIncome,
                        BusinessType = f.BusinessType
                    };
                    var brepo = new MicroLend.DAL.Repositories.BorrowerRepository();
                    await brepo.AddAsync(borrower);
                }

                MessageBox.Show("User created. You can now log in.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sign up error: " + ex.Message);
            }
        }

        private void BtnSignOut_Click(object? sender, EventArgs e)
        {
            _currentUser = null;
            _currentBorrower = null;
            UpdateSignedInState();
            try { dgvLoans.DataSource = null; } catch { }
            MessageBox.Show("Signed out.");
        }

        private async Task LoadBorrowerLoansAsync(int borrowerId)
        {
            var loans = await _loanRepo.GetLoansByBorrowerAsync(borrowerId);
            dgvLoans.DataSource = loans.Select(l => new
            {
                l.Id,
                l.Purpose,
                Target = l.TargetAmount,
                Current = l.CurrentAmount,
                l.InterestRate,
                l.Status,
                l.IsCrowdfunded,
                l.DateGranted
            }).ToList();
        }

        private async void BtnListUsers_Click(object? sender, EventArgs e)
        {
            try
            {
                var users = await _userRepo.GetAllAsync();
                if (users == null || !users.Any())
                {
                    MessageBox.Show("No users found in database.");
                    return;
                }

                using var dlg = new Form();
                dlg.Text = "Select user (dev)";
                dlg.Size = new System.Drawing.Size(420, 420);
                var lb = new ListBox { Dock = DockStyle.Top, Height = 340 };
                var userMap = users.ToDictionary(u => $"{u.Id}: {u.Username} ({u.Role})", u => u);
                foreach (var key in userMap.Keys) lb.Items.Add(key);
                dlg.Controls.Add(lb);
                var btnOk = new Button() { Text = "OK", Dock = DockStyle.Bottom, Height = 30 };
                dlg.Controls.Add(btnOk);

                btnOk.Click += (s, ea) => dlg.DialogResult = DialogResult.OK;
                lb.DoubleClick += (s, ea) => dlg.DialogResult = DialogResult.OK;

                if (dlg.ShowDialog() == DialogResult.OK && lb.SelectedItem != null)
                {
                    var key = lb.SelectedItem.ToString()!;
                    if (userMap.TryGetValue(key, out var selected))
                    {
                        txtUsername.Text = selected.Username;
                        txtPassword.Text = selected.PasswordHash;
                        MessageBox.Show($"Autofilled username and password for user id={selected.Id}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error listing users: " + ex.Message);
            }
        }

        private async void BtnCreateLoan_Click(object? sender, EventArgs e)
        {
            if (_currentBorrower == null)
            {
                MessageBox.Show("Please login as a borrower first.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtLoanPurpose.Text) || !decimal.TryParse(txtLoanAmount.Text, out var amount) || amount <= 0)
            {
                MessageBox.Show("Enter valid purpose and amount.");
                return;
            }

            var loan = new MicroLend.DAL.Entities.Loan
            {
                Purpose = txtLoanPurpose.Text.Trim(),
                TargetAmount = amount,
                Amount = amount,
                CurrentAmount = 0,
                InterestRate = 5.0m,
                Status = "Funding",
                IsCrowdfunded = true,
                BorrowerId = _currentBorrower.Id,
                DateGranted = null,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            await _loanRepo.AddAsync(loan);
            MessageBox.Show("Loan created.");
            await LoadBorrowerLoansAsync(_currentBorrower.Id);
        }

        private async void BtnMakeRepayment_Click(object? sender, EventArgs e)
        {
            if (_currentBorrower == null)
            {
                MessageBox.Show("Please login as a borrower first.");
                return;
            }
            if (!decimal.TryParse(txtRepaymentAmount.Text, out var amount) || amount <= 0)
            {
                MessageBox.Show("Enter a valid repayment amount.");
                return;
            }

            MicroLend.DAL.Entities.Loan? loan = null;
            if (dgvLoans.CurrentRow?.DataBoundItem != null)
            {
                var row = dgvLoans.CurrentRow.DataBoundItem;
                var idProp = row.GetType().GetProperty("Id");
                if (idProp != null)
                {
                    var idVal = idProp.GetValue(row);
                    if (idVal is int selId) loan = await _loanRepo.GetByIdAsync(selId);
                }
            }

            if (loan == null)
            {
                var activeLoans = await _loanRepo.GetActiveLoansByBorrowerAsync(_currentBorrower.Id);
                loan = activeLoans.FirstOrDefault();
            }

            if (loan == null)
            {
                MessageBox.Show("No active loan to repay.");
                return;
            }

            if (loan.BorrowerId != _currentBorrower.Id)
            {
                MessageBox.Show("Selected loan does not belong to the logged in borrower.");
                return;
            }

            var repayment = new MicroLend.DAL.Entities.Repayment
            {
                LoanId = loan.Id,
                Amount = amount,
                PaymentDate = DateTime.Now,
                UserId = _currentBorrower.UserId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // ask for payment method
            using (var pm = new RepaymentMethodForm())
            {
                if (pm.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
                {
                    MessageBox.Show("Repayment cancelled: payment method not selected.");
                    return;
                }
                repayment.PaymentMethod = pm.Method;
                repayment.PaymentReference = pm.Reference;
            }

            var repaymentRepo = new MicroLend.DAL.Repositories.Repository<MicroLend.DAL.Entities.Repayment>();
            await repaymentRepo.AddAsync(repayment);

            loan.CurrentAmount -= amount;
            if (loan.CurrentAmount <= 0) loan.Status = "Paid";
            await _loanRepo.UpdateAsync(loan);

            MessageBox.Show("Repayment recorded.");
            await LoadBorrowerLoansAsync(_currentBorrower.Id);
        }

        private async void BtnApproveLoan_Click(object? sender, EventArgs e)
        {
            if (dgvLoans.CurrentRow?.DataBoundItem == null)
            {
                MessageBox.Show("Select a loan to approve.");
                return;
            }

            var row = dgvLoans.CurrentRow.DataBoundItem;
            var idProp = row.GetType().GetProperty("Id");
            if (idProp == null)
            {
                MessageBox.Show("Unable to determine selected loan id.");
                return;
            }

            var id = (int)idProp.GetValue(row)!;
            await _loanRepo.ActivateLoanAsync(id);
            MessageBox.Show($"Loan #{id} approved and activated.");

            if (_currentBorrower != null) await LoadBorrowerLoansAsync(_currentBorrower.Id);
        }

        private void DgvLoans_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvLoans.CurrentRow == null)
            {
                if (lblSelectedLoan != null) lblSelectedLoan.Text = "Selected: None";
                if (lblPrediction != null) lblPrediction.Text = "Prediction: N/A";
                return;
            }

            var row = dgvLoans.CurrentRow.DataBoundItem;
            if (row == null) return;
            var props = row.GetType().GetProperties();
            var idProp = props.FirstOrDefault(p => p.Name == "Id");
            if (idProp != null)
            {
                var id = (int)idProp.GetValue(row)!;
                if (lblSelectedLoan != null) lblSelectedLoan.Text = $"Selected: Loan #{id}";
                if (lblPrediction != null) lblPrediction.Text = $"Prediction: score not calculated";
            }
        }

        private void txtUsername_TextChanged(object sender, EventArgs e) { }

        // Navigation click handlers open lightweight placeholder forms
        private void BtnNavDashboard_Click(object? sender, EventArgs e)
        {
            // open appropriate dashboard based on signed-in role
            if (_currentUser == null)
            {
                MessageBox.Show("Please sign in to open dashboard.");
                return;
            }

            if (string.Equals(_currentUser.Role, "Borrower", StringComparison.OrdinalIgnoreCase))
            {
                var b = new BorrowerDashboardForm(_currentUser.Id);
                b.Show();
            }
            else if (string.Equals(_currentUser.Role, "Lender", StringComparison.OrdinalIgnoreCase))
            {
                var l = new LenderDashboardForm(_currentUser.Id);
                l.Show();
            }
            else
            {
                var a = new AdminDashboardForm();
                a.Show();
            }
        }

        private void BtnNavBorrowers_Click(object? sender, EventArgs e)
        {
            var f = new BorrowersForm();
            f.Show();
        }

        private void BtnNavLenders_Click(object? sender, EventArgs e)
        {
            var f = new LendersForm();
            f.Show();
        }

        private void BtnNavLoans_Click(object? sender, EventArgs e)
        {
            var f = new LoansForm();
            f.Show();
        }

        private void BtnNavCreditQuiz_Click(object? sender, EventArgs e)
        {
            var f = new CreditQuizForm();
            f.Show();
        }

        private void BtnNavEmergency_Click(object? sender, EventArgs e)
        {
            var f = new EmergencyPoolForm();
            f.Show();
        }
    }
}
