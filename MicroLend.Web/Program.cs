using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MicroLend.BLL.Services;
using MicroLend.DAL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// Authentication (cookie)
builder.Services.AddAuthentication("Cookies").AddCookie("Cookies", options => {
    options.LoginPath = "/Account/Login";
});
builder.Services.AddAuthorization();

// Register BLL services (simple DI)
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IInvestmentService, InvestmentService>();
builder.Services.AddScoped<IRepaymentService, RepaymentService>();
builder.Services.AddScoped<ICreditScoreService, CreditScoreService>();
// DocumentService: web implementation will be provided in MicroLend.Web
builder.Services.AddScoped<IDocumentService, MicroLend.Web.Services.WebDocumentService>();

// register DB context from DAL
builder.Services.AddDbContext<MicroLendDbContext>();

var app = builder.Build();

// Ensure the web app listens on a predictable URL for local testing
builder.WebHost.UseUrls("http://localhost:5000");

// Apply any pending EF Core migrations on startup so database schema matches model
try
{
    using (var scope = app.Services.CreateScope())
    {
        var ctx = scope.ServiceProvider.GetRequiredService<MicroLendDbContext>();
        ctx.Database.Migrate();
            // Seed initial data (users, borrowers, loans etc.) from Data folder when DB is empty
            try { MicroLend.DAL.DataSeeder.SeedAsync(ctx).GetAwaiter().GetResult(); } catch { }

            // Ensure an admin account exists (upsert) so Admin can always sign in
            try
            {
                var adminUser = ctx.Users.FirstOrDefault(u => u.Username == "admin");
                if (adminUser == null)
                {
                    var pwd = "admin123";
                    using var sha = System.Security.Cryptography.SHA256.Create();
                    var hash = Convert.ToHexString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pwd)));
                    var u = new MicroLend.DAL.Entities.User { Username = "admin", PasswordHash = hash, Role = "Admin", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };
                    ctx.Users.Add(u);
                    ctx.SaveChanges();
                }
            }
            catch { }
    }
}
catch
{
    // swallow migration errors; developer can inspect logs if needed
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();