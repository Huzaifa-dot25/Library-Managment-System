namespace LibraryMS.Domain.Entities;

public class Category
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
}
