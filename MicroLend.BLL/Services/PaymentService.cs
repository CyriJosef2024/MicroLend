using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    public class PaymentService : IPaymentService
    {
        // Replace this method with a real payment provider integration. Example shows a mock GCash/HTTP provider call.
        public async Task<string> ProcessPaymentAsync(int loanId, decimal amount, string method)
        {
            // Example: call a hypothetical payment gateway API (this is still a scaffold; replace details per provider)
            try
            {
                using var client = new System.Net.Http.HttpClient();
                var payload = new System.Collections.Generic.Dictionary<string, string>
                {
                    ["loanId"] = loanId.ToString(),
                    ["amount"] = amount.ToString("F2"),
                    ["method"] = method
                };
                var resp = await client.PostAsync("https://httpbin.org/post", new System.Net.Http.FormUrlEncodedContent(payload));
                resp.EnsureSuccessStatusCode();
                var j = await resp.Content.ReadAsStringAsync();
                // Return a simple transaction marker using timestamp
                return "TX-" + System.DateTime.UtcNow.Ticks;
            }
            catch
            {
                // If provider call fails, throw so caller shows error
                throw;
            }
        }
    }
}
