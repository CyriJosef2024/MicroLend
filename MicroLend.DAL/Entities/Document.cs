namespace MicroLend.DAL.Entities;

public class Document
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? LoanId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
