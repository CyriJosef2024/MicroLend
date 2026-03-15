using System;

namespace MicroLend.DAL.Entities;

public class CreditScore : BaseEntity
{
    public int UserId { get; set; }
    public int Score { get; set; }
    public DateTime QuizDate { get; set; } = DateTime.Now;
    public string Details { get; set; } = string.Empty; // JSON answer storage
}
