using MicroLend.DAL.Entities;
using MicroLend.DAL.Exceptions;
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
            try
            {
                var loan = await _loanRepo.GetByIdAsync(loanId);
                if (loan == null || !loan.IsCrowdfunded || loan.Status != "Funding")
                    throw new BusinessException("This loan is not currently available for funding. It may have been fully funded or cancelled.");

                if (loan.CurrentAmount + amount > loan.TargetAmount)
                    throw new BusinessException($"Your investment amount exceeds the remaining funding needed. Maximum investment allowed: ₱{(loan.TargetAmount - loan.CurrentAmount):N2}");

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
            catch (BusinessException)
            {
                // Re-throw business exceptions as they're already properly handled
                throw;
            }
            catch (DataAccessException ex)
            {
                MicroLend.DAL.Logger.LogError($"Data access error while investing in loan {loanId} by lender {lenderId}", ex);
                throw new BusinessException("Unable to process your investment. Please try again later.");
            }
            catch (Exception ex)
            {
                MicroLend.DAL.Logger.LogError($"Unexpected error while investing in loan {loanId} by lender {lenderId}", ex);
                throw new BusinessException("An unexpected error occurred while processing your investment. Please contact support.");
            }
        }

        // Called when a repayment is made
        public async Task DistributeInterest(int loanId, decimal repaymentAmount)
        {
            try
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
            catch (DataAccessException ex)
            {
                MicroLend.DAL.Logger.LogError($"Data access error while distributing interest for loan {loanId}", ex);
                // Don't throw - this is a background operation
            }
            catch (Exception ex)
            {
                MicroLend.DAL.Logger.LogError($"Unexpected error while distributing interest for loan {loanId}", ex);
                // Don't throw - this is a background operation
            }
        }
    }
}
