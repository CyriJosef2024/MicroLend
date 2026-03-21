using System;
using System.Drawing;
using System.Windows.Forms;

namespace MicroLend.UI
{
    public class LandingPageForm : Form
    {
        private Button btnLogin;
        private Button btnSignup;
        
        public LandingPageForm()
        {
            Text = "MicroLend - Community Micro-Lending";
            Width = 1000;
            Height = 680;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(245, 245, 245);
            
            // Main container - fixed layout
            var mainPanel = new Panel
            { 
                Dock = DockStyle.Fill,
                Size = new Size(1000, 680)
            };
            
            // Header with Logo and Buttons - Fixed position
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1000, 60),
                BackColor = Color.White
            };
            headerPanel.Paint += (s, e) => {
                using var p = new Pen(Color.FromArgb(200, 200, 200));
                e.Graphics.DrawLine(p, 0, 59, 1000, 59);
            };
            
            // Logo/Title on left - fixed position
            var lblTitle = new Label
            {
                Text = "🏦 MicroLend",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(25, 12),
                AutoSize = true
            };
            
            // Buttons on right
            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(760, 12),
                Size = new Size(90, 36),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            
            btnSignup = new Button
            {
                Text = "Sign Up",
                Location = new Point(860, 12),
                Size = new Size(100, 36),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSignup.FlatAppearance.BorderSize = 0;
            btnSignup.Click += BtnSignup_Click;
            
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(btnLogin);
            headerPanel.Controls.Add(btnSignup);
            mainPanel.Controls.Add(headerPanel);
            
            // Welcome Section - reduced spacing
            var lblWelcome = new Label
            {
                Text = "Welcome to MicroLend",
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(290, 75),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblWelcome);
            
            var lblTagline = new Label
            {
                Text = "Empowering Communities Through Micro-Lending",
                Font = new Font("Segoe UI", 14),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(295, 110),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblTagline);
            
            // Benefits Title - reduced spacing
            var lblBenefitsTitle = new Label
            {
                Text = "Why Choose MicroLend?",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(370, 150),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblBenefitsTitle);
            
            // Benefits Grid - Fixed positions with tighter spacing (14px icons)
            var benefits = new (string icon, string title, string desc)[]
            {
                ("💰", "Low-Interest Loans", "Access affordable micro-loans with competitive rates for your needs."),
                ("🤝", "Community-Powered", "Join a trusted community supporting each other's growth."),
                ("🔒", "Secure & Reliable", "Your financial data is protected with secure measures."),
                ("📊", "Transparent Process", "Track loans and investments in real-time."),
                ("📱", "Easy Management", "User-friendly dashboards for all your needs."),
                ("⚡", "Fast Approvals", "Quick processing to get you funded when needed.")
            };
            
            int[] xPos = { 50, 530 };
            int[] yPos = { 185, 185, 275, 275, 365, 365 };
            
            for (int i = 0; i < benefits.Length; i++)
            {
                var (icon, title, desc) = benefits[i];
                var card = CreateBenefitCard(icon, title, desc, xPos[i % 2], yPos[i]);
                mainPanel.Controls.Add(card);
            }
            
            // Roles Title - reduced spacing
            var rolesTitle = new Label
            {
                Text = "Choose Your Path",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(390, 455),
                AutoSize = true
            };
            mainPanel.Controls.Add(rolesTitle);
            
            // Borrower Card - reduced spacing
            var borrowerCard = CreateRoleCard(
                "📋 For Borrowers",
                "• Apply for micro-loans up to ₱100,000\n" +
                "• Track your loan status in real-time\n" +
                "• Make easy repayments\n" +
                "• Build your credit score",
                Color.FromArgb(0, 150, 136),
                100,
                490
            );
            mainPanel.Controls.Add(borrowerCard);
            
            // Lender Card - reduced spacing
            var lenderCard = CreateRoleCard(
                "💵 For Lenders",
                "• Browse and fund loan opportunities\n" +
                "• Earn returns on investments\n" +
                "• Support community businesses\n" +
                "• Track your portfolio",
                Color.FromArgb(255, 152, 0),
                530,
                490
            );
            mainPanel.Controls.Add(lenderCard);
            
            // Copyright Footer - Fixed at bottom
            var footerPanel = new Panel
            {
                Location = new Point(0, 630),
                Size = new Size(1000, 50),
                BackColor = Color.FromArgb(0, 102, 204)
            };
            
            var lblCopyright = new Label
            {
                Text = "Copyright © 2026 MicroLend. All rights reserved.",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White,
                Location = new Point(330, 15),
                AutoSize = true
            };
            
            footerPanel.Controls.Add(lblCopyright);
            mainPanel.Controls.Add(footerPanel);
            
            Controls.Add(mainPanel);
        }
        
        private Panel CreateBenefitCard(string icon, string title, string description, int x, int y)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(450, 75),
                BackColor = Color.White,
                Padding = new Padding(8)
            };
            card.Paint += (s, e) => {
                using var p = new Pen(Color.FromArgb(210, 210, 210));
                e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
            };
            
            // 14px icon as specified
            var iconLabel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 14),
                Location = new Point(12, 18),
                AutoSize = true
            };
            
            // 14px font for title
            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                Location = new Point(40, 10),
                AutoSize = true
            };
            
            // 12px font for description
            var descLabel = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(90, 90, 90),
                Location = new Point(40, 32),
                Size = new Size(400, 35),
                AutoSize = false
            };
            
            card.Controls.Add(iconLabel);
            card.Controls.Add(titleLabel);
            card.Controls.Add(descLabel);
            
            return card;
        }
        
        private Panel CreateRoleCard(string title, string features, Color accentColor, int x, int y)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(420, 115),
                BackColor = Color.White,
                Padding = new Padding(12)
            };
            card.Paint += (s, e) => {
                using var p = new Pen(accentColor, 3);
                e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
            };
            
            // 14px font for title
            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(12, 8),
                AutoSize = true
            };
            
            // 12px font for features
            var featuresLabel = new Label
            {
                Text = features,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(51, 51, 51),
                Location = new Point(12, 32),
                Size = new Size(395, 75),
                AutoSize = false
            };
            
            card.Controls.Add(titleLabel);
            card.Controls.Add(featuresLabel);
            
            return card;
        }
        
        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            var loginForm = new Form1();
            loginForm.ShowDialog();
        }
        
        private void BtnSignup_Click(object? sender, EventArgs e)
        {
            var signupForm = new SignupForm();
            signupForm.ShowDialog();
        }
    }
}
