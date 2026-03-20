namespace MicroLend.DAL.Entities;

public class EmergencyPool
{
    public int Id { get; set; }
    public decimal TotalBalance { get; set; }
    // Placeholder fields used by UI document upload (not ideal - replace with proper Document entity later)
    // public string? Name { get; set; }
    // public DateTime CreatedAt { get; set; }
}
 