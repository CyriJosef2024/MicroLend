using MicroLend.DAL.Entities;
using MicroLend.DAL.Exceptions;
using MicroLend.DAL.Repositories;
using System;
using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    public class RepaymentService : IRepaymentService
    {
        private readonly Repository<Repayment> _repRepo = new Repository<Repayment>();
        private readonly LoanRepository _loanRepo = new LoanRepository();
        private readonly EmergencyPoolRepository _poolRepo = new EmergencyPoolRepository();

        public async Task<int> RecordRepaymentAsync(int loanId, int? userId, decimal amount, decimal poolDonation)
        {
            try
            {
                var loan = await _loanRepo.GetByIdAsync(loanId);
                if (loan == null) 
                    throw new BusinessException("The loan could not be found.");
                
                if (loan.Status != "Active") 
                    throw new BusinessException("You can only make repayments on active loans.");

                var r = new Repayment
                {
                    LoanId = loanId,
                    Amount = amount,
                    PaymentDate = DateTime.Now,
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    PaymentMethod = "Manual",
                    PaymentReference = null
                };
                await _repRepo.AddAsync(r);

                loan.CurrentAmount -= amount;
                if (loan.CurrentAmount <= 0)
                {
                    loan.Status = "Repaid";
                    loan.CurrentAmount = 0;
                }
                await _loanRepo.UpdateAsync(loan);

                if (poolDonation > 0)
                {
                    // EmergencyPoolRepository exposes UpdatePoolAsync to adjust balance
                    await _poolRepo.UpdatePoolAsync(poolDonation);
                }

                return r.Id;
            }
            catch (BusinessException)
            {
                // Re-throw business exceptions as they're already properly handled
                throw;
            }
            catch (DataAccessException ex)
            {
                MicroLend.DAL.Logger.LogError($"Data access error while recording repayment for loan {loanId}", ex);
                throw new BusinessException("Unable to record your repayment. Please try again later.");
            }
            catch (Exception ex)
            {
                MicroLend.DAL.Logger.LogError($"Unexpected error while recording repayment for loan {loanId}", ex);
                throw new BusinessException("An unexpected error occurred while recording your repayment. Please contact support.");
            }
        }
    }
}
