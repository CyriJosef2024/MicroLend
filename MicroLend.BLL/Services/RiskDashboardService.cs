using MicroLend.DAL.Entities;
using MicroLend.DAL.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    public class RiskDashboardService
    {
        private readonly LoanFunderRepository _funderRepo = new LoanFunderRepository();
        private readonly LoanRepository _loanRepo = new LoanRepository();

        public async Task<double> GetLenderPortfolioRisk(int lenderId)
        {
            var fundings = await _funderRepo.GetByLenderIdAsync(lenderId);
            if (!fundings.Any()) return 0;

            double weightedSum = 0;
            decimal totalAmount = 0;
            foreach (var f in fundings.ToList())
            {
                var loan = await _loanRepo.GetByIdAsync(f.LoanId);
                if (loan != null)
                {
                    weightedSum += loan.RiskScore * (double)f.Amount;
                    totalAmount += f.Amount;
                }
            }
            return totalAmount > 0 ? weightedSum / (double)totalAmount : 0;
        }

        public async Task<List<object>> GetLenderFundedLoans(int lenderId)
        {
            var fundings = await _funderRepo.GetByLenderIdAsync(lenderId);
            var result = new List<object>();
            foreach (var f in fundings.ToList())
            {
                var loan = await _loanRepo.GetByIdAsync(f.LoanId);
                if (loan != null)
                {
                    result.Add(new
                    {
                        LoanId = loan.Id,
                        BorrowerName = loan.Borrower?.FullName,
                        AmountInvested = f.Amount,
                        loan.RiskScore,
                        loan.Status,
                        ExpectedInterest = f.ExpectedInterest
                    });
                }
            }
            return result;
        }
    }
}
