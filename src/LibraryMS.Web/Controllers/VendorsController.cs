using LibraryMS.Application.Interfaces;
using LibraryMS.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMS.Web.Controllers;

public class VendorsController : Controller
{
    private readonly IVendorService _svc;
    public VendorsController(IVendorService svc) => _svc = svc;

    public IActionResult Index()
    {
        ViewData["Title"] = "Manage Vendors";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Json(await _svc.GetAllAsync());

    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
        var vm = await _svc.GetByIdAsync(id);
        if (vm == null) return Json(new { success = false });
        return Json(new { success = true, data = vm });
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] VendorViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
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

    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] int id)
    {
        try
        {
            var ok = await _svc.DeleteAsync(id);
            return Json(new { success = ok, message = ok ? "Deleted." : "Not found." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
