using MicroLend.DAL.Entities;
using MicroLend.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    public class CreditScoringService
    {
        private readonly CreditScoreRepository _creditRepo = new CreditScoreRepository();
        private readonly UserRepository _userRepo = new UserRepository();

        // Hardcoded quiz questions (could be stored in DB)
        public List<QuizQuestion> GetQuestions()
        {
            return new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Id = 1,
                    Text = "How often do you pay bills on time?",
                    Options = new List<QuizOption>
                    {
                        new QuizOption { Id = 101, Text = "Always", Points = 20 },
                        new QuizOption { Id = 102, Text = "Often", Points = 10 },
                        new QuizOption { Id = 103, Text = "Sometimes", Points = 0 },
                        new QuizOption { Id = 104, Text = "Rarely", Points = -10 }
                    }
                },
                new QuizQuestion
                {
                    Id = 2,
                    Text = "If you faced an unexpected expense, would you:",
                    Options = new List<QuizOption>
                    {
                        new QuizOption { Id = 201, Text = "Use savings", Points = 15 },
                        new QuizOption { Id = 202, Text = "Borrow from family", Points = 5 },
                        new QuizOption { Id = 203, Text = "Take a high-interest loan", Points = -15 },
                        new QuizOption { Id = 204, Text = "Sell assets", Points = -5 }
                    }
                },
            };
        }

        public async Task<int> CalculateScore(Dictionary<int, int> selectedOptionIds)
        {
            var questions = GetQuestions();
            int total = 0;
            foreach (var q in questions)
            {
                if (selectedOptionIds.TryGetValue(q.Id, out int optId))
                {
                    var opt = q.Options.FirstOrDefault(o => o.Id == optId);
                    if (opt != null)
                        total += opt.Points;
                }
            }
            // Normalize 0-100 assuming min possible = -50, max = 100 (adjust based on your questions)
            int minPossible = -50, maxPossible = 100;
            int normalized = (int)((total - minPossible) / (double)(maxPossible - minPossible) * 100);
            return Math.Max(0, Math.Min(100, normalized));
        }

        public async Task SaveScore(int userId, int score, Dictionary<int, int> answers)
        {
            var details = JsonSerializer.Serialize(answers);
            var cs = new CreditScore
            {
                UserId = userId,
                Score = score,
                QuizDate = DateTime.Now,
                Details = details
            };
            await _creditRepo.AddAsync(cs);
        }

        public async Task<int?> GetLatestScore(int userId)
        {
            var scores = await _creditRepo.GetByUserIdAsync(userId);
            return scores.OrderByDescending(s => s.QuizDate).FirstOrDefault()?.Score;
        }
    }

    public class QuizQuestion
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public List<QuizOption> Options { get; set; }
    }

    public class QuizOption
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Points { get; set; }
    }
}
