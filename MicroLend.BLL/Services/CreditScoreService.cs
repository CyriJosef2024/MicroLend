using MicroLend.DAL.Repositories;
using MicroLend.DAL.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    // small DTOs for quiz are defined in this file already (QuizQuestion/QuizOption). Avoid duplicate types.

    public class CreditScoreService : ICreditScoreService
    {
        private readonly CreditScoreRepository _repo = new CreditScoreRepository();

        public List<QuizQuestion> GetQuestions()
        {
            return new List<QuizQuestion>
            {
                new QuizQuestion{ Id=1, Text="How many months have you been self-employed?", Options = new List<QuizOption>{ new QuizOption{Id=1,Text="<6", Points=0}, new QuizOption{Id=2,Text="6-12", Points=5}, new QuizOption{Id=3,Text=">12", Points=10} } },
                new QuizQuestion{ Id=2, Text="Do you have other outstanding loans?", Options = new List<QuizOption>{ new QuizOption{Id=4,Text="Yes", Points=0}, new QuizOption{Id=5,Text="No", Points=10} } },
                new QuizQuestion{ Id=3, Text="What is your average monthly income?", Options = new List<QuizOption>{ new QuizOption{Id=6,Text="<10000", Points=0}, new QuizOption{Id=7,Text="10000-30000", Points=5}, new QuizOption{Id=8,Text=">30000", Points=10} } },
                new QuizQuestion{ Id=4, Text="Have you repaid previous loans on time?", Options = new List<QuizOption>{ new QuizOption{Id=9,Text="Mostly late", Points=0}, new QuizOption{Id=10,Text="Mostly on time", Points=10} } },
                new QuizQuestion{ Id=5, Text="Do you have regular savings?", Options = new List<QuizOption>{ new QuizOption{Id=11,Text="No", Points=0}, new QuizOption{Id=12,Text="Yes", Points=10} } }
            };
        }

        public Task<int> CalculateScoreAsync(Dictionary<int, int> answers)
        {
            var questions = GetQuestions();
            int score = 0;
            foreach (var q in questions)
            {
                if (answers.TryGetValue(q.Id, out var optId))
                {
                    var opt = q.Options.FirstOrDefault(o => o.Id == optId);
                    if (opt != null) score += opt.Points;
                }
            }
            // normalize to 0-100
            return Task.FromResult(score * 2);
        }

        public async Task<int> ScoreAndSaveAsync(int userId, Dictionary<int, int> answers, decimal monthlyIncome)
        {
            var score = await CalculateScoreAsync(answers);
            var repo = new CreditScoreRepository();
            var cs = new CreditScore { UserId = userId, Score = score, QuizDate = System.DateTime.Now, Details = "Quiz-based score", CreatedAt = System.DateTime.Now, UpdatedAt = System.DateTime.Now };
            await repo.AddAsync(cs);

            // update user initial score
            var userRepo = new MicroLend.DAL.Repositories.UserRepository();
            var user = await userRepo.GetByIdAsync(userId);
            if (user != null)
            {
                user.InitialCreditScore = score;
                await userRepo.UpdateAsync(user);
            }

            return score;
        }
    }
}
