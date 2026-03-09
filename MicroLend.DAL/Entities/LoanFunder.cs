namespace MicroLend.DAL.Entities;

public class LoanFunder
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public Loan? Loan { get; set; }

    // The user who funded the loan
    public int UserId { get; set; }
    public User? User { get; set; }

    public decimal AmountFunded { get; set; }
    public DateTime FundedAt { get; set; }
}
