using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

public class BooksController : Controller
{
    private readonly IBookService      _bookSvc;
    private readonly IWebHostEnvironment _env;

    public BooksController(IBookService bookSvc, IWebHostEnvironment env)
    {
        _bookSvc = bookSvc;
        _env     = env;
    }

    // ── Manage Books (index with filter bar) ────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Manage Books";
        // Dropdowns for filter bar
        var form = await _bookSvc.GetFormDataAsync(0);
        return View(form);
    }

    // ── Server-side paged data (AJAX) ────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        int?   bookTypeId,
        int?   categoryId,
        int?   authorId,
        int?   publisherId,
        string? searchText,
        string? accCallText,
        string? status,
        int page     = 1,
        int pageSize = 20)
    {
        var filter = new BookFilterViewModel
        {
            BookTypeId  = bookTypeId,
            CategoryId  = categoryId,
            AuthorId    = authorId,
            PublisherId = publisherId,
            SearchText  = searchText,
            AccCallText = accCallText,
            Status      = status,
            Page        = page,
            PageSize    = pageSize
        };
        var result = await _bookSvc.GetPagedAsync(filter);
        return Json(result);
    }

    // ── Add Book ─────────────────────────────────────────────────────
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Add Book";
        var vm = await _bookSvc.GetFormDataAsync(0);
        return View("BookForm", vm);
    }

    // ── Edit Book ────────────────────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Book";
        var vm = await _bookSvc.GetFormDataAsync(id);
        if (vm.BookId == 0 && id != 0)
            return NotFound();
        return View("BookForm", vm);
    }

    // ── Save (POST) ──────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(BookFormViewModel vm, IFormFile? CoverImage)
    {
        if (!ModelState.IsValid)
        {
            var fresh = await _bookSvc.GetFormDataAsync(vm.BookId);
            vm.Authors = fresh.Authors; vm.Publishers = fresh.Publishers;
            vm.Categories = fresh.Categories; vm.BookTypes = fresh.BookTypes; vm.Vendors = fresh.Vendors;
            ViewData["Title"] = vm.BookId == 0 ? "Add Book" : "Edit Book";
            return View("BookForm", vm);
        }

        try
        {
            Stream? imageStream = null;
            string? imageFileName = null;

            if (CoverImage != null && CoverImage.Length > 0)
            {
                if (CoverImage.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "Cover image must be under 5 MB.");
                    var fresh2 = await _bookSvc.GetFormDataAsync(vm.BookId);
                    vm.Authors = fresh2.Authors; vm.Publishers = fresh2.Publishers;
                    vm.Categories = fresh2.Categories; vm.BookTypes = fresh2.BookTypes; vm.Vendors = fresh2.Vendors;
                    ViewData["Title"] = vm.BookId == 0 ? "Add Book" : "Edit Book";
                    return View("BookForm", vm);
                }
                imageStream   = CoverImage.OpenReadStream();
                imageFileName = CoverImage.FileName;
            }

            await _bookSvc.SaveAsync(vm, _env.WebRootPath, imageStream, imageFileName);
            TempData["Success"] = vm.BookId == 0 ? "Book added successfully." : "Book updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            var fresh = await _bookSvc.GetFormDataAsync(vm.BookId);
            vm.Authors = fresh.Authors; vm.Publishers = fresh.Publishers;
            vm.Categories = fresh.Categories; vm.BookTypes = fresh.BookTypes; vm.Vendors = fresh.Vendors;
            ViewData["Title"] = vm.BookId == 0 ? "Add Book" : "Edit Book";
            return View("BookForm", vm);
        }
    }

    // ── Delete (AJAX) ────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] int id)
    {
        try
        {
            var ok = await _bookSvc.DeleteAsync(id);
            return Json(new { success = ok });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // ── Book Search widget (AJAX) ────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Search(string criteria, string keyword)
    {
        var results = await _bookSvc.SearchAsync(criteria ?? "", keyword ?? "");
        return Json(results);
    }

    // ── Quick-add Publisher inline "+" (AJAX) ────────────────────────
    [HttpPost]
    public async Task<IActionResult> QuickAddPublisher(
        [FromServices] IPublisherService pubSvc,
        [FromBody] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Json(new { success = false, message = "Name required." });
        var vm = await pubSvc.SaveAsync(new PublisherViewModel { Name = name.Trim() });
        return Json(new { success = true, data = vm });
    }

    // ── Quick-add Vendor inline "+" (AJAX) ───────────────────────────
    [HttpPost]
    public async Task<IActionResult> QuickAddVendor(
        [FromServices] IVendorService vendorSvc,
        [FromBody] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Json(new { success = false, message = "Name required." });
        var vm = await vendorSvc.SaveAsync(new VendorViewModel { Name = name.Trim() });
        return Json(new { success = true, data = vm });
    }
}
