namespace LibraryMS.Domain.Entities;

/// <summary>
/// Explicit many-to-many join table between Books and Categories.
/// </summary>
public class BookCategory
{
    public int BookId     { get; set; }
    public Book Book     { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
