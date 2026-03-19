using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using MicroLend.DAL.Repositories;

namespace MicroLend.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepo = new UserRepository();

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var users = await _userRepo.GetAllAsync();
            var user = users.Find(u => u.Username == username && u.PasswordHash == password);
            if (user == null) { ModelState.AddModelError("", "Invalid credentials"); return View(); }

            var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, user.Role), new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
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
            var repo = new MicroLend.DAL.Repositories.Repository<MicroLend.DAL.Entities.User>();
            var u = new MicroLend.DAL.Entities.User { Username = username, PasswordHash = password, Role = role };
            await repo.AddAsync(u);

            // Auto sign-in after registration
            var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, u.Username), new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, u.Role), new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, u.Id.ToString()) };
            var id = new System.Security.Claims.ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, new System.Security.Claims.ClaimsPrincipal(id));

            TempData["Success"] = "Account created and signed in.";
            if (string.Equals(role, "Borrower", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Upload", "Borrower");
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
