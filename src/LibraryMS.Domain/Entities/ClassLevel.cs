namespace LibraryMS.Domain.Entities;

/// <summary>
/// Class levels: Playgroup, FS 1, FS 2, Year 1–6,
/// Pre IGCSE I/II/III, IGCSE I/Grade 9, IGCSE II/Grade 10,
/// AS LEVEL, A LEVEL.
/// SortOrder controls the dropdown display sequence.
/// </summary>
public class ClassLevel
{
    public int ClassLevelId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;

    // Navigation
    public ICollection<Section> Sections { get; set; } = new List<Section>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
