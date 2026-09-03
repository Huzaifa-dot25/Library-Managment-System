using System.ComponentModel.DataAnnotations;
using LibraryMS.Domain.Enums;

namespace LibraryMS.Application.ViewModels;

// ── Purchase Item (one line in the entry grid) ────────────────────────────
public class PurchaseItemViewModel
{
    public int     PurchaseItemId  { get; set; }
    public int?    BookId          { get; set; }
    public string? ItemName        { get; set; }
    public string? SerialNo        { get; set; }
    public int     Quantity        { get; set; } = 1;
    public decimal PricePerUnit    { get; set; } = 0;
    public decimal DiscountPercent { get; set; } = 0;
    public decimal NetPrice        { get; set; } = 0;
    // Display only
    public int     ExistingStock   { get; set; } = 0;
}

// ── Purchase Form (header + items) ────────────────────────────────────────
public class PurchaseFormViewModel
{
    public int          PurchaseId           { get; set; }
    public string       PurchaseNumber       { get; set; } = string.Empty;
    public PurchaseType InvoiceOrOrder       { get; set; } = PurchaseType.Invoice;
    public int?         VendorId             { get; set; }
    public DateTime     PurchaseDate         { get; set; } = DateTime.Today;
    public string?      ManualPurchaseNumber { get; set; }
    public bool         ManualNumberEnabled  { get; set; } = false;
    public string?      Subject              { get; set; }
    public decimal      TotalAmount          { get; set; } = 0;
    public decimal      OverallDiscount      { get; set; } = 0;
    public decimal      NetAmount            { get; set; } = 0;

    public List<PurchaseItemViewModel> Items   { get; set; } = new();
    public List<VendorViewModel>       Vendors { get; set; } = new();
}

// ── Purchase List row (history grid) ─────────────────────────────────────
public class PurchaseListViewModel
{
    public int         PurchaseId           { get; set; }
    public string      PurchaseNumber       { get; set; } = string.Empty;
    public string?     VendorName           { get; set; }
    public DateTime    PurchaseDate         { get; set; }
    public PurchaseType InvoiceOrOrder      { get; set; }
    public string      InvoiceOrOrderLabel  => InvoiceOrOrder == PurchaseType.Invoice ? "Invoice" : "Order";
    public decimal     NetAmount            { get; set; }
    public int         ItemCount            { get; set; }
}

// ── Filter for history search ─────────────────────────────────────────────
public class PurchaseFilterViewModel
{
    public DateTime?    FromDate        { get; set; }
    public DateTime?    ToDate          { get; set; }
    public PurchaseType? InvoiceOrOrder { get; set; }         // null = both
    public int?         VendorId        { get; set; }
    public string?      PurchaseNumber  { get; set; }
    public bool         AllVendors      { get; set; } = true;
    public bool         AllPurchaseNums { get; set; } = true;
    public bool         BothTypes       { get; set; } = true;

    // Dropdown data
    public List<VendorViewModel> Vendors { get; set; } = new();
}
