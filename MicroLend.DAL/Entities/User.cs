namespace MicroLend.DAL.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // For PasswordHelper.cs
    public string Role { get; set; } = "Borrower"; // Admin, Lender, Borrower
}