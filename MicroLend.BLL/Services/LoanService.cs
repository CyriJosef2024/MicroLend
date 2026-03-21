using MicroLend.DAL.Entities;
using MicroLend.DAL.Exceptions;
using MicroLend.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroLend.BLL.Services
{
    public class LoanService : ILoanService
    {
        private readonly LoanRepository _loanRepo = new LoanRepository();
        private readonly BorrowerRepository _borrowerRepo = new BorrowerRepository();
        private readonly CreditScoringService _creditScoring = new CreditScoringService();

        // Prediction Algorithm (FR2)
        public async Task<double> CalculateRepaymentPredictionScore(int borrowerId, decimal requestedAmount)
        {
            try
            {
                var borrower = await _borrowerRepo.GetByIdAsync(borrowerId);
                if (borrower == null) return 0;

                double score = 50; // base

                // Factor 1: Income-to-loan ratio (max +20)
   
                if (borrower.MonthlyIncome > 0)
                {
                    double ratio = (double)(borrower.MonthlyIncome / requestedAmount);
                    score += Math.Min(20, ratio * 20);
                }

                // Factor 2: Past loan history
                var pastLoans = await _loanRepo.GetLoansByBorrowerIdAsync(borrowerId);
                foreach (var loan in pastLoans.Where(l => l.Status == "Paid" || l.Status == "Defaulted"))
                {
                    if (loan.Status == "Paid") score += 10;
                    if (loan.Status == "Defaulted") score -= 30;
                }

                // Factor 3: Credit score from quiz (if exists)
                if (borrower.UserId.HasValue)
                {
                    var creditScore = await _creditScoring.GetLatestScore(borrower.UserId.Value);
                    if (creditScore.HasValue)
                    {
                        // map 0-100 credit score to -10 to +10 adjustment
                        score += (creditScore.Value - 50) / 5.0;
                    }
                }

                // Factor 4: Business type risk
                var riskMap = new Dictionary<string, int>
                {
                    { "farming", 10 },
                    { "sari-sari", 15 },
                    { "food", 5 },
                    { "clothing", 0 },
                    { "electronics", -10 },
                    { "expansion", -5 }
                };
                foreach (var kv in riskMap)
                {
                    if (borrower.BusinessType.ToLower().Contains(kv.Key))
                    {
                        score += kv.Value;
                        break;
                    }
                }

                return Math.Max(0, Math.Min(100, score));
            }
            catch (DataAccessException ex)
            {
                MicroLend.DAL.Logger.LogError($"Data access error during repayment prediction score calculation for borrower {borrowerId}", ex);
                throw new BusinessException("Unable to calculate repayment prediction due to data access issues. Please try again later.");
            }
            catch (Exception ex)
            {
                MicroLend.DAL.Logger.LogError($"Unexpected error during repayment prediction score calculation for borrower {borrowerId}", ex);
                throw new BusinessException("An unexpected error occurred while calculating your repayment prediction. Please contact support.");
            }
        }

        // Business rule: Borrower cannot have more than one active loan
        public async Task<bool> CanBorrowerGetNewLoan(int borrowerId)
        {
            try
            {
                var activeLoans = await _loanRepo.GetActiveLoansByBorrowerAsync(borrowerId);
                return !activeLoans.Any();
            }
            catch (DataAccessException ex)
            {
                MicroLend.DAL.Logger.LogError($"Data access error checking active loans for borrower {borrowerId}", ex);
                throw new BusinessException("Unable to verify loan status. Please try again later.");
            }
            catch (Exception ex)
            {
                MicroLend.DAL.Logger.LogError($"Unexpected error checking active loans for borrower {borrowerId}", ex);
                throw new BusinessException("An unexpected error occurred while checking your loan status. Please contact support.");
            }
        }

        // Create a new loan (individual or crowdfunded)
        public async Task CreateLoan(Loan loan)
        {
            try
            {
                if (!await CanBorrowerGetNewLoan(loan.BorrowerId))
                    throw new BusinessException("You already have an active loan. Please complete or repay your current loan before applying for a new one.");

                loan.RiskScore = await CalculateRepaymentPredictionScore(loan.BorrowerId, loan.Amount);
                if (loan.IsCrowdfunded)
                {
                    loan.Status = "Funding";
                    loan.CurrentAmount = 0;
                }
                else
                {
                    loan.Status = "Active";
                    loan.CurrentAmount = loan.Amount;
                    loan.DateGranted = DateTime.Now;
                }
                await _loanRepo.AddAsync(loan);
            }
            catch (BusinessException)
            {
                // Re-throw business exceptions as they're already properly handled
                throw;
            }
            catch (DataAccessException ex)
            {
                MicroLend.DAL.Logger.LogError($"Data access error while creating loan for borrower {loan.BorrowerId}", ex);
                throw new BusinessException("Unable to create your loan application. Please try again later.");
            }
            catch (Exception ex)
            {
                MicroLend.DAL.Logger.LogError($"Unexpected error while creating loan for borrower {loan.BorrowerId}", ex);
                throw new BusinessException("An unexpected error occurred while processing your loan application. Please contact support.");
            }
        }

        public async Task<int> ApplyLoanAsync(int borrowerId, decimal amount, string purpose, int initialScore)
        {
            try
            {
                if (!await CanBorrowerGetNewLoan(borrowerId))
                    throw new BusinessException("You already have an active loan. Please complete or repay your current loan before applying for a new one.");

                // require at least one approved document for borrower before applying
                using (var ctx = new MicroLend.DAL.MicroLendDbContext())
                {
                    var hasApproved = ctx.Documents.Any(d => d.UserId == borrowerId && d.Status == "Approved");
                    if (!hasApproved)
                        throw new BusinessException("You must have an approved document before applying for a loan. Please upload and get your documents approved first.");
                }

                var loan = new Loan
                {
                    BorrowerId = borrowerId,
                    Amount = amount,
                    Purpose = purpose,
                    RiskScore = await CalculateRepaymentPredictionScore(borrowerId, amount),
                    Status = "Pending"
                };

                await _loanRepo.AddAsync(loan);
                return loan.Id;
            }
            catch (BusinessException)
            {
                // Re-throw business exceptions as they're already properly handled
                throw;
            }
            catch (DataAccessException ex)
            {
                MicroLend.DAL.Logger.LogError($"Data access error while applying for loan for borrower {borrowerId}", ex);
                throw new BusinessException("Unable to submit your loan application. Please try again later.");
            }
            catch (Exception ex)
            {
                MicroLend.DAL.Logger.LogError($"Unexpected error while applying for loan for borrower {borrowerId}", ex);
                throw new BusinessException("An unexpected error occurred while submitting your loan application. Please contact support.");
            }
        }

        public async Task ApproveLoanAsync(int loanId, bool approve)
        {
            try
            {
                var loan = await _loanRepo.GetByIdAsync(loanId);
                if (loan == null)
                    throw new BusinessException("The loan application could not be found.");

                if (approve)
                {
                    loan.Status = "Active";
                    loan.DateGranted = DateTime.Now;
                    loan.CurrentAmount = loan.Amount;
                }
                else
                {
                    loan.Status = "Rejected";
                }

                await _loanRepo.UpdateAsync(loan);
            }
            catch (BusinessException)
            {
                // Re-throw business exceptions as they're already properly handled
                throw;
            }
            catch (DataAccessException ex)
            {
                MicroLend.DAL.Logger.LogError($"Data access error while approving loan {loanId}", ex);
                throw new BusinessException("Unable to process the loan approval. Please try again later.");
            }
            catch (Exception ex)
            {
                MicroLend.DAL.Logger.LogError($"Unexpected error while approving loan {loanId}", ex);
                throw new BusinessException("An unexpected error occurred while processing the loan approval. Please contact support.");
            }
        }
    }
}
