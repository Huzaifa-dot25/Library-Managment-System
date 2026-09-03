using LibraryMS.Domain.Enums;

namespace LibraryMS.Domain.Entities;

public class BookType
{
    public int BookTypeId { get; set; }

    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Link Account: Library Books or Academic Books.
    /// Shown as a dropdown in Manage Types screen.
    /// </summary>
    public LinkedAccountType LinkedAccount { get; set; } = LinkedAccountType.LibraryBooks;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
