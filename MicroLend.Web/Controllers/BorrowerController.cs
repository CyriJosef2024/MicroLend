using Microsoft.AspNetCore.Mvc;
using MicroLend.BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace MicroLend.Web.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Borrower")]
    public class BorrowerController : Controller
    {
        private readonly ICreditScoreService _credit;
        private readonly ILoanService _loanSvc;
        private readonly IDocumentService _documentSvc;
        private readonly IRepaymentService _repaymentSvc;

        public BorrowerController(ICreditScoreService credit, ILoanService loanSvc, IDocumentService documentSvc, IRepaymentService repaymentSvc)
        {
            _credit = credit;
            _loanSvc = loanSvc;
            _documentSvc = documentSvc;
            _repaymentSvc = repaymentSvc;
        }

        public IActionResult Dashboard()
        {
            // show basic borrower summary: credit score and active loans
            int userId = 1;
            if (User.Identity?.IsAuthenticated == true)
            {
                var idClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out var parsed)) userId = parsed;
            }
            var creditRepo = new MicroLend.DAL.Repositories.CreditScoreRepository();
            var latest = creditRepo.GetByUserIdAsync(userId).GetAwaiter().GetResult().OrderByDescending(c => c.QuizDate).FirstOrDefault();
            ViewBag.CreditScore = latest?.Score ?? 0;
            var loanRepo = new MicroLend.DAL.Repositories.LoanRepository();
            var active = loanRepo.GetActiveLoansByBorrowerAsync(userId).GetAwaiter().GetResult().Count;
            ViewBag.ActiveLoans = active;
            return View();
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Borrower")]
        public IActionResult Apply() => View(_credit.GetQuestions());

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Borrower")]
        public async Task<IActionResult> ApplyPost([FromForm] int amount, [FromForm] string purpose)
        {
            int userId = 1;
            if (User.Identity?.IsAuthenticated == true)
            {
                var idClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out var parsed)) userId = parsed;
            }

            // Require at least one uploaded document for this borrower
            using (var ctx = new MicroLend.DAL.MicroLendDbContext())
            {
                var hasDoc = ctx.Documents.Any(d => d.UserId == userId);
                if (!hasDoc)
                {
                    TempData["Error"] = "You must upload required documents before applying for a loan.";
                    return RedirectToAction("Upload");
                }
            }

            var answers = Request.Form.Where(kv => kv.Key.StartsWith("q_")).ToDictionary(kv => int.Parse(kv.Key.Substring(2)), kv => int.Parse(kv.Value));
            var score = await _credit.ScoreAndSaveAsync(userId, answers, 0);

            await _loanSvc.ApplyLoanAsync(userId, amount, purpose, score);
            TempData["Success"] = "Loan application submitted.";
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> MyLoans()
        {
            int userId = 1;
            if (User.Identity?.IsAuthenticated == true)
            {
                var idClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out var parsed)) userId = parsed;
            }
            var repo = new MicroLend.DAL.Repositories.LoanRepository();
            var loans = await repo.GetLoansByBorrowerAsync(userId);
            return View(loans);
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Borrower")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(IFormFile file)
        {
            if (file == null) { TempData["Error"] = "No file selected"; return RedirectToAction("Dashboard"); }
            int userId = 1;
            if (User.Identity?.IsAuthenticated == true)
            {
                var idClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out var parsed)) userId = parsed;
            }
            string path;
            try
            {
                path = await _documentSvc.SaveDocumentAsync(file, userId, null);
            }
            catch (System.Exception ex)
            {
                var msg = "Error saving file: " + ex.Message;
                TempData["Error"] = msg;
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") return Json(new { success = false, error = msg });
                return RedirectToAction("Upload");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                TempData["Error"] = "Server failed to save the uploaded file.";
                return RedirectToAction("Upload");
            }

            // persist metadata in Documents table and return created id
            try
            {
                using var ctx = new MicroLend.DAL.MicroLendDbContext();
                var doc = new MicroLend.DAL.Entities.Document { UserId = userId, FileName = file.FileName, FilePath = path, UploadedAt = System.DateTime.Now };
                ctx.Documents.Add(doc);
                await ctx.SaveChangesAsync();
                // return id as JSON when called via AJAX or from client
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, id = doc.Id, path });
                }
                TempData["Success"] = "File uploaded: " + path;
            }
            catch (System.Exception ex)
            {
                // surface error details for debugging and return JSON when AJAX
                var msg = "Error saving document record: " + ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "");
                TempData["Error"] = msg;
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = msg });
                }
            }
            return RedirectToAction("Dashboard");
        }

        // show upload page (used after registration redirect)
        public IActionResult Upload()
        {
            return View();
        }

        public IActionResult Repay()
        {
            // show simple repay form
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RepayPost([FromForm] int loanId, [FromForm] decimal amount, [FromForm] decimal poolDonation)
        {
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var idClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out var parsed)) userId = parsed;
            }
            await _repaymentSvc.RecordRepaymentAsync(loanId, userId, amount, poolDonation);
            TempData["Success"] = "Repayment recorded.";
            return RedirectToAction("MyLoans");
        }
    }
}
