namespace LibraryMS.Domain.Entities;

public class Student
{
    public int StudentId { get; set; }

    public string? RegistrationNo { get; set; }
    public string? RollNo         { get; set; }

    public string Name { get; set; } = string.Empty;

    public int? SessionId     { get; set; }
    public Session? Session   { get; set; }

    public int? ClassLevelId       { get; set; }
    public ClassLevel? ClassLevel  { get; set; }

    public int? SectionId    { get; set; }
    public Section? Section  { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<IssuedBook> IssuedBooks { get; set; } = new List<IssuedBook>();
}
