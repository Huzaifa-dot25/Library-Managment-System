namespace LibraryMS.Domain.Entities;

public class Author
{
    public int AuthorId { get; set; }

    /// <summary>Author's full name. Required.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>e.g. Male / Female</summary>
    public string? Gender { get; set; }

    public string? Country { get; set; }

    // Navigation
    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}
