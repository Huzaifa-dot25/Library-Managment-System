using System.ComponentModel.DataAnnotations;

namespace LibraryMS.Application.ViewModels;

// ── Lookup ViewModels ─────────────────────────────────────────────────────

public class SessionViewModel
{
    public int       SessionId   { get; set; }
    [Required(ErrorMessage = "Session name is required.")]
    [StringLength(50)]
    public string    SessionName { get; set; } = string.Empty;
    public DateTime? StartDate   { get; set; }
    public DateTime? EndDate     { get; set; }
}

public class ClassLevelViewModel
{
    public int    ClassLevelId { get; set; }
    [Required(ErrorMessage = "Class name is required.")]
    [StringLength(100)]
    public string Name         { get; set; } = string.Empty;
    public int    SortOrder    { get; set; }
}

public class SectionViewModel
{
    public int    SectionId    { get; set; }
    public int    ClassLevelId { get; set; }
    public string ClassLevelName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Section name is required.")]
    [StringLength(50)]
    public string Name         { get; set; } = string.Empty;
}

// ── Student ───────────────────────────────────────────────────────────────

public class StudentViewModel
{
    public int     StudentId       { get; set; }

    [StringLength(50)]
    public string? RegistrationNo  { get; set; }

    [StringLength(20)]
    public string? RollNo          { get; set; }

    [Required(ErrorMessage = "Student name is required.")]
    [StringLength(150)]
    public string  Name            { get; set; } = string.Empty;

    public int?    SessionId       { get; set; }
    public string? SessionName     { get; set; }

    public int?    ClassLevelId    { get; set; }
    public string? ClassLevelName  { get; set; }

    public int?    SectionId       { get; set; }
    public string? SectionName     { get; set; }

    public bool    IsActive        { get; set; } = true;

    // Dropdown data for form
    public List<SessionViewModel>    Sessions    { get; set; } = new();
    public List<ClassLevelViewModel> ClassLevels { get; set; } = new();
    public List<SectionViewModel>    Sections    { get; set; } = new();
}

// ── Employee ──────────────────────────────────────────────────────────────

public class EmployeeViewModel
{
    public int     EmployeeId   { get; set; }

    [Required(ErrorMessage = "Employee name is required.")]
    [StringLength(150)]
    public string  Name         { get; set; } = string.Empty;

    [StringLength(50)]
    public string? EmployeeCode { get; set; }

    [StringLength(100)]
    public string? Department   { get; set; }

    public bool    IsActive     { get; set; } = true;
}

// ── Member search result (used by Phase 6 Issue/Return) ──────────────────

public class MemberSearchResultViewModel
{
    public string  MemberType     { get; set; } = string.Empty;  // "Student" | "Employee"
    public int     MemberId       { get; set; }
    public string  Name           { get; set; } = string.Empty;
    public string? RegistrationNo { get; set; }
    public string? RollNo         { get; set; }
    public string? ClassLevel     { get; set; }
    public string? Section        { get; set; }
    public string? Session        { get; set; }
    public string? Department     { get; set; }
    public bool    IsActive       { get; set; }
    public int     IssuedBooksCount { get; set; }
}

// ── Filter for member search ──────────────────────────────────────────────

public class MemberFilterViewModel
{
    public string  MemberType   { get; set; } = "Student";  // Student | Employee
    public int?    SessionId    { get; set; }
    public int?    ClassLevelId { get; set; }
    public int?    SectionId    { get; set; }
    public string  ActiveStatus { get; set; } = "Active";   // Active | Inactive | Both
    public string  SearchBy     { get; set; } = "Name";     // Name | RegistrationNo | RollNo
    public string? SearchTerm   { get; set; }
}
