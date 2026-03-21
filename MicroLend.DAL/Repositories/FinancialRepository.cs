using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using MicroLend.DAL.Exceptions;

namespace MicroLend.DAL.Repositories
{
    public class FinancialRepository
    {
        private readonly MicroLendDbContext _context = new MicroLendDbContext();

        public async Task AddRepaymentAsync(Repayment repayment)
        {
            try
            {
                await _context.Repayments.AddAsync(repayment);
                await _context.SaveChangesAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError("Database error while adding new repayment", ex);
                throw new DataAccessException("Unable to save repayment data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError("Unexpected error while adding new repayment", ex);
                throw new DataAccessException("An unexpected error occurred while saving repayment data.");
            }
        }

        public async Task<List<Repayment>> GetRepaymentsForLoanAsync(int loanId)
        {
            try
            {
                return await _context.Repayments.Where(r => r.LoanId == loanId).ToListAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError($"Database error while retrieving repayments for loan ID: {loanId}", ex);
                throw new DataAccessException("Unable to retrieve repayment data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Unexpected error while retrieving repayments for loan ID: {loanId}", ex);
                throw new DataAccessException("An unexpected error occurred while accessing repayment data.");
            }
        }

        public async Task AddEmergencyTransactionAsync(EmergencyPoolTransaction tx)
        {
            try
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

                // Transaction Type stored as string in model ("Donation" or "Withdrawal")
                pool.TotalBalance += tx.Type == "Donation" ? tx.Amount : -tx.Amount;
                await _context.SaveChangesAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError("Database error while adding emergency pool transaction", ex);
                throw new DataAccessException("Unable to save emergency transaction. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError("Unexpected error while adding emergency pool transaction", ex);
                throw new DataAccessException("An unexpected error occurred while saving emergency transaction.");
            }
        }

        public async Task<decimal> GetEmergencyPoolBalanceAsync()
        {
            try
            {
                var pool = await _context.EmergencyPools.FirstOrDefaultAsync();
                return pool?.TotalBalance ?? 0m;
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError("Database error while retrieving emergency pool balance", ex);
                throw new DataAccessException("Unable to retrieve pool balance. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError("Unexpected error while retrieving emergency pool balance", ex);
                throw new DataAccessException("An unexpected error occurred while accessing pool balance.");
            }
        }
    }
}
