using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroLend.DAL.Exceptions;

namespace MicroLend.DAL.Repositories
{
    public class RepaymentRepository : Repository<Repayment>
    {
        public async Task<List<Repayment>> GetByUserIdAsync(int userId)
        {
            try
            {
                return await _dbSet.Where(r => r.UserId == userId).ToListAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError($"Database error while retrieving repayments for user ID: {userId}", ex);
                throw new DataAccessException("Unable to retrieve repayment data. Please check your database connection.");
            }
            catch (System.InvalidOperationException ex)
            {
                Logger.LogError($"Query error while retrieving repayments for user ID: {userId}", ex);
                throw new DataAccessException("Unable to process repayment query. Please contact support.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Unexpected error while retrieving repayments for user ID: {userId}", ex);
                throw new DataAccessException("An unexpected error occurred while accessing repayment data.");
            }
        }

        public async Task<List<Repayment>> GetByLoanIdAsync(int loanId)
        {
            try
            {
                return await _dbSet.Where(r => r.LoanId == loanId).ToListAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError($"Database error while retrieving repayments for loan ID: {loanId}", ex);
                throw new DataAccessException("Unable to retrieve repayment data. Please check your database connection.");
            }
            catch (System.InvalidOperationException ex)
            {
                Logger.LogError($"Query error while retrieving repayments for loan ID: {loanId}", ex);
                throw new DataAccessException("Unable to process repayment query. Please contact support.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Unexpected error while retrieving repayments for loan ID: {loanId}", ex);
                throw new DataAccessException("An unexpected error occurred while accessing repayment data.");
            }
        }
    }
}
