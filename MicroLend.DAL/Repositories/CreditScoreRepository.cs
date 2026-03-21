using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroLend.DAL.Exceptions;

namespace MicroLend.DAL.Repositories;

public class CreditScoreRepository : Repository<CreditScore>
{
    // Crucial for CreditScoringService.GetLatestScore
    public async Task<List<CreditScore>> GetByUserIdAsync(int userId)
    {
        try
        {
            return await _dbSet.Where(s => s.UserId == userId).ToListAsync();
        }
        catch (System.Data.Common.DbException ex)
        {
            Logger.LogError($"Database error while retrieving credit scores for user ID: {userId}", ex);
            throw new DataAccessException("Unable to retrieve credit score data. Please check your database connection.");
        }
        catch (System.InvalidOperationException ex)
        {
            Logger.LogError($"Query error while retrieving credit scores for user ID: {userId}", ex);
            throw new DataAccessException("Unable to process credit score query. Please contact support.");
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Unexpected error while retrieving credit scores for user ID: {userId}", ex);
            throw new DataAccessException("An unexpected error occurred while accessing credit score data.");
        }
    }
}