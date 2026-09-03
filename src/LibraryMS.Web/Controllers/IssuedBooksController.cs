using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

[Authorize]
public class IssuedBooksController : Controller
{
    private readonly IIssueService _svc;
    public IssuedBooksController(IIssueService svc) => _svc = svc;

    // ── Manage Issued Books page ──────────────────────────────────
    public IActionResult Index()
    {
        ViewData["Title"] = "Manage Issued Books";
        return View();
    }

    // ── AJAX: search ─────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Search([FromBody] IssuedBookFilterViewModel filter)
    {
        var results = await _svc.GetIssuedBooksAsync(filter);
        return Json(results);
    }

    // ── AJAX: get single issued book for edit modal ───────────────
    [HttpGet]
    public async Task<IActionResult> GetDetail(int id)
    {
        var filter = new IssuedBookFilterViewModel { SearchText = id.ToString() };
        var all    = await _svc.GetIssuedBooksAsync(filter);
        var item   = all.FirstOrDefault(i => i.IssuedBookId == id);
        if (item == null) return Json(new { success = false });
        return Json(new { success = true, data = item });
    }

    // ── AJAX: return a single book from this page ─────────────────
    [HttpPost]
    public async Task<IActionResult> ReturnBook([FromBody] int issuedBookId)
    {
        try
        {
            await _svc.ReturnBooksAsync(new ReturnBookViewModel
            {
                IssuedBookIds = new List<int> { issuedBookId }
            });
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // ── AJAX: issued book search for dashboard widget ─────────────
    [HttpGet]
    public async Task<IActionResult> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return Json(Array.Empty<object>());

        var filter = new IssuedBookFilterViewModel { SearchText = keyword };
        var results = await _svc.GetIssuedBooksAsync(filter);

        return Json(results.Take(20).Select(r => new
        {
            issueDate  = r.IssueDate.ToString("dd/MM/yyyy"),
            isbn       = r.ISBN,
            bookTitle  = r.BookTitle,
            memberName = r.MemberName
        }));
    }
}
