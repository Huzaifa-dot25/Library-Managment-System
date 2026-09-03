namespace LibraryMS.Domain.Entities;

public class Vendor
{
    public int VendorId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Email, phone, address — stored as free text.</summary>
    public string? ContactInfo { get; set; }

    // Navigation
    public ICollection<Book>     Books     { get; set; } = new List<Book>();
    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
