using System;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;
using MicroLend.DAL;
using MicroLend.DAL.Entities;

namespace MicroLend.UI
{
    public class LoansForm : Form
    {
        private readonly int _userId;
        private TextBox txtPurpose;
        private TextBox txtAmount;
        private Button btnUploadDoc;
        private Label lblUploaded;
        private ComboBox cmbStatus;
        private Button btnApply;
        private DataGridView dgvLoans;

        public LoansForm(int userId)
        {
            _userId = userId;
            InitializeComponent();
            LoadLoans();
        }

        private void InitializeComponent()
        {
            Text = "Apply for Loan - MicroLend";
            Width = 820;
            Height = 640;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(240, 248, 255);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var titleLabel = new Label
            {
                Text = "Apply for a Loan",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(300, 20),
                AutoSize = true
            };

            var formPanel = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(370, 360),
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            formPanel.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(0, 120, 215));
                e.Graphics.DrawRectangle(p, 0, 0, formPanel.Width - 1, formPanel.Height - 1);
            };

            var lblPurpose = new Label { Text = "Loan Purpose", Location = new Point(15, 15), AutoSize = true };
            txtPurpose = new TextBox { Location = new Point(15, 35), Width = 330 };

            var lblAmount = new Label { Text = "Amount (₱)", Location = new Point(15, 70), AutoSize = true };
            txtAmount = new TextBox { Location = new Point(15, 90), Width = 150 };

            var lblStatus = new Label { Text = "Loan Type", Location = new Point(180, 70), AutoSize = true };
            cmbStatus = new ComboBox { Location = new Point(180, 90), Width = 165, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new[] { "Personal", "Business", "Emergency", "Education" });
            cmbStatus.SelectedIndex = 0;

            btnApply = new Button
            {
                Text = "Submit Application",
                Location = new Point(15, 140),
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnApply.Click += BtnApply_Click;

            btnUploadDoc = new Button
            {
                Text = "Upload Requirements",
                Location = new Point(190, 140),
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnUploadDoc.Click += BtnUploadDoc_Click;

            lblUploaded = new Label { Text = "No document uploaded", Location = new Point(15, 200), AutoSize = true, ForeColor = Color.Gray };

            formPanel.Controls.Add(lblPurpose);
            formPanel.Controls.Add(txtPurpose);
            formPanel.Controls.Add(lblAmount);
            formPanel.Controls.Add(txtAmount);
            formPanel.Controls.Add(lblStatus);
            formPanel.Controls.Add(cmbStatus);
            formPanel.Controls.Add(btnApply);
            formPanel.Controls.Add(btnUploadDoc);
            formPanel.Controls.Add(lblUploaded);

            var listPanel = new Panel
            {
                Location = new Point(410, 70),
                Size = new Size(380, 520),
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblMyLoans = new Label
            {
                Text = "My Loan Applications",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            dgvLoans = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false
            };
            dgvLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Loan ID", DataPropertyName = "Id", Width = 60 });
            dgvLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "Purpose", HeaderText = "Purpose", DataPropertyName = "Purpose" });
            dgvLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "TargetAmount", HeaderText = "Amount (₱)", DataPropertyName = "TargetAmount", DefaultCellStyle = { Format = "N2" } });
            dgvLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status" });

            listPanel.Controls.Add(lblMyLoans);
            listPanel.Controls.Add(dgvLoans);

            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(700, 580),
                Size = new Size(100, 30)
            };
            btnClose.Click += (s, e) => Close();

            Controls.Add(titleLabel);
            Controls.Add(formPanel);
            Controls.Add(listPanel);
            Controls.Add(btnClose);
        }

        private void LoadLoans()
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                var borrower = ctx.Borrowers.FirstOrDefault(b => b.UserId == _userId);

                if (borrower == null)
                {
                    MessageBox.Show("Borrower profile not found. Please contact support.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var loans = ctx.Loans
                    .Where(l => l.BorrowerId == borrower.Id)
                    .Select(l => new
                    {
                        l.Id,
                        l.Purpose,
                        TargetAmount = l.TargetAmount,
                        CurrentAmount = l.CurrentAmount,
                        l.Status,
                        l.RiskScore
                    })
                    .ToList();

                dgvLoans.DataSource = loans;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading loans: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string? GetLocalApiToken()
        {
            try
            {
                var p = System.IO.Path.Combine(AppContext.BaseDirectory, "apitoken.txt");
                if (System.IO.File.Exists(p)) return System.IO.File.ReadAllText(p).Trim();
            }
            catch { }
            return null;
        }

        private void SaveUploadedDocumentMarker(int docId, string fileName)
        {
            try
            {
                var p = System.IO.Path.Combine(AppContext.BaseDirectory, "uploaded_docs.json");
                System.Collections.Generic.List<object> list = new();
                if (System.IO.File.Exists(p))
                {
                    var j = System.IO.File.ReadAllText(p);
                    list = JsonSerializer.Deserialize<System.Collections.Generic.List<object>>(j) ?? new();
                }
                list.Add(new { id = docId, file = fileName, uploaded = System.DateTime.Now });
                System.IO.File.WriteAllText(p, JsonSerializer.Serialize(list));
            }
            catch { }
        }

        private void BtnApply_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPurpose.Text))
            {
                MessageBox.Show("Please enter loan purpose.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtAmount.Text, out var amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var ctx = new MicroLendDbContext();
                var borrower = ctx.Borrowers.FirstOrDefault(b => b.UserId == _userId);

                if (borrower == null)
                {
                    MessageBox.Show("Borrower profile not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var hasDoc = ctx.Documents.Any(d => d.UserId == _userId);
                if (!hasDoc)
                {
                    MessageBox.Show("You must upload required documents before applying. Use 'Upload Requirements' button.", "Requirements Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var loan = new Loan
                {
                    Purpose = $"[{cmbStatus.SelectedItem}] {txtPurpose.Text}",
                    TargetAmount = amount,
                    Amount = amount,
                    CurrentAmount = 0,
                    Status = "Pending",
                    InterestRate = 5.0m,
                    IsCrowdfunded = true,
                    BorrowerId = borrower.Id,
                    RiskScore = 0,
                    DateGranted = null // DateGranted will be set when lender funds and admin approves
                };

                ctx.Loans.Add(loan);
                ctx.SaveChanges();

                MessageBox.Show("Loan application submitted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtPurpose.Clear();
                txtAmount.Clear();

                LoadLoans();

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error submitting loan application: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnUploadDoc_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "PDF or Image|*.pdf;*.jpg;*.jpeg;*.png|All files|*.*" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var client = new HttpClient();
                using var fs = System.IO.File.OpenRead(ofd.FileName);
                using var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(fs);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                content.Add(streamContent, "file", System.IO.Path.GetFileName(ofd.FileName));

                // Try local web project URL(s). The Web project's launchSettings uses ports 54433/54434 by default.
                var candidateUrls = new[] { "http://localhost:54434/Borrower/UploadDocument", "https://localhost:54433/Borrower/UploadDocument" };
                string url = candidateUrls[0];
                var token = GetLocalApiToken();
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    content.Headers.Add("X-Requested-With", "XMLHttpRequest");
                }
                System.Net.Http.HttpResponseMessage? resp = null;
                System.Exception? lastEx = null;
                foreach (var tryUrl in candidateUrls)
                {
                    try
                    {
                        resp = await client.PostAsync(tryUrl, content);
                        url = tryUrl;
                        break;
                    }
                    catch (System.Net.Http.HttpRequestException ex)
                    {
                        // try next URL
                        lastEx = ex;
                    }
                }
                if (resp == null)
                {
                    // couldn't reach web API; fall back to saving document locally
                    resp = null;
                }

                if (resp == null || !resp.IsSuccessStatusCode)
                {
                    // Fallback: save file into local uploads and persist Documents table so user can proceed offline
                    try
                    {
                        using var ctx = new MicroLendDbContext();
                        var doc = new MicroLend.DAL.Entities.Document
                        {
                            UserId = _userId,
                            FileName = System.IO.Path.GetFileName(ofd.FileName),
                            UploadedAt = DateTime.Now
                        };
                        ctx.Documents.Add(doc);
                        ctx.SaveChanges();

                        var dest = System.IO.Path.Combine(AppContext.BaseDirectory, "uploads");
                        System.IO.Directory.CreateDirectory(dest);
                        var dst = System.IO.Path.Combine(dest, doc.Id + "_" + doc.FileName);
                        System.IO.File.Copy(ofd.FileName, dst, true);
                        // update stored path
                        doc.FilePath = dst;
                        ctx.SaveChanges();

                        SaveUploadedDocumentMarker(doc.Id, ofd.FileName);
                        lblUploaded.Text = "Document saved locally (id: " + doc.Id + ")";
                        MessageBox.Show("Server unavailable — document saved locally. It will sync when the web service is available.", "Offline Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving document locally: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                var json = await resp.Content.ReadAsStringAsync();
                try
                {
                    var obj = JsonDocument.Parse(json);
                    if (obj.RootElement.TryGetProperty("id", out var idEl))
                    {
                        var docId = idEl.GetInt32();
                        SaveUploadedDocumentMarker(docId, ofd.FileName);
                        lblUploaded.Text = "Document uploaded (id: " + docId + ")";
                        return;
                    }
                }
                catch { }

                lblUploaded.Text = "Document uploaded: " + System.IO.Path.GetFileName(ofd.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error uploading document: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
