# MicroLend - Community Micro-Loan Tracker

A comprehensive system to manage small, interest-free loans within a community (like a community savings group). Tracks borrowers, loan amounts, repayment schedules, and social impact.

## Sustainable Development Goals Alignment
**SDG 8: Decent Work and Economic Growth**

---

## System Architecture

### Project Structure

```
MicroLend.slnx
├── MicroLend.DAL/          # Data Access Layer
├── MicroLend.BLL/          # Business Logic Layer
├── MicroLend.UI/           # Windows Forms Desktop Application
└── MicroLend.Web/          # ASP.NET Core Web Application
```

---

## Architecture Layers

### 1. Data Access Layer (DAL)
**Location:** `MicroLend.DAL/`

The DAL handles all database operations and entity management using Entity Framework Core with SQLite.

**Components:**
- **Entities/** - Domain models
  - `User.cs` - User authentication and roles
  - `Borrower.cs` - Borrower profiles with income and business info
  - `Loan.cs` - Loan applications and tracking
  - `Repayment.cs` - Payment records
  - `LoanFunder.cs` - Lender contributions to crowdfunded loans
  - `CreditScore.cs` - Credit scoring records
  - `EmergencyPool.cs` - Community emergency fund

- **Repositories/** - Data access patterns
  - `UserRepository.cs`
  - `BorrowerRepository.cs`
  - `LoanRepository.cs`
  - `RepaymentRepository.cs`
  - `LoanFunderRepository.cs`
  - `CreditScoreRepository.cs`

- **Migrations/** - Database schema versioning

**Database:** SQLite (`MicroLend.db`)

---

### 2. Business Logic Layer (BLL)
**Location:** `MicroLend.BLL/`

The BLL contains all business rules, algorithms, and service implementations.

**Services:**

- **CreditScoringService.cs** - Credit scoring algorithm
  - Implements quiz-based credit assessment
  - Calculates credit scores based on financial behavior

- **CreditScoreEngine.cs** - Advanced credit risk evaluation
  - Risk score calculations using multiple factors

- **LoanService.cs** - Loan management
  - `ApplyLoanAsync()` - Submit loan applications
  - `ApproveLoanAsync()` - Approve/reject loans
  - `CalculateRepaymentPredictionScore()` - Predicts repayment likelihood

- **RepaymentService.cs** - Payment processing
  - Records and tracks loan repayments

- **InvestmentService.cs** - Lender investment management
  - Manages fund contributions to loans

- **CrowdfundingService.cs** - Crowdfunded loan handling
  - Tracks multiple lenders per loan

- **EmergencyPoolService.cs** - Community emergency fund
  - Manages emergency loan pool transactions

- **RiskDashboardService.cs** - Analytics and reporting
  - Provides risk metrics and loan statistics

- **DocumentService.cs** - Document management
  - Handles borrower document uploads

---

### 3. User Interface (UI)
**Location:** `MicroLend.UI/`

Windows Forms desktop application providing a graphical interface.

**Forms:**

 | Form | Description |
 |------|-------------|
 | `Form1.cs` | Main login/entry point with authentication |
 | `LandingPageForm.cs` | Welcome landing page with system info |
 | `AdminDashboardForm.cs` | Admin management interface |
 | `BorrowerDashboardForm.cs` | Borrower self-service portal |
 | `LenderDashboardForm.cs` | Lender investment dashboard |
 | `SignupForm.cs` | New user registration (Borrower/Lender) |
 | `AccountSettingsForm.cs` | Account settings and payment methods |
 | `LoansForm.cs` | Loan application and management |
 | `FundLoanForm.cs` | Fund contribution interface |
 | `RepaymentMethodForm.cs` | Payment processing |
 | `CreditQuizForm.cs` | Credit assessment quiz |
 | `EmergencyPoolForm.cs` | Emergency fund interface |
 | `BorrowersForm.cs` | Borrower CRUD operations |
 | `LendersForm.cs` | Lender management |

---

### 4. Web Application (Web)
**Location:** `MicroLend.Web/`

ASP.NET Core MVC web application providing browser-based access.

**Features:**
- RESTful API controllers
- Razor views for each user role
- Session management
- Responsive design

---

## System Functionality

### Core Features

1. **User Authentication & Authorization**
   - Role-based access control (Admin, Lender, Borrower)
   - Secure password hashing (SHA256)

2. **Borrower Management**
   - Profile creation and management
   - Business type tracking
   - Monthly income verification
   - Credit score assignment

3. **Loan Management**
   - Individual and crowdfunded loans
   - Application submission
   - Approval workflow
   - Risk score calculation
   - Payment tracking

4. **Lender Portal**
   - Browse available loans
   - Fund individual or crowdfunded loans
   - Track investment portfolio
   - View expected returns

5. **Repayment System**
   - Multiple payment methods
   - Payment tracking and recording
   - Payment history

6. **Credit Scoring**
   - Quiz-based credit assessment
   - Risk prediction algorithm
   - Score history tracking

7. **Emergency Pool**
   - Community emergency fund
   - Emergency loan requests
   - Fund management

8. **Analytics & Reporting**
   - Risk dashboard
   - Loan purpose distribution
   - Repayment trends
   - Portfolio performance

---

## Login Instructions

### Default Test Accounts

The system comes pre-seeded with test accounts. Use the following credentials:

#### 👑 Admin Account
| Field | Value |
|-------|-------|
| **Username** | `admin` |
| **Password** | `admin123` |
| **Role** | Admin |

#### 👤 Borrower Accounts
| Username | Password | Full Name | Monthly Income |
|----------|----------|-----------|----------------|
| `alice` | `pass1` | Alice Smith | ₱12,000 |
| `bob` | `pass2` | Bob Johnson | ₱15,000 |
| `charlie` | `pass3` | Charlie Park | ₱9,000 |

#### 💰 Lender Accounts
| Username | Password |
|----------|----------|
| `lender_alex` | `lendpass1` |
| `lender_maya` | `lendpass2` |
| `lender_john` | `lendpass3` |

> **Note:** New users can sign up through the application. Only Borrower and Lender roles are available for self-registration. Admin accounts must be created directly in the database.

---

## How to Login

### Desktop Application (UI)

1. Run the application:
   ```
   dotnet run --project MicroLend.UI
   ```

2. The login form will appear

3. Enter your username and password

4. Click **Login** to access your dashboard

5. New users can click **Sign Up** to create an account

### Web Application

1. Run the web application:
   ```
   dotnet run --project MicroLend.Web
   ```

2. Navigate to `http://localhost:5000`

3. Login with your credentials

---

## User Role Capabilities

### Admin
- Full system access
- Manage all users
- View all dashboards
- System configuration

### Borrower
- Apply for loans
- View loan status
- Make repayments
- Take credit quizzes
- Manage account settings

### Lender
- Browse available loans
- Fund loans
- View investment portfolio
- Track returns
- Manage account settings

---

## Password Requirements

- Minimum 4 characters
- SHA256 hashed for storage
- Case-sensitive

---

## Database

- **Type:** SQLite
- **Location:** `MicroLend.db` (created on first run)
- **Auto-migration:** Enabled on startup
- **Seeding:** Sample data loaded on first run

### Applying EF Migrations (development)

This project uses Entity Framework Core migrations stored in `MicroLend.DAL/Migrations`. To apply migrations manually or when deploying, run the following from the solution root:

1. Add a new migration (if you modify entities):

   ```powershell
   cd MicroLend.DAL
   dotnet ef migrations add YourMigrationName
   ```

2. Apply migrations to the database (the Web app is used as the startup project here):

   ```powershell
   dotnet ef database update -p MicroLend.DAL -s MicroLend.Web
   ```

3. The WinForms `Program.Main` also calls `Database.Migrate()` on startup for convenience during development; however, for production it's recommended to run migrations explicitly and verify schema changes.

## Deployment notes

- Start the web server first to enable authenticated upload and API calls used by the desktop client:
  - `dotnet run --project MicroLend.Web`
  - Ensure it listens on `http://localhost:5000` as configured in `Program.cs`.
- When using the desktop client, create a local API token file before calling upload (development convenience):
  - Create `apitoken.txt` in the application directory containing a valid bearer token.
  - This demo stores tokens locally; in production use a proper secure token store and authentication flow.

## Payment provider

The repository includes a payment service scaffold that posts to an example endpoint. To integrate a real provider:

- Recommended: Stripe (credit card) or Paymongo (Philippine cards/payments). Tell me which provider you prefer and I will implement it.
- You will need provider API keys; store them securely (user-secrets, environment variables, or Azure Key Vault) and never commit them to source control.



---

## Building the Project

### Build All Projects
```bash
dotnet build MicroLend.slnx
```

### Build Individual Projects
```bash
dotnet build MicroLend.DAL/MicroLend.DAL.csproj
dotnet build MicroLend.BLL/MicroLend.BLL.csproj
dotnet build MicroLend.UI/MicroLend.UI.csproj
dotnet build MicroLend.Web/MicroLend.Web.csproj
```

---

## Group Members

- Josef, Cyr Michael
- Bonilla, Jasmin Claire
- Ong, Maria Shakira
- Onia, James
- Pariñas, Diana

---

## Technical Details

- **Framework:** .NET 10.0
- **Database:** SQLite (Entity Framework Core 10.0)
- **UI:** Windows Forms
- **Web:** ASP.NET Core MVC
- **Password Hashing:** SHA256

## Technology stack

- Languages: C# (targeting .NET 10, C# 14)
- Web: ASP.NET Core MVC with Razor views and Bootstrap 5 for responsive UI
- Data access: Entity Framework Core 10 (SQLite provider)
- Desktop: Windows Forms (.NET WinForms)
- Client-side: jQuery (validation) and optional AJAX for uploads

## Document upload & verification (what we changed and how it works)

- Upload storage: uploaded files are saved to `wwwroot/uploads` by the web implementation `MicroLend.Web/Services/WebDocumentService.cs`.
- Database: metadata is stored in the `Documents` table via `MicroLend.DAL.Entities.Document` (fields: `Id`, `UserId`, `LoanId`, `FileName`, `FilePath`, `UploadedAt`, `Status`, `ReviewedBy`, `ReviewedAt`).
- Default status: when a borrower uploads a document its `Status` is `Pending`.
- Admin review: an admin can view `/Admin/UploadedDocuments` and click Approve/Reject. That calls `AdminController.VerifyDocument` which updates `Status`, `ReviewedBy`, and `ReviewedAt`.

## DAL / BLL responsibilities (brief)

- DAL (MicroLend.DAL): owns entity definitions, DbContext, migrations, and repository classes. Persisting a document involves adding a `Document` entity and calling `SaveChanges()` on `MicroLendDbContext`.
- BLL (MicroLend.BLL): contains service-layer logic (credit scoring, loan business rules, repayment handling, investment workflows). The credit quiz service calculates the score and persists a `CreditScore` record and updates `User.InitialCreditScore`.

## Why an upload might appear to "just refresh" and not be recorded

1. Missing anti-forgery token on the form. The upload form needs an antiforgery token (the project now includes `@Html.AntiForgeryToken()` in the upload partial). If you use AJAX you must include the token in the request.
2. Client-side JS may be intercepting the form and not sending the file. Check the browser DevTools -> Network to confirm the POST to `/Borrower/UploadDocument` contains the file payload and returns a JSON response or redirect.
3. Server-side validation rejects the file silently. `WebDocumentService.SaveDocumentAsync` will return an empty string when:
   - file extension is not allowed (.pdf, .jpg, .jpeg, .png, .doc, .docx are permitted),
   - file size exceeds 5 MB, or
   - an IO error occurred while writing the file.
   When that happens the controller sets `TempData["Error"]` and redirects to `/Borrower/Upload`.
4. Database save failed. After saving the file, the controller creates a `Document` entity and calls `SaveChanges()`. If `SaveChanges()` throws, the controller captures the exception and shows an error message. Check application logs or the Admin `Logs` page.

## How to verify an upload succeeded (quick checklist)

1. Upload the file from a Borrower account.
2. Check the web response in the browser DevTools Network tab. A successful form POST will either return JSON (if AJAX) or redirect to a page with a success TempData message.
3. Confirm the file exists under `MicroLend.Web/wwwroot/uploads`.
4. Inspect the `Documents` table in the SQLite DB (file path printed by `MicroLendDbContext` configuration). Look for a row with `UserId`, `FilePath` and `UploadedAt`.
5. Admin: visit `/Admin/UploadedDocuments` to view the pending file and use Approve/Reject. Approval sets `Status = "Approved"` and updates `ReviewedBy`/`ReviewedAt`.

## Next steps I applied in code (DAL / BLL)

- Added `Status`, `ReviewedBy`, and `ReviewedAt` to `Document` entity and migration so Admin can approve/reject uploaded documents.
- Ensured the upload partial emits an antiforgery token and the controller action honors it.
- Controller persists a `Document` row after the file is written to disk.
- Admin controller exposes `UploadedDocuments` view and `VerifyDocument` action for approving/rejecting documents.

If you still see the page refresh without records after following the checklist, capture the browser Network request and the server exception text (if any) and paste them here. I will inspect the failing request and pinpoint the DAL or BLL problem.


Adding Additional Co-Authors for testing. 