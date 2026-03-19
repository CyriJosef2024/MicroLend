using System;
using System.Drawing;
using System.Windows.Forms;

namespace MicroLend.UI
{
    // UI inspired layout (buttons, navigation, login card, header) - placeholder content only
    public class EmergencyPoolForm : Form
    {
        public EmergencyPoolForm()
        {
            Text = "MicroLend - Emergency Pool";
            Width = 980;
            Height = 620;
            StartPosition = FormStartPosition.CenterParent;

            // Header banner
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 88,
                BackColor = Color.FromArgb(10, 106, 179) // deep blue
            };

            var logo = new Label
            {
                Text = "MICROLEND",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                Location = new Point(16, 18),
                AutoSize = true
            };
            header.Controls.Add(logo);

            // right-side small links area in header
            var headerLinks = new Label
            {
                Text = "Help | FAQ | Contact",
                ForeColor = Color.WhiteSmoke,
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                Location = new Point(Width - 260, 32)
            };
            header.Controls.Add(headerLinks);

            Controls.Add(header);

            // Main content
            var main = new Panel { Dock = DockStyle.Fill, BackColor = Color.WhiteSmoke };

            // Left column - big welcome and info cards
            var leftCol = new Panel { Width = 560, Dock = DockStyle.Left, Padding = new Padding(24) };

            var title = new Label
            {
                Text = "Welcome to MicroLend",
                Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                ForeColor = Color.FromArgb(10, 106, 179),
                AutoSize = true,
                Location = new Point(8, 8)
            };
            leftCol.Controls.Add(title);

            var intro = new Label
            {
                Text = "A simple, responsive interface for community lending operations.",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.DimGray,
                AutoSize = false,
                Size = new Size(500, 60),
                Location = new Point(10, 72)
            };
            leftCol.Controls.Add(intro);

            // three small info boxes similar to the reference layout
            var infoPanel = new FlowLayoutPanel
            {
                Location = new Point(10, 150),
                Size = new Size(520, 140),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            infoPanel.Controls.Add(CreateInfoBox("Realtime Alerts", "Notify members of repayments and events."));
            infoPanel.Controls.Add(CreateInfoBox("Security", "Secure data and role-based access."));
            infoPanel.Controls.Add(CreateInfoBox("Rewards", "Track impact and dividends."));
            leftCol.Controls.Add(infoPanel);

            main.Controls.Add(leftCol);

            // Right column - login / quick actions card
            var rightCol = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };

            var loginCard = new Panel
            {
                Size = new Size(320, 240),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(24, 48)
            };

            var lblUid = new Label { Text = "User ID", Location = new Point(16, 16), AutoSize = true };
            var txtUid = new TextBox { Location = new Point(16, 36), Width = 272, PlaceholderText = "User ID" };
            var lblPwd = new Label { Text = "Password", Location = new Point(16, 72), AutoSize = true };
            var txtPwd = new TextBox { Location = new Point(16, 92), Width = 272, UseSystemPasswordChar = true, PlaceholderText = "Password" };

            var btnLogin = new Button
            {
                Text = "Login",
                BackColor = Color.FromArgb(110, 195, 74), // green
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(16, 132),
                Size = new Size(120, 36)
            };
            btnLogin.FlatAppearance.BorderSize = 0;

            var btnEnroll = new Button
            {
                Text = "ENROLL NOW",
                Location = new Point(160, 132),
                Size = new Size(128, 36)
            };
            btnEnroll.Click += (s, e) => ShowEnrollmentOptions();

            loginCard.Controls.Add(lblUid);
            loginCard.Controls.Add(txtUid);
            loginCard.Controls.Add(lblPwd);
            loginCard.Controls.Add(txtPwd);
            loginCard.Controls.Add(btnLogin);
            loginCard.Controls.Add(btnEnroll);

            // small note link under card
            var lnkForgot = new LinkLabel { Text = "Forgot Password?", Location = new Point(16, 182), AutoSize = true };
            loginCard.Controls.Add(lnkForgot);

            rightCol.Controls.Add(loginCard);
            main.Controls.Add(rightCol);

            Controls.Add(main);
        }

        private Control CreateInfoBox(string title, string subtitle)
        {
            var p = new Panel { Size = new Size(160, 120), Margin = new Padding(6), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            var t = new Label { Text = title, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(8, 8), AutoSize = true };
            var s = new Label { Text = subtitle, Font = new Font("Segoe UI", 9F), Location = new Point(8, 36), Size = new Size(140, 64), ForeColor = Color.DimGray };
            p.Controls.Add(t);
            p.Controls.Add(s);
            return p;
        }

        // Shows a simple modal with enrollment options (UI behavior only)
        private void ShowEnrollmentOptions()
        {
            using var dlg = new Form();
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.Size = new Size(420, 240);
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.Text = "Choose enrollment type";

            var header = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.FromArgb(10, 106, 179) };
            var hlbl = new Label { Text = "Please choose the type of account to enroll", ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, Location = new Point(12, 12) };
            header.Controls.Add(hlbl);
            dlg.Controls.Add(header);

            var lb = new ListBox { Location = new Point(16, 64), Size = new Size(372, 100) };
            lb.Items.Add("Bank Account");
            lb.Items.Add("Credit Card");
            lb.Items.Add("Mobile Wallet");
            dlg.Controls.Add(lb);

            var btnOk = new Button { Text = "OK", Location = new Point(220, 174), Size = new Size(80, 28) };
            btnOk.Click += (s, e) => dlg.DialogResult = DialogResult.OK;
            dlg.Controls.Add(btnOk);

            var btnCancel = new Button { Text = "Cancel", Location = new Point(312, 174), Size = new Size(80, 28) };
            btnCancel.Click += (s, e) => dlg.DialogResult = DialogResult.Cancel;
            dlg.Controls.Add(btnCancel);

            dlg.ShowDialog(this);
        }
    }
}
