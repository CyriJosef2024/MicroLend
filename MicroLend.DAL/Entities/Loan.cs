namespace MicroLend.DAL.Entities
{
    public class Loan : BaseEntity
    {
        public string Purpose { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        // New: canonical loan amount (for BLL compatibility)
        public decimal Amount { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal InterestRate { get; set; } = 5.0m;
        public string Status { get; set; } = "Funding"; 
        public bool IsCrowdfunded { get; set; } = true;
        public int BorrowerId { get; set; }
        // Navigation to borrower
        public Borrower? Borrower { get; set; }
        // Risk score assigned by business logic
        public double RiskScore { get; set; }
        public DateTime? DateGranted { get; set; }

        // Navigation property for interest distribution
        public virtual ICollection<Repayment> Repayments { get; set; } = new List<Repayment>();
    
        // Navigation for funders (investors)
    public virtual ICollection<LoanFunder> Funders { get; set; } = new List<LoanFunder>();
    }
}