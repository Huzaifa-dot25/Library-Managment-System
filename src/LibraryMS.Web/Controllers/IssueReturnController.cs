using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

[Authorize]
public class IssueReturnController : Controller
{
    private readonly IIssueService  _issueSvc;
    private readonly IMemberService _memberSvc;
    private readonly IBookService   _bookSvc;

    public IssueReturnController(
        IIssueService  issueSvc,
        IMemberService memberSvc,
        IBookService   bookSvc)
    {
        _issueSvc  = issueSvc;
        _memberSvc = memberSvc;
        _bookSvc   = bookSvc;
    }

    // ── Main page ─────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Issue / Return Book Members";
        var vm = new StudentViewModel
        {
            Sessions    = await _memberSvc.GetSessionsAsync(),
            ClassLevels = await _memberSvc.GetClassLevelsAsync(),
            Sections    = await _memberSvc.GetSectionsAsync()
        };
        return View(vm);
    }

    // ── AJAX: search members ──────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> SearchMembers([FromBody] MemberFilterViewModel filter)
    {
        var results = await _memberSvc.GetMembersAsync(
            filter.SessionId,
            filter.ClassLevelId,
            filter.SectionId,
            filter.ActiveStatus,
            filter.SearchBy,
            filter.SearchTerm,
            filter.MemberType);

        // Enrich with issued book count
        foreach (var m in results)
        {
            var issued = await _issueSvc.GetIssuedByMemberAsync(m.MemberType, m.MemberId);
            m.IssuedBooksCount = issued.Count;
        }

        return Json(results);
    }

    // ── AJAX: get sections for cascade dropdown ───────────────────
    [HttpGet]
    public async Task<IActionResult> GetSections(int classLevelId)
    {
        var sections = await _memberSvc.GetSectionsAsync(classLevelId);
        return Json(sections);
    }

    // ── AJAX: issue a book ────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> IssueBook([FromBody] IssueBookViewModel vm)
    {
        if (vm.BookId == 0 || vm.MemberId == 0)
            return Json(new { success = false, message = "Book and member are required." });
        try
        {
            var result = await _issueSvc.IssueBookAsync(vm);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // ── AJAX: get books currently issued to a member ──────────────
    [HttpGet]
    public async Task<IActionResult> GetIssuedBooks(string memberType, int memberId)
    {
        var books = await _issueSvc.GetIssuedByMemberAsync(memberType, memberId);
        return Json(books);
    }

    // ── AJAX: return books ────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> ReturnBooks([FromBody] ReturnBookViewModel vm)
    {
        try
        {
            await _issueSvc.ReturnBooksAsync(vm);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // ── AJAX: bulk issue to all class ─────────────────────────────
    [HttpPost]
    public async Task<IActionResult> BulkIssue([FromBody] BulkIssueViewModel vm)
    {
        if (vm.BookId == 0)
            return Json(new { success = false, message = "Select a book first." });
        if (vm.Members == null || !vm.Members.Any())
            return Json(new { success = false, message = "No members in current filter. Search members first." });
        try
        {
            var (success, skipped) = await _issueSvc.BulkIssueAsync(
                vm.BookId, vm.DueDate, vm.Members);
            return Json(new
            {
                success = true,
                message = $"Issued to {success} member(s). {skipped} skipped (already issued or no stock)."
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // ── AJAX: search books for issue dialog ───────────────────────
    [HttpGet]
    public async Task<IActionResult> SearchBooks(string keyword)
    {
        var results = await _bookSvc.SearchAsync("title", keyword ?? "");
        return Json(results);
    }
}

// ── Bulk issue payload ────────────────────────────────────────────────────
public class BulkIssueViewModel
{
    public int      BookId  { get; set; }
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);
    public List<MemberSearchResultViewModel> Members { get; set; } = new();
}