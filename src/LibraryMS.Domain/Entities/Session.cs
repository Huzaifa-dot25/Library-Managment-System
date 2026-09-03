namespace LibraryMS.Domain.Entities;

/// <summary>
/// Academic year session, e.g. "2026-2027".
/// </summary>
public class Session
{
    public int SessionId { get; set; }

    /// <summary>e.g. "2026 - 2027"</summary>
    public string SessionName { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate   { get; set; }

    // Navigation
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
