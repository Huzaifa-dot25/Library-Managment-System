namespace LibraryMS.Domain.Entities;

public class Book
{
    public int BookId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? ISBN    { get; set; }
    public string? BarCode { get; set; }

    public string? Description { get; set; }

    public int? Pages { get; set; }

    /// <summary>Rack or Grade field from Add Book form.</summary>
    public string? RackOrGrade { get; set; }

    /// <summary>
    /// Book status: "New Book", "Old Book", etc.
    /// Stored as a string for flexibility.
    /// </summary>
    public string? BookStatus { get; set; }

    // FK → BookType (Select Type dropdown)
    public int? BookTypeId { get; set; }
    public BookType? BookType { get; set; }

    // FK → Vendor
    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    // FK → Publisher
    public int? PublisherId { get; set; }
    public Publisher? Publisher { get; set; }

    public string? AccessionNumber     { get; set; }
    public string? CallNumberDeweyCode { get; set; }

    public decimal? Price { get; set; }

    public int TotalQuantity     { get; set; } = 0;
    public int RemainingQuantity { get; set; } = 0;
    public int IssuedCount       { get; set; } = 0;
    public int LostCount         { get; set; } = 0;

    public string? RackName          { get; set; }
    public string? LibrarianRemarks  { get; set; }

    /// <summary>Relative path stored, e.g. /uploads/books/cover.jpg</summary>
    public string? CoverImagePath { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Many-to-many navigations
    public ICollection<BookAuthor>   BookAuthors    { get; set; } = new List<BookAuthor>();
    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();

    // One-to-many
    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    public ICollection<IssuedBook>   IssuedBooks   { get; set; } = new List<IssuedBook>();
}
