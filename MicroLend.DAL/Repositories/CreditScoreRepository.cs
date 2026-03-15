using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroLend.DAL.Repositories;

public class CreditScoreRepository : Repository<CreditScore>
{
    // Crucial for CreditScoringService.GetLatestScore
    public async Task<List<CreditScore>> GetByUserIdAsync(int userId)
    {
        return await _dbSet.Where(s => s.UserId == userId).ToListAsync();
    }
}