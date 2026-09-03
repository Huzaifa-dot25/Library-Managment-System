namespace LibraryMS.Domain.Entities;

/// <summary>
/// A section within a class level, e.g. "Blue".
/// </summary>
public class Section
{
    public int SectionId { get; set; }

    public int ClassLevelId { get; set; }
    public ClassLevel ClassLevel { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
