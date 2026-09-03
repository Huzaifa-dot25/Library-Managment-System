using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IBookService    _bookSvc;
    private readonly IIssueService   _issueSvc;
    private readonly IPurchaseService _purchaseSvc;
    private readonly IMemberService  _memberSvc;

    public ReportsController(
        IBookService    bookSvc,
        IIssueService   issueSvc,
        IPurchaseService purchaseSvc,
        IMemberService  memberSvc)
    {
        _bookSvc     = bookSvc;
        _issueSvc    = issueSvc;
        _purchaseSvc = purchaseSvc;
        _memberSvc   = memberSvc;
    }

    // ── Books Inventory ───────────────────────────────────────────
    public async Task<IActionResult> BooksInventory()
    {
        ViewData["Title"] = "Books Inventory Report";
        var vm = await _bookSvc.GetFormDataAsync(0);
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> BooksInventoryData([FromBody] BookFilterViewModel filter)
    {
        filter.PageSize = 1000;
        var result = await _bookSvc.GetPagedAsync(filter);
        return Json(result.Items);
    }

    // ── Issued Books ──────────────────────────────────────────────
    public IActionResult IssuedBooks()
    {
        ViewData["Title"] = "Issued Books Report";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> IssuedBooksData([FromBody] IssuedBookFilterViewModel filter)
    {
        var result = await _issueSvc.GetIssuedBooksAsync(filter);
        return Json(result);
    }

    // ── Overdue ───────────────────────────────────────────────────
    public IActionResult Overdue()
    {
        ViewData["Title"] = "Overdue Books Report";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> OverdueData([FromBody] OverdueFilterViewModel filter)
    {
        var result = await _issueSvc.GetOverdueBooksAsync(filter);
        return Json(result);
    }

    // ── Purchases ─────────────────────────────────────────────────
    public IActionResult Purchases()
    {
        ViewData["Title"] = "Purchases Report";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> PurchasesData([FromBody] PurchaseFilterViewModel filter)
    {
        filter.AllVendors      = true;
        filter.AllPurchaseNums = true;
        filter.BothTypes       = true;
        var result = await _purchaseSvc.SearchAsync(filter);
        return Json(result);
    }
}
