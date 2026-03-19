using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    public interface IInvestmentService
    {
        Task<int> CreateInvestmentAsync(int lenderId, int loanId, decimal amount);
        Task ApproveInvestmentAsync(int investmentId, bool approve);
    }
}
