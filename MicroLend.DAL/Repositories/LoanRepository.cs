using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using MicroLend.DAL.Exceptions;

namespace MicroLend.DAL.Repositories
{
    public class LoanRepository
    {
        private readonly MicroLendDbContext _context = new MicroLendDbContext();

        public async Task<List<Loan>> GetLoansByBorrowerAsync(int borrowerId)
        {
            try
            {
                return await _context.Loans.Where(l => l.BorrowerId == borrowerId).ToListAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError($"Database error while retrieving loans for borrower ID: {borrowerId}", ex);
                throw new DataAccessException("Unable to retrieve loan data. Please check your database connection.");
            }
            catch (System.InvalidOperationException ex)
            {
                Logger.LogError($"Query error while retrieving loans for borrower ID: {borrowerId}", ex);
                throw new DataAccessException("Unable to process loan query. Please contact support.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Unexpected error while retrieving loans for borrower ID: {borrowerId}", ex);
                throw new DataAccessException("An unexpected error occurred while accessing loan data.");
            }
        }

        public async Task<List<Loan>> GetApprovedLoansAsync()
        {
            try
            {
                return await _context.Loans.Where(l => l.Status == "Approved").ToListAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError("Database error while retrieving approved loans", ex);
                throw new DataAccessException("Unable to retrieve loan data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError("Unexpected error while retrieving approved loans", ex);
                throw new DataAccessException("An unexpected error occurred while accessing loan data.");
            }
        }

        public async Task<List<Loan>> GetPendingLoansAsync()
        {
            try
            {
                return await _context.Loans.Where(l => l.Status == "Pending").ToListAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError("Database error while retrieving pending loans", ex);
                throw new DataAccessException("Unable to retrieve loan data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError("Unexpected error while retrieving pending loans", ex);
                throw new DataAccessException("An unexpected error occurred while accessing loan data.");
            }
        }

        public async Task AddAsync(Loan loan)
        {
            try
            {
                await _context.Loans.AddAsync(loan);
                await _context.SaveChangesAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError("Database error while adding new loan", ex);
                throw new DataAccessException("Unable to save loan data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError("Unexpected error while adding new loan", ex);
                throw new DataAccessException("An unexpected error occurred while saving loan data.");
            }
        }

        public async Task<Loan?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Loans.FindAsync(id);
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError($"Database error while retrieving loan ID: {id}", ex);
                throw new DataAccessException("Unable to retrieve loan data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Unexpected error while retrieving loan ID: {id}", ex);
                throw new DataAccessException("An unexpected error occurred while accessing loan data.");
            }
        }

        public async Task UpdateAsync(Loan loan)
        {
            try
            {
                _context.Loans.Update(loan);
                await _context.SaveChangesAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError($"Database error while updating loan ID: {loan.Id}", ex);
                throw new DataAccessException("Unable to update loan data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Unexpected error while updating loan ID: {loan.Id}", ex);
                throw new DataAccessException("An unexpected error occurred while updating loan data.");
            }
        }

        // Compatibility methods used by BLL services
        public async Task<List<Loan>> GetLoansByBorrowerIdAsync(int borrowerId)
        {
            return await GetLoansByBorrowerAsync(borrowerId);
        }

        public async Task<List<Loan>> GetActiveLoansByBorrowerAsync(int borrowerId)
        {
            try
            {
                return await _context.Loans.Where(l => l.BorrowerId == borrowerId && l.Status == "Active").ToListAsync();
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError($"Database error while retrieving active loans for borrower ID: {borrowerId}", ex);
                throw new DataAccessException("Unable to retrieve loan data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Unexpected error while retrieving active loans for borrower ID: {borrowerId}", ex);
                throw new DataAccessException("An unexpected error occurred while accessing loan data.");
            }
        }

        public async Task ActivateLoanAsync(int loanId)
        {
            try
            {
                var loan = await GetByIdAsync(loanId);
                if (loan == null) return;
                loan.Status = "Active";
                loan.DateGranted = DateTime.Now;
                if (loan.CurrentAmount <= 0) loan.CurrentAmount = loan.Amount > 0 ? loan.Amount : loan.TargetAmount;
                await UpdateAsync(loan);
            }
            catch (DataAccessException)
            {
                // Re-throw DataAccessException as it's already properly handled
                throw;
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Unexpected error while activating loan ID: {loanId}", ex);
                throw new DataAccessException("An unexpected error occurred while activating the loan.");
            }
        }

        public async Task<Loan?> GetLoanWithFundingAsync(int loanId)
        {
            try
            {
                // include funders and repayments
                return await _context.Loans
                    .Include(l => l.Funders)
                    .Include(l => l.Repayments)
                    .FirstOrDefaultAsync(l => l.Id == loanId);
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError($"Database error while retrieving loan with funding for loan ID: {loanId}", ex);
                throw new DataAccessException("Unable to retrieve loan data. Please check your database connection.");
            }
            catch (System.InvalidOperationException ex)
            {
                Logger.LogError($"Query error while retrieving loan with funding for loan ID: {loanId}", ex);
                throw new DataAccessException("Unable to process loan query. Please contact support.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Unexpected error while retrieving loan with funding for loan ID: {loanId}", ex);
                throw new DataAccessException("An unexpected error occurred while accessing loan data.");
            }
        }

        public async Task<decimal> GetCurrentFundedAmountAsync(int loanId)
        {
            try
            {
                var fundersSum = await _context.LoanFunders
                    .Where(f => f.LoanId == loanId)
                    .SumAsync(f => (decimal?)f.Amount) ?? 0m;

                // keep compatibility with older Investment entity
                var investmentsSum = await _context.Investments
                    .Where(i => i.LoanId == loanId)
                    .SumAsync(i => (decimal?)i.AmountInvested) ?? 0m;

                return fundersSum + investmentsSum;
            }
            catch (System.Data.Common.DbException ex)
            {
                Logger.LogError($"Database error while calculating funded amount for loan ID: {loanId}", ex);
                throw new DataAccessException("Unable to retrieve funding data. Please check your database connection.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Unexpected error while calculating funded amount for loan ID: {loanId}", ex);
                throw new DataAccessException("An unexpected error occurred while calculating funding data.");
            }
        }
    }
}