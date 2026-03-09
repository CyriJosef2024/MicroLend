using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroLend.DAL.Repositories
{
    public class FinancialRepository
    {
        private readonly MicroLendDbContext _context = new MicroLendDbContext();

        public async Task AddRepaymentAsync(Repayment repayment)
        {
            await _context.Repayments.AddAsync(repayment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Repayment>> GetRepaymentsForLoanAsync(int loanId)
        {
            return await _context.Repayments.Where(r => r.LoanId == loanId).ToListAsync();
        }

        public async Task AddEmergencyTransactionAsync(EmergencyPoolTransaction tx)
        {
            await _context.EmergencyPoolTransactions.AddAsync(tx);
            await _context.SaveChangesAsync();

            // update aggregate pool balance
            var pool = await _context.EmergencyPools.FirstOrDefaultAsync();
            if (pool == null)
            {
                pool = new EmergencyPool { TotalBalance = 0m };
                await _context.EmergencyPools.AddAsync(pool);
            }

            pool.TotalBalance += tx.TransactionType == EmergencyTransactionType.Donation ? tx.Amount : -tx.Amount;
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetEmergencyPoolBalanceAsync()
        {
            var pool = await _context.EmergencyPools.FirstOrDefaultAsync();
            return pool?.TotalBalance ?? 0m;
        }
    }
}
