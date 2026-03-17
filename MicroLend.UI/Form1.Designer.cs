namespace MicroLend.UI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            btnLogin = new Button();
            dgvLoans = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvLoans).BeginInit();
            SuspendLayout();
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(12, 12);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "Username";
            txtUsername.Size = new Size(200, 23);
            txtUsername.TabIndex = 0;
            txtUsername.TextChanged += txtUsername_TextChanged;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(12, 41);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "Password";
            txtPassword.Size = new Size(200, 23);
            txtPassword.TabIndex = 1;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(218, 12);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(75, 52);
            btnLogin.TabIndex = 2;
            btnLogin.Text = "Login";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += BtnLogin_Click;
            // 
            // btnListUsers
            // 
            btnListUsers = new Button();
            btnListUsers.Location = new Point(299, 12);
            btnListUsers.Name = "btnListUsers";
            btnListUsers.Size = new Size(100, 52);
            btnListUsers.TabIndex = 4;
            btnListUsers.Text = "List Users";
            btnListUsers.UseVisualStyleBackColor = true;
            btnListUsers.Click += BtnListUsers_Click;
            // 
            // txtLoanPurpose
            // 
            txtLoanPurpose = new TextBox();
            txtLoanPurpose.Location = new Point(12, 80);
            txtLoanPurpose.Name = "txtLoanPurpose";
            txtLoanPurpose.Size = new Size(200, 23);
            txtLoanPurpose.PlaceholderText = "Loan purpose (e.g. sewing machine)";
            // 
            // txtLoanAmount
            // 
            txtLoanAmount = new TextBox();
            txtLoanAmount.Location = new Point(218, 80);
            txtLoanAmount.Name = "txtLoanAmount";
            txtLoanAmount.Size = new Size(100, 23);
            txtLoanAmount.PlaceholderText = "Amount";
            // 
            // btnCreateLoan
            // 
            btnCreateLoan = new Button();
            btnCreateLoan.Location = new Point(324, 78);
            btnCreateLoan.Name = "btnCreateLoan";
            btnCreateLoan.Size = new Size(75, 26);
            btnCreateLoan.Text = "Create Loan";
            btnCreateLoan.UseVisualStyleBackColor = true;
            btnCreateLoan.Click += BtnCreateLoan_Click;
            // 
            // txtRepaymentAmount
            // 
            txtRepaymentAmount = new TextBox();
            txtRepaymentAmount.Location = new Point(405, 80);
            txtRepaymentAmount.Name = "txtRepaymentAmount";
            txtRepaymentAmount.Size = new Size(100, 23);
            txtRepaymentAmount.PlaceholderText = "Repayment";
            // 
            // btnMakeRepayment
            // 
            btnMakeRepayment = new Button();
            btnMakeRepayment.Location = new Point(511, 78);
            btnMakeRepayment.Name = "btnMakeRepayment";
            btnMakeRepayment.Size = new Size(100, 26);
            btnMakeRepayment.Text = "Make Repayment";
            btnMakeRepayment.UseVisualStyleBackColor = true;
            btnMakeRepayment.Click += BtnMakeRepayment_Click;
            // 
            // dgvLoans
            // 
            dgvLoans.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvLoans.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLoans.Location = new Point(12, 110);
            dgvLoans.Name = "dgvLoans";
            dgvLoans.Size = new Size(760, 360);
            dgvLoans.TabIndex = 3;
            dgvLoans.SelectionChanged += DgvLoans_SelectionChanged;

            // 
            // lblSelectedLoan
            // 
            lblSelectedLoan = new Label();
            lblSelectedLoan.Location = new Point(12, 470);
            lblSelectedLoan.Size = new Size(400, 23);
            lblSelectedLoan.Name = "lblSelectedLoan";
            lblSelectedLoan.Text = "Selected: None";

            // 
            // lblPrediction
            // 
            lblPrediction = new Label();
            lblPrediction.Location = new Point(420, 470);
            lblPrediction.Size = new Size(352, 23);
            lblPrediction.Name = "lblPrediction";
            lblPrediction.Text = "Prediction: N/A";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 520);
            Controls.Add(dgvLoans);
            Controls.Add(btnMakeRepayment);
            Controls.Add(txtRepaymentAmount);
            Controls.Add(btnCreateLoan);
            Controls.Add(txtLoanAmount);
            Controls.Add(txtLoanPurpose);
            Controls.Add(lblSelectedLoan);
            Controls.Add(lblPrediction);
            // 
            // btnApproveLoan
            // 
            btnApproveLoan = new Button();
            btnApproveLoan.Location = new Point(617, 78);
            btnApproveLoan.Name = "btnApproveLoan";
            btnApproveLoan.Size = new Size(100, 26);
            btnApproveLoan.Text = "Approve Loan";
            btnApproveLoan.UseVisualStyleBackColor = true;
            btnApproveLoan.Click += BtnApproveLoan_Click;
            Controls.Add(btnApproveLoan);
            Controls.Add(btnLogin);
            Controls.Add(btnListUsers);
            Controls.Add(txtPassword);
            Controls.Add(txtUsername);
            Name = "Form1";
            Text = "MicroLend";
            ((System.ComponentModel.ISupportInitialize)dgvLoans).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
