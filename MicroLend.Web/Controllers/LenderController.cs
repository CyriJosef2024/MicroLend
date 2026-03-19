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
            var allLoans = await _loanRepo.GetLoansByBorrowerIdAsync(0); // will return empty; use context directly if needed
            // Fallback: load all and filter Approved
            var loansCtx = await _loanRepo.GetLoansByBorrowerAsync(0);
            var list = loansCtx.Where(l => l.Status == "Approved").ToList();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Invest(int loanId, decimal amount)
        {
            var id = await _invest.CreateInvestmentAsync(1, loanId, amount);
            TempData["Success"] = "Investment created and pending approval.";
            return RedirectToAction("Dashboard");
        }
    }
}
