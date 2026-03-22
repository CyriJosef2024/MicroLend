using MicroLend.BLL.Services;
using MicroLend.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicroLend.UI
{
    public class CreditQuizForm : Form
    {
        private readonly CreditScoringService _service = new CreditScoringService();
        private readonly UserRepository _userRepo = new UserRepository();
        private readonly BorrowerRepository _borrowerRepo = new BorrowerRepository();
        private readonly CreditScoringFacade _facade = new CreditScoringFacade();
        private readonly int? _currentUserId;

        private Panel _mainPanel;
        private Panel _headerPanel;
        private Panel _questionsPanel;
        private Panel _footerPanel;
        private ProgressBar _progressBar;
        private Label _progressLabel;
        private Button _btnSubmit;
        private Button _btnCancel;
        private List<Panel> _questionCards = new List<Panel>();
        private List<ComboBox> _answerBoxes = new List<ComboBox>();
        private int _currentQuestionIndex = 0;
        private Label _scoreDisplayLabel;
        private Panel _scoreResultPanel;

        public CreditQuizForm(int? currentUserId = null)
        {
            _currentUserId = currentUserId;
            InitializeComponents();
            LoadQuestions();
        }

        private void InitializeComponents()
        {
            Text = "Credit Assessment Quiz";
            Width = 700;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.FromArgb(240, 245, 250);

            // Header Panel with title and progress
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(0, 120, 215)
            };

            var titleLabel = new Label
            {
                Text = "📊 Credit Assessment Quiz",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true
            };

            var subtitleLabel = new Label
            {
                Text = "Answer the following questions to calculate your credit score",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(220, 235, 250),
                Location = new Point(20, 55),
                AutoSize = true
            };

            _progressLabel = new Label
            {
                Text = "Progress: 0%",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(500, 40),
                AutoSize = true
            };

            _progressBar = new ProgressBar
            {
                Location = new Point(20, 80),
                Width = 660,
                Height = 15,
                Style = ProgressBarStyle.Continuous,
                BackColor = Color.FromArgb(180, 200, 230),
                ForeColor = Color.White
            };

            _headerPanel.Controls.Add(titleLabel);
            _headerPanel.Controls.Add(subtitleLabel);
            _headerPanel.Controls.Add(_progressLabel);
            _headerPanel.Controls.Add(_progressBar);

            // Main Questions Panel with scroll
            _questionsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = Color.Transparent
            };

            // Footer Panel with buttons
            _footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.FromArgb(235, 240, 245),
                Padding = new Padding(15)
            };

            var buttonContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                Width = 250,
                Height = 50
            };

            _btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(150, 150, 150),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(5)
            };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Click += (s, e) => Close();

            _btnSubmit = new Button
            {
                Text = "Submit Quiz ✓",
                Size = new Size(130, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 150, 80),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(5)
            };
            _btnSubmit.FlatAppearance.BorderSize = 0;
            _btnSubmit.Click += async (s, e) => await OnSubmitAsync();

            buttonContainer.Controls.Add(_btnCancel);
            buttonContainer.Controls.Add(_btnSubmit);

            _footerPanel.Controls.Add(buttonContainer);

            // Score Result Panel (hidden initially)
            _scoreResultPanel = new Panel
            {
                Visible = false,
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var scoreContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(50)
            };
            scoreContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            scoreContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            scoreContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            scoreContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var scoreTitle = new Label
            {
                Text = "🎉 Quiz Completed!",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            _scoreDisplayLabel = new Label
            {
                Text = "Your Credit Score: --",
                Font = new Font("Segoe UI", 32F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 150, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                AutoSize = true,
                MaximumSize = new Size(500, 60)
            };

            var scoreDescription = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                AutoSize = true,
                Name = "scoreDescription"
            };

            var btnClose = new Button
            {
                Text = "Close",
                Size = new Size(150, 45),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            scoreContainer.Controls.Add(scoreTitle, 0, 0);
            scoreContainer.Controls.Add(_scoreDisplayLabel, 0, 1);
            scoreContainer.Controls.Add(scoreDescription, 0, 2);
            scoreContainer.Controls.Add(btnClose, 0, 3);

            _scoreResultPanel.Controls.Add(scoreContainer);

            // Add all panels
            Controls.Add(_questionsPanel);
            Controls.Add(_footerPanel);
            Controls.Add(_headerPanel);
        }

        private void LoadQuestions()
        {
            var questions = _service.GetQuestions();
            // Add additional relevant questions
            questions.AddRange(new[] {
                new QuizQuestion {
                    Id = 3,
                    Text = "How stable is your monthly income?",
                    Options = new List<QuizOption> {
                        new QuizOption { Id = 301, Text = "Very stable - Consistent regular income", Points = 20 },
                        new QuizOption { Id = 302, Text = "Somewhat stable - Income varies slightly", Points = 10 },
                        new QuizOption { Id = 303, Text = "Unstable - Irregular income", Points = -10 }
                    }
                },
                new QuizQuestion {
                    Id = 4,
                    Text = "Do you have any existing loans?",
                    Options = new List<QuizOption> {
                        new QuizOption { Id = 401, Text = "No - No current debts", Points = 20 },
                        new QuizOption { Id = 402, Text = "Yes, one - Managing one loan", Points = 0 },
                        new QuizOption { Id = 403, Text = "Multiple - Several active loans", Points = -20 }
                    }
                }
            });

            int yOffset = 20;
            for (int i = 0; i < questions.Count; i++)
            {
                var q = questions[i];
                var card = CreateQuestionCard(q, i, ref yOffset);
                _questionCards.Add(card);
                _questionsPanel.Controls.Add(card);
            }

            UpdateProgress(0);
        }

        private Panel CreateQuestionCard(QuizQuestion question, int index, ref int yOffset)
        {
            var card = new Panel
            {
                Location = new Point(0, yOffset),
                Width = 620,
                Height = 140,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 15),
                Padding = new Padding(15)
            };
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.DrawRectangle(new Pen(Color.FromArgb(200, 210, 220), 1), 0, 0, card.Width - 1, card.Height - 1);
                // Draw shadow effect
                using (var brush = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                    g.FillRectangle(brush, 2, card.Height - 2, card.Width - 2, 3);
            };

            // Question number badge
            var badge = new Label
            {
                Text = $"Q{index + 1}",
                Location = new Point(15, 12),
                Size = new Size(45, 25),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            // Question text
            var questionLabel = new Label
            {
                Text = question.Text,
                Location = new Point(70, 12),
                Size = new Size(530, 30),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50)
            };

            // Answer ComboBox with styling
            var answerBox = new ComboBox
            {
                Location = new Point(15, 50),
                Width = 590,
                Height = 35,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                FlatStyle = FlatStyle.Flat
            };
            answerBox.BackColor = Color.FromArgb(248, 250, 252);
            answerBox.DrawMode = DrawMode.OwnerDrawFixed;
            answerBox.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                e.DrawBackground();
                var text = ((ComboBox)s).Items[e.Index].ToString();
                using (var brush = new SolidBrush(e.ForeColor))
                    e.Graphics.DrawString(text, new Font("Segoe UI", 10F), brush, e.Bounds.X + 5, e.Bounds.Y + 3);
            };

            foreach (var opt in question.Options)
                answerBox.Items.Add(new ComboItem(opt.Id, opt.Text));
            
            if (answerBox.Items.Count > 0)
                answerBox.SelectedIndex = 0;

            answerBox.SelectedIndexChanged += (s, e) => UpdateProgress(CalculateProgress());

            _answerBoxes.Add(answerBox);

            // Help text
            var helpLabel = new Label
            {
                Text = "💡 Select the option that best describes your situation",
                Location = new Point(15, 95),
                Size = new Size(550, 20),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(120, 130, 140)
            };

            card.Controls.Add(badge);
            card.Controls.Add(questionLabel);
            card.Controls.Add(answerBox);
            card.Controls.Add(helpLabel);

            yOffset += 160;
            return card;
        }

        private int CalculateProgress()
        {
            int answered = 0;
            foreach (var cb in _answerBoxes)
            {
                if (cb.SelectedIndex >= 0)
                    answered++;
            }
            return (int)((double)answered / _answerBoxes.Count * 100);
        }

        private void UpdateProgress(int percentage)
        {
            _progressBar.Value = Math.Min(100, Math.Max(0, percentage));
            _progressLabel.Text = $"Progress: {percentage}%";
        }

        private async Task OnSubmitAsync()
        {
            // Validate all questions are answered
            for (int i = 0; i < _answerBoxes.Count; i++)
            {
                if (_answerBoxes[i].SelectedIndex < 0)
                {
                    MessageBox.Show($"Please answer question {i + 1} before submitting.", 
                        "Incomplete Quiz", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _answerBoxes[i].Focus();
                    return;
                }
            }

            // Disable button during processing
            _btnSubmit.Enabled = false;
            _btnSubmit.Text = "Calculating...";

            try
            {
                // Map answers
                var questions = _service.GetQuestions();
                questions.AddRange(new[] {
                    new QuizQuestion { Id = 3, Text = "", Options = new List<QuizOption>() },
                    new QuizQuestion { Id = 4, Text = "", Options = new List<QuizOption>() }
                });

                var map = new Dictionary<int, int>();
                for (int i = 0; i < questions.Count && i < _answerBoxes.Count; i++)
                {
                    var sel = _answerBoxes[i].SelectedItem as ComboItem;
                    if (sel != null)
                        map[questions[i].Id] = sel.Id;
                }

                int score;
                if (_currentUserId.HasValue)
                {
                    var borrowers = await _borrowerRepo.GetAllAsync();
                    var br = borrowers.FirstOrDefault(b => b.UserId == _currentUserId.Value);
                    var income = br?.MonthlyIncome ?? 0m;
                    score = await _facade.ScoreAndSaveAsync(_currentUserId.Value, map, income);
                }
                else
                {
                    score = await _service.CalculateScore(map);
                }

                // Show results
                ShowScoreResult(score);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while processing your quiz: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnSubmit.Enabled = true;
                _btnSubmit.Text = "Submit Quiz ✓";
            }
        }

        private void ShowScoreResult(int score)
        {
            // Hide questions and show result
            _questionsPanel.Visible = false;
            _footerPanel.Visible = false;
            _scoreResultPanel.Visible = true;
            Controls.Remove(_questionsPanel);
            Controls.Remove(_footerPanel);
            Controls.Add(_scoreResultPanel);

            // Update score display with visual
            _scoreDisplayLabel.Text = $"Your Credit Score: {score}";

            // Color based on score
            if (score >= 80)
            {
                _scoreDisplayLabel.ForeColor = Color.FromArgb(0, 150, 80); // Green
            }
            else if (score >= 60)
            {
                _scoreDisplayLabel.ForeColor = Color.FromArgb(200, 150, 0); // Orange
            }
            else
            {
                _scoreDisplayLabel.ForeColor = Color.FromArgb(200, 50, 50); // Red
            }

            // Update description
            var descLabel = _scoreResultPanel.Controls[0].Controls["scoreDescription"] as Label;
            if (descLabel != null)
            {
                if (score >= 80)
                    descLabel.Text = "Excellent! Your credit profile shows strong financial responsibility.";
                else if (score >= 60)
                    descLabel.Text = "Good! Keep improving your financial habits for a better score.";
                else
                    descLabel.Text = "Fair. Consider improving your payment history and reducing debts.";
            }
        }

        private class ComboItem
        {
            public int Id { get; }
            public string Text { get; }
            public ComboItem(int id, string text) { Id = id; Text = text; }
            public override string ToString() => Text;
        }
    }
}
