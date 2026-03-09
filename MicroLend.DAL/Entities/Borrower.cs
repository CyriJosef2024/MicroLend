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

    // Optional one-to-one credit score record
    public CreditScore? CreditScore { get; set; }

    // Loans requested by this borrower
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
