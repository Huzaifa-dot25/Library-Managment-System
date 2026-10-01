# Library Management System — Requirements

## Project Overview

A web-based school library management system built with ASP.NET Core MVC, Entity Framework Core (Code-First), SQL Server, and Bootstrap 5. It handles the full lifecycle of library operations: book cataloging, purchasing, member management, book issuance/return, and overdue tracking.

---

## User Roles

| Role       | Description                                                                 |
|------------|-----------------------------------------------------------------------------|
| Admin      | Full access to all modules including system configuration and user mgmt     |
| Librarian  | Day-to-day operations: issue/return books, manage members, search catalog   |

---

## Phase-by-Phase User Stories

---

### Phase 1 — Database Schema & EF Core Foundation

**US-001** As a developer, I want a fully scaffolded ASP.NET Core MVC solution with a layered architecture (Presentation / Application / Domain / Infrastructure) so the codebase stays maintainable as it grows.

**US-002** As a developer, I want all EF Core entities defined Code-First with proper PKs, FKs, indexes, and constraints so the database schema is generated consistently via migrations.

**US-003** As a developer, I want an `appsettings.json` with a SQL Server connection string placeholder so any developer can point the app at their local SQL Server instance.

#### Acceptance Criteria

- AC-001-1: `dotnet build` succeeds with zero errors on a clean checkout.
- AC-001-2: `dotnet ef database update` creates all tables in the target SQL Server database.
- AC-001-3: Solution contains at minimum four projects/folders: `LibraryMS.Web` (MVC), `LibraryMS.Application` (services/DTOs), `LibraryMS.Domain` (entities), `LibraryMS.Infrastructure` (DbContext/repos).
- AC-002-1: All 14 entity types are present as C# classes with the correct property types.
- AC-002-2: Many-to-many joins (`BookAuthors`, `BookCategories`) are represented as explicit join entities or fluent-API configurations.
- AC-002-3: All foreign keys have corresponding navigation properties.
- AC-002-4: `RemainingQuantity`, `IssuedCount`, and `LostCount` are integer columns defaulting to 0.
- AC-003-1: Connection string key is `"DefaultConnection"` under `"ConnectionStrings"` in `appsettings.json`.

---

### Phase 2 — Master Data (Authors, Publishers, Categories, Book Types)

**US-004** As a librarian, I want to add, edit, and delete Authors (Name, Gender, Country) in an inline grid so I can maintain the author catalog without navigating away.

**US-005** As a librarian, I want to add, edit, and delete Publishers (Name, Email, PhoneNo, Address, Gender, Country, City) in an inline grid.

**US-006** As a librarian, I want to add, edit, and delete Categories in an inline grid.

**US-007** As an admin, I want to add, edit, and delete Book Types (TypeName, LinkedAccountName, IsActive) in an inline grid so I can configure the classification scheme.

#### Acceptance Criteria

- AC-004-1: Grid saves changes via AJAX (no full-page reload).
- AC-004-2: Delete shows a confirmation prompt before removing the record.
- AC-004-3: Validation errors (e.g. blank Name) are shown inline without losing other row data.
- AC-007-1: IsActive toggle immediately reflects in the grid after save.

---

### Phase 3 — Vendors & Book Catalog

**US-008** As an admin, I want to add and manage Vendors (Name, ContactInfo) via a modal form.

**US-009** As a librarian, I want to add a new Book with all metadata (Title, ISBN, BarCode, Publisher, Authors × many, Categories × many, Type, Rack, Vendor, Accession#, Dewey Code, Price, Description, Cover Image) so the catalog is complete.

**US-010** As a librarian, I want to search and browse the Manage Books grid with server-side paging and multi-field filtering so large collections (10,000+ books) load quickly.

**US-011** As a librarian, I want to use a reusable Book Search widget (search by ISBN / Title / Price / Category / Author) that I can place on any page, including the dashboard.

#### Acceptance Criteria

- AC-009-1: Authors and Categories support multi-select (add multiple, remove individual).
- AC-009-2: Cover image is uploaded to `wwwroot/uploads/books/` and only the relative path is stored in the DB.
- AC-010-1: Grid uses server-side paging; default page size is 20.
- AC-010-2: Filters are combinable (AND logic).
- AC-011-1: Book Search is implemented as a Razor partial view, reusable by inclusion.

---

### Phase 4 — Purchases

**US-012** As a librarian, I want to create a Purchase Order or Invoice with a header (Vendor, Date, Manual #) and line items (Book/item, Qty, Price, Discount%) so procurement is tracked.

**US-013** As a librarian, I want Net Price per line and Net Amount totals auto-calculated in real time so I don't have to calculate manually.

**US-014** As a librarian, I want to search Purchase history by date range, vendor, invoice/order type, and purchase number so I can audit past orders.

**US-015** As the system, when a Purchase is saved and a line item references an existing Book, the book's `TotalQuantity` and `RemainingQuantity` must be incremented by the purchased quantity.

#### Acceptance Criteria

- AC-012-1: Purchase Number auto-increments and is read-only.
- AC-013-1: Net Price = Qty × PricePerUnit × (1 − Disc/100), recalculated on any field change.
- AC-013-2: Net Amount = Total − OverallDiscount, recalculated on change.
- AC-015-1: Book quantities update atomically with the Purchase save (same DB transaction).

---

### Phase 5 — Members (Students & Employees)

**US-016** As an admin, I want to manage Sessions (academic years), Class Levels, and Sections as lookup data.

**US-017** As a librarian, I want to add/edit/search Students by Session, Class, Section, Active status, Name, Registration No, or Roll No.

**US-018** As a librarian, I want to add/edit/search Employees by Name, Employee Code, Department, and Active status.

**US-019** As a developer, I need a `GetMembers(sessionId, classId, sectionId, activeStatus, searchTerm)` service method that Phase 6 can call.

#### Acceptance Criteria

- AC-017-1: Student list filters are combinable.
- AC-018-1: Employee Code is unique.
- AC-019-1: Method is present on `IMemberService` with an async signature.

---

### Phase 6 — Issue / Return Books

**US-020** As a librarian, I want to search members by type (Student/Employee), filters, and name/ID, then issue a book to a selected member, with the system checking `RemainingQuantity > 0` before allowing issuance.

**US-021** As a librarian, I want to return a book for a member, which increments that book's `RemainingQuantity`.

**US-022** As a librarian, I want to issue the same book to every member in a currently-filtered class in one action ("Issue to all Class").

#### Acceptance Criteria

- AC-020-1: Issue fails with a user-friendly message if `RemainingQuantity <= 0`.
- AC-020-2: `IssueNo` is unique and auto-generated.
- AC-021-1: Return sets `IssuedBook.ReturnDate = DateTime.Today`.
- AC-021-2: `RemainingQuantity` increments atomically with the return save.
- AC-022-1: Bulk issue applies only to members visible in the current filter result.

---

### Phase 7 — Manage Issued Books & Overdue Tracking

**US-023** As a librarian, I want to search all issued books by date range, issue number, member name/ID, and member type.

**US-024** As a librarian, I want to see all overdue books filtered by a minimum "Days Passed" threshold, showing Issue #, Days Passed, Member, Issue Date, Due Date.

#### Acceptance Criteria

- AC-024-1: Days Passed = `DateTime.Today − DueDate` for any `IssuedBook` where `ReturnDate IS NULL` and `DueDate < today`.
- AC-024-2: The threshold filter shows only records where `DaysPassesd >= entered value`.

---

### Phase 8 — Dashboard & Reports Shell

**US-025** As a librarian, I want a dashboard home page with quick-access tiles to all major modules and the embedded Book Search and Issued Book Search widgets.

**US-026** As a librarian, I want a Reports menu with placeholder pages for Books Inventory, Issued Books, Overdue, and Purchases reports.

#### Acceptance Criteria

- AC-025-1: Dashboard renders correctly without login (pre-auth) — auth is added in Phase 9.
- AC-026-1: Each report page exists as a Razor view; PDF/Excel export is marked TODO.

---

###

