using Microsoft.AspNetCore.Mvc;
using MicroLend.BLL.Services;
using System.Threading.Tasks;
using MicroLend.DAL.Repositories;
using System.Linq;

namespace MicroLend.Web.Controllers
{
    public class LenderController : Controller
    {
        private readonly IInvestmentService _invest;
        private readonly LoanRepository _loanRepo = new LoanRepository();
        public LenderController(IInvestmentService invest) { _invest = invest; }

        public IActionResult Dashboard() => View();

        public async Task<IActionResult> Browse()
        {
            // Load all approved loans
            var list = await _loanRepo.GetApprovedLoansAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Invest(int loanId, decimal amount)
        {
            int lenderId = 1;
            if (User.Identity?.IsAuthenticated == true)
            {
                var idClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out var parsed)) lenderId = parsed;
            }

            var id = await _invest.CreateInvestmentAsync(lenderId, loanId, amount);
            TempData["Success"] = "Investment created and pending approval.";
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> Details(int id)
        {
            var loan = await _loanRepo.GetLoanWithFundingAsync(id);
            if (loan == null) return NotFound();
            return View(loan);
        }
    }
}
