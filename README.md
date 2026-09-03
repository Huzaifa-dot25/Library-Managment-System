# Library Management System (LibraryMS)

A robust, enterprise-grade Library Management System built with **ASP.NET Core 9 MVC**, **Entity Framework Core 9**, and **Bootstrap 5.3**. The application follows Clean Architecture principles, dividing responsibilities across Domain, Application, Infrastructure, and Presentation layers.

---

## 🚀 Quick Start

### 1. Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or higher
- [Microsoft SQL Server](https://www.microsoft.com/sql-server/) (SQL Express, LocalDB, or Developer Edition)
- EF Core CLI Tool (optional for manual CLI migrations):
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

### 2. Connection String Configuration

The default connection string is located in [`src/LibraryMS.Web/appsettings.json`](src/LibraryMS.Web/appsettings.json):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=LibraryMS;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

#### Custom SQL Server Instance
If your local instance is different (e.g. `(localdb)\mssqllocaldb` or custom instance name), update `DefaultConnection` in `appsettings.json` or configure it securely via .NET User Secrets:

```bash
cd src/LibraryMS.Web
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=LibraryMS;Trusted_Connection=True;MultipleActiveResultSets=true"
```

---

### 3. Database Migrations

The database is **automatically migrated and seeded** when you start the application (`Program.cs` invokes `DbSeeder.SeedAsync` inside an `IServiceScope`).

If you prefer to apply migrations manually via CLI, run:

```bash
dotnet ef database update --project src/LibraryMS.Infrastructure --startup-project src/LibraryMS.Web
```

To create a new migration in the future:
```bash
dotnet ef migrations add <MigrationName> --project src/LibraryMS.Infrastructure --startup-project src/LibraryMS.Web
```

---

### 4. Running the Application

From the root directory:

```bash
dotnet run --project src/LibraryMS.Web
```

The application will launch at:
- **HTTPS:** `https://localhost:7289` (or assigned port)
- **HTTP:** `http://localhost:5289` (or assigned port)

Navigate to `https://localhost:7289`. If you are unauthenticated, you will be automatically redirected to `/Account/Login`.

---

## 🔐 Default Credentials & Roles

The system automatically seeds two default accounts with predefined roles on the first run:

| Role | Email / Username | Password | Access Level |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@library.com` | `Admin@123` | Full access to all modules, including system configuration, lookup master data, categories, book types, and vendors. |
| **Librarian** | `librarian@library.com` | `Librarian@123` | Operational access: issue/return books, manage members (students/employees), manage catalog (books, authors, publishers), view reports and dashboard. |

> [!NOTE]
> Default passwords satisfy the configured Identity policy: minimum 6 characters with at least one numeric digit.

---

## 🛡️ Role-Based Access Control (RBAC)

- **Authentication Enforcement:** `[Authorize]` is applied at the controller level across all module controllers. Unauthenticated requests are redirected to `/Account/Login?ReturnUrl=...`.
- **Admin-Only Areas:** The following controllers are decorated with `[Authorize(Roles = "Admin")]`:
  - `CategoriesController` (Manage Book Categories)
  - `BookTypesController` (Manage Book Types & Linked Accounts)
  - `VendorsController` (Manage Vendors)
  - `LookupController` (Academic Sessions, Class Levels, and Sections)
- **Access Denied:** If an authenticated Librarian attempts to access Admin-only routes, ASP.NET Core Identity redirects them to `/Account/AccessDenied`.

---

## 📦 Automatic Seed Data

`DbSeeder.SeedAsync` is idempotent (safe to run multiple times without duplicating entries). On initial run, it automatically seeds:

1. **Identity Roles:** `Admin`, `Librarian`
2. **Default Accounts:** Administrator and Librarian users
3. **Book Types:** General Book, Reference Book, Textbook, Periodical/Journal, Digital Media
4. **Categories:** Computer Science, Software Engineering, Data Science & AI, Mathematics, Physics, Literature & Fiction, etc.
5. **Publishers:** O'Reilly Media, Addison-Wesley Professional, Manning Publications, Pearson Education, MIT Press, Packt Publishing
6. **Authors:** Robert C. Martin, Martin Fowler, Andrew Hunt, David Thomas, Eric Evans, Donald E. Knuth, Jon Skeet, Steve McConnell
7. **Vendors:** Apex Academic Book Distributors, National Book Foundation, Global Educational Supplies
8. **Academic Lookups:** Sessions (`2025-2026`, `2026-2027`), Class Levels (`BS Computer Science`, `BS Software Engineering`, `Class 9`, `Class 10`), and Sections (`Section A`, `Section B`)
9. **Members:** Sample students and faculty members for immediate circulation testing
10. **Books Catalog:** Popular software engineering and computer science books (e.g. *Clean Code*, *The Pragmatic Programmer*, *Refactoring*, *Domain-Driven Design*, *C# in Depth*, *Code Complete*, *The Art of Computer Programming*) with ISBNs, barcodes, authors, categories, accession numbers, and shelf locations.

---

## 🏗️ Architecture & Solution Structure

```
LibraryMS/
├── src/
│   ├── LibraryMS.Domain/           # Core domain entities, enums, zero dependencies
│   ├── LibraryMS.Application/      # ViewModels, DTOs, Service interfaces
│   ├── LibraryMS.Infrastructure/   # EF Core DbContext, Identity, Migrations, DbSeeder, Service implementations
│   └── LibraryMS.Web/              # ASP.NET Core MVC controllers, views, layout, wwwroot
├── design.md                       # Architecture and design specifications
├── requirements.md                 # Product requirements and user stories
└── README.md                       # Setup and operational documentation
```

### Key Technical Patterns
- **Async/Await Everywhere:** All database access is asynchronous via EF Core `async` APIs (NFR-1).
- **Strict ViewModel Separation:** Views only bind to dedicated ViewModels/DTOs, never raw database entities (NFR-2).
- **Validation:** Server-side Data Annotations paired with unobtrusive jQuery client-side validation (NFR-3).
- **Responsive UI:** Bootstrap 5.3 layout with custom glassmorphic styling, live datetime status badge, and user dropdown menu (NFR-4).
- **Paging & Filtering:** Server-side paging on book catalog and search grids (NFR-5).
