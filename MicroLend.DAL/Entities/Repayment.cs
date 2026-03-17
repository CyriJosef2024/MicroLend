namespace MicroLend.DAL.Entities;

public class Repayment
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public Loan? Loan { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Optional link to the user who made the repayment (investor or borrower)
    public int? UserId { get; set; }
    public User? User { get; set; }
}
