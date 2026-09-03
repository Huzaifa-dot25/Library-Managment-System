using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using LibraryMS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

public class BookTypesController : Controller
{
    private readonly IBookTypeService _svc;
    public BookTypesController(IBookTypeService svc) => _svc = svc;

    public IActionResult Index()
    {
        ViewData["Title"] = "Manage Types";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _svc.GetAllAsync();
        // Return int value for LinkedAccount so JS can map it
        return Json(list.Select(bt => new
        {
            bt.BookTypeId,
            bt.TypeName,
            linkedAccount = (int)bt.LinkedAccount,
            bt.IsActive
        }));
    }

    [HttpPost]
    public async Task<IActionResult> SaveAll([FromBody] List<BookTypeViewModel> rows)
    {
        if (rows == null || rows.Count == 0)
            return Json(new { success = true });
        try
        {
            foreach (var vm in rows)
            {
                if (string.IsNullOrWhiteSpace(vm.TypeName)) continue;
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
