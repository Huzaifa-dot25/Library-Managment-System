using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using LibraryMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

[Authorize]
public class PurchasesController : Controller
{
    private readonly IPurchaseService _svc;
    private readonly IVendorService   _vendorSvc;

    public PurchasesController(IPurchaseService svc, IVendorService vendorSvc)
    {
        _svc       = svc;
        _vendorSvc = vendorSvc;
    }

    // ── Purchase History ─────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Purchase History";
        var vm = new PurchaseFilterViewModel
        {
            FromDate   = DateTime.Today,
            ToDate     = DateTime.Today,
            AllVendors = true,
            BothTypes  = true,
            Vendors    = await _vendorSvc.GetAllAsync()
        };
        return View(vm);
    }

    // ── AJAX: history search ─────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Search([FromBody] PurchaseFilterViewModel filter)
    {
        filter.Vendors = new(); // not needed for search
        var results = await _svc.SearchAsync(filter);
        return Json(results);
    }

    // ── New Purchase ─────────────────────────────────────────────
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "New Purchase Order / Invoice";
        var vm = await _svc.GetNewFormAsync();
        return View("PurchaseForm", vm);
    }

    // ── Edit Purchase ────────────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Purchase";
        try
        {
            var vm = await _svc.GetForEditAsync(id);
            return View("PurchaseForm", vm);
        }
        catch
        {
            return NotFound();
        }
    }

    // ── Save (AJAX POST) ─────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Save([FromBody] PurchaseFormViewModel vm)
    {
        if (vm == null)
            return Json(new { success = false, message = "Invalid data." });
        try
        {
            var id = await _svc.SaveAsync(vm);
            return Json(new { success = true, purchaseId = id });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // ── AJAX: book stock for "Existing Stock" label ──────────────
    [HttpGet]
    public async Task<IActionResult> GetBookStock(int bookId)
    {
        var stock = await _svc.GetBookStockAsync(bookId);
        return Json(new { stock });
    }

    // ── AJAX: search books for item dropdown ─────────────────────
    [HttpGet]
    public async Task<IActionResult> SearchBooks(
        [FromServices] IBookService bookSvc,
        string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return Json(Array.Empty<object>());

        var results = await bookSvc.SearchAsync("title", keyword);
        return Json(results.Select(b => new
        {
            b.BookId,
            b.Title,
            b.ISBN,
            b.RemainingQuantity
        }));
    }

    // ── AJAX: quick-add vendor from Purchase form ────────────────
    [HttpPost]
    public async Task<IActionResult> QuickAddVendor([FromBody] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Json(new { success = false, message = "Name required." });
        var vm = await _vendorSvc.SaveAsync(new VendorViewModel { Name = name.Trim() });
        return Json(new { success = true, data = vm });
    }
}
