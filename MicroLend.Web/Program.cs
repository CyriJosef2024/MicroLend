using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MicroLend.BLL.Services;
using MicroLend.DAL;

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