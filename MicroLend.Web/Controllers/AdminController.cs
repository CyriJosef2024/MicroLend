using Microsoft.AspNetCore.Mvc;
using MicroLend.BLL.Services;
using System.Threading.Tasks;

namespace MicroLend.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILoanService _loan;
        private readonly IInvestmentService _inv;
        public AdminController(ILoanService loan, IInvestmentService inv) { _loan = loan; _inv = inv; }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public IActionResult Dashboard() => View();

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public IActionResult PendingLoans() => View();
    }
}
