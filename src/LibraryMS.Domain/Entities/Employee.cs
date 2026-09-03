namespace LibraryMS.Domain.Entities;

public class Employee
{
    public int EmployeeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? EmployeeCode { get; set; }
    public string? Department   { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<IssuedBook> IssuedBooks { get; set; } = new List<IssuedBook>();
}
