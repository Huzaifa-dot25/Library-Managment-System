using System.ComponentModel.DataAnnotations;

namespace LibraryMS.Application.ViewModels;

/// <summary>Used for Add Book and Edit Book forms.</summary>
public class BookFormViewModel
{
    public int BookId { get; set; }

    [Required(ErrorMessage = "Book title is required.")]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [StringLength(50)]
    public string? BookStatus { get; set; } = "New Book";

    public int? PublisherId { get; set; }

    public string? Description { get; set; }

    [StringLength(30)]
    public string? ISBN { get; set; }

    public int? Pages { get; set; }

    [StringLength(50)]
    public string? BarCode { get; set; }

    [StringLength(50)]
    public string? BookKind { get; set; } = "Book";

    [StringLength(100)]
    public string? RackOrGrade { get; set; }

    public int? BookTypeId { get; set; }
    public int? VendorId   { get; set; }

    [StringLength(50)]
    public string? AccessionNumber     { get; set; }

    [StringLength(50)]
    public string? CallNumberDeweyCode { get; set; }

    [Range(0, 9999999)]
    public decimal? Price { get; set; }

    public int TotalQuantity { get; set; } = 1;

    [StringLength(100)]
    public string? RackName { get; set; }

    [StringLength(500)]
    public string? LibrarianRemarks { get; set; }

    /// <summary>Existing cover image path (populated on edit).</summary>
    public string? CoverImagePath { get; set; }

    // Multi-select: IDs of selected authors / categories
    public List<int> SelectedAuthorIds   { get; set; } = new();
    public List<int> SelectedCategoryIds { get; set; } = new();

    // ── Dropdown data (populated by controller) ───────────────────────
    public List<AuthorViewModel>    Authors     { get; set; } = new();
    public List<PublisherViewModel> Publishers  { get; set; } = new();
    public List<CategoryViewModel>  Categories  { get; set; } = new();
    public List<BookTypeViewModel>  BookTypes   { get; set; } = new();
    public List<VendorViewModel>    Vendors     { get; set; } = new();
}
