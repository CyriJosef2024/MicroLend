namespace MicroLend.DAL.Entities;

public class LoanFunder : BaseEntity
{
    public int LoanId { get; set; }
    public int LenderId { get; set; }
    public decimal Amount { get; set; }
    public decimal ExpectedInterest { get; set; }
    public DateTime FundingDate { get; set; } = DateTime.Now;
}