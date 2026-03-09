using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroLend.DAL.Repositories
{
    public class InvestmentRepository
    {
        private readonly MicroLendDbContext _context = new MicroLendDbContext();

        public async Task AddAsync(Investment investment)
        {
            await _context.Investments.AddAsync(investment);
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalInvestedInLoanAsync(int loanId)
        {
            return await _context.Investments
                .Where(i => i.LoanId == loanId)
                .SumAsync(i => i.AmountInvested);
        }
    }
}
