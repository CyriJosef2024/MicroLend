namespace MicroLend.DAL.Entities;

public class CreditScore
{
    public int UserId;
    public string Details;

    public int Id { get; set; }
    public int BorrowerId { get; set; }
    public Borrower? Borrower { get; set; }

    public int Score { get; set; }
    public DateTime AssessedAt { get; set; }

    // Optional metadata about how the score was produced
    public string? Source { get; set; }
    public DateTime QuizDate { get; set; }
}
