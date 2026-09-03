namespace LibraryMS.Domain.Entities;

public class PurchaseItem
{
    public int PurchaseItemId { get; set; }

    public int PurchaseId { get; set; }
    public Purchase Purchase { get; set; } = null!;

    /// <summary>Nullable — item may not yet be in the book catalog.</summary>
    public int? BookId  { get; set; }
    public Book? Book   { get; set; }

    /// <summary>Free-text item name if not linked to a catalog book.</summary>
    public string? ItemName  { get; set; }
    public string? SerialNo  { get; set; }

    public int Quantity { get; set; } = 1;

    public decimal PricePerUnit     { get; set; } = 0;
    public decimal DiscountPercent  { get; set; } = 0;

    /// <summary>Qty × PricePerUnit × (1 - DiscountPercent/100). Calculated in service.</summary>
    public decimal NetPrice { get; set; } = 0;
}
