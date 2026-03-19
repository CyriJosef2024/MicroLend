using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroLend.DAL.Repositories
{
    public class RepaymentRepository : Repository<Repayment>
    {
        public async Task<List<Repayment>> GetByUserIdAsync(int userId)
        {
            return await _dbSet.Where(r => r.UserId == userId).ToListAsync();
        }

        public async Task<List<Repayment>> GetByLoanIdAsync(int loanId)
        {
            return await _dbSet.Where(r => r.LoanId == loanId).ToListAsync();
        }
    }
}
