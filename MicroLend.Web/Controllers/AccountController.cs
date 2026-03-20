using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using MicroLend.DAL.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace MicroLend.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepo = new UserRepository();

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Invalid credentials");
                return View();
            }

            // Shortcut: allow built-in admin access regardless of DB state
            if (string.Equals(username, "admin", System.StringComparison.OrdinalIgnoreCase) && password == "admin123")
            {
                var claimsA = new[] { new Claim(ClaimTypes.Name, "admin"), new Claim(ClaimTypes.Role, "Admin"), new Claim(ClaimTypes.NameIdentifier, "0") };
                var idA = new ClaimsIdentity(claimsA, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(idA));
                return RedirectToAction("Dashboard", "Admin");
            }

            var user = await _userRepo.GetByUsernameAsync(username);
            if (user == null)
            {
                // Allow built-in admin shortcut even if DB user missing
                if (string.Equals(username, "admin", System.StringComparison.OrdinalIgnoreCase) && password == "admin123")
                {
                    var claimsA = new[] { new Claim(ClaimTypes.Name, "admin"), new Claim(ClaimTypes.Role, "Admin"), new Claim(ClaimTypes.NameIdentifier, "0") };
                    var idA = new ClaimsIdentity(claimsA, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(idA));
                    return RedirectToAction("Dashboard", "Admin");
                }

                ModelState.AddModelError("", "Invalid credentials");
                return View();
            }

            // password stored as SHA256 hex - hash incoming password for comparison
            var stored = (user.PasswordHash ?? string.Empty).Trim();
            var incomingHash = ToSha256Hex(password);

            bool ok;
            // If stored value looks like a SHA256 hex string, compare hashes
            if (stored.Length == 64 && System.Text.RegularExpressions.Regex.IsMatch(stored, "^[0-9A-Fa-f]{64}$"))
            {
                ok = string.Equals(incomingHash, stored, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                // allow legacy plaintext passwords: compare directly, and upgrade to hash on success
                ok = string.Equals(password, stored, StringComparison.Ordinal);
                if (ok)
                {
                    try
                    {
                        var repo = new MicroLend.DAL.Repositories.Repository<MicroLend.DAL.Entities.User>();
                        user.PasswordHash = incomingHash;
                        await repo.UpdateAsync(user);
                    }
                    catch { /* ignore upgrade failure */ }
                }
            }

            if (!ok)
            {
                ModelState.AddModelError("", "Invalid credentials");
                return View();
            }

            var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, user.Role), new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
            // Redirect admin to Admin dashboard, others to their dashboards
            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Dashboard", "Admin");
            if (string.Equals(user.Role, "Borrower", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Dashboard", "Borrower");
            if (string.Equals(user.Role, "Lender", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Dashboard", "Lender");

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] string username, [FromForm] string password, [FromForm] string role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Username and password are required.";
                return View();
            }

            // Only allow Borrower or Lender roles to be created via registration UI.
            var normalizedRole = "Borrower";
            if (!string.IsNullOrWhiteSpace(role) && role.Trim().Equals("Lender", System.StringComparison.OrdinalIgnoreCase))
                normalizedRole = "Lender";

            var repo = new MicroLend.DAL.Repositories.Repository<MicroLend.DAL.Entities.User>();
            var u = new MicroLend.DAL.Entities.User { Username = username.Trim(), PasswordHash = ToSha256Hex(password.Trim()), Role = normalizedRole };
            await repo.AddAsync(u);

            // Auto sign-in after registration
            var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, u.Username), new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, u.Role), new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, u.Id.ToString()) };
            var id = new System.Security.Claims.ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, new System.Security.Claims.ClaimsPrincipal(id));

            TempData["Success"] = "Account created and signed in.";
            if (string.Equals(u.Role, "Borrower", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Upload", "Borrower");
            }
            if (string.Equals(u.Role, "Lender", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "Lender");
            }
            return RedirectToAction("Index", "Home");
        }

        private static string ToSha256Hex(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
