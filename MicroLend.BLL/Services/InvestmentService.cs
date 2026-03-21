using MicroLend.DAL.Entities;
using MicroLend.DAL.Exceptions;
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
            try
            {
                var loan = await _loanRepo.GetByIdAsync(loanId);
                if (loan == null) 
                    throw new BusinessException("The loan you are trying to invest in could not be found.");
                
                if (loan.Status != "Approved") 
                    throw new BusinessException("You can only invest in approved loans. This loan is not currently available for investment.");

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
            catch (BusinessException)
            {
                // Re-throw business exceptions as they're already properly handled
                throw;
            }
            catch (DataAccessException ex)
            {
                MicroLend.DAL.Logger.LogError($"Data access error while creating investment for lender {lenderId} in loan {loanId}", ex);
                throw new BusinessException("Unable to process your investment. Please try again later.");
            }
            catch (Exception ex)
            {
                MicroLend.DAL.Logger.LogError($"Unexpected error while creating investment for lender {lenderId} in loan {loanId}", ex);
                throw new BusinessException("An unexpected error occurred while processing your investment. Please contact support.");
            }
        }

        public async Task ApproveInvestmentAsync(int investmentId, bool approve)
        {
            try
            {
                var inv = await _funderRepo.GetByIdAsync(investmentId);
                if (inv == null) 
                    throw new BusinessException("The investment could not be found.");
                
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
            catch (BusinessException)
            {
                // Re-throw business exceptions as they're already properly handled
                throw;
            }
            catch (DataAccessException ex)
            {
                MicroLend.DAL.Logger.LogError($"Data access error while approving investment {investmentId}", ex);
                throw new BusinessException("Unable to process the investment approval. Please try again later.");
            }
            catch (Exception ex)
            {
                MicroLend.DAL.Logger.LogError($"Unexpected error while approving investment {investmentId}", ex);
                throw new BusinessException("An unexpected error occurred while processing the investment approval. Please contact support.");
            }
        }
    }
}
