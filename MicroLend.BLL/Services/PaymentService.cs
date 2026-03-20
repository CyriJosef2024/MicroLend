using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    public class PaymentService : IPaymentService
    {
        // Replace this method with a real payment provider integration. Example shows a mock GCash/HTTP provider call.
        public async Task<string> ProcessPaymentAsync(int loanId, decimal amount, string method)
        {
            // For the demo, do not call an external provider. Return a quick fake transaction id.
            await Task.Delay(100); // small delay to simulate work
            return "TX-" + System.DateTime.UtcNow.Ticks;
        }
    }
}
