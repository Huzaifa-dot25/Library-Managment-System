using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

[Authorize(Roles = "Admin")]

public class CategoriesController : Controller
{
    private readonly ICategoryService _svc;
    public CategoriesController(ICategoryService svc) => _svc = svc;

    public IActionResult Index()
    {
        ViewData["Title"] = "Manage Categories";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _svc.GetAllAsync();
        return Json(list);
    }

    [HttpPost]
    public async Task<IActionResult> SaveAll([FromBody] List<CategoryViewModel> rows)
    {
        if (rows == null || rows.Count == 0)
            return Json(new { success = true });
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

    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] int id)
    {
        try
        {
            var ok = await _svc.DeleteAsync(id);
            return Json(new { success = ok });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
