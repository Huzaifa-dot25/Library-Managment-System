namespace LibraryMS.Domain.Entities;

/// <summary>
/// Explicit many-to-many join table between Books and Authors.
/// A book can have multiple authors; authors appear as a comma-list in the UI.
/// </summary>
public class BookAuthor
{
    public int BookId   { get; set; }
    public Book Book   { get; set; } = null!;

    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;
}
