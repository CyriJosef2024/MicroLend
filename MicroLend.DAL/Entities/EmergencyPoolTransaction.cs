using System;

namespace MicroLend.DAL.Entities;

public class EmergencyPoolTransaction : BaseEntity
{
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = "Donation"; // "Donation" or "Withdrawal"
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } = DateTime.Now;
}