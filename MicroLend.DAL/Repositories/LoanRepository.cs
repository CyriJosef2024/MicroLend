using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroLend.DAL.Repositories
{
    public class LoanRepository
    {
        private readonly MicroLendDbContext _context = new MicroLendDbContext();

        public async Task<List<Loan>> GetLoansByBorrowerAsync(int borrowerId)
        {
            return await _context.Loans.Where(l => l.BorrowerId == borrowerId).ToListAsync();
        }

        public async Task AddAsync(Loan loan)
        {
            await _context.Loans.AddAsync(loan);
            await _context.SaveChangesAsync();
        }

        public async Task<Loan?> GetLoanWithFundingAsync(int loanId)
        {
            // include funders and repayments
            return await _context.Loans
                .Include(l => l.Funders)
                .Include(l => l.Repayments)
                .FirstOrDefaultAsync(l => l.Id == loanId);
        }

        public async Task<decimal> GetCurrentFundedAmountAsync(int loanId)
        {
            var fundersSum = await _context.LoanFunders
                .Where(f => f.LoanId == loanId)
                .SumAsync(f => (decimal?)f.AmountFunded) ?? 0m;

            // keep compatibility with older Investment entity
            var investmentsSum = await _context.Investments
                .Where(i => i.LoanId == loanId)
                .SumAsync(i => (decimal?)i.AmountInvested) ?? 0m;

            return fundersSum + investmentsSum;
        }
    }
}