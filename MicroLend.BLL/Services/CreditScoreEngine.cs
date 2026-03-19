using MicroLend.DAL.Entities;
using MicroLend.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    // New deterministic but explainable gamified credit score engine
    public class CreditScoreEngine
    {
        private readonly LoanRepository _loanRepo = new LoanRepository();
        private readonly RepaymentRepository _repRepo = new RepaymentRepository();

        // Compute credit score 0-100 based on quizAnswers (questionId->optionId) and historical repayment behavior and income
        // This engine intentionally avoids randomness - gamified elements are reflected in question weights and combo bonuses
        public async Task<int> CalculateScoreAsync(int userId, Dictionary<int,int> quizAnswers, decimal monthlyIncome)
        {
            // base from quiz
            int quizBase = 50; // neutral
            foreach(var kv in quizAnswers)
            {
                // simple mapping: option id ranges determine points
                var opt = kv.Value;
                if (opt % 10 == 1) quizBase += 15; // best
                else if (opt % 10 == 2) quizBase += 8;
                else if (opt % 10 == 3) quizBase += 0;
                else quizBase -= 8;
            }

            // income modifier: higher income slightly increases score
            int incomeBonus = 0;
            if (monthlyIncome >= 50000) incomeBonus = 12;
            else if (monthlyIncome >= 20000) incomeBonus = 6;
            else if (monthlyIncome >= 5000) incomeBonus = 2;

            // historical repayment: compute late payments ratio
            var borrower = await new BorrowerRepository().GetAllAsync();
            var reps = await _repRepo.GetByUserIdAsync(userId);
            int repBonus = 0;
            if (reps.Any())
            {
                var onTime = reps.Count(r => (DateTime.Now - r.PaymentDate).TotalDays < 60); // simple proxy
                var ratio = onTime / (double)reps.Count();
                repBonus = (int)((ratio - 0.5) * 40); // scale
            }

            int score = quizBase + incomeBonus + repBonus;
            score = Math.Max(0, Math.Min(100, score));
            return score;
        }
    }
}
