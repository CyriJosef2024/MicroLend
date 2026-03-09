namespace MicroLend.DAL.Entities;

public class Investment

{
    public int Id { get; set; }
    public decimal AmountInvested { get; set; }
    public int LoanId { get; set; }
}