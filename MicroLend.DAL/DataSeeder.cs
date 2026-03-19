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
        private static readonly string SeederLogPath = Path.Combine(Path.GetTempPath(), "MicroLend_seeder_log.txt");

        private static void Log(string text)
        {
            try
            {
                File.AppendAllText(SeederLogPath, DateTime.Now.ToString("s") + " " + text + Environment.NewLine);
            }
            catch { }
        }

        // Safe SaveChanges wrapper to prevent seeding exceptions from crashing startup.
        private static async Task SafeSaveChangesAsync(MicroLendDbContext context)
        {
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                try { Log("SaveChanges failed: " + ex.ToString()); } catch { }
            }
        }

        // Ensure CSV files contain any parent records referenced by other files.
        private static async Task NormalizeSeedFilesAsync(MicroLendDbContext context, string dataDir)
        {
            var usersFile = Path.Combine(dataDir, "Users_Seeding.csv");
            var borrowersFile = Path.Combine(dataDir, "Borrowers_Seeding.csv");
            var loansFile = Path.Combine(dataDir, "Loans_Seeding.csv");
            var repaymentsFile = Path.Combine(dataDir, "Repayments_Seeding.csv");
            var loanFundersFile = Path.Combine(dataDir, "LoanFunders_Seeding.csv");

            // load existing ids
            var existingUsers = new HashSet<int>();
            if (File.Exists(usersFile))
            {
                var lines = await File.ReadAllLinesAsync(usersFile);
                foreach (var l in lines.Skip(1))
                {
                    var cols = SplitCsv(l);
                    if (cols.Length == 0) continue;
                    if (int.TryParse(cols[0], out var id)) existingUsers.Add(id);
                }
            }
            else
            {
                // create basic users file
                await File.WriteAllTextAsync(usersFile, "Id,Username,Password,Role" + Environment.NewLine);
            }

            var existingBorrowers = new HashSet<int>();
            if (File.Exists(borrowersFile))
            {
                var lines = await File.ReadAllLinesAsync(borrowersFile);
                foreach (var l in lines.Skip(1))
                {
                    var cols = SplitCsv(l);
                    if (cols.Length == 0) continue;
                    if (int.TryParse(cols[0], out var id)) existingBorrowers.Add(id);
                }
            }
            else
            {
                await File.WriteAllTextAsync(borrowersFile, "Id,FullName,ContactInfo,MonthlyIncome,UserId" + Environment.NewLine);
            }

            var existingLoans = new HashSet<int>();
            if (File.Exists(loansFile))
            {
                var lines = await File.ReadAllLinesAsync(loansFile);
                foreach (var l in lines.Skip(1))
                {
                    var cols = SplitCsv(l);
                    if (cols.Length == 0) continue;
                    if (int.TryParse(cols[0], out var id)) existingLoans.Add(id);
                }
            }
            else
            {
                await File.WriteAllTextAsync(loansFile, "Id,Purpose,TargetAmount,CurrentAmount,IsApproved,BorrowerId" + Environment.NewLine);
            }

            // Ensure users referenced by borrowers exist
            if (File.Exists(borrowersFile))
            {
                var lines = await File.ReadAllLinesAsync(borrowersFile);
                var toAppendUsers = new List<string>();
                foreach (var l in lines.Skip(1))
                {
                    var cols = SplitCsv(l);
                    if (cols.Length < 5) continue;
                    if (int.TryParse(cols[4], out var userId) && userId > 0 && !existingUsers.Contains(userId))
                    {
                        // create realistic username from name
                        var name = cols[1];
                        var uname = new string(name.Where(ch => !char.IsWhiteSpace(ch)).ToArray()).ToLower();
                        if (string.IsNullOrWhiteSpace(uname)) uname = "user" + userId;
                        var line = $"{userId},{uname},password,Borrower";
                        toAppendUsers.Add(line);
                        existingUsers.Add(userId);
                    }
                }
                if (toAppendUsers.Any()) await File.AppendAllLinesAsync(usersFile, toAppendUsers);
            }

            // Ensure borrowers referenced by loans exist
            if (File.Exists(loansFile))
            {
                var lines = await File.ReadAllLinesAsync(loansFile);
                var toAppendBorrowers = new List<string>();
                foreach (var l in lines.Skip(1))
                {
                    var cols = SplitCsv(l);
                    if (cols.Length < 6) continue;
                    if (int.TryParse(cols[5], out var borrowerId) && borrowerId > 0 && !existingBorrowers.Contains(borrowerId))
                    {
                        var fullname = "Auto Borrower " + borrowerId;
                        var contact = "auto" + borrowerId + "@example.com";
                        var income = "0";
                        var userId = borrowerId;
                        // ensure user exists
                        if (!existingUsers.Contains(userId))
                        {
                            var uname = "user" + userId;
                            await File.AppendAllLinesAsync(usersFile, new[] { $"{userId},{uname},password,Borrower" });
                            existingUsers.Add(userId);
                        }
                        toAppendBorrowers.Add($"{borrowerId},{fullname},{contact},{income},{userId}");
                        existingBorrowers.Add(borrowerId);
                    }
                }
                if (toAppendBorrowers.Any()) await File.AppendAllLinesAsync(borrowersFile, toAppendBorrowers);
            }

            // Ensure loans referenced by repayments exist
            if (File.Exists(repaymentsFile))
            {
                var lines = await File.ReadAllLinesAsync(repaymentsFile);
                var toAppendLoans = new List<string>();
                foreach (var l in lines.Skip(1))
                {
                    var cols = SplitCsv(l);
                    if (cols.Length < 2) continue;
                    if (int.TryParse(cols[1], out var loanId) && loanId > 0 && !existingLoans.Contains(loanId))
                    {
                        // create placeholder borrower if needed
                        if (!existingBorrowers.Contains(loanId))
                        {
                            var fullname = "Auto Borrower " + loanId;
                            var contact = "auto" + loanId + "@example.com";
                            var income = "0";
                            var userId = loanId;
                            if (!existingUsers.Contains(userId))
                            {
                                await File.AppendAllLinesAsync(usersFile, new[] { $"{userId},user{userId},password,Borrower" });
                                existingUsers.Add(userId);
                            }
                            await File.AppendAllLinesAsync(borrowersFile, new[] { $"{loanId},{fullname},{contact},{income},{userId}" });
                            existingBorrowers.Add(loanId);
                        }

                        toAppendLoans.Add($"{loanId},Placeholder loan for repayment,0,0,True,{loanId}");
                        existingLoans.Add(loanId);
                    }
                }
                if (toAppendLoans.Any()) await File.AppendAllLinesAsync(loansFile, toAppendLoans);
            }

            // Ensure lenders in LoanFunders file exist and loans exist
            if (File.Exists(loanFundersFile))
            {
                var lines = await File.ReadAllLinesAsync(loanFundersFile);
                var toAppendUsers2 = new List<string>();
                var toAppendLoans2 = new List<string>();
                foreach (var l in lines.Skip(1))
                {
                    var cols = SplitCsv(l);
                    if (cols.Length < 2) continue;
                    int.TryParse(cols[0], out var loanId);
                    int.TryParse(cols[1], out var lenderId);
                    if (lenderId > 0 && !existingUsers.Contains(lenderId))
                    {
                        toAppendUsers2.Add($"{lenderId},user{lenderId},password,Lender");
                        existingUsers.Add(lenderId);
                    }
                    if (loanId > 0 && !existingLoans.Contains(loanId))
                    {
                        // create borrower and loan
                        if (!existingBorrowers.Contains(loanId))
                        {
                            await File.AppendAllLinesAsync(borrowersFile, new[] { $"{loanId},Auto Borrower {loanId},auto{loanId}@example.com,0,{loanId}" });
                            existingBorrowers.Add(loanId);
                        }
                        toAppendLoans2.Add($"{loanId},Placeholder loan for funder,0,0,True,{loanId}");
                        existingLoans.Add(loanId);
                    }
                }
                if (toAppendUsers2.Any()) await File.AppendAllLinesAsync(usersFile, toAppendUsers2);
                if (toAppendLoans2.Any()) await File.AppendAllLinesAsync(loansFile, toAppendLoans2);
            }
        }

        // Note: placeholder creation via DB was removed to avoid save-time FK conflicts.

        public static async Task SeedAsync(MicroLendDbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            try { if (File.Exists(SeederLogPath)) File.Delete(SeederLogPath); } catch { }

            var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
            if (!Directory.Exists(dataDir)) return; // no data to seed

            // Make sure CSV files are normalized and contain referenced parent rows.
            try { await NormalizeSeedFilesAsync(context, dataDir); } catch (Exception ex) { Log("NormalizeSeedFiles failed: " + ex.Message); }

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
                    // CSV may include an Id column as the first value. Handle both formats:
                    // Format A: Id,Username,Password,Role
                    // Format B: Username,Password,Role
                    int parsedId = 0;
                    string username;
                    string password;
                    string role;

                    if (cols.Length >= 4 && int.TryParse(cols[0], out parsedId))
                    {
                        username = cols.ElementAtOrDefault(1) ?? string.Empty;
                        password = cols.ElementAtOrDefault(2) ?? string.Empty;
                        role = cols.ElementAtOrDefault(3) ?? "Borrower";
                    }
                    else
                    {
                        username = cols.ElementAtOrDefault(0) ?? string.Empty;
                        password = cols.ElementAtOrDefault(1) ?? string.Empty;
                        role = cols.ElementAtOrDefault(2) ?? "Borrower";
                    }

                    // Normalize trimmed values
                    username = (username ?? string.Empty).Trim();
                    password = (password ?? string.Empty).Trim();
                    role = (role ?? "Borrower").Trim();

                    var user = new User
                    {
                        Username = username,
                        PasswordHash = password,
                        Role = role,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    if (parsedId > 0)
                    {
                        user.Id = parsedId;
                    }
                    await context.Users.AddAsync(user);
                }
                catch { /* skip bad row */ }
            }
            await SafeSaveChangesAsync(context);
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

                    // CSV may include Id as the first column. Handle both formats:
                    // Format A: Id,FullName,ContactInfo,MonthlyIncome,UserId
                    // Format B: FullName,ContactInfo,MonthlyIncome,UserId
                    int parsedId = 0;
                    string name = string.Empty;
                    string contact = string.Empty;
                    decimal income = 0m;
                    int? userId = null;

                    if (cols.Length >= 5 && int.TryParse(cols[0], out parsedId))
                    {
                        name = cols.ElementAtOrDefault(1) ?? string.Empty;
                        contact = cols.ElementAtOrDefault(2) ?? string.Empty;
                        income = decimal.TryParse(cols.ElementAtOrDefault(3), NumberStyles.Any, CultureInfo.InvariantCulture, out var inc) ? inc : 0m;
                        userId = int.TryParse(cols.ElementAtOrDefault(4), out var uid) ? uid : (int?)null;
                    }
                    else
                    {
                        name = cols.ElementAtOrDefault(0) ?? string.Empty;
                        contact = cols.ElementAtOrDefault(1) ?? string.Empty;
                        income = decimal.TryParse(cols.ElementAtOrDefault(2), NumberStyles.Any, CultureInfo.InvariantCulture, out var inc) ? inc : 0m;
                        userId = int.TryParse(cols.ElementAtOrDefault(3), out var uid) ? uid : (int?)null;
                    }

                    var borrower = new Borrower
                    {
                        Name = name,
                        ContactNumber = contact,
                        MonthlyIncome = income,
                        UserId = userId
                    };

                    if (parsedId > 0) borrower.Id = parsedId;
                    await context.Borrowers.AddAsync(borrower);
                }
                catch { }
            }
            await SafeSaveChangesAsync(context);
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

                    // ensure user exists
                    var userExists = await context.Users.FindAsync(userId) != null;
                    if (!userExists)
                    {
                        Log($"Skipped CreditScore row - user missing: data={string.Join(',', cols)}");
                        continue;
                    }

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
            await SafeSaveChangesAsync(context);
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

                    // Handle different CSV formats. Common format in this project:
                    // Id,Purpose,TargetAmount,CurrentAmount,IsApproved,BorrowerId
                    int parsedId = 0;
                    string purpose = string.Empty;
                    decimal target = 0m;
                    decimal current = 0m;
                    bool isApproved = false;
                    int borrowerId = 0;

                    if (cols.Length >= 6 && int.TryParse(cols[0], out parsedId))
                    {
                        purpose = cols.ElementAtOrDefault(1) ?? string.Empty;
                        target = decimal.TryParse(cols.ElementAtOrDefault(2), NumberStyles.Any, CultureInfo.InvariantCulture, out var t) ? t : 0m;
                        current = decimal.TryParse(cols.ElementAtOrDefault(3), NumberStyles.Any, CultureInfo.InvariantCulture, out var c) ? c : 0m;
                        isApproved = bool.TryParse(cols.ElementAtOrDefault(4), out var ap) ? ap : false;
                        borrowerId = int.TryParse(cols.ElementAtOrDefault(5), out var bid) ? bid : 0;
                    }
                    else
                    {
                        // fallback to previous mapping
                        purpose = cols.ElementAtOrDefault(0) ?? string.Empty;
                        target = decimal.TryParse(cols.ElementAtOrDefault(1), NumberStyles.Any, CultureInfo.InvariantCulture, out var t) ? t : 0m;
                        current = decimal.TryParse(cols.ElementAtOrDefault(2), NumberStyles.Any, CultureInfo.InvariantCulture, out var c) ? c : 0m;
                        isApproved = bool.TryParse(cols.ElementAtOrDefault(3), out var ap) ? ap : false;
                        borrowerId = int.TryParse(cols.ElementAtOrDefault(4), out var bid) ? bid : 0;
                    }

                    // ensure borrower exists (borrowerId must be present). If missing skip the loan.
                    if (borrowerId <= 0)
                    {
                        Log($"Skipped Loan row - invalid borrower id: data={string.Join(',', cols)}");
                        continue;
                    }
                    var borrowerExists = await context.Borrowers.FindAsync(borrowerId) != null;
                    if (!borrowerExists)
                    {
                        Log($"Skipped Loan row - borrower missing: data={string.Join(',', cols)}");
                        continue;
                    }

                    var loan = new Loan
                    {
                        Purpose = purpose,
                        TargetAmount = target,
                        CurrentAmount = current,
                        Amount = target,
                        InterestRate = 5.0m,
                        Status = isApproved ? "Active" : "Funding",
                        IsCrowdfunded = true,
                        BorrowerId = borrowerId,
                        DateGranted = null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    if (parsedId > 0) loan.Id = parsedId;
                    await context.Loans.AddAsync(loan);
                }
                catch { }
            }
            await SafeSaveChangesAsync(context);
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
                    // Support two CSV formats:
                    // A: Id,LoanId,LenderId,ContributionAmount
                    // B: LoanId,LenderId,Amount,ExpectedInterest,FundingDate
                    int parsedId = 0;
                    int loanId = 0;
                    int lenderId = 0;
                    decimal amount = 0m;
                    decimal expectedInterest = 0m;
                    DateTime fundingDate = DateTime.Now;

                    if (cols.Length >= 4 && int.TryParse(cols[0], out parsedId) && int.TryParse(cols[1], out var lIdFromIdRow))
                    {
                        // format A
                        loanId = lIdFromIdRow;
                        lenderId = int.TryParse(cols.ElementAtOrDefault(2), out var u1) ? u1 : 0;
                        amount = decimal.TryParse(cols.ElementAtOrDefault(3), NumberStyles.Any, CultureInfo.InvariantCulture, out var a1) ? a1 : 0m;
                    }
                    else
                    {
                        loanId = int.TryParse(cols.ElementAtOrDefault(0), out var l0) ? l0 : 0;
                        lenderId = int.TryParse(cols.ElementAtOrDefault(1), out var u0) ? u0 : 0;
                        amount = decimal.TryParse(cols.ElementAtOrDefault(2), NumberStyles.Any, CultureInfo.InvariantCulture, out var a0) ? a0 : 0m;
                        expectedInterest = decimal.TryParse(cols.ElementAtOrDefault(3), NumberStyles.Any, CultureInfo.InvariantCulture, out var ei0) ? ei0 : 0m;
                        fundingDate = DateTime.TryParse(cols.ElementAtOrDefault(4), out var fd0) ? fd0 : DateTime.Now;
                    }

                    var lf = new LoanFunder
                    {
                        LoanId = loanId,
                        LenderId = lenderId,
                        Amount = amount,
                        ExpectedInterest = expectedInterest,
                        FundingDate = fundingDate,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    // Validate foreign keys: ensure referenced loan and lender user exist. If not, skip the entry to avoid FK constraint errors.
                    var loanExists = await context.Loans.FindAsync(lf.LoanId) != null;
                    var lenderExists = await context.Users.FindAsync(lf.LenderId) != null;
                    if (!lenderExists || !loanExists)
                    {
                        Log($"Skipped LoanFunder row - missing parent(s): loanExists={loanExists} lenderExists={lenderExists} data={string.Join(',', cols)}");
                        continue;
                    }

                    await context.LoanFunders.AddAsync(lf);
                }
                catch { }
            }
            await SafeSaveChangesAsync(context);
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

                    var loanExists = await context.Loans.FindAsync(r.LoanId) != null;
                    var userExists = r.UserId.HasValue ? await context.Users.FindAsync(r.UserId.Value) != null : true;
                    if (!loanExists || !userExists)
                    {
                        Log($"Skipped Repayment row - missing parent(s): loanExists={loanExists} userExists={userExists} data={string.Join(',', cols)}");
                        continue;
                    }

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
                    // Validate user exists
                    var userExists = await context.Users.FindAsync(tx.UserId) != null;
                    if (!userExists)
                    {
                        Log($"Skipped EmergencyPool row - user missing: data={string.Join(',', cols)}");
                        continue;
                    }

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
                    var loanExists = await context.Loans.FindAsync(inv.LoanId) != null;
                    if (!loanExists)
                    {
                        Log($"Skipped Investment row - loan missing: data={string.Join(',', cols)}");
                        continue;
                    }

                    await context.Set<Investment>().AddAsync(inv);
                }
                catch { }
            }
            await context.SaveChangesAsync();
        }
    }
}
