using LibraryMS.Domain.Enums;

namespace LibraryMS.Domain.Entities;

public class IssuedBook
{
    public int IssuedBookId { get; set; }

    /// <summary>Auto-generated unique issue number, e.g. "ISS-000001"</summary>
    public string IssueNo { get; set; } = string.Empty;

    public int BookId { get; set; }
    public Book Book  { get; set; } = null!;

    public MemberType MemberType { get; set; }

    /// <summary>Populated when MemberType == Student.</summary>
    public int? StudentId    { get; set; }
    public Student? Student  { get; set; }

    /// <summary>Populated when MemberType == Employee.</summary>
    public int? EmployeeId    { get; set; }
    public Employee? Employee { get; set; }

    public DateTime IssueDate { get; set; } = DateTime.Today;
    public DateTime DueDate   { get; set; }

    /// <summary>Null = book still issued. Set on return.</summary>
    public DateTime? ReturnDate { get; set; }

    public string? IssuedDetail { get; set; }
}
