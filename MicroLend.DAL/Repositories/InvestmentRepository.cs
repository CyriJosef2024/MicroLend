using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using MicroLend.DAL.Exceptions;

namespace MicroLend.DAL.Repositories
{
    public class InvestmentRepository
    {
        private readonly MicroLendDbContext _context = new MicroLendDbContext();

        public async Task AddAsync(Investment investment)
        {
            try
            {
                await _context.Investments.AddAsync(investment);
                await _context.SaveChangesAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError("Database error while adding new investment", ex);
                throw new DataAccessException("Unable to save investment data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError("Unexpected error while adding new investment", ex);
                throw new DataAccessException("An unexpected error occurred while saving investment data.");
            }
        }

        public async Task<decimal> GetTotalInvestedInLoanAsync(int loanId)
        {
            try
            {
                return await _context.Investments
                    .Where(i => i.LoanId == loanId)
                    .SumAsync(i => i.AmountInvested);
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError($"Database error while calculating total investment for loan ID: {loanId}", ex);
                throw new DataAccessException("Unable to retrieve investment data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Unexpected error while calculating total investment for loan ID: {loanId}", ex);
                throw new DataAccessException("An unexpected error occurred while accessing investment data.");
            }
        }
    }
}
