namespace MicroLend.DAL.Entities;

public class Document
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? LoanId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    // Status: Pending, Approved, Rejected
    public string Status { get; set; } = "Pending";
    // Optional admin reviewer id and timestamp
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
