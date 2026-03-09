using Microsoft.EntityFrameworkCore;
using MicroLend.DAL.Entities;

public class MicroLendDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Borrower> Borrowers { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<Repayment> Repayments { get; set; }
    public DbSet<LoanFunder> LoanFunders { get; set; }
    public DbSet<CreditScore> CreditScores { get; set; }
    public DbSet<EmergencyPool> EmergencyPools { get; set; }
    public DbSet<EmergencyPoolTransaction> EmergencyPoolTransactions { get; set; }
    public DbSet<Investment> Investments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=MicroLend.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User <-> Borrower : one-to-one (optional)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Borrower)
            .WithOne(b => b.User)
            .HasForeignKey<Borrower>(b => b.UserId)
            .IsRequired(false);

        // Borrower <-> CreditScore : one-to-one
        modelBuilder.Entity<Borrower>()
            .HasOne(b => b.CreditScore)
            .WithOne(c => c.Borrower)
            .HasForeignKey<CreditScore>(c => c.BorrowerId)
            .IsRequired(false);

        // Loan <-> Repayment : one-to-many
        modelBuilder.Entity<Loan>()
            .HasMany(l => l.Repayments)
            .WithOne(r => r.Loan)
            .HasForeignKey(r => r.LoanId);

        // Loan <-> LoanFunder : one-to-many
        modelBuilder.Entity<Loan>()
            .HasMany(l => l.Funders)
            .WithOne(f => f.Loan)
            .HasForeignKey(f => f.LoanId);

        // User <-> LoanFunder : one-to-many
        modelBuilder.Entity<User>()
            .HasMany(u => u.FundedLoans)
            .WithOne(f => f.User)
            .HasForeignKey(f => f.UserId);

        base.OnModelCreating(modelBuilder);
    }
}
