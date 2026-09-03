using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using LibraryMS.Domain.Entities;
using LibraryMS.Domain.Enums;
using LibraryMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryMS.Infrastructure.Services;

public class PurchaseService : IPurchaseService
{
    private readonly LibraryDbContext _db;
    public PurchaseService(LibraryDbContext db) => _db = db;

    // ── Vendor dropdown helper ────────────────────────────────────
    private async Task<List<VendorViewModel>> GetVendorsAsync() =>
        await _db.Vendors
            .OrderBy(v => v.Name)
            .Select(v => new VendorViewModel { VendorId = v.VendorId, Name = v.Name })
            .ToListAsync();

    // ── Auto purchase number ──────────────────────────────────────
    private async Task<string> NextPurchaseNumberAsync()
    {
        var last = await _db.Purchases
            .OrderByDescending(p => p.PurchaseId)
            .Select(p => p.PurchaseNumber)
            .FirstOrDefaultAsync();

        if (last == null) return "00001";
        if (int.TryParse(last, out int n)) return (n + 1).ToString("D5");
        return (await _db.Purchases.CountAsync() + 1).ToString("D5");
    }

    // ── New form ──────────────────────────────────────────────────
    public async Task<PurchaseFormViewModel> GetNewFormAsync() =>
        new PurchaseFormViewModel
        {
            PurchaseNumber = await NextPurchaseNumberAsync(),
            PurchaseDate   = DateTime.Today,
            Vendors        = await GetVendorsAsync()
        };

    // ── Load for edit ─────────────────────────────────────────────
    public async Task<PurchaseFormViewModel> GetForEditAsync(int purchaseId)
    {
        var p = await _db.Purchases
            .Include(x => x.PurchaseItems).ThenInclude(pi => pi.Book)
            .Include(x => x.Vendor)
            .FirstOrDefaultAsync(x => x.PurchaseId == purchaseId)
            ?? throw new KeyNotFoundException($"Purchase {purchaseId} not found.");

        return new PurchaseFormViewModel
        {
            PurchaseId           = p.PurchaseId,
            PurchaseNumber       = p.PurchaseNumber,
            InvoiceOrOrder       = p.InvoiceOrOrder,
            VendorId             = p.VendorId,
            PurchaseDate         = p.PurchaseDate,
            ManualPurchaseNumber = p.ManualPurchaseNumber,
            Subject              = p.Subject,
            TotalAmount          = p.TotalAmount,
            OverallDiscount      = p.OverallDiscount,
            NetAmount            = p.NetAmount,
            Vendors              = await GetVendorsAsync(),
            Items = p.PurchaseItems.Select(pi => new PurchaseItemViewModel
            {
                PurchaseItemId  = pi.PurchaseItemId,
                BookId          = pi.BookId,
                ItemName        = pi.ItemName ?? pi.Book?.Title,
                SerialNo        = pi.SerialNo,
                Quantity        = pi.Quantity,
                PricePerUnit    = pi.PricePerUnit,
                DiscountPercent = pi.DiscountPercent,
                NetPrice        = pi.NetPrice
            }).ToList()
        };
    }

    // ── Save ──────────────────────────────────────────────────────
    public async Task<int> SaveAsync(PurchaseFormViewModel vm)
    {
        // ── Use a transaction so quantity updates are atomic ──────
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            Purchase purchase;
            List<PurchaseItem> oldItems = new();

            if (vm.PurchaseId == 0)
            {
                // New purchase
                purchase = new Purchase
                {
                    PurchaseNumber = await NextPurchaseNumberAsync()
                };
                _db.Purchases.Add(purchase);
            }
            else
            {
                // Edit — load existing items so we can reverse old quantities
                purchase = await _db.Purchases
                    .Include(p => p.PurchaseItems)
                    .FirstOrDefaultAsync(p => p.PurchaseId == vm.PurchaseId)
                    ?? throw new KeyNotFoundException($"Purchase {vm.PurchaseId} not found.");

                oldItems = purchase.PurchaseItems.ToList();

                // Reverse old quantity additions
                foreach (var oi in oldItems.Where(i => i.BookId.HasValue))
                {
                    var book = await _db.Books.FindAsync(oi.BookId!.Value);
                    if (book != null)
                    {
                        book.TotalQuantity     -= oi.Quantity;
                        book.RemainingQuantity -= oi.Quantity;
                    }
                }
                _db.PurchaseItems.RemoveRange(oldItems);
            }

            // Map header
            purchase.InvoiceOrOrder       = vm.InvoiceOrOrder;
            purchase.VendorId             = vm.VendorId;
            purchase.PurchaseDate         = vm.PurchaseDate;
            purchase.ManualPurchaseNumber = vm.ManualNumberEnabled
                ? vm.ManualPurchaseNumber?.Trim() : null;
            purchase.Subject              = vm.Subject?.Trim();

            // Recalculate totals server-side (never trust client)
            decimal total = 0;
            var newItems = new List<PurchaseItem>();

            foreach (var item in vm.Items)
            {
                if (item.Quantity <= 0) continue;
                var net = Math.Round(
                    item.Quantity * item.PricePerUnit * (1 - item.DiscountPercent / 100m), 2);
                total += net;

                newItems.Add(new PurchaseItem
                {
                    PurchaseId      = purchase.PurchaseId,
                    BookId          = item.BookId,
                    ItemName        = item.ItemName?.Trim(),
                    SerialNo        = item.SerialNo?.Trim(),
                    Quantity        = item.Quantity,
                    PricePerUnit    = item.PricePerUnit,
                    DiscountPercent = item.DiscountPercent,
                    NetPrice        = net
                });
            }

            purchase.TotalAmount     = total;
            purchase.OverallDiscount = vm.OverallDiscount >= 0 ? vm.OverallDiscount : 0;
            purchase.NetAmount       = Math.Max(0, total - purchase.OverallDiscount);

            await _db.SaveChangesAsync();

            // Set PurchaseId on new items and add
            foreach (var ni in newItems)
            {
                ni.PurchaseId = purchase.PurchaseId;
                _db.PurchaseItems.Add(ni);
            }
            await _db.SaveChangesAsync();

            // Update book quantities for new items
            foreach (var ni in newItems.Where(i => i.BookId.HasValue))
            {
                var book = await _db.Books.FindAsync(ni.BookId!.Value);
                if (book != null)
                {
                    book.TotalQuantity     += ni.Quantity;
                    book.RemainingQuantity += ni.Quantity;
                }
            }
            await _db.SaveChangesAsync();

            await tx.CommitAsync();
            return purchase.PurchaseId;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ── History search ────────────────────────────────────────────
    public async Task<List<PurchaseListViewModel>> SearchAsync(PurchaseFilterViewModel f)
    {
        var q = _db.Purchases
            .Include(p => p.Vendor)
            .Include(p => p.PurchaseItems)
            .AsQueryable();

        if (f.FromDate.HasValue)
            q = q.Where(p => p.PurchaseDate >= f.FromDate.Value);

        if (f.ToDate.HasValue)
            q = q.Where(p => p.PurchaseDate <= f.ToDate.Value.AddDays(1).AddSeconds(-1));

        if (!f.BothTypes && f.InvoiceOrOrder.HasValue)
            q = q.Where(p => p.InvoiceOrOrder == f.InvoiceOrOrder.Value);

        if (!f.AllVendors && f.VendorId.HasValue)
            q = q.Where(p => p.VendorId == f.VendorId.Value);

        if (!f.AllPurchaseNums && !string.IsNullOrWhiteSpace(f.PurchaseNumber))
            q = q.Where(p => p.PurchaseNumber == f.PurchaseNumber.Trim());

        return await q
            .OrderByDescending(p => p.PurchaseDate)
            .ThenByDescending(p => p.PurchaseId)
            .Take(500)
            .Select(p => new PurchaseListViewModel
            {
                PurchaseId     = p.PurchaseId,
                PurchaseNumber = p.PurchaseNumber,
                VendorName     = p.Vendor != null ? p.Vendor.Name : null,
                PurchaseDate   = p.PurchaseDate,
                InvoiceOrOrder = p.InvoiceOrOrder,
                NetAmount      = p.NetAmount,
                ItemCount      = p.PurchaseItems.Count
            })
            .ToListAsync();
    }

    // ── Book stock ────────────────────────────────────────────────
    public async Task<int> GetBookStockAsync(int bookId)
    {
        var book = await _db.Books.FindAsync(bookId);
        return book?.RemainingQuantity ?? 0;
    }
}
