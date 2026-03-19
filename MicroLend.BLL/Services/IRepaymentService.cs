using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    public interface IRepaymentService
    {
        Task<int> RecordRepaymentAsync(int loanId, int? userId, decimal amount, decimal poolDonation);
    }
}
