using MicroLend.DAL.Entities;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=MicroLend.db");
}
