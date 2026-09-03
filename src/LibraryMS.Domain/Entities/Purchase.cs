using LibraryMS.Domain.Enums;

namespace LibraryMS.Domain.Entities;

public class Purchase
{
    public int PurchaseId { get; set; }

    /// <summary>Auto-generated, zero-padded, e.g. "00001"</summary>
    public string PurchaseNumber { get; set; } = string.Empty;

    public PurchaseType InvoiceOrOrder { get; set; } = PurchaseType.Invoice;

    public int? VendorId  { get; set; }
    public Vendor? Vendor { get; set; }

    public DateTime PurchaseDate { get; set; } = DateTime.Today;

    /// <summary>Optional reference number from the supplier's invoice.</summary>
    public string? ManualPurchaseNumber { get; set; }

    public string? Subject { get; set; }

    public decimal TotalAmount      { get; set; } = 0;
    public decimal OverallDiscount  { get; set; } = 0;
    public decimal NetAmount        { get; set; } = 0;

    // Navigation
    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
}
