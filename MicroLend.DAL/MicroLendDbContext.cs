using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.IO;

namespace MicroLend.DAL;

public class MicroLendDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Borrower> Borrowers { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<LoanFunder> LoanFunders { get; set; }
    public DbSet<Repayment> Repayments { get; set; }
    public DbSet<CreditScore> CreditScores { get; set; }
    public DbSet<EmergencyPoolTransaction> EmergencyPoolTransactions { get; set; }
    public DbSet<Investment> Investments { get; set; }
    public DbSet<EmergencyPool> EmergencyPools { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<ApiToken> ApiTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // Use an absolute path so callers that inspect the DB file (Program.cs) target the same file
        var dbPath = Path.Combine(AppContext.BaseDirectory, "MicroLend.db");
        options.UseSqlite($"Data Source={dbPath}");
        // During active development there may be pending model changes; either apply migrations or
        // suppress the warning. We prefer migrations, but suppress here to avoid startup failure
        // when developer hasn't applied migrations yet.
        options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }
}
