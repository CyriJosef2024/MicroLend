namespace MicroLend.DAL.Entities;

public enum EmergencyTransactionType
{
    Donation,
    Withdrawal
}

public class EmergencyPoolTransaction
{
    public int Id { get; set; }
    public EmergencyTransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Description { get; set; }

    // Optional actor
    public int? UserId { get; set; }
    public User? User { get; set; }
}
