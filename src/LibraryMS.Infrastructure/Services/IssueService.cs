using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using LibraryMS.Domain.Entities;
using LibraryMS.Domain.Enums;
using LibraryMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Infrastructure.Services;

public class IssueService : IIssueService
{
    private readonly LibraryDbContext _db;
    public IssueService(LibraryDbContext db) => _db = db;

    // ── Auto-generate IssueNo ─────────────────────────────────────
    private async Task<string> NextIssueNoAsync()
    {
        var last = await _db.IssuedBooks
            .OrderByDescending(i => i.IssuedBookId)
            .Select(i => i.IssueNo)
            .FirstOrDefaultAsync();

        if (last == null) return "ISS-00001";
        var parts = last.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[1], out int n))
            return $"ISS-{(n + 1):D5}";
        return $"ISS-{(await _db.IssuedBooks.CountAsync() + 1):D5}";
    }

    // ── Issue a book ──────────────────────────────────────────────
    public async Task<IssuedBookDetailViewModel> IssueBookAsync(IssueBookViewModel vm)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var book = await _db.Books.FindAsync(vm.BookId)
                ?? throw new InvalidOperationException("Book not found.");

            if (book.RemainingQuantity <= 0)
                throw new InvalidOperationException(
                    $"No copies available. Remaining quantity is 0.");

            // Check member exists
            if (vm.MemberType == "Student")
            {
                var student = await _db.Students.FindAsync(vm.MemberId)
                    ?? throw new InvalidOperationException("Student not found.");
                if (!student.IsActive)
                    throw new InvalidOperationException("Student is inactive.");
            }
            else
            {
                var employee = await _db.Employees.FindAsync(vm.MemberId)
                    ?? throw new InvalidOperationException("Employee not found.");
                if (!employee.IsActive)
                    throw new InvalidOperationException("Employee is inactive.");
            }

            var issued = new IssuedBook
            {
                IssueNo    = await NextIssueNoAsync(),
                BookId     = vm.BookId,
                MemberType = vm.MemberType == "Student" ? MemberType.Student : MemberType.Employee,
                StudentId  = vm.MemberType == "Student"  ? vm.MemberId : null,
                EmployeeId = vm.MemberType == "Employee" ? vm.MemberId : null,
                IssueDate  = DateTime.Today,
                DueDate    = vm.DueDate,
                IssuedDetail = vm.Notes
            };

            _db.IssuedBooks.Add(issued);

            book.RemainingQuantity--;
            book.IssuedCount++;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return new IssuedBookDetailViewModel
            {
                IssuedBookId = issued.IssuedBookId,
                IssueNo      = issued.IssueNo,
                BookId       = book.BookId,
                BookTitle    = book.Title,
                ISBN         = book.ISBN,
                IssueDate    = issued.IssueDate,
                DueDate      = issued.DueDate
            };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ── Return books ──────────────────────────────────────────────
    public async Task ReturnBooksAsync(ReturnBookViewModel vm)
    {
        if (vm.IssuedBookIds == null || !vm.IssuedBookIds.Any()) return;

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var id in vm.IssuedBookIds)
            {
                var issued = await _db.IssuedBooks
                    .Include(i => i.Book)
                    .FirstOrDefaultAsync(i => i.IssuedBookId == id);

                if (issued == null || issued.ReturnDate != null) continue;

                issued.ReturnDate = DateTime.Today;

                if (issued.Book != null)
                {
                    issued.Book.RemainingQuantity++;
                    if (issued.Book.IssuedCount > 0)
                        issued.Book.IssuedCount--;
                }
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ── Issued by member ──────────────────────────────────────────
    public async Task<List<IssuedBookDetailViewModel>> GetIssuedByMemberAsync(
        string memberType, int memberId)
    {
        var q = _db.IssuedBooks
            .Include(i => i.Book)
            .Where(i => i.ReturnDate == null);

        if (memberType == "Student")
            q = q.Where(i => i.StudentId == memberId);
        else
            q = q.Where(i => i.EmployeeId == memberId);

        return await q.OrderByDescending(i => i.IssueDate)
            .Select(i => new IssuedBookDetailViewModel
            {
                IssuedBookId = i.IssuedBookId,
                IssueNo      = i.IssueNo,
                BookId       = i.BookId,
                BookTitle    = i.Book.Title,
                ISBN         = i.Book.ISBN,
                IssueDate    = i.IssueDate,
                DueDate      = i.DueDate,
                ReturnDate   = i.ReturnDate
            }).ToListAsync();
    }

    // ── Bulk issue to class ───────────────────────────────────────
    public async Task<(int success, int skipped)> BulkIssueAsync(
        int bookId, DateTime dueDate, List<MemberSearchResultViewModel> members)
    {
        int success = 0, skipped = 0;

        var book = await _db.Books.FindAsync(bookId)
            ?? throw new InvalidOperationException("Book not found.");

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
        foreach (var member in members)
        {
            if (book.RemainingQuantity <= 0) { skipped++; continue; }

            // Skip if already has this book issued
            bool alreadyIssued = member.MemberType == "Student"
                ? await _db.IssuedBooks.AnyAsync(i =>
                    i.BookId == bookId && i.StudentId == member.MemberId && i.ReturnDate == null)
                : await _db.IssuedBooks.AnyAsync(i =>
                    i.BookId == bookId && i.EmployeeId == member.MemberId && i.ReturnDate == null);

            if (alreadyIssued) { skipped++; continue; }

            var issued = new IssuedBook
            {
                IssueNo    = await NextIssueNoAsync(),
                BookId     = bookId,
                MemberType = member.MemberType == "Student" ? MemberType.Student : MemberType.Employee,
                StudentId  = member.MemberType == "Student"  ? member.MemberId : null,
                EmployeeId = member.MemberType == "Employee" ? member.MemberId : null,
                IssueDate  = DateTime.Today,
                DueDate    = dueDate
            };
            _db.IssuedBooks.Add(issued);
            book.RemainingQuantity--;
            book.IssuedCount++;
            await _db.SaveChangesAsync();
            success++;
        }

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        return (success, skipped);
    }

    // ── All issued books (Phase 7) ────────────────────────────────
    public async Task<List<IssuedBookListViewModel>> GetIssuedBooksAsync(
        IssuedBookFilterViewModel f)
    {
        var q = _db.IssuedBooks
            .Include(i => i.Book)
            .Include(i => i.Student)
            .Include(i => i.Employee)
            .AsQueryable();

        if (f.IssuedFrom.HasValue)
            q = q.Where(i => i.IssueDate >= f.IssuedFrom.Value);

        if (f.IssuedTo.HasValue)
            q = q.Where(i => i.IssueDate <= f.IssuedTo.Value.AddDays(1).AddSeconds(-1));

        if (!string.IsNullOrEmpty(f.MemberType) && f.MemberType != "All")
        {
            var mt = f.MemberType == "Student"
                ? MemberType.Student : MemberType.Employee;
            q = q.Where(i => i.MemberType == mt);
        }

        if (!string.IsNullOrWhiteSpace(f.SearchText))
        {
            var t = f.SearchText.Trim().ToLower();
            q = q.Where(i =>
                i.IssueNo.ToLower().Contains(t) ||
                (i.Student != null && (i.Student.Name.ToLower().Contains(t) ||
                    (i.Student.RegistrationNo != null && i.Student.RegistrationNo.ToLower().Contains(t)))) ||
                (i.Employee != null && i.Employee.Name.ToLower().Contains(t)));
        }

        return await q.OrderByDescending(i => i.IssueDate)
            .Take(500)
            .Select(i => new IssuedBookListViewModel
            {
                IssuedBookId   = i.IssuedBookId,
                IssueNo        = i.IssueNo,
                MemberType     = i.MemberType == MemberType.Student ? "Student" : "Employee",
                MemberId       = i.MemberType == MemberType.Student
                                    ? (i.StudentId ?? 0) : (i.EmployeeId ?? 0),
                MemberName     = i.MemberType == MemberType.Student
                                    ? (i.Student != null ? i.Student.Name : "")
                                    : (i.Employee != null ? i.Employee.Name : ""),
                RegistrationNo = i.Student != null ? i.Student.RegistrationNo : null,
                BookTitle      = i.Book.Title,
                ISBN           = i.Book.ISBN,
                IssueDate      = i.IssueDate,
                DueDate        = i.DueDate,
                ReturnDate     = i.ReturnDate
            }).ToListAsync();
    }

    // ── Overdue books (Phase 7) ───────────────────────────────────
    public async Task<List<IssuedBookListViewModel>> GetOverdueBooksAsync(
        OverdueFilterViewModel f)
    {
        var today = DateTime.Today;
        var q = _db.IssuedBooks
            .Include(i => i.Book)
            .Include(i => i.Student)
            .Include(i => i.Employee)
            .Where(i => i.ReturnDate == null && i.DueDate < today);

        if (f.MemberType == "Students")
            q = q.Where(i => i.MemberType == MemberType.Student);
        else if (f.MemberType == "Employees")
            q = q.Where(i => i.MemberType == MemberType.Employee);

        var all = await q
            .Select(i => new IssuedBookListViewModel
            {
                IssuedBookId   = i.IssuedBookId,
                IssueNo        = i.IssueNo,
                MemberType     = i.MemberType == MemberType.Student ? "Student" : "Employee",
                MemberId       = i.MemberType == MemberType.Student
                                    ? (i.StudentId ?? 0) : (i.EmployeeId ?? 0),
                MemberName     = i.MemberType == MemberType.Student
                                    ? (i.Student != null ? i.Student.Name : "")
                                    : (i.Employee != null ? i.Employee.Name : ""),
                RegistrationNo = i.Student != null ? i.Student.RegistrationNo : null,
                BookTitle      = i.Book.Title,
                IssueDate      = i.IssueDate,
                DueDate        = i.DueDate,
                ReturnDate     = i.ReturnDate
            }).ToListAsync();

        // Filter by days passed threshold (computed client-side can't be done in EF)
        return all
            .Where(i => (today - i.DueDate).Days >= f.DaysPassed)
            .OrderByDescending(i => (today - i.DueDate).Days)
            .ToList();
    }
}
