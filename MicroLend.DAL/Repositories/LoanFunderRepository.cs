using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroLend.DAL.Repositories;

public class LoanFunderRepository : Repository<LoanFunder>
{
    // Crucial for CrowdfundingService.DistributeInterest
    public async Task<List<LoanFunder>> GetByLoanIdAsync(int loanId)
    {
        return await _dbSet.Where(f => f.LoanId == loanId).ToListAsync();
    }
}
