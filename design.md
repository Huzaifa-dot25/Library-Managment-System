# Library Management System — Design Document (Phase 1)

## 1. Solution Architecture

```
LibraryMS/
├── LibraryMS.Domain/          # Plain C# entities, enums, no dependencies
├── LibraryMS.Application/     # Service interfaces + implementations, DTOs/ViewModels
├── LibraryMS.Infrastructure/  # EF Core DbContext, Migrations, Repositories
└── LibraryMS.Web/             # ASP.NET Core MVC — Controllers, Views, wwwroot
```

### Dependency Direction

```
Web → Application → Domain
Infrastructure → Domain (implements Application interfaces)
Web → Infrastructure (DI registration only)
```

- **Domain** has zero external dependencies.
- **Application** depends only on Domain.
- **Infrastructure** implements Application interfaces using EF Core.
- **Web** wires everything together via DI in `Program.cs`.

---

## 2. Technology Stack

| Concern           | Choice                                      |
|-------------------|---------------------------------------------|
| Framework         | ASP.NET Core 8 MVC                          |
| Language          | C# 12                                       |
| ORM               | Entity Framework Core 8 (Code-First)        |
| Database          | Microsoft SQL Server (LocalDB / Express)    |
| UI Framework      | Bootstrap 5.3                               |
| Grid / Tables     | jQuery DataTables (server-side mode)        |
| Dynamic UI        | Vanilla JS + jQuery                         |
| File Storage      | Local disk (`wwwroot/uploads/books/`)       |
| Auth (Phase 9)    | ASP.NET Core Identity                       |

---

## 3. Entity Relationship Diagram (Description)

### 3.1 Core Entities and Relationships

```
Author ──────────────────────────────┐
                                     │  BookAuthors (join)
Book ────────────────────────────────┤
  │                                  │  BookCategories (join)
  │                              Category
  │
  ├── Publisher (FK: PublisherId)
  ├── Vendor (FK: VendorId)
  ├── BookType (FK: BookTypeId)
  │
PurchaseItem ── Purchase ── Vendor
  │
  └── Book (FK: BookId, nullable)

IssuedBook ── Book
  ├── Student (FK: StudentId, nullable)
  └── Employee (FK: EmployeeId, nullable)

Student ── Session
         ── ClassLevel
         └── Section ── ClassLevel

Section ── ClassLevel
```

### 3.2 Full Entity List

| # | Entity        | Table Name       | PK                  |
|---|---------------|------------------|---------------------|
| 1 | Author        | Authors          | AuthorId (int)      |
| 2 | Publisher     | Publishers       | PublisherId (int)   |
| 3 | Category      | Categories       | CategoryId (int)    |
| 4 | BookType      | BookTypes        | BookTypeId (int)    |
| 5 | Vendor        | Vendors          | VendorId (int)      |
| 6 | Book          | Books            | BookId (int)        |
| 7 | BookAuthor    | BookAuthors      | (BookId, AuthorId)  |
| 8 | BookCategory  | BookCategories   | (BookId, CategoryId)|
| 9 | Session       | Sessions         | SessionId (int)     |
|10 | ClassLevel    | ClassLevels      | ClassLevelId (int)  |
|11 | Section       | Sections         | SectionId (int)     |
|12 | Student       | Students         | StudentId (int)     |
|13 | Employee      | Employees        | EmployeeId (int)    |
|14 | Purchase      | Purchases        | PurchaseId (int)    |
|15 | PurchaseItem  | PurchaseItems    | PurchaseItemId (int)|
|16 | IssuedBook    | IssuedBooks      | IssuedBookId (int)  |

---

## 4. Detailed Entity Definitions

### 4.1 Author
| Column    | Type         | Constraints         |
|-----------|--------------|---------------------|
| AuthorId  | int          | PK, Identity        |
| Name      | nvarchar(150) | Required, Index    |
| Gender    | nvarchar(10) | nullable            |
| Country   | nvarchar(100)| nullable            |

---

### 4.2 Publisher
| Column      | Type          | Constraints  |
|-------------|---------------|--------------|
| PublisherId | int           | PK, Identity |
| Name        | nvarchar(150) | Required     |
| Email       | nvarchar(100) | nullable     |
| PhoneNo     | nvarchar(30)  | nullable     |
| Address     | nvarchar(300) | nullable     |
| Gender      | nvarchar(10)  | nullable     |
| Country     | nvarchar(100) | nullable     |
| City        | nvarchar(100) | nullable     |

---

### 4.3 Category
| Column     | Type          | Constraints  |
|------------|---------------|--------------|
| CategoryId | int           | PK, Identity |
| Name       | nvarchar(100) | Required     |

---

### 4.4 BookType
| Column            | Type          | Constraints         |
|-------------------|---------------|---------------------|
| BookTypeId        | int           | PK, Identity        |
| TypeName          | nvarchar(100) | Required            |
| LinkedAccountName | nvarchar(150) | nullable            |
| IsActive          | bit           | Required, default 1 |

---

### 4.5 Vendor
| Column      | Type          | Constraints  |
|-------------|---------------|--------------|
| VendorId    | int           | PK, Identity |
| Name        | nvarchar(150) | Required     |
| ContactInfo | nvarchar(500) | nullable     |

---

### 4.6 Book
| Column              | Type           | Constraints              |
|---------------------|----------------|--------------------------|
| BookId              | int            | PK, Identity             |
| Title               | nvarchar(300)  | Required, Index          |
| ISBN                | nvarchar(30)   | nullable, Index (unique) |
| BarCode             | nvarchar(50)   | nullable, Index          |
| Description         | nvarchar(max)  | nullable                 |
| Pages               | int            | nullable                 |
| RackOrGrade         | nvarchar(100)  | nullable                 |
| BookTypeId          | int            | FK → BookTypes, nullable |
| VendorId            | int            | FK → Vendors, nullable   |
| AccessionNumber     | nvarchar(50)   | nullable                 |
| CallNumberDeweyCode | nvarchar(50)   | nullable                 |
| PublisherId         | int            | FK → Publishers, nullable|
| Price               | decimal(10,2)  | nullable                 |
| TotalQuantity       | int            | Required, default 0      |
| RemainingQuantity   | int            | Required, default 0      |
| IssuedCount         | int            | Required, default 0      |
| LostCount           | int            | Required, default 0      |
| RackName            | nvarchar(100)  | nullable                 |
| LibrarianRemarks    | nvarchar(500)  | nullable                 |
| CoverImagePath      | nvarchar(500)  | nullable                 |
| CreatedDate         | datetime2      | Required, default now    |

**Indexes:** Title, ISBN (unique), BarCode

---

### 4.7 BookAuthor (join table)
| Column   | Type | Constraints      |
|----------|------|------------------|
| BookId   | int  | PK part, FK → Books   |
| AuthorId | int  | PK part, FK → Authors |

Composite PK: (BookId, AuthorId)

---

### 4.8 BookCategory (join table)
| Column     | Type | Constraints         |
|------------|------|---------------------|
| BookId     | int  | PK part, FK → Books      |
| CategoryId | int  | PK part, FK → Categories |

Composite PK: (BookId, CategoryId)

---

### 4.9 Session
| Column      | Type          | Constraints  |
|-------------|---------------|--------------|
| SessionId   | int           | PK, Identity |
| SessionName | nvarchar(50)  | Required     |
| StartDate   | datetime2     | nullable     |
| EndDate     | datetime2     | nullable     |

---

### 4.10 ClassLevel
| Column       | Type          | Constraints  |
|--------------|---------------|--------------|
| ClassLevelId | int           | PK, Identity |
| Name         | nvarchar(100) | Required     |
| SortOrder    | int           | Required, default 0 |

---

### 4.11 Section
| Column       | Type          | Constraints                |
|--------------|---------------|----------------------------|
| SectionId    | int           | PK, Identity               |
| ClassLevelId | int           | FK → ClassLevels, Required |
| Name         | nvarchar(50)  | Required                   |

---

### 4.12 Student
| Column           | Type          | Constraints                  |
|------------------|---------------|------------------------------|
| StudentId        | int           | PK, Identity                 |
| RegistrationNo   | nvarchar(50)  | nullable, Index              |
| RollNo           | nvarchar(20)  | nullable                     |
| Name             | nvarchar(150) | Required                     |
| SessionId        | int           | FK → Sessions, nullable      |
| ClassLevelId     | int           | FK → ClassLevels, nullable   |
| SectionId        | int           | FK → Sections, nullable      |
| IsActive         | bit           | Required, default 1          |

---

### 4.13 Employee
| Column       | Type          | Constraints       |
|--------------|---------------|-------------------|
| EmployeeId   | int           | PK, Identity      |
| Name         | nvarchar(150) | Required          |
| EmployeeCode | nvarchar(50)  | nullable, Index (unique) |
| Department   | nvarchar(100) | nullable          |
| IsActive     | bit           | Required, default 1 |

---

### 4.14 Purchase
| Column              | Type          | Constraints                     |
|---------------------|---------------|---------------------------------|
| PurchaseId          | int           | PK, Identity                    |
| PurchaseNumber      | nvarchar(30)  | Required, unique, Index         |
| InvoiceOrOrder      | int (enum)    | Required (0=Invoice, 1=Order)   |
| VendorId            | int           | FK → Vendors, nullable          |
| PurchaseDate        | datetime2     | Required                        |
| ManualPurchaseNumber| nvarchar(50)  | nullable                        |
| Subject             | nvarchar(300) | nullable                        |
| TotalAmount         | decimal(10,2) | Required, default 0             |
| OverallDiscount     | decimal(10,2) | nullable, default 0             |
| NetAmount           | decimal(10,2) | Required, default 0             |

---

### 4.15 PurchaseItem
| Column         | Type          | Constraints                    |
|----------------|---------------|--------------------------------|
| PurchaseItemId | int           | PK, Identity                   |
| PurchaseId     | int           | FK → Purchases, Required       |
| BookId         | int           | FK → Books, nullable           |
| ItemName       | nvarchar(300) | nullable                       |
| SerialNo       | nvarchar(50)  | nullable                       |
| Quantity       | int           | Required                       |
| PricePerUnit   | decimal(10,2) | Required                       |
| DiscountPercent| decimal(5,2)  | nullable, default 0            |
| NetPrice       | decimal(10,2) | Required                       |

---

### 4.16 IssuedBook
| Column       | Type          | Constraints                          |
|--------------|---------------|--------------------------------------|
| IssuedBookId | int           | PK, Identity                         |
| IssueNo      | nvarchar(30)  | Required, unique, Index              |
| BookId       | int           | FK → Books, Required                 |
| MemberType   | int (enum)    | Required (0=Student, 1=Employee)     |
| StudentId    | int           | FK → Students, nullable              |
| EmployeeId   | int           | FK → Employees, nullable             |
| IssueDate    | datetime2     | Required                             |
| DueDate      | datetime2     | Required                             |
| ReturnDate   | datetime2     | nullable (null = still issued)       |
| IssuedDetail | nvarchar(500) | nullable                             |

---

## 5. Business Rules

| Rule | Description |
|------|-------------|
| BR-1 | `RemainingQuantity = TotalQuantity − (count of IssuedBooks where BookId matches and ReturnDate IS NULL)`. This is maintained in application logic, not as a DB computed column, to avoid trigger complexity. |
| BR-2 | Before issuing a book, the service must check `RemainingQuantity > 0`. If not, throw a domain exception with message "No copies available." |
| BR-3 | `IssueNo` is auto-generated as a zero-padded sequential number (e.g. "ISS-000001"). |
| BR-4 | Overdue definition: `ReturnDate IS NULL AND DueDate < DateTime.Today`. Days Passed = `(DateTime.Today − DueDate).Days`. |
| BR-5 | When a Purchase is saved, for each `PurchaseItem` where `BookId IS NOT NULL`, increment `Books.TotalQuantity` and `Books.RemainingQuantity` by `PurchaseItem.Quantity` within the same transaction. |
| BR-6 | Cover image uploads are restricted to jpg, jpeg, png, gif and max 5 MB. |
| BR-7 | A Student may only have one active issue per book at a time (application-level check). |

---

## 6. Enumerations

```csharp
public enum PurchaseType
{
    Invoice = 0,
    Order   = 1
}

public enum MemberType
{
    Student  = 0,
    Employee = 1
}
```

---

## 7. Indexes Summary

| Table       | Columns Indexed                         | Type   |
|-------------|-----------------------------------------|--------|
| Books       | Title                                   | Non-unique |
| Books       | ISBN                                    | Unique (nullable) |
| Books       | BarCode                                 | Non-unique |
| Authors     | Name                                    | Non-unique |
| Students    | RegistrationNo                          | Non-unique |
| Employees   | EmployeeCode                            | Unique (nullable) |
| Purchases   | PurchaseNumber                          | Unique |
| IssuedBooks | IssueNo                                 | Unique |
| IssuedBooks | BookId                                  | Non-unique |
| IssuedBooks | StudentId, EmployeeId                   | Non-unique |

---

## 8. Application Layer Design

### Service Interfaces (to be implemented in Phase 2+)

```
IAuthorService       — CRUD for Authors
IPublisherService    — CRUD for Publishers
ICategoryService     — CRUD for Categories
IBookTypeService     — CRUD for BookTypes
IVendorService       — CRUD for Vendors
IBookService         — CRUD + Search for Books, image upload
IPurchaseService     — Create/Edit Purchase + quantity update
IMemberService       — GetMembers, CRUD Students + Employees
IIssueService        — IssueBook, ReturnBook, GetIssuedBooks, GetOverdueBooks
ILookupService       — Sessions, ClassLevels, Sections CRUD
```

### ViewModel Convention

Every controller action receives/returns a ViewModel (suffix `Vm` or `ViewModel`), never a raw entity. AutoMapper is used to map between entities and ViewModels.

---

## 9. Infrastructure

### DbContext

`LibraryDbContext : DbContext` in `LibraryMS.Infrastructure`

- Registered as `AddDbContext<LibraryDbContext>` in `Program.cs`
- Connection string: `appsettings.json → ConnectionStrings:DefaultConnection`

### Migrations

Location: `LibraryMS.Infrastructure/Migrations/`

Run command:
```bash
dotnet ef database update --project LibraryMS.Infrastructure --startup-project LibraryMS.Web
```

---

## 10. Folder Structure (Web Project)

```
LibraryMS.Web/
├── Controllers/
│   ├── HomeController.cs
│   └── (per-module controllers added each phase)
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _ValidationScriptsPartial.cshtml
│   └── Home/
│       └── Index.cshtml
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── uploads/
│       └── books/        ← cover images stored here
├── appsettings.json
└── Program.cs
```
