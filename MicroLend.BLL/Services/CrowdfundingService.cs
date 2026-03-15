using MicroLend.DAL.Entities;
using MicroLend.DAL.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    public class CrowdfundingService
    {
        private readonly LoanRepository _loanRepo = new LoanRepository();
        private readonly LoanFunderRepository _funderRepo = new LoanFunderRepository();

        public async Task InvestInLoan(int lenderId, int loanId, decimal amount)
        {
            var loan = await _loanRepo.GetByIdAsync(loanId);
            if (loan == null || !loan.IsCrowdfunded || loan.Status != "Funding")
                throw new InvalidOperationException("Loan not available for funding.");

            if (loan.CurrentAmount + amount > loan.TargetAmount)
                throw new InvalidOperationException("Investment exceeds remaining target.");

            var funder = new LoanFunder
            {
                LoanId = loanId,
                LenderId = lenderId,
                Amount = amount,
                FundingDate = DateTime.Now,
                ExpectedInterest = 0
            };
            await _funderRepo.AddAsync(funder);

            loan.CurrentAmount += amount;
            if (loan.CurrentAmount >= loan.TargetAmount)
            {
                loan.Status = "Active";
                loan.DateGranted = DateTime.Now;
            }
            await _loanRepo.UpdateAsync(loan);
        }

        // Called when a repayment is made
        public async Task DistributeInterest(int loanId, decimal repaymentAmount)
        {
            var loan = await _loanRepo.GetByIdAsync(loanId);
            if (loan == null || !loan.IsCrowdfunded || loan.Status != "Active")
                return;

            var funders = await _funderRepo.GetByLoanIdAsync(loanId);
            if (!funders.Any()) return;

            decimal totalInterest = loan.TargetAmount * (loan.InterestRate / 100);
            decimal principalRepaid = loan.Repayments.Sum(r => r.Amount);
            decimal fractionOfLoanRepaid = (principalRepaid + repaymentAmount) / loan.TargetAmount;
            if (fractionOfLoanRepaid > 1) fractionOfLoanRepaid = 1;
            decimal interestEarnedSoFar = totalInterest * fractionOfLoanRepaid;
            decimal previousInterest = funders.Sum(f => f.ExpectedInterest);

            decimal newInterest = interestEarnedSoFar - previousInterest;
            if (newInterest <= 0) return;

            foreach (var funder in funders)
            {
                decimal share = funder.Amount / loan.TargetAmount;
                funder.ExpectedInterest += newInterest * share;
                await _funderRepo.UpdateAsync(funder);
            }
        }
    }
}
