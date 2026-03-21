using MicroLend.DAL.Entities;
using MicroLend.DAL.Exceptions;
using MicroLend.DAL.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    public class EmergencyPoolService
    {
        private readonly EmergencyPoolTransactionRepository _transactionRepo = new EmergencyPoolTransactionRepository();

        public async Task<decimal> GetBalance()
        {
            try
            {
                var transactions = await _transactionRepo.GetAllAsync();
                return transactions.Sum(t => t.Type == "Donation" ? t.Amount : -t.Amount);
            }
            catch (DataAccessException ex)
            {
                MicroLend.DAL.Logger.LogError("Data access error while getting emergency pool balance", ex);
                throw new BusinessException("Unable to retrieve the emergency pool balance. Please try again later.");
            }
            catch (Exception ex)
            {
                MicroLend.DAL.Logger.LogError("Unexpected error while getting emergency pool balance", ex);
                throw new BusinessException("An unexpected error occurred while retrieving the emergency pool balance. Please contact support.");
            }
        }

        public async Task Donate(int userId, decimal amount)
        {
            try
            {
                if (amount <= 0) 
                    throw new BusinessException("Donation amount must be greater than zero.");
                
                var transaction = new EmergencyPoolTransaction
                {
                    UserId = userId,
                    Amount = amount,
                    TransactionDate = DateTime.Now,
                    Type = "Donation",
                    Description = "Donation to emergency pool"
                };
                await _transactionRepo.AddAsync(transaction);
            }
            catch (BusinessException)
            {
                // Re-throw business exceptions as they're already properly handled
                throw;
            }
            catch (DataAccessException ex)
            {
                MicroLend.DAL.Logger.LogError($"Data access error while processing donation for user {userId}", ex);
                throw new BusinessException("Unable to process your donation. Please try again later.");
            }
            catch (Exception ex)
            {
                MicroLend.DAL.Logger.LogError($"Unexpected error while processing donation for user {userId}", ex);
                throw new BusinessException("An unexpected error occurred while processing your donation. Please contact support.");
            }
        }

        public async Task RequestWithdrawal(int userId, decimal amount, string reason)
        {
            try
            {
                var transaction = new EmergencyPoolTransaction
                {
                    UserId = userId,
                    Amount = amount,
                    TransactionDate = DateTime.Now,
                    Type = "Withdrawal",
                    Description = reason
                };
                await _transactionRepo.AddAsync(transaction);
            }
            catch (DataAccessException ex)
            {
                MicroLend.DAL.Logger.LogError($"Data access error while processing withdrawal request for user {userId}", ex);
                throw new BusinessException("Unable to process your withdrawal request. Please try again later.");
            }
            catch (Exception ex)
            {
                MicroLend.DAL.Logger.LogError($"Unexpected error while processing withdrawal request for user {userId}", ex);
                throw new BusinessException("An unexpected error occurred while processing your withdrawal request. Please contact support.");
            }
        }

        public async Task ApproveWithdrawal(int transactionId)
        {
            // In a full implementation, you'd have a Pending state.
            try
            {
                // Implementation would go here
            }
            catch (DataAccessException ex)
            {
                MicroLend.DAL.Logger.LogError($"Data access error while approving withdrawal {transactionId}", ex);
                throw new BusinessException("Unable to approve the withdrawal. Please try again later.");
            }
            catch (Exception ex)
            {
                MicroLend.DAL.Logger.LogError($"Unexpected error while approving withdrawal {transactionId}", ex);
                throw new BusinessException("An unexpected error occurred while approving the withdrawal. Please contact support.");
            }
        }
    }
}
