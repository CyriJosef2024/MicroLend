using MicroLend.DAL.Entities;
using MicroLend.DAL.Repositories;
using System;
using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    public class InvestmentService : IInvestmentService
    {
        private readonly LoanFunderRepository _funderRepo = new LoanFunderRepository();
        private readonly LoanRepository _loanRepo = new LoanRepository();

        public async Task<int> CreateInvestmentAsync(int lenderId, int loanId, decimal amount)
        {
            var loan = await _loanRepo.GetByIdAsync(loanId);
            if (loan == null) throw new Exception("Loan not found");
            if (loan.Status != "Approved") throw new InvalidOperationException("Can only invest in approved loans");

            var inv = new LoanFunder
            {
                LoanId = loanId,
                LenderId = lenderId,
                Amount = amount,
                ExpectedInterest = 0m,
                FundingDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            await _funderRepo.AddAsync(inv);
            return inv.Id;
        }

        public async Task ApproveInvestmentAsync(int investmentId, bool approve)
        {
            var inv = await _funderRepo.GetByIdAsync(investmentId);
            if (inv == null) throw new Exception("Investment not found");
            inv.UpdatedAt = DateTime.Now;
            if (approve)
            {
                inv.ExpectedInterest = inv.Amount * 0.05m;
                await _funderRepo.UpdateAsync(inv);

                var loan = await _loanRepo.GetByIdAsync(inv.LoanId);
                loan.CurrentAmount += inv.Amount;
                if (loan.CurrentAmount >= loan.Amount)
                {
                    loan.Status = "Active";
                    loan.DateGranted = DateTime.Now;
                }
                await _loanRepo.UpdateAsync(loan);
            }
            else
            {
                // rejected - we mark as such and leave loan unchanged
                await _funderRepo.UpdateAsync(inv);
            }
        }
    }
}
