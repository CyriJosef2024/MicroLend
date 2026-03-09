namespace MicroLend.DAL.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Role: e.g. "Admin", "Borrower", "Lender"
    public string Role { get; set; } = "Borrower";

    // If this user is a borrower, link to Borrower profile
    public Borrower? Borrower { get; set; }

    // If this user funds loans
    public ICollection<LoanFunder> FundedLoans { get; set; } = new List<LoanFunder>();
}
