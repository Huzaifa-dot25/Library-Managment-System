namespace LibraryMS.Application.ViewModels;

// ── Issue a book to a member ──────────────────────────────────────────────
public class IssueBookViewModel
{
    public int    BookId     { get; set; }
    public string MemberType { get; set; } = "Student";   // Student | Employee
    public int    MemberId   { get; set; }
    public DateTime DueDate  { get; set; } = DateTime.Today.AddDays(14);
    public string? Notes     { get; set; }
}

// ── One row in the "currently issued to this member" list ─────────────────
public class IssuedBookDetailViewModel
{
    public int      IssuedBookId { get; set; }
    public string   IssueNo      { get; set; } = string.Empty;
    public int      BookId       { get; set; }
    public string   BookTitle    { get; set; } = string.Empty;
    public string?  ISBN         { get; set; }
    public DateTime IssueDate    { get; set; }
    public DateTime DueDate      { get; set; }
    public DateTime? ReturnDate  { get; set; }
    public bool     IsOverdue    => ReturnDate == null && DueDate < DateTime.Today;
    public int      DaysOverdue  => IsOverdue ? (DateTime.Today - DueDate).Days : 0;
}

// ── Return one or many books ──────────────────────────────────────────────
public class ReturnBookViewModel
{
    public List<int> IssuedBookIds { get; set; } = new();
}

// ── Row in Manage Issued Books (Phase 7) ─────────────────────────────────
public class IssuedBookListViewModel
{
    public int      IssuedBookId { get; set; }
    public string   IssueNo      { get; set; } = string.Empty;
    public string   MemberType   { get; set; } = string.Empty;
    public int      MemberId     { get; set; }
    public string   MemberName   { get; set; } = string.Empty;
    public string?  RegistrationNo { get; set; }
    public string   BookTitle    { get; set; } = string.Empty;
    public string?  ISBN         { get; set; }
    public DateTime IssueDate    { get; set; }
    public DateTime DueDate      { get; set; }
    public DateTime? ReturnDate  { get; set; }
    public bool     IsReturned   => ReturnDate.HasValue;
    public bool     IsOverdue    => ReturnDate == null && DueDate < DateTime.Today;
    public int      DaysOverdue  => IsOverdue ? (DateTime.Today - DueDate).Days : 0;
}

// ── Filter for Manage Issued Books (Phase 7) ─────────────────────────────
public class IssuedBookFilterViewModel
{
    public DateTime? IssuedFrom  { get; set; }
    public DateTime? IssuedTo    { get; set; }
    public string?   SearchText  { get; set; }   // IssueNo / MemberName / MemberId
    public string    MemberType  { get; set; } = "All";  // All | Student | Employee
}

// ── Overdue filter (Phase 7) ──────────────────────────────────────────────
public class OverdueFilterViewModel
{
    public int    DaysPassed { get; set; } = 1;
    public string MemberType { get; set; } = "All";  // All | Students | Employees
}

// ── Bulk issue payload ────────────────────────────────────────────────────
public class BulkIssueViewModel
{
    public int      BookId  { get; set; }
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);
    public List<MemberSearchResultViewModel> Members { get; set; } = new();
}
