namespace LibraryMS.Application.ViewModels;

/// <summary>One row in the Manage Books grid.</summary>
public class BookListViewModel
{
    public int     BookId              { get; set; }
    public string  Title               { get; set; } = string.Empty;
    public string? ISBN                { get; set; }
    public string? BarCode             { get; set; }
    public string  Authors             { get; set; } = string.Empty;   // comma-joined
    public string  Categories          { get; set; } = string.Empty;   // comma-joined
    public string? Publisher           { get; set; }
    public int     TotalQuantity       { get; set; }
    public int     RemainingQuantity   { get; set; }
    public int     IssuedCount         { get; set; }
    public int     LostCount           { get; set; }
    public string? RackName            { get; set; }
    public string? AccessionNumber     { get; set; }
    public string? CallNumberDeweyCode { get; set; }
    public string? LibrarianRemarks    { get; set; }
    public string? BookType            { get; set; }
    public string? CoverImagePath      { get; set; }
}

/// <summary>Paged result wrapper returned to the grid.</summary>
public class BookPagedResult
{
    public List<BookListViewModel> Items      { get; set; } = new();
    public int                     TotalCount { get; set; }
    public int                     Page       { get; set; }
    public int                     PageSize   { get; set; }
}

/// <summary>Filter parameters for the Manage Books search bar.</summary>
public class BookFilterViewModel
{
    public int?    BookTypeId  { get; set; }
    public int?    CategoryId  { get; set; }
    public int?    AuthorId    { get; set; }
    public int?    PublisherId { get; set; }
    public string? SearchText  { get; set; }   // title / ISBN / barcode
    public string? AccCallText { get; set; }   // accession / callno / dewey
    public string? Status      { get; set; }   // New Book / Old Book / etc.
    public int     Page        { get; set; } = 1;
    public int     PageSize    { get; set; } = 20;
}

/// <summary>Result row for the Book Search widget.</summary>
public class BookSearchResultViewModel
{
    public int     BookId            { get; set; }
    public string? ISBN              { get; set; }
    public string  Title             { get; set; } = string.Empty;
    public string  Authors           { get; set; } = string.Empty;
    public int     TotalQuantity     { get; set; }
    public int     RemainingQuantity { get; set; }
}
