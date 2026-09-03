using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

[Authorize]
public class AuthorsController : Controller
{
    private readonly IAuthorService _svc;
    public AuthorsController(IAuthorService svc) => _svc = svc;

    // GET /Authors
    public IActionResult Index()
    {
        ViewData["Title"] = "Manage Authors";
        return View();
    }

    // GET /Authors/GetAll  — AJAX
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _svc.GetAllAsync();
        return Json(list);
    }

    // POST /Authors/Save  — AJAX (insert or update)
    [HttpPost]
    public async Task<IActionResult> Save([FromBody] AuthorViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return Json(new { success = false, message = string.Join("; ", errors) });
        }
        try
        {
            var saved = await _svc.SaveAsync(vm);
            return Json(new { success = true, data = saved });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST /Authors/SaveAll  — AJAX (bulk save entire grid)
    [HttpPost]
    public async Task<IActionResult> SaveAll([FromBody] List<AuthorViewModel> rows)
    {
        if (rows == null || rows.Count == 0)
            return Json(new { success = true, message = "Nothing to save." });
        try
        {
            foreach (var vm in rows)
            {
                if (string.IsNullOrWhiteSpace(vm.Name)) continue;
                await _svc.SaveAsync(vm);
            }
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // DELETE /Authors/Delete/5  — AJAX
    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] int id)
    {
        try
        {
            var ok = await _svc.DeleteAsync(id);
            return Json(new { success = ok, message = ok ? "Deleted." : "Record not found." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
