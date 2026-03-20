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
        // Allow an environment variable to override the DB path so multiple projects
        // (Web and WinForms) can be pointed at the same file during local testing.
        var env = System.Environment.GetEnvironmentVariable("MICROLEND_DB_PATH");
        var dbPath = string.IsNullOrWhiteSpace(env)
            ? Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "MicroLend", "MicroLend.db")
            : env;
        // Ensure the directory for the SQLite file exists. If the directory doesn't exist
        // SQLite will fail with "unable to open database file" when trying to create the file.
        var dbDir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
        {
            Directory.CreateDirectory(dbDir);
        }
        options.UseSqlite($"Data Source={dbPath}");
        // During active development there may be pending model changes; either apply migrations or
        // suppress the warning. We prefer migrations, but suppress here to avoid startup failure
        // when developer hasn't applied migrations yet.
        options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }
}
