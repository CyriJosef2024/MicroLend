using MicroLend.DAL.Entities;
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
            var transactions = await _transactionRepo.GetAllAsync();
            return transactions.Sum(t => t.Type == "Donation" ? t.Amount : -t.Amount);
        }

        public async Task Donate(int userId, decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive.");
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

        public async Task RequestWithdrawal(int userId, decimal amount, string reason)
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

        public async Task ApproveWithdrawal(int transactionId)
        {
            // In a full implementation, you'd have a Pending state.
        }
    }
}
