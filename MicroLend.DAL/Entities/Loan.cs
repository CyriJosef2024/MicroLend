namespace MicroLend.DAL.Entities;

public class Loan
{
    public int Id { get; set; }

    // Basic loan information
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public bool IsApproved { get; set; }
    public bool IsOpen { get; set; } = true;

    // Borrower
    public int BorrowerId { get; set; }
    public Borrower? Borrower { get; set; }

    // Repayments and funders
    public ICollection<Repayment> Repayments { get; set; } = new List<Repayment>();
    public ICollection<LoanFunder> Funders { get; set; } = new List<LoanFunder>();
}
