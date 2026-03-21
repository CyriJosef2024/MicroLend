# MicroLend - Community Micro-Loan Tracker

A comprehensive system to manage small, interest-free loans within a community (like a community savings group). Tracks borrowers, loan amounts, repayment schedules, and social impact.

## Sustainable Development Goals Alignment
**SDG 8: Decent Work and Economic Growth**

---

## Project Structure

```
MicroLend/
├── MicroLend.sln              # Solution file
├── README.md                  # This file
├── MicroLend.DAL/             # Data Access Layer
│   ├── Entities/              # Domain models
│   ├── Repositories/         # Data access patterns
│   ├── Migrations/            # Database schema versioning
│   ├── Exceptions/           # Custom exceptions
│   ├── Logger.cs              # Centralized logging
│   └── MicroLendDbContext.cs   # EF Core DbContext
├── MicroLend.BLL/            # Business Logic Layer
│   ├── Services/              # Business services
│   ├── Exceptions/            # Business exceptions
│   └── Logger.cs              # Logging utility
├── MicroLend.UI/             # Windows Forms Desktop Application
└── logs/                     # Error and audit logs (generated at runtime)
```

---

## N-Tier Architecture

### Layer Responsibilities

| Layer | Responsibility | Access |
|-------|----------------|--------|
| **UI (Presentation)** | Windows Forms, async calls to BLL, charts, user interaction | No database access |
| **BLL (Business Logic)** | Credit scoring, dividend distribution, business rules, validation | Calls DAL |
| **DAL (Data Access)** | Entity Framework Core, repositories, JSON export, logging | Database only |

### Exception Handling Flow (NFR3 - Reliability)

1. **DAL**: Catches database errors → Logs to `logs/error_log.txt` → Throws `DataAccessException`
2. **BLL**: Catches `DataAccessException` → Adds business context → Throws `BusinessException` with user-friendly message
3. **UI**: Catches `BusinessException` → Displays user-friendly message → Logs unexpected errors

---

## Entity Relationship Diagram (ERD)

| Table | Key Fields | Relationships |
|-------|-------------|---------------|
| User | UserID, Username, PasswordHash, Role, CreditScore | One-to-Many with Loan and Investment |
| Borrower | Id, UserId, BusinessType, MonthlyIncome | UserId → User.UserID |
| Loan | Id, BorrowerId, Amount, Status, Purpose | BorrowerId → User.UserID |
| Repayment | Id, LoanId, AmountPaid, Date, PoolDonation | LoanId → Loan.Id |
| Investment/LoanFunder | Id, LenderId, LoanId, AmountContributed | LenderId → User.UserID, LoanId → Loan.Id |
| CreditScore | Id, UserId, Score, QuizDate | UserId → User.UserID |
| EmergencyPool | Id, TotalBalance | Single record |
| Document | Id, UserId, FileName, Status | UserId → User.UserID |

---

## Database

- **Type:** SQLite
- **Location:** `MicroLend.db` (created on first run)
- **Auto-migration:** Enabled on startup
- **Seeding:** Sample data loaded on first run

---

## How to Run

### Prerequisites
- .NET 10.0 SDK
- Windows OS (for Windows Forms)

### Running the Application

1. **Build the solution:**
   ```bash
   dotnet build MicroLend.sln
   ```

2. **Run the application:**
   ```bash
   dotnet run --project MicroLend.UI
   ```

3. **The login form will appear. Use the default credentials below.**

---

## Default Test Accounts

### 👑 Admin Account
| Field | Value |
|-------|-------|
| **Username** | `admin` |
| **Password** | `admin123` |
| **Role** | Admin |

### 👤 Borrower Accounts
| Username | Password | Full Name | Monthly Income | Business |
|----------|----------|-----------|----------------|----------|
| `alice` | `pass1` | Alice Smith | ₱12,000 | Retail |
| `bob` | `pass2` | Bob Johnson | ₱15,000 | Food |
| `charlie` | `pass3` | Charlie Park | ₱9,000 | Services |
| `diana` | `pass4` | Diana Perez | ₱18,000 | Tech |

### 💰 Lender Accounts
| Username | Password | Full Name | Initial Balance |
|----------|----------|-----------|----------------|
| `lender_alex` | `lendpass1` | Alex Thompson | ₱100,000 |
| `lender_maya` | `lendpass2` | Maya Santos | ₱150,000 |
| `lender_john` | `lendpass3` | John Davis | ₱200,000 |

---

## Credit Scoring Algorithm

The credit scoring system uses the following formula:

```csharp
// BLL Credit Scoring Formula
public async Task<int> CalculateScoreAsync(int borrowerId)
{
    var history = await _repo.GetLoanHistoryAsync(borrowerId);
    int onTime = history.Count(l => l.Status == "Paid");
    return (int)(quizScore * 0.3 + onTime * 0.5 - debtToIncome * 0.2);
}
```

**Components:**
- **Quiz Score (30%):** Based on borrower questionnaire responses
- **On-time Payments (50%):** History of successful repayments
- **Debt-to-Income Ratio (20%):** Monthly debt payments vs income

---

## Repository Pattern (DAL)

The DAL uses the Repository pattern with a generic interface:

```csharp
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
```

All repository implementations include exception handling following NFR3 (Reliability) requirements.

---

## Logging System

The application uses centralized logging with the following structure:

```
logs/
├── error_log.txt   # All error messages with stack traces
├── audit_log.txt   # Critical operations (loan approvals, user changes)
└── debug_log.txt   # Debug information (development only)
```

**Logger Methods:**
- `LogError(message, exception)` - Logs errors with full exception details
- `LogWarning(message)` - Logs warnings
- `LogAudit(action, user, details)` - Logs critical operations
- `LogDebug(message)` - Logs debug information
- `LogInfo(message)` - Logs informational messages

---

## Core Features

1. **User Authentication & Authorization**
   - Role-based access control (Admin, Lender, Borrower)
   - Secure password hashing (SHA256)

2. **Borrower Management**
   - Profile creation and management
   - Credit score assignment
   - Document upload and verification

3. **Loan Management**
   - Individual and crowdfunded loans
   - Application submission and approval workflow
   - Risk score calculation
   - Payment tracking

4. **Lender Portal**
   - Browse available loans
   - Fund individual or crowdfunded loans
   - Track investment portfolio

5. **Repayment System**
   - Multiple payment methods
   - Payment tracking and history

6. **Emergency Pool**
   - Community emergency fund management

---

## User Role Capabilities

### Admin
- Full system access
- Manage all users
- View all dashboards
- System configuration
- Document approval/rejection

### Borrower
- Apply for loans
- View loan status
- Make repayments
- Take credit quizzes
- Upload documents
- Manage account settings

### Lender
- Browse available loans
- Fund loans
- View investment portfolio
- Track returns
- Manage account settings

---

## Building the Project

### Build All Projects
```bash
dotnet build MicroLend.sln
```

### Build Individual Projects
```bash
dotnet build MicroLend.DAL/MicroLend.DAL.csproj
dotnet build MicroLend.BLL/MicroLend.BLL.csproj
dotnet build MicroLend.UI/MicroLend.UI.csproj
```

---

## Applying EF Migrations (Development)

This project uses Entity Framework Core migrations stored in `MicroLend.DAL/Migrations`.

1. Add a new migration (if you modify entities):
   ```powershell
   cd MicroLend.DAL
   dotnet ef migrations add YourMigrationName
   ```

2. Apply migrations to the database:
   ```powershell
   dotnet ef database update -p MicroLend.DAL -s MicroLend.UI
   ```

---

## Technology Stack

- **Framework:** .NET 10.0
- **Language:** C# 14
- **Database:** SQLite (Entity Framework Core 10.0)
- **UI:** Windows Forms
- **Password Hashing:** SHA256

---

## Implementation Credits

### Exception Handling Implementation (NFR3 - Reliability)

The exception handling system across all three layers (DAL, BLL, UI) was implemented following the specifications in section 3.5 of the project documentation. This includes:

**Data Access Layer (DAL):**
- Custom [`DataAccessException`](MicroLend.DAL/Exceptions/DataAccessException.cs) class
- Centralized [`Logger`](MicroLend.DAL/Logger.cs) with multi-file logging (error_log.txt, audit_log.txt, debug_log.txt)
- Exception handling in all 11 repositories:
  - [`BorrowerRepository.cs`](MicroLend.DAL/Repositories/BorrowerRepository.cs)
  - [`LoanRepository.cs`](MicroLend.DAL/Repositories/LoanRepository.cs)
  - [`InvestmentRepository.cs`](MicroLend.DAL/Repositories/InvestmentRepository.cs)
  - [`CreditScoreRepository.cs`](MicroLend.DAL/Repositories/CreditScoreRepository.cs)
  - [`UserRepository.cs`](MicroLend.DAL/Repositories/UserRepository.cs)
  - [`EmergencyPoolRepository.cs`](MicroLend.DAL/Repositories/EmergencyPoolRepository.cs)
  - [`RepaymentRepository.cs`](MicroLend.DAL/Repositories/RepaymentRepository.cs)
  - [`LoanFunderRepository.cs`](MicroLend.DAL/Repositories/LoanFunderRepository.cs)
  - [`EmergencyPoolTransactionRepository.cs`](MicroLend.DAL/Repositories/EmergencyPoolTransactionRepository.cs)
  - [`FinancialRepository.cs`](MicroLend.DAL/Repositories/FinancialRepository.cs)
  - [`Repository.cs`](MicroLend.DAL/Repositories/Repository.cs) (generic base class)

**Business Logic Layer (BLL):**
- Custom [`BusinessException`](MicroLend.BLL/Exceptions/BusinessException.cs) class
- Exception handling in all 7 services:
  - [`LoanService.cs`](MicroLend.BLL/Services/LoanService.cs)
  - [`CreditScoreService.cs`](MicroLend.BLL/Services/CreditScoreService.cs)
  - [`InvestmentService.cs`](MicroLend.BLL/Services/InvestmentService.cs)
  - [`CrowdfundingService.cs`](MicroLend.BLL/Services/CrowdfundingService.cs)
  - [`RepaymentService.cs`](MicroLend.BLL/Services/RepaymentService.cs)
  - [`EmergencyPoolService.cs`](MicroLend.BLL/Services/EmergencyPoolService.cs)
  - [`DocumentService.cs`](MicroLend.BLL/Services/DocumentService.cs)

**Presentation Layer (UI):**
- User-friendly error messages in all forms
- [`LoansForm.cs`](MicroLend.UI/LoansForm.cs) - Complete exception handling example

**Repository Pattern:**
- Generic [`IRepository<T>`](MicroLend.DAL/Repositories/IRepository.cs) interface with CRUD operations
- Implemented by [`Repository<T>`](MicroLend.DAL/Repositories/Repository.cs) base class

---

## Group Members

- Josef, Cyr Michael
- Bonilla, Jasmin Claire
- Ong, Maria Shakira
- Onia, James
- Pariñas, Diana