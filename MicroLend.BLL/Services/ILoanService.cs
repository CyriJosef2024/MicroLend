using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    public interface ILoanService
    {
        Task<int> ApplyLoanAsync(int borrowerId, decimal amount, string purpose, int initialScore);
        Task ApproveLoanAsync(int loanId, bool approve);
    }
}
