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
        private readonly MicroLend.BLL.Services.CreditScoringFacade _facade = new MicroLend.BLL.Services.CreditScoringFacade();
        private readonly int? _currentUserId;

        private FlowLayoutPanel panel;
        private Button btnSubmit;
        private List<ComboBox> answerBoxes = new List<ComboBox>();

        public CreditQuizForm(int? currentUserId = null)
        {
            _currentUserId = currentUserId;
            Text = "Credit Scoring Quiz";
            Width = 620;
            Height = 520;
            StartPosition = FormStartPosition.CenterParent;

            panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown };
            Controls.Add(panel);

            var questions = _service.GetQuestions();
            foreach (var q in questions)
            {
                var p = new Panel { Width = 560, Height = 100, BorderStyle = BorderStyle.None };
                var lbl = new Label { Text = q.Text, Location = new Point(4, 4), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
                var cb = new ComboBox { Location = new Point(8, 34), Width = 520, DropDownStyle = ComboBoxStyle.DropDownList };
                foreach (var opt in q.Options) cb.Items.Add(new ComboItem(opt.Id, opt.Text));
                if (cb.Items.Count > 0) cb.SelectedIndex = 0;
                p.Controls.Add(lbl);
                p.Controls.Add(cb);
                panel.Controls.Add(p);
                answerBoxes.Add(cb);
            }

            btnSubmit = new Button { Text = "Submit Quiz", Location = new Point(420, 420), Size = new Size(120, 36) };
            btnSubmit.Click += async (s, e) => await OnSubmit();
            Controls.Add(btnSubmit);
        }

        private async Task OnSubmit()
        {
            // map answers
            var questions = _service.GetQuestions();
            var map = new Dictionary<int, int>();
            for (int i = 0; i < questions.Count; i++)
            {
                if (i >= answerBoxes.Count) break;
                var sel = answerBoxes[i].SelectedItem as ComboItem;
                if (sel != null) map[questions[i].Id] = sel.Id;
            }

            int score;
            if (_currentUserId.HasValue)
            {
                // attempt to get borrower income
                var borrowers = await _borrowerRepo.GetAllAsync();
                var br = borrowers.FirstOrDefault(b => b.UserId == _currentUserId.Value);
                var income = br?.MonthlyIncome ?? 0m;
                score = await _facade.ScoreAndSaveAsync(_currentUserId.Value, map, income);
            }
            else
            {
                score = await _service.CalculateScore(map);
            }

            MessageBox.Show($"Your score: {score}", "Quiz Result");
            Close();
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
