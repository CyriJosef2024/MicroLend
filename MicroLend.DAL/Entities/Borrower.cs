namespace MicroLend.DAL.Entities;

public class Borrower
{
    public int Id { get; set; }

    // Link to application user (login / permissions)
    public int? UserId { get; set; }
    public User? User { get; set; }

    public string Name { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public decimal MonthlyIncome { get; set; }
    // Business type description used by scoring rules (e.g. "sari-sari", "farming")
    public string BusinessType { get; set; } = string.Empty;

    // Convenience full name for UI / dashboards
    public string FullName => Name;

    // Optional one-to-one credit score record
    public CreditScore? CreditScore { get; set; }

    // Whether borrower uploaded verification documents (computed from Documents table)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsVerified
    {
        get
        {
            try
            {
                using var ctx = new MicroLend.DAL.MicroLendDbContext();
                if (UserId == null) return false;
                return ctx.Documents.Any(d => d.UserId == UserId.Value);
            }
            catch
            {
                return false;
            }
        }
    }

    // Loans requested by this borrower
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
