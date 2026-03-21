using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroLend.DAL.Exceptions;

namespace MicroLend.DAL.Repositories;

public class LoanFunderRepository : Repository<LoanFunder>
{
    // Crucial for CrowdfundingService.DistributeInterest
    public async Task<List<LoanFunder>> GetByLoanIdAsync(int loanId)
    {
        try
        {
            return await _dbSet.Where(f => f.LoanId == loanId).ToListAsync();
        }
        catch (System.Data.Common.DbException ex)
        {
            Logger.LogError($"Database error while retrieving loan funders for loan ID: {loanId}", ex);
            throw new DataAccessException("Unable to retrieve funder data. Please check your database connection.");
        }
        catch (System.InvalidOperationException ex)
        {
            Logger.LogError($"Query error while retrieving loan funders for loan ID: {loanId}", ex);
            throw new DataAccessException("Unable to process funder query. Please contact support.");
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Unexpected error while retrieving loan funders for loan ID: {loanId}", ex);
            throw new DataAccessException("An unexpected error occurred while accessing funder data.");
        }
    }

    public async Task<List<LoanFunder>> GetByLenderIdAsync(int lenderId)
    {
        try
        {
            return await _dbSet.Where(f => f.LenderId == lenderId).ToListAsync();
        }
        catch (System.Data.Common.DbException ex)
        {
            Logger.LogError($"Database error while retrieving loan funders for lender ID: {lenderId}", ex);
            throw new DataAccessException("Unable to retrieve funder data. Please check your database connection.");
        }
        catch (System.InvalidOperationException ex)
        {
            Logger.LogError($"Query error while retrieving loan funders for lender ID: {lenderId}", ex);
            throw new DataAccessException("Unable to process funder query. Please contact support.");
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Unexpected error while retrieving loan funders for lender ID: {lenderId}", ex);
            throw new DataAccessException("An unexpected error occurred while accessing funder data.");
        }
    }

    public async Task UpdateAsync(LoanFunder funder)
    {
        try
        {
            _context.Update(funder);
            await _context.SaveChangesAsync();
        }
        catch (System.Data.Common.DbException ex)
        {
            Logger.LogError($"Database error while updating loan funder ID: {funder.Id}", ex);
            throw new DataAccessException("Unable to update funder data. Please check your database connection.");
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Unexpected error while updating loan funder ID: {funder.Id}", ex);
            throw new DataAccessException("An unexpected error occurred while updating funder data.");
        }
    }

    public async Task AddAsync(LoanFunder funder)
    {
        try
        {
            await _dbSet.AddAsync(funder);
            await _context.SaveChangesAsync();
        }
        catch (System.Data.Common.DbException ex)
        {
            Logger.LogError("Database error while adding new loan funder", ex);
            throw new DataAccessException("Unable to save funder data. Please check your database connection.");
        }
        catch (System.Exception ex)
        {
            Logger.LogError("Unexpected error while adding new loan funder", ex);
            throw new DataAccessException("An unexpected error occurred while saving funder data.");
        }
    }
}
