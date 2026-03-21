using Microsoft.AspNetCore.Mvc;
using MicroLend.BLL.Services;
using System.Threading.Tasks;
using MicroLend.DAL;
using System.Linq;


namespace MicroLend.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILoanService _loan;
        private readonly IInvestmentService _inv;
        public AdminController(ILoanService loan, IInvestmentService inv) { _loan = loan; _inv = inv; }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public IActionResult Dashboard()
        {
            try
            {
                using var ctx = new MicroLendDbContext();
                ViewBag.UsersCount = ctx.Users.Count();
                ViewBag.BorrowersCount = ctx.Users.Count(u => u.Role == "Borrower");
                ViewBag.LendersCount = ctx.Users.Count(u => u.Role == "Lender");
                ViewBag.LoansPending = ctx.Loans.Count(l => l.Status == "Pending");
                ViewBag.LoansActive = ctx.Loans.Count(l => l.Status == "Active" || l.Status == "Approved");
                ViewBag.InvestmentsCount = ctx.LoanFunders.Count();
                ViewBag.RecentUsers = ctx.Users.OrderByDescending(u => u.CreatedAt).Take(5).ToList();
                ViewBag.PendingLoans = ctx.Loans.Where(l => l.Status == "Pending").OrderByDescending(l => l.CreatedAt).Take(8).ToList();
            }
            catch
            {
                ViewBag.UsersCount = 0;
                ViewBag.BorrowersCount = 0;
                ViewBag.LendersCount = 0;
                ViewBag.LoansPending = 0;
                ViewBag.LoansActive = 0;
                ViewBag.InvestmentsCount = 0;
                ViewBag.RecentUsers = new System.Collections.Generic.List<MicroLend.DAL.Entities.User>();
                ViewBag.PendingLoans = new System.Collections.Generic.List<MicroLend.DAL.Entities.Loan>();
            }
            return View();
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public IActionResult PendingLoans()
        {
            try
            {
                using var ctx = new MicroLend.DAL.MicroLendDbContext();
                var list = ctx.Loans.Where(l => l.Status == "Pending").OrderByDescending(l => l.CreatedAt).ToList();
                return View(list);
            }
            catch
            {
                return View(new System.Collections.Generic.List<MicroLend.DAL.Entities.Loan>());
            }
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveLoan(int id)
        {
            try
            {
                using var ctx = new MicroLend.DAL.MicroLendDbContext();
                var loan = ctx.Loans.Find(id);
                if (loan != null)
                {
                    loan.Status = "Approved";
                    loan.DateGranted = System.DateTime.Now;
                    ctx.Loans.Update(loan);
                    ctx.SaveChanges();
                }
                TempData["Success"] = "Loan approved.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Error approving loan: " + ex.Message;
            }
            return RedirectToAction("PendingLoans");
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectLoan(int id)
        {
            try
            {
                using var ctx = new MicroLend.DAL.MicroLendDbContext();
                var loan = ctx.Loans.Find(id);
                if (loan != null)
                {
                    loan.Status = "Rejected";
                    ctx.Loans.Update(loan);
                    ctx.SaveChanges();
                }
                TempData["Success"] = "Loan rejected.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Error rejecting loan: " + ex.Message;
            }
            return RedirectToAction("PendingLoans");
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public IActionResult Users()
        {
            try
            {
                using var ctx = new MicroLend.DAL.MicroLendDbContext();
                var list = ctx.Users.OrderBy(u => u.Id).ToList();
                return View(list);
            }
            catch
            {
                return View(new System.Collections.Generic.List<MicroLend.DAL.Entities.User>());
            }
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public IActionResult EditUser(int id)
        {
            try
            {
                using var ctx = new MicroLend.DAL.MicroLendDbContext();
                var u = ctx.Users.Find(id);
                if (u == null) return NotFound();
                return View(u);
            }
            catch
            {
                return NotFound();
            }
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(int id, [FromForm] string role, [FromForm] string password)
        {
            try
            {
                using var ctx = new MicroLend.DAL.MicroLendDbContext();
                var u = ctx.Users.Find(id);
                if (u == null) return NotFound();
                if (!string.IsNullOrWhiteSpace(role)) u.Role = role;
                if (!string.IsNullOrWhiteSpace(password))
                {
                    using var sha = System.Security.Cryptography.SHA256.Create();
                    u.PasswordHash = Convert.ToHexString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
                }
                ctx.Users.Update(u);
                ctx.SaveChanges();
                TempData["Success"] = "User updated.";
                return RedirectToAction("Users");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Error updating user: " + ex.Message;
                return RedirectToAction("Users");
            }
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeactivateUser(int id)
        {
            try
            {
                using var ctx = new MicroLend.DAL.MicroLendDbContext();
                var u = ctx.Users.Find(id);
                if (u != null)
                {
                    u.Role = "Deactivated";
                    ctx.Users.Update(u);
                    ctx.SaveChanges();
                }
                TempData["Success"] = "User deactivated.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Error deactivating user: " + ex.Message;
            }
            return RedirectToAction("Users");
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public IActionResult Logs()
        {
            try
            {
                var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MicroLend_seeder_log.txt");
                if (!System.IO.File.Exists(path)) return View((object)"No logs available.");
                var txt = System.IO.File.ReadAllText(path);
                return View((object)txt);
            }
            catch
            {
                return View((object)"Error reading logs.");
            }

        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public IActionResult UploadedDocuments()
        {
            try
            {
                using var ctx = new MicroLend.DAL.MicroLendDbContext();
                var list = ctx.Documents.OrderByDescending(d => d.UploadedAt).ToList();
                return View(list);
            }
            catch
            {
                return View(new System.Collections.Generic.List<MicroLend.DAL.Entities.Document>());
            }
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyDocument(int id, bool approve)
        {
            try
            {
                using var ctx = new MicroLend.DAL.MicroLendDbContext();
                var d = ctx.Documents.Find(id);
                if (d == null) return NotFound();
                d.Status = approve ? "Approved" : "Rejected";
                var idClaim = User?.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out var adminId)) d.ReviewedBy = adminId;
                d.ReviewedAt = System.DateTime.Now;
                ctx.Documents.Update(d);
                ctx.SaveChanges();
                TempData["Success"] = approve ? "Document approved." : "Document rejected.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Error updating document: " + ex.Message;
            }
            return RedirectToAction("UploadedDocuments");
        }
    }
}
