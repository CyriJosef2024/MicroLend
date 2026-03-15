using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MicroLend.DAL.Entities;

namespace MicroLend.DAL
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(MicroLendDbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
            if (!Directory.Exists(dataDir)) return; // no data to seed

            // Users
            if (!await context.Users.AnyAsync())
            {
                var file = Path.Combine(dataDir, "Users_Seeding.csv");
                await SeedUsersAsync(context, file);
            }

            // Borrowers
            if (!await context.Borrowers.AnyAsync())
            {
                var file = Path.Combine(dataDir, "Borrowers_Seeding.csv");
                await SeedBorrowersAsync(context, file);
            }

            // Credit scores
            if (!await context.CreditScores.AnyAsync())
            {
                var file = Path.Combine(dataDir, "CreditScores_Seeding.csv");
                await SeedCreditScoresAsync(context, file);
            }

            // Loans
            if (!await context.Loans.AnyAsync())
            {
                var file = Path.Combine(dataDir, "Loans_Seeding.csv");
                await SeedLoansAsync(context, file);
            }

            // Loan funders
            if (!await context.LoanFunders.AnyAsync())
            {
                var file = Path.Combine(dataDir, "LoanFunders_Seeding.csv");
                await SeedLoanFundersAsync(context, file);
            }

            // Repayments
            if (!await context.Repayments.AnyAsync())
            {
                var file = Path.Combine(dataDir, "Repayments_Seeding.csv");
                await SeedRepaymentsAsync(context, file);
            }

            // Emergency pool transactions
            if (!await context.EmergencyPoolTransactions.AnyAsync())
            {
                var file = Path.Combine(dataDir, "EmergencyPool_Seeding.csv");
                await SeedEmergencyPoolAsync(context, file);
            }

            // Investments (legacy)
            if (!await context.Set<Investment>().AnyAsync())
            {
                var file = Path.Combine(dataDir, "Loans_Seeding.csv"); // maybe investments not provided, skip or use Investments_Seeding.csv if exists
                var investmentsFile = Path.Combine(dataDir, "Investments_Seeding.csv");
                if (File.Exists(investmentsFile))
                    await SeedInvestmentsAsync(context, investmentsFile);
            }
        }

        static string[] SplitCsv(string line)
        {
            // Very small CSV splitter: trims quotes and splits on commas.
            // This is intentionally simple; it will fail for quoted commas.
            return line.Split(',').Select(p => p.Trim().Trim('"')).ToArray();
        }

        static async Task SeedUsersAsync(MicroLendDbContext context, string file)
        {
            if (!File.Exists(file)) return;
            var lines = await File.ReadAllLinesAsync(file);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var cols = SplitCsv(line);
                    var user = new User
                    {
                        Username = cols.ElementAtOrDefault(0) ?? string.Empty,
                        PasswordHash = cols.ElementAtOrDefault(1) ?? string.Empty,
                        Role = cols.ElementAtOrDefault(2) ?? "Borrower",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    await context.Users.AddAsync(user);
                }
                catch { /* skip bad row */ }
            }
            await context.SaveChangesAsync();
        }

        static async Task SeedBorrowersAsync(MicroLendDbContext context, string file)
        {
            if (!File.Exists(file)) return;
            var lines = await File.ReadAllLinesAsync(file);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var cols = SplitCsv(line);
                    var borrower = new Borrower
                    {
                        Name = cols.ElementAtOrDefault(0) ?? string.Empty,
                        ContactNumber = cols.ElementAtOrDefault(1) ?? string.Empty,
                        MonthlyIncome = decimal.TryParse(cols.ElementAtOrDefault(2), NumberStyles.Any, CultureInfo.InvariantCulture, out var inc) ? inc : 0m,
                        // attempt to assign UserId if present
                        UserId = int.TryParse(cols.ElementAtOrDefault(3), out var uid) ? uid : (int?)null
                    };
                    await context.Borrowers.AddAsync(borrower);
                }
                catch { }
            }
            await context.SaveChangesAsync();
        }

        static async Task SeedCreditScoresAsync(MicroLendDbContext context, string file)
        {
            if (!File.Exists(file)) return;
            var lines = await File.ReadAllLinesAsync(file);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var cols = SplitCsv(line);
                    // Expect: UserId,Score,QuizDate,Details
                    var userId = int.TryParse(cols.ElementAtOrDefault(0), out var uid) ? uid : 0;
                    var score = int.TryParse(cols.ElementAtOrDefault(1), out var s) ? s : 0;
                    var date = DateTime.TryParse(cols.ElementAtOrDefault(2), out var d) ? d : DateTime.Now;
                    var details = cols.ElementAtOrDefault(3) ?? string.Empty;

                    var cs = new CreditScore
                    {
                        UserId = userId,
                        Score = score,
                        QuizDate = date,
                        Details = details,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    await context.CreditScores.AddAsync(cs);
                }
                catch { }
            }
            await context.SaveChangesAsync();
        }

        static async Task SeedLoansAsync(MicroLendDbContext context, string file)
        {
            if (!File.Exists(file)) return;
            var lines = await File.ReadAllLinesAsync(file);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var cols = SplitCsv(line);
                    var loan = new Loan
                    {
                        Purpose = cols.ElementAtOrDefault(0) ?? string.Empty,
                        TargetAmount = decimal.TryParse(cols.ElementAtOrDefault(1), NumberStyles.Any, CultureInfo.InvariantCulture, out var t) ? t : 0m,
                        CurrentAmount = decimal.TryParse(cols.ElementAtOrDefault(2), NumberStyles.Any, CultureInfo.InvariantCulture, out var c) ? c : 0m,
                        InterestRate = decimal.TryParse(cols.ElementAtOrDefault(3), NumberStyles.Any, CultureInfo.InvariantCulture, out var ir) ? ir : 0m,
                        Status = cols.ElementAtOrDefault(4) ?? "Funding",
                        IsCrowdfunded = bool.TryParse(cols.ElementAtOrDefault(5), out var ic) ? ic : true,
                        BorrowerId = int.TryParse(cols.ElementAtOrDefault(6), out var bid) ? bid : 0,
                        DateGranted = DateTime.TryParse(cols.ElementAtOrDefault(7), out var dg) ? dg : (DateTime?)null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    await context.Loans.AddAsync(loan);
                }
                catch { }
            }
            await context.SaveChangesAsync();
        }

        static async Task SeedLoanFundersAsync(MicroLendDbContext context, string file)
        {
            if (!File.Exists(file)) return;
            var lines = await File.ReadAllLinesAsync(file);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var cols = SplitCsv(line);
                    var lf = new LoanFunder
                    {
                        LoanId = int.TryParse(cols.ElementAtOrDefault(0), out var lid) ? lid : 0,
                        LenderId = int.TryParse(cols.ElementAtOrDefault(1), out var uid) ? uid : 0,
                        Amount = decimal.TryParse(cols.ElementAtOrDefault(2), NumberStyles.Any, CultureInfo.InvariantCulture, out var a) ? a : 0m,
                        ExpectedInterest = decimal.TryParse(cols.ElementAtOrDefault(3), NumberStyles.Any, CultureInfo.InvariantCulture, out var ei) ? ei : 0m,
                        FundingDate = DateTime.TryParse(cols.ElementAtOrDefault(4), out var fd) ? fd : DateTime.Now,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    await context.LoanFunders.AddAsync(lf);
                }
                catch { }
            }
            await context.SaveChangesAsync();
        }

        static async Task SeedRepaymentsAsync(MicroLendDbContext context, string file)
        {
            if (!File.Exists(file)) return;
            var lines = await File.ReadAllLinesAsync(file);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var cols = SplitCsv(line);
                    var r = new Repayment
                    {
                        LoanId = int.TryParse(cols.ElementAtOrDefault(0), out var lid) ? lid : 0,
                        Amount = decimal.TryParse(cols.ElementAtOrDefault(1), NumberStyles.Any, CultureInfo.InvariantCulture, out var a) ? a : 0m,
                        PaymentDate = DateTime.TryParse(cols.ElementAtOrDefault(2), out var pd) ? pd : DateTime.Now,
                        UserId = int.TryParse(cols.ElementAtOrDefault(3), out var uid) ? uid : (int?)null
                    };
                    await context.Repayments.AddAsync(r);
                }
                catch { }
            }
            await context.SaveChangesAsync();
        }

        static async Task SeedEmergencyPoolAsync(MicroLendDbContext context, string file)
        {
            if (!File.Exists(file)) return;
            var lines = await File.ReadAllLinesAsync(file);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var cols = SplitCsv(line);
                    // Expect: UserId,Amount,Type,Description,TransactionDate
                    var tx = new EmergencyPoolTransaction
                    {
                        UserId = int.TryParse(cols.ElementAtOrDefault(0), out var uid) ? uid : 0,
                        Amount = decimal.TryParse(cols.ElementAtOrDefault(1), NumberStyles.Any, CultureInfo.InvariantCulture, out var amt) ? amt : 0m,
                        Type = cols.ElementAtOrDefault(2) ?? "Donation",
                        Description = cols.ElementAtOrDefault(3) ?? string.Empty,
                        TransactionDate = DateTime.TryParse(cols.ElementAtOrDefault(4), out var td) ? td : DateTime.Now,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    await context.EmergencyPoolTransactions.AddAsync(tx);

                    // update or create pool
                    var pool = await context.Set<EmergencyPool>().FirstOrDefaultAsync();
                    if (pool == null)
                    {
                        pool = new EmergencyPool { TotalBalance = 0m };
                        await context.Set<EmergencyPool>().AddAsync(pool);
                    }

                    pool.TotalBalance += tx.Type == "Donation" ? tx.Amount : -tx.Amount;
                }
                catch { }
            }
            await context.SaveChangesAsync();
        }

        static async Task SeedInvestmentsAsync(MicroLendDbContext context, string file)
        {
            if (!File.Exists(file)) return;
            var lines = await File.ReadAllLinesAsync(file);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var cols = SplitCsv(line);
                    var inv = new Investment
                    {
                        AmountInvested = decimal.TryParse(cols.ElementAtOrDefault(0), NumberStyles.Any, CultureInfo.InvariantCulture, out var a) ? a : 0m,
                        LoanId = int.TryParse(cols.ElementAtOrDefault(1), out var lid) ? lid : 0
                    };
                    await context.Set<Investment>().AddAsync(inv);
                }
                catch { }
            }
            await context.SaveChangesAsync();
        }
    }
}
