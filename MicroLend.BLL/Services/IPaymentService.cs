using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    public interface IPaymentService
    {
        /// <summary>
        /// Process a payment and return a transaction/reference id.
        /// </summary>
        Task<string> ProcessPaymentAsync(int loanId, decimal amount, string method);
    }
}
