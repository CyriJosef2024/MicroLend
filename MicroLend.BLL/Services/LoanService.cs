using MicroLend.DAL.Entities;
using MicroLend.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    public class LoanService : ILoanService
    {
        private readonly LoanRepository _loanRepo = new LoanRepository();
        private readonly BorrowerRepository _borrowerRepo = new BorrowerRepository();
        private readonly CreditScoringService _creditScoring = new CreditScoringService();

        // Prediction Algorithm (FR2)
        public async Task<double> CalculateRepaymentPredictionScore(int borrowerId, decimal requestedAmount)
        {

            var borrower = await _borrowerRepo.GetByIdAsync(borrowerId);
            if (borrower == null) return 0;

            double score = 50; // base

            // Factor 1: Income-to-loan ratio (max +20)
  
               if (borrower.MonthlyIncome > 0)
            {
                double ratio = (double)(borrower.MonthlyIncome / requestedAmount);
                score += Math.Min(20, ratio * 20);
            }

            // Factor 2: Past loan history
            var pastLoans = await _loanRepo.GetLoansByBorrowerIdAsync(borrowerId);
            foreach (var loan in pastLoans.Where(l => l.Status == "Paid" || l.Status == "Defaulted"))
            {
                if (loan.Status == "Paid") score += 10;
                if (loan.Status == "Defaulted") score -= 30;
            }

            // Factor 3: Credit score from quiz (if exists)
            if (borrower.UserId.HasValue)
            {
                var creditScore = await _creditScoring.GetLatestScore(borrower.UserId.Value);
                if (creditScore.HasValue)
                {
                    // map 0-100 credit score to -10 to +10 adjustment
                    score += (creditScore.Value - 50) / 5.0;
                }
            }

            // Factor 4: Business type risk
            var riskMap = new Dictionary<string, int>
            {
                { "farming", 10 },
                { "sari-sari", 15 },
                { "food", 5 },
                { "clothing", 0 },
                { "electronics", -10 },
                { "expansion", -5 }
            };
            foreach (var kv in riskMap)
            {
                if (borrower.BusinessType.ToLower().Contains(kv.Key))
                {
                    score += kv.Value;
                    break;
                }
            }

            return Math.Max(0, Math.Min(100, score));
        }

        // Business rule: Borrower cannot have more than one active loan
        public async Task<bool> CanBorrowerGetNewLoan(int borrowerId)
        {
            var activeLoans = await _loanRepo.GetActiveLoansByBorrowerAsync(borrowerId);
            return !activeLoans.Any();
        }

        // Create a new loan (individual or crowdfunded)
        public async Task CreateLoan(Loan loan)
        {
            if (!await CanBorrowerGetNewLoan(loan.BorrowerId))
                throw new InvalidOperationException("Borrower already has an active loan.");

            loan.RiskScore = await CalculateRepaymentPredictionScore(loan.BorrowerId, loan.Amount);
            if (loan.IsCrowdfunded)
            {
                loan.Status = "Funding";
                loan.CurrentAmount = 0;
            }
            else
            {
                loan.Status = "Active";
                loan.CurrentAmount = loan.Amount;
                loan.DateGranted = DateTime.Now;
            }
            await _loanRepo.AddAsync(loan);
        }

        public async Task<int> ApplyLoanAsync(int borrowerId, decimal amount, string purpose, int initialScore)
        {
            if (!await CanBorrowerGetNewLoan(borrowerId))
                throw new InvalidOperationException("Borrower already has an active loan.");

            // require at least one approved document for borrower before applying
            using (var ctx = new MicroLend.DAL.MicroLendDbContext())
            {
                var hasApproved = ctx.Documents.Any(d => d.UserId == borrowerId && d.Status == "Approved");
                if (!hasApproved)
                    throw new InvalidOperationException("Borrower must have an approved document before applying for a loan.");
            }

            var loan = new Loan
            {
                BorrowerId = borrowerId,
                Amount = amount,
                Purpose = purpose,
                RiskScore = await CalculateRepaymentPredictionScore(borrowerId, amount),
                Status = "Pending"
            };

            await _loanRepo.AddAsync(loan);
            return loan.Id;
        }

        public async Task ApproveLoanAsync(int loanId, bool approve)
        {
            var loan = await _loanRepo.GetByIdAsync(loanId);
            if (loan == null)
                throw new InvalidOperationException("Loan not found.");

            if (approve)
            {
                loan.Status = "Active";
                loan.DateGranted = DateTime.Now;
                loan.CurrentAmount = loan.Amount;
            }
            else
            {
                loan.Status = "Rejected";
            }

            await _loanRepo.UpdateAsync(loan);
        }
    }
}
